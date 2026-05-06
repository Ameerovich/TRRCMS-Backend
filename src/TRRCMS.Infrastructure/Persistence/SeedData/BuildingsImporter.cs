using System.Globalization;
using System.Reflection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using TRRCMS.Application.Common;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Infrastructure.Persistence.SeedData;

/// <summary>
/// Shared loader + UPSERT applier for the sample-buildings dataset sourced from
/// the client's GIS shapefile via <c>tools/SeedBuildingsFromShapefile</c>.
///
/// Used by both the <c>SeedSampleBuildingsFromGIS</c> EF migration (raw SQL)
/// and the <c>POST /api/v1/Buildings/import-bulk</c> admin endpoint (EF UPSERT).
///
/// Behavior:
///   * Reads <c>Data/buildings_sample_v{n}.json</c> from embedded resources
///     (or accepts an injected payload for the admin endpoint).
///   * For each item: resolves OCHA pCodes → raw admin codes, looks up the
///     community by <c>ExternalPCode</c>, composes the 17-digit BuildingId,
///     INSERTs if missing or UPDATEs geometry/notes in place if present.
///   * Geometry is parsed as POINT (or POLYGON if the source ever supplies one)
///     at SRID 4326. <see cref="Building.SetGeometry"/> populates Latitude/Longitude.
///   * Defaults: BuildingType=Residential, Status=Unknown, all counts=0 — same
///     as the QGIS register-building flow.
///   * Idempotent: re-running on a DB already at this dataset is a no-op.
///   * If a referenced community/neighborhood is missing (admin hierarchy not
///     yet seeded), the row is skipped rather than throwing.
/// </summary>
public static class BuildingsImporter
{
    private const string EmbeddedResourceName = "TRRCMS.Infrastructure.Data.buildings_sample_v1.json";

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public static BuildingsDataset LoadEmbedded()
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(EmbeddedResourceName)
            ?? throw new InvalidOperationException(
                $"Embedded resource '{EmbeddedResourceName}' not found. " +
                $"Ensure Data/buildings_sample_v1.json is marked as EmbeddedResource in the .csproj.");
        return JsonSerializer.Deserialize<BuildingsDataset>(stream, JsonOpts)
            ?? throw new InvalidOperationException("Embedded buildings dataset is empty or malformed.");
    }

    public static BuildingsDataset LoadFromJson(string json)
        => JsonSerializer.Deserialize<BuildingsDataset>(json, JsonOpts)
           ?? throw new InvalidOperationException("Provided JSON is empty or malformed.");

    /// <summary>
    /// Apply the dataset against the live <c>ApplicationDbContext</c>.
    /// Caller is responsible for <c>SaveChangesAsync</c>.
    /// </summary>
    public static async Task<ImportSummary> ApplyAsync(
        ApplicationDbContext context,
        BuildingsDataset dataset,
        Guid systemUserId,
        CancellationToken cancellationToken = default)
    {
        var summary = new ImportSummary();
        if (dataset.Items is null || dataset.Items.Count == 0)
            return summary;

        var geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
        var wktReader = new WKTReader(geometryFactory);

        foreach (var item in dataset.Items)
        {
            var (govCode, distCode, subDistCode) = OchaCommandNormalizer.ResolveAdmCodes(
                governorateCode: null, districtCode: null, subDistrictCode: null,
                governoratePCode: item.GovernoratePCode,
                districtPCode: item.DistrictPCode,
                subDistrictPCode: item.SubDistrictPCode);
            var neighCode = OchaCommandNormalizer.ResolveNeighborhoodCode(null, item.NeighborhoodPCode);
            var commPCode = OchaCommandNormalizer.NormalizeCommunityPCode(item.CommunityPCode);

            if (string.IsNullOrEmpty(govCode) || string.IsNullOrEmpty(distCode)
                || string.IsNullOrEmpty(subDistCode) || string.IsNullOrEmpty(neighCode)
                || string.IsNullOrEmpty(commPCode))
            {
                summary.Skipped++;
                continue;
            }

            // Resolve community local code (3-digit) by OCHA ExternalPCode.
            var community = await context.Communities
                .FirstOrDefaultAsync(c =>
                    c.ExternalPCode == commPCode &&
                    c.GovernorateCode == govCode &&
                    c.DistrictCode == distCode &&
                    c.SubDistrictCode == subDistCode, cancellationToken);
            if (community is null)
            {
                summary.Skipped++;
                continue;
            }
            var commCode = community.Code;

            var buildingNumber = (item.BuildingNumber ?? string.Empty).Trim();
            if (buildingNumber.Length != 5 || !buildingNumber.All(char.IsDigit))
            {
                summary.Skipped++;
                continue;
            }

            var buildingIdCode = govCode + distCode + subDistCode + commCode + neighCode + buildingNumber;

            // Parse the WKT geometry (POINT in current data; POLYGON also supported).
            Geometry? geometry = null;
            if (!string.IsNullOrWhiteSpace(item.GeometryWkt))
            {
                geometry = wktReader.Read(item.GeometryWkt);
                geometry.SRID = 4326;
            }

            var existing = await context.Buildings
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(b => b.BuildingId == buildingIdCode, cancellationToken);

            if (existing is null)
            {
                // Look up admin names from the hierarchy tables.
                var govName = await context.Governorates
                    .Where(g => g.Code == govCode)
                    .Select(g => g.NameArabic)
                    .FirstOrDefaultAsync(cancellationToken) ?? string.Empty;
                var distName = await context.Districts
                    .Where(d => d.GovernorateCode == govCode && d.Code == distCode)
                    .Select(d => d.NameArabic)
                    .FirstOrDefaultAsync(cancellationToken) ?? string.Empty;
                var subDistName = await context.SubDistricts
                    .Where(s => s.GovernorateCode == govCode && s.DistrictCode == distCode && s.Code == subDistCode)
                    .Select(s => s.NameArabic)
                    .FirstOrDefaultAsync(cancellationToken) ?? string.Empty;
                var neighName = await context.Neighborhoods
                    .Where(n => n.GovernorateCode == govCode
                        && n.DistrictCode == distCode
                        && n.SubDistrictCode == subDistCode
                        && n.CommunityCode == commCode
                        && n.NeighborhoodCode == neighCode)
                    .Select(n => n.NameArabic)
                    .FirstOrDefaultAsync(cancellationToken) ?? string.Empty;

                var entity = Building.Create(
                    governorateCode:   govCode,
                    districtCode:      distCode,
                    subDistrictCode:   subDistCode,
                    communityCode:     commCode,
                    neighborhoodCode:  neighCode,
                    buildingNumber:    buildingNumber,
                    governorateName:   govName,
                    districtName:      distName,
                    subDistrictName:   subDistName,
                    communityName:     community.NameArabic,
                    neighborhoodName:  neighName,
                    buildingType:      BuildingType.Residential,
                    status:            BuildingStatus.Unknown,
                    createdByUserId:   systemUserId);

                if (geometry is not null)
                    entity.SetGeometry(geometry, systemUserId);

                if (!string.IsNullOrWhiteSpace(item.Notes))
                    entity.UpdateDetails(item.Notes, systemUserId);

                await context.Buildings.AddAsync(entity, cancellationToken);
                summary.Inserted++;
            }
            else
            {
                // Update geometry only if it actually changed; never touch admin codes.
                if (geometry is not null)
                {
                    var existingWkt = existing.BuildingGeometry?.AsText();
                    if (existingWkt != geometry.AsText())
                    {
                        existing.SetGeometry(geometry, systemUserId);
                        summary.Updated++;
                        continue;
                    }
                }
                summary.Unchanged++;
            }
        }

        return summary;
    }

    public sealed class ImportSummary
    {
        public int Inserted;
        public int Updated;
        public int Unchanged;
        public int Skipped;
        public override string ToString()
            => $"inserted={Inserted}, updated={Updated}, unchanged={Unchanged}, skipped={Skipped}";
    }

    // ────────────────────────────────────────────────────────────────────────
    //  Raw-SQL path — used by the EF migration where DbContext is not available.
    //  Produces idempotent INSERTs (ON CONFLICT (BuildingId) DO NOTHING) backed
    //  by JOINs to the admin hierarchy tables to resolve community.Code and the
    //  Arabic names. If the hierarchy is missing for a row, the JOIN returns
    //  zero rows and the INSERT is silently skipped.
    // ────────────────────────────────────────────────────────────────────────

    public static IEnumerable<string> BuildSeedSqlStatements(BuildingsDataset dataset)
    {
        if (dataset.Items is null || dataset.Items.Count == 0)
            yield break;

        var systemUser = "00000000-0000-0000-0000-000000000000";
        var buildingType = (int)BuildingType.Residential;
        var status = (int)BuildingStatus.Unknown;

        foreach (var item in dataset.Items)
        {
            var (govCode, distCode, subDistCode) = OchaCommandNormalizer.ResolveAdmCodes(
                governorateCode: null, districtCode: null, subDistrictCode: null,
                governoratePCode: item.GovernoratePCode,
                districtPCode: item.DistrictPCode,
                subDistrictPCode: item.SubDistrictPCode);
            var neighCode = OchaCommandNormalizer.ResolveNeighborhoodCode(null, item.NeighborhoodPCode);
            var commPCode = OchaCommandNormalizer.NormalizeCommunityPCode(item.CommunityPCode);
            var buildingNumber = (item.BuildingNumber ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(govCode) || string.IsNullOrEmpty(distCode)
                || string.IsNullOrEmpty(subDistCode) || string.IsNullOrEmpty(neighCode)
                || string.IsNullOrEmpty(commPCode) || buildingNumber.Length != 5)
                continue;

            var geometrySql = string.IsNullOrWhiteSpace(item.GeometryWkt)
                ? "NULL"
                : $"ST_GeomFromText({SqlString(item.GeometryWkt)}, 4326)";

            // Latitude/Longitude derived from WKT (matches Building.SetGeometry behavior).
            // For POINT WKT we can extract coords; for any other shape we use centroid via
            // ST_Y(ST_Centroid(geom)) / ST_X(ST_Centroid(geom)).
            var latSql = string.IsNullOrWhiteSpace(item.GeometryWkt)
                ? "NULL"
                : $"ST_Y(ST_Centroid({geometrySql}))::numeric(10,7)";
            var lngSql = string.IsNullOrWhiteSpace(item.GeometryWkt)
                ? "NULL"
                : $"ST_X(ST_Centroid({geometrySql}))::numeric(10,7)";

            var notesSql = string.IsNullOrWhiteSpace(item.Notes) ? "NULL" : SqlString(item.Notes!);

            // The SELECT side of the INSERT does the admin-hierarchy lookup. If any
            // JOIN fails (admin row missing), zero rows are produced and the INSERT
            // becomes a no-op — safe.
            yield return $@"
INSERT INTO ""Buildings"" (
    ""Id"", ""BuildingId"",
    ""GovernorateCode"", ""DistrictCode"", ""SubDistrictCode"",
    ""CommunityCode"", ""NeighborhoodCode"", ""BuildingNumber"",
    ""GovernorateName"", ""DistrictName"", ""SubDistrictName"",
    ""CommunityName"", ""NeighborhoodName"",
    ""BuildingType"", ""Status"",
    ""NumberOfPropertyUnits"", ""NumberOfApartments"", ""NumberOfShops"",
    ""BuildingGeometry"", ""Latitude"", ""Longitude"",
    ""Notes"", ""IsAssigned"", ""IsLocked"", ""IsDeleted"",
    ""CreatedAtUtc"", ""CreatedBy""
)
SELECT
    gen_random_uuid(),
    {SqlString(govCode)} || {SqlString(distCode)} || {SqlString(subDistCode)} || c.""Code"" || {SqlString(neighCode)} || {SqlString(buildingNumber)},
    {SqlString(govCode)}, {SqlString(distCode)}, {SqlString(subDistCode)},
    c.""Code"", {SqlString(neighCode)}, {SqlString(buildingNumber)},
    g.""NameArabic"", d.""NameArabic"", s.""NameArabic"",
    c.""NameArabic"", n.""NameArabic"",
    {buildingType}, {status},
    0, 0, 0,
    {geometrySql}, {latSql}, {lngSql},
    {notesSql}, false, false, false,
    NOW() AT TIME ZONE 'UTC', '{systemUser}'
FROM ""Communities"" c
JOIN ""Governorates"" g ON g.""Code"" = {SqlString(govCode)}
JOIN ""Districts""    d ON d.""GovernorateCode"" = {SqlString(govCode)} AND d.""Code"" = {SqlString(distCode)}
JOIN ""SubDistricts"" s ON s.""GovernorateCode"" = {SqlString(govCode)} AND s.""DistrictCode"" = {SqlString(distCode)} AND s.""Code"" = {SqlString(subDistCode)}
JOIN ""Neighborhoods"" n ON n.""GovernorateCode"" = {SqlString(govCode)}
                        AND n.""DistrictCode""    = {SqlString(distCode)}
                        AND n.""SubDistrictCode"" = {SqlString(subDistCode)}
                        AND n.""CommunityCode""   = c.""Code""
                        AND n.""NeighborhoodCode"" = {SqlString(neighCode)}
WHERE c.""ExternalPCode""  = {SqlString(commPCode!)}
  AND c.""GovernorateCode"" = {SqlString(govCode)}
  AND c.""DistrictCode""    = {SqlString(distCode)}
  AND c.""SubDistrictCode"" = {SqlString(subDistCode)}
ON CONFLICT (""BuildingId"") DO NOTHING;";
        }
    }

    private static string SqlString(string s) => "'" + s.Replace("'", "''") + "'";
}

/// <summary>JSON shape of the embedded sample-buildings dataset.</summary>
public sealed class BuildingsDataset
{
    public int Version { get; set; }
    public string? Source { get; set; }
    public string? Crs { get; set; }
    public DateTime? GeneratedAtUtc { get; set; }
    public List<BuildingItem> Items { get; set; } = new();
}

public sealed class BuildingItem
{
    public string? BuildingNumber { get; set; }
    public string? GovernoratePCode { get; set; }
    public string? DistrictPCode { get; set; }
    public string? SubDistrictPCode { get; set; }
    public string? CommunityPCode { get; set; }
    public string? NeighborhoodPCode { get; set; }
    public string? GeometryWkt { get; set; }
    public string? Notes { get; set; }
}
