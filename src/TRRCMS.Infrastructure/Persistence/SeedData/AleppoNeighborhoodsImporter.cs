using System.Reflection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Infrastructure.Persistence.SeedData;

/// <summary>
/// Shared loader + UPSERT applier for the Aleppo neighborhoods dataset.
/// Used by both the SeedAleppoNeighborhoodsFromGIS EF migration and the
/// admin bulk-import endpoint, so the two paths behave identically.
///
/// Behavior:
///   * Reads <c>Data/aleppo_neighborhoods_v{n}.json</c> from embedded resources
///     (or accepts an injected payload for the admin endpoint).
///   * For each row: INSERT if FullCode missing, UPDATE in place if present.
///   * Soft-deletes any neighborhood whose FullCode is in the placeholder
///     range (<c>020000001001</c>..<c>020000001020</c>) and is NOT in the new
///     dataset — these are the legacy hardcoded seed entries.
///   * Idempotent: re-running on a DB already at this dataset is a no-op.
///   * Geometry comes in as WKT (POLYGON or MULTIPOLYGON) at SRID 4326.
/// </summary>
public static class AleppoNeighborhoodsImporter
{
    private const string EmbeddedResourceName = "TRRCMS.Infrastructure.Data.aleppo_neighborhoods_v1.json";

    // Placeholder rows from the legacy NeighborhoodSeedData (codes 001..020 under 02/00/00/001).
    private const string PlaceholderFullCodePrefix = "02000000100";
    // Range: 02000000100 1 .. 02000000100 20  → matched by FullCode LIKE '02000000100%' AND length 12
    // (codes 001..020 → "020000001001".."020000001020")

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public static AleppoNeighborhoodsDataset LoadEmbedded()
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(EmbeddedResourceName)
            ?? throw new InvalidOperationException(
                $"Embedded resource '{EmbeddedResourceName}' not found. " +
                $"Ensure Data/aleppo_neighborhoods_v1.json is marked as EmbeddedResource in the .csproj.");
        return JsonSerializer.Deserialize<AleppoNeighborhoodsDataset>(stream, JsonOpts)
            ?? throw new InvalidOperationException("Embedded Aleppo dataset is empty or malformed.");
    }

    public static AleppoNeighborhoodsDataset LoadFromJson(string json)
        => JsonSerializer.Deserialize<AleppoNeighborhoodsDataset>(json, JsonOpts)
           ?? throw new InvalidOperationException("Provided JSON is empty or malformed.");

    /// <summary>
    /// Apply the dataset against the live <c>ApplicationDbContext</c>.
    /// Returns a summary of what changed. Caller is responsible for SaveChanges.
    /// </summary>
    public static async Task<ImportSummary> ApplyAsync(
        ApplicationDbContext context,
        AleppoNeighborhoodsDataset dataset,
        Guid systemUserId,
        CancellationToken cancellationToken = default)
    {
        var summary = new ImportSummary();
        if (dataset.Items is null || dataset.Items.Count == 0)
            return summary;

        var geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
        var wktReader = new WKTReader(geometryFactory);

        // Build the set of FullCodes coming in; everything outside this set in the
        // placeholder range gets soft-deleted at the end.
        var incomingFullCodes = new HashSet<string>(dataset.Items.Count);

        foreach (var item in dataset.Items)
        {
            var govCode  = string.IsNullOrWhiteSpace(item.GovernorateCode)  ? dataset.GovernorateCode  : item.GovernorateCode;
            var distCode = string.IsNullOrWhiteSpace(item.DistrictCode)     ? dataset.DistrictCode     : item.DistrictCode;
            var subCode  = string.IsNullOrWhiteSpace(item.SubDistrictCode)  ? dataset.SubDistrictCode  : item.SubDistrictCode;
            var commCode = string.IsNullOrWhiteSpace(item.CommunityCode)    ? dataset.CommunityCode    : item.CommunityCode;
            var neighCode = (item.NeighborhoodCode ?? string.Empty).Trim();
            if (neighCode.Length == 0)
                throw new InvalidOperationException($"Item missing NeighborhoodCode (pCode={item.PCode}).");

            var fullCode = govCode + distCode + subCode + commCode + neighCode;
            incomingFullCodes.Add(fullCode);

            // Parse boundary (optional). If a single point is supplied we wrap it.
            Geometry? boundary = null;
            if (!string.IsNullOrWhiteSpace(item.BoundaryWkt))
            {
                boundary = wktReader.Read(item.BoundaryWkt);
                boundary.SRID = 4326;
            }

            // Look up existing row by FullCode (ignoring soft-delete filter so we can resurrect).
            var existing = context.Neighborhoods
                .IgnoreQueryFilters()
                .FirstOrDefault(n => n.FullCode == fullCode);

            if (existing is null)
            {
                var entity = Neighborhood.Create(
                    governorateCode:    govCode!,
                    districtCode:       distCode!,
                    subDistrictCode:    subCode!,
                    communityCode:      commCode!,
                    neighborhoodCode:   neighCode,
                    nameArabic:         item.NameArabic ?? string.Empty,
                    nameEnglish:        item.NameEnglish,
                    centerLatitude:     item.CenterLatitude,
                    centerLongitude:    item.CenterLongitude,
                    boundaryGeometry:   boundary,
                    areaSquareKm:       item.AreaSquareKm,
                    zoomLevel:          item.ZoomLevel ?? dataset.ZoomLevel ?? 15,
                    createdByUserId:    systemUserId);

                await context.Neighborhoods.AddAsync(entity, cancellationToken);
                summary.Inserted++;
            }
            else
            {
                // Update names + spatial data if anything actually changed.
                var nameChanged = existing.NameArabic != (item.NameArabic ?? string.Empty)
                                || existing.NameEnglish != item.NameEnglish;
                if (nameChanged)
                    existing.UpdateNames(item.NameArabic ?? string.Empty, item.NameEnglish, systemUserId);

                if (boundary is not null)
                {
                    var existingWkt = existing.BoundaryGeometry?.AsText();
                    if (existingWkt != boundary.AsText())
                        existing.UpdateBoundary(boundary, systemUserId);
                }

                if (existing.IsDeleted)
                {
                    existing.Restore(systemUserId);
                    summary.Restored++;
                }

                summary.Updated++;
            }
        }

        // Soft-delete legacy placeholders (codes 001..020 under 02/00/00/001) that are not in the new set.
        var placeholders = context.Neighborhoods
            .IgnoreQueryFilters()
            .Where(n => !n.IsDeleted
                     && n.GovernorateCode == "02"
                     && n.DistrictCode == "00"
                     && n.SubDistrictCode == "00"
                     && n.CommunityCode == "001"
                     && n.NeighborhoodCode.Length == 3
                     && n.NeighborhoodCode.CompareTo("001") >= 0
                     && n.NeighborhoodCode.CompareTo("020") <= 0)
            .ToList();

        foreach (var p in placeholders)
        {
            if (!incomingFullCodes.Contains(p.FullCode))
            {
                p.MarkAsDeleted(systemUserId);
                summary.SoftDeletedPlaceholders++;
            }
        }

        return summary;
    }

    public sealed class ImportSummary
    {
        public int Inserted;
        public int Updated;
        public int Restored;
        public int SoftDeletedPlaceholders;
        public override string ToString()
            => $"inserted={Inserted}, updated={Updated}, restored={Restored}, soft-deleted-placeholders={SoftDeletedPlaceholders}";
    }

    // ────────────────────────────────────────────────────────────────────────
    //  Raw-SQL path — used by the EF migration where DbContext is not available.
    //  Produces idempotent UPSERTs (INSERT … ON CONFLICT (FullCode) DO UPDATE …)
    //  plus a single soft-delete for the 001..020 placeholder range.
    // ────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Build the SQL statements needed to apply the dataset against a PostGIS-enabled
    /// PostgreSQL DB. The migration enqueues each string via <c>migrationBuilder.Sql(...)</c>;
    /// the admin endpoint can execute the same strings via <c>Database.ExecuteSqlRaw</c>.
    /// </summary>
    public static IEnumerable<string> BuildSeedSqlStatements(AleppoNeighborhoodsDataset dataset)
    {
        if (dataset.Items is null || dataset.Items.Count == 0)
            yield break;

        var systemUser = "00000000-0000-0000-0000-000000000000";
        var defaultZoom = dataset.ZoomLevel ?? 15;

        var incomingFullCodes = new List<string>(dataset.Items.Count);

        foreach (var item in dataset.Items)
        {
            var gov  = NonEmpty(item.GovernorateCode, dataset.GovernorateCode);
            var dist = NonEmpty(item.DistrictCode,    dataset.DistrictCode);
            var sub  = NonEmpty(item.SubDistrictCode, dataset.SubDistrictCode);
            var comm = NonEmpty(item.CommunityCode,   dataset.CommunityCode);
            var neighCode = (item.NeighborhoodCode ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(neighCode))
                throw new InvalidOperationException($"Item missing NeighborhoodCode (pCode={item.PCode}).");

            var fullCode = gov + dist + sub + comm + neighCode;
            incomingFullCodes.Add(fullCode);

            var nameAr = SqlString(item.NameArabic ?? string.Empty);
            var nameEn = item.NameEnglish is null ? "NULL" : SqlString(item.NameEnglish);
            var areaSql = item.AreaSquareKm.HasValue
                ? item.AreaSquareKm.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)
                : "NULL";
            var zoom = item.ZoomLevel ?? defaultZoom;
            var lat = item.CenterLatitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
            var lng = item.CenterLongitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
            var boundarySql = string.IsNullOrWhiteSpace(item.BoundaryWkt)
                ? "NULL"
                : $"ST_GeomFromText({SqlString(item.BoundaryWkt)}, 4326)";

            yield return $@"
INSERT INTO ""Neighborhoods"" (
    ""Id"", ""GovernorateCode"", ""DistrictCode"", ""SubDistrictCode"",
    ""CommunityCode"", ""NeighborhoodCode"", ""FullCode"",
    ""NameArabic"", ""NameEnglish"",
    ""CenterLatitude"", ""CenterLongitude"", ""CenterPoint"",
    ""BoundaryGeometry"", ""AreaSquareKm"", ""ZoomLevel"",
    ""IsActive"", ""IsDeleted"", ""CreatedAtUtc"", ""CreatedBy""
) VALUES (
    gen_random_uuid(),
    {SqlString(gov!)}, {SqlString(dist!)}, {SqlString(sub!)},
    {SqlString(comm!)}, {SqlString(neighCode)}, {SqlString(fullCode)},
    {nameAr}, {nameEn},
    {lat}, {lng}, ST_SetSRID(ST_MakePoint({lng}, {lat}), 4326),
    {boundarySql}, {areaSql}, {zoom},
    true, false, NOW() AT TIME ZONE 'UTC', '{systemUser}'
)
ON CONFLICT (""FullCode"") DO UPDATE SET
    ""NameArabic""        = EXCLUDED.""NameArabic"",
    ""NameEnglish""       = EXCLUDED.""NameEnglish"",
    ""CenterLatitude""    = EXCLUDED.""CenterLatitude"",
    ""CenterLongitude""   = EXCLUDED.""CenterLongitude"",
    ""CenterPoint""       = EXCLUDED.""CenterPoint"",
    ""BoundaryGeometry""  = EXCLUDED.""BoundaryGeometry"",
    ""AreaSquareKm""      = EXCLUDED.""AreaSquareKm"",
    ""ZoomLevel""         = EXCLUDED.""ZoomLevel"",
    ""IsActive""          = true,
    ""IsDeleted""         = false,
    ""DeletedAtUtc""      = NULL,
    ""DeletedBy""         = NULL,
    ""LastModifiedAtUtc"" = NOW() AT TIME ZONE 'UTC',
    ""LastModifiedBy""    = '{systemUser}';";
        }

        // Soft-delete placeholder rows (codes 001..020 under 02/00/00/001) that
        // are NOT in the incoming dataset. Idempotent: re-running has no effect
        // because already-deleted rows are excluded by the WHERE.
        var inList = string.Join(", ", incomingFullCodes.Select(SqlString));
        yield return $@"
UPDATE ""Neighborhoods""
   SET ""IsDeleted""    = true,
       ""DeletedAtUtc"" = NOW() AT TIME ZONE 'UTC',
       ""DeletedBy""    = '{systemUser}'
 WHERE ""GovernorateCode""  = '02'
   AND ""DistrictCode""     = '00'
   AND ""SubDistrictCode""  = '00'
   AND ""CommunityCode""    = '001'
   AND ""NeighborhoodCode"" >= '001'
   AND ""NeighborhoodCode"" <= '020'
   AND ""IsDeleted""        = false
   AND ""FullCode"" NOT IN ({inList});";
    }

    private static string SqlString(string s) => "'" + s.Replace("'", "''") + "'";
    private static string? NonEmpty(string? primary, string? fallback)
        => string.IsNullOrWhiteSpace(primary) ? fallback : primary;
}

/// <summary>JSON shape of the embedded Aleppo dataset.</summary>
public sealed class AleppoNeighborhoodsDataset
{
    public int Version { get; set; }
    public string? Source { get; set; }
    public string? Crs { get; set; }
    public string? GovernorateCode { get; set; }
    public string? DistrictCode { get; set; }
    public string? SubDistrictCode { get; set; }
    public string? CommunityCode { get; set; }
    public string? CommunityExternalPCode { get; set; }
    public int? ZoomLevel { get; set; }
    public List<AleppoNeighborhoodItem> Items { get; set; } = new();
}

public sealed class AleppoNeighborhoodItem
{
    public string? PCode { get; set; }
    public string? NeighborhoodCode { get; set; }
    public string? GovernorateCode { get; set; }
    public string? DistrictCode { get; set; }
    public string? SubDistrictCode { get; set; }
    public string? CommunityCode { get; set; }
    public string? NameArabic { get; set; }
    public string? NameEnglish { get; set; }
    public decimal CenterLatitude { get; set; }
    public decimal CenterLongitude { get; set; }
    public string? BoundaryWkt { get; set; }
    public double? AreaSquareKm { get; set; }
    public int? ZoomLevel { get; set; }
}
