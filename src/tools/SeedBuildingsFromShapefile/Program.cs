// =============================================================================
// SeedBuildingsFromShapefile — converts a client shapefile into TRRCMS buildings.
//
// Three output modes (pick one):
//
//   1. --output-json <path>   Convert shapefile → canonical JSON (no API calls).
//                             Used to generate the embedded resource for the
//                             SeedSampleBuildingsFromGIS migration, or to feed
//                             POST /api/v1/Buildings/import-bulk.
//
//   2. --dry-run              Print parsed rows without doing anything else.
//
//   3. (default)              POST each row to /api/v1/Buildings/register.
//
// Pipeline (shared by all modes):
//   * Read .shp + .dbf via NetTopologySuite.IO.Esri
//   * Parse the .prj and reproject coordinates to EPSG:4326 via ProjNet
//   * Build POINT(lon lat) WKT (X Y order, per OGC)
//   * Map DBF attributes → OCHA pCodes (Governorat, DistrictCo, SubDistric,
//                                         CommunityC, Neighborho, BuildingNu)
//
// Examples:
//   # Generate the embedded JSON for the migration
//   dotnet run --project tools/SeedBuildingsFromShapefile -- \
//     --shapefile "tools/SeedBuildingsFromShapefile/shapefiles/aleppo-2026-05-batch1/buidings.shp" \
//     --output-json "TRRCMS.Infrastructure/Data/buildings_sample_v1.json"
//
//   # Live-import into a running API
//   dotnet run --project tools/SeedBuildingsFromShapefile -- \
//     --shapefile "...buidings.shp" --api "https://localhost:7204" \
//     --username admin --password "Admin@123" --insecure
// =============================================================================

using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Esri;
using NetTopologySuite.IO.Esri.Shapefiles.Readers;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;

// ── CLI parsing ──────────────────────────────────────────────────────────────
var argMap = ParseArgs(args);

string Required(string name) =>
    argMap.TryGetValue(name, out var v) && !string.IsNullOrWhiteSpace(v)
        ? v : throw new ArgumentException($"Missing required argument: --{name}");

string Optional(string name, string @default) =>
    argMap.TryGetValue(name, out var v) && !string.IsNullOrWhiteSpace(v) ? v : @default;

bool Flag(string name) => argMap.ContainsKey(name);

var shapefile = Required("shapefile");
var apiBase = Optional("api", "https://localhost:7204").TrimEnd('/');
var username = Optional("username", "");
var password = Optional("password", "");
var dryRun = Flag("dry-run");
var insecure = Flag("insecure");
var outputJsonPath = Optional("output-json", "");

var fieldBuildingNumber = Optional("building-number-field", "BuildingNu");
var fieldGovPCode = Optional("gov-pcode-field", "Governorat");
var fieldDistPCode = Optional("district-pcode-field", "DistrictCo");
var fieldSubDistPCode = Optional("subdistrict-pcode-field", "SubDistric");
var fieldCommPCode = Optional("community-pcode-field", "CommunityC");
var fieldNeighPCode = Optional("neighborhood-pcode-field", "Neighborho");

var writeJson = !string.IsNullOrEmpty(outputJsonPath);
var liveMode = !dryRun && !writeJson;

if (liveMode && (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)))
    throw new ArgumentException("--username and --password required unless --dry-run or --output-json is used");

if (!File.Exists(shapefile))
    throw new FileNotFoundException($"Shapefile not found: {shapefile}");

// ── Load CRS from .prj and build transform → EPSG:4326 ───────────────────────
var prjPath = Path.ChangeExtension(shapefile, ".prj");
if (!File.Exists(prjPath))
    throw new FileNotFoundException($".prj file not found next to .shp: {prjPath}");

var prjWkt = await File.ReadAllTextAsync(prjPath);
Console.WriteLine($"Source CRS (.prj): {prjWkt.Substring(0, Math.Min(80, prjWkt.Length))}...");

var csFactory = new CoordinateSystemFactory();
var sourceCs = csFactory.CreateFromWkt(prjWkt);
var targetCs = GeographicCoordinateSystem.WGS84; // EPSG:4326

var ctFactory = new CoordinateTransformationFactory();
var transform = ctFactory.CreateFromCoordinateSystems(sourceCs, targetCs);
var mathTransform = transform.MathTransform;

var sameCrs = sourceCs.AuthorityCode == 4326;
Console.WriteLine(sameCrs
    ? "Source already in EPSG:4326 — passthrough."
    : $"Reprojecting → EPSG:4326 (source authority code: {sourceCs.AuthorityCode}).");

// ── Read shapefile rows ──────────────────────────────────────────────────────
var rows = new List<RegisterBuildingPayload>();
using (var reader = Shapefile.OpenRead(shapefile))
{
    var fields = new List<string>();
    for (int i = 0; i < reader.Fields.Count; i++)
        fields.Add(reader.Fields[i].Name);
    AssertField(fields, fieldBuildingNumber);
    AssertField(fields, fieldGovPCode);
    AssertField(fields, fieldDistPCode);
    AssertField(fields, fieldSubDistPCode);
    AssertField(fields, fieldCommPCode);
    AssertField(fields, fieldNeighPCode);

    int rowIdx = 0;
    while (reader.Read())
    {
        rowIdx++;
        var geom = reader.Geometry;
        if (geom is null || geom.IsEmpty)
        {
            Console.WriteLine($"  [{rowIdx}] SKIP — empty geometry");
            continue;
        }

        // Get a representative coordinate. For Point: itself. For Polygon/Line: centroid.
        var coord = geom is Point p ? p.Coordinate : geom.Centroid.Coordinate;

        // Reproject to lon/lat (EPSG:4326). Source axis order matches what NTS reads from .shp.
        var (lon, lat) = ReprojectToLonLat(coord.X, coord.Y, mathTransform, sameCrs);

        // Build POINT(lon lat) WKT — OGC X Y order.
        var wkt = $"POINT({lon.ToString("F8", CultureInfo.InvariantCulture)} {lat.ToString("F8", CultureInfo.InvariantCulture)})";

        var payload = new RegisterBuildingPayload
        {
            BuildingNumber = ReadString(reader, fieldBuildingNumber),
            GovernoratePCode = ReadString(reader, fieldGovPCode),
            DistrictPCode = ReadString(reader, fieldDistPCode),
            SubDistrictPCode = ReadString(reader, fieldSubDistPCode),
            CommunityPCode = ReadString(reader, fieldCommPCode),
            NeighborhoodPCode = ReadString(reader, fieldNeighPCode),
            BuildingGeometryWkt = wkt,
        };

        rows.Add(payload);
        Console.WriteLine(
            $"  [{rowIdx}] BldgNum={payload.BuildingNumber} | {payload.NeighborhoodPCode}/{payload.CommunityPCode} | {wkt}");
    }
}

Console.WriteLine($"\nLoaded {rows.Count} rows from shapefile.\n");

if (dryRun)
{
    Console.WriteLine("--dry-run set — exiting without API calls.");
    return;
}

// ── --output-json: write canonical dataset JSON, no API calls ────────────────
if (writeJson)
{
    var dataset = new BuildingsDataset
    {
        Version = 1,
        Source = Path.GetFileName(shapefile),
        Crs = "EPSG:4326",
        GeneratedAtUtc = DateTime.UtcNow,
        Items = rows.Select(r => new BuildingItem
        {
            BuildingNumber = r.BuildingNumber,
            GovernoratePCode = r.GovernoratePCode,
            DistrictPCode = r.DistrictPCode,
            SubDistrictPCode = r.SubDistrictPCode,
            CommunityPCode = r.CommunityPCode,
            NeighborhoodPCode = r.NeighborhoodPCode,
            GeometryWkt = r.BuildingGeometryWkt,
            Notes = r.Notes,
        }).ToList(),
    };

    var fullPath = Path.GetFullPath(outputJsonPath);
    var dir = Path.GetDirectoryName(fullPath);
    if (!string.IsNullOrEmpty(dir))
        Directory.CreateDirectory(dir);

    var jsonOpts = new JsonSerializerOptions
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };
    var jsonText = JsonSerializer.Serialize(dataset, jsonOpts);
    await File.WriteAllTextAsync(fullPath, jsonText);

    Console.WriteLine($"Wrote {rows.Count} buildings to {fullPath}");
    return;
}

// ── Authenticate ─────────────────────────────────────────────────────────────
var handler = new HttpClientHandler();
if (insecure)
    handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;

using var http = new HttpClient(handler) { BaseAddress = new Uri(apiBase) };

Console.WriteLine($"Logging in to {apiBase} as {username}...");
var loginResp = await http.PostAsJsonAsync("/api/v1/Auth/login", new
{
    username,
    password,
    deviceId = "SeedBuildingsFromShapefile"
});
if (!loginResp.IsSuccessStatusCode)
{
    var err = await loginResp.Content.ReadAsStringAsync();
    throw new Exception($"Login failed: {loginResp.StatusCode} — {err}");
}
var loginJson = await loginResp.Content.ReadFromJsonAsync<JsonElement>();
var token = loginJson.GetProperty("accessToken").GetString()
    ?? throw new Exception("No accessToken in login response.");
http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
Console.WriteLine("Login OK.\n");

// ── POST each building ───────────────────────────────────────────────────────
int created = 0, conflict = 0, failed = 0;
foreach (var (payload, idx) in rows.Select((r, i) => (r, i + 1)))
{
    var resp = await http.PostAsJsonAsync("/api/v1/Buildings/register", payload);
    var body = await resp.Content.ReadAsStringAsync();

    if (resp.IsSuccessStatusCode)
    {
        created++;
        Console.WriteLine($"  [{idx}/{rows.Count}] ✓ created — BldgNum={payload.BuildingNumber}");
    }
    else if ((int)resp.StatusCode == 409)
    {
        conflict++;
        Console.WriteLine($"  [{idx}/{rows.Count}] ↺ duplicate — BldgNum={payload.BuildingNumber}");
    }
    else
    {
        failed++;
        Console.WriteLine($"  [{idx}/{rows.Count}] ✗ {(int)resp.StatusCode} — BldgNum={payload.BuildingNumber}");
        Console.WriteLine($"      {Truncate(body, 300)}");
    }
}

Console.WriteLine($"\nDone. Created: {created}  Duplicates: {conflict}  Failed: {failed}  Total: {rows.Count}");
Environment.ExitCode = failed > 0 ? 1 : 0;

// ── Helpers ──────────────────────────────────────────────────────────────────
static (double lon, double lat) ReprojectToLonLat(double x, double y, MathTransform t, bool sameCrs)
{
    if (sameCrs)
    {
        // Source is EPSG:4326. The .prj GEOGCS axis order is lon/lat in the EPSG/OGC
        // convention used by NTS Shapefile reader, so X=lon, Y=lat.
        return (x, y);
    }
    var result = t.Transform(new[] { x, y });
    return (result[0], result[1]);
}

static string ReadString(ShapefileReader r, string field)
{
    var v = r.Fields[field].Value;
    return v?.ToString()?.Trim() ?? string.Empty;
}

static void AssertField(List<string> fields, string name)
{
    if (!fields.Contains(name, StringComparer.OrdinalIgnoreCase))
        throw new InvalidOperationException(
            $"DBF field '{name}' not found. Available: {string.Join(", ", fields)}");
}

static Dictionary<string, string> ParseArgs(string[] args)
{
    var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    for (int i = 0; i < args.Length; i++)
    {
        if (!args[i].StartsWith("--")) continue;
        var key = args[i].Substring(2);
        if (i + 1 < args.Length && !args[i + 1].StartsWith("--"))
        {
            dict[key] = args[i + 1];
            i++;
        }
        else
        {
            dict[key] = "true"; // bare flag
        }
    }
    return dict;
}

static string Truncate(string s, int max) => s.Length <= max ? s : s.Substring(0, max) + "…";

// ── Payload (matches RegisterBuildingCommand) ────────────────────────────────
public class RegisterBuildingPayload
{
    public string BuildingNumber { get; set; } = string.Empty;
    public string? GovernoratePCode { get; set; }
    public string? DistrictPCode { get; set; }
    public string? SubDistrictPCode { get; set; }
    public string? CommunityPCode { get; set; }
    public string? NeighborhoodPCode { get; set; }
    public string BuildingGeometryWkt { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

// ── Canonical dataset JSON (matches BuildingsDataset on the server side) ─────
public class BuildingsDataset
{
    public int Version { get; set; }
    public string? Source { get; set; }
    public string? Crs { get; set; }
    public DateTime GeneratedAtUtc { get; set; }
    public List<BuildingItem> Items { get; set; } = new();
}

public class BuildingItem
{
    public string BuildingNumber { get; set; } = string.Empty;
    public string? GovernoratePCode { get; set; }
    public string? DistrictPCode { get; set; }
    public string? SubDistrictPCode { get; set; }
    public string? CommunityPCode { get; set; }
    public string? NeighborhoodPCode { get; set; }
    public string GeometryWkt { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
