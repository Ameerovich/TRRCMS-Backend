namespace TRRCMS.Application.Common.Interfaces;

/// <summary>
/// Bulk-import service for sample buildings sourced from the client's GIS shapefile
/// via <c>tools/SeedBuildingsFromShapefile</c>. Used both at startup (the
/// SeedSampleBuildingsFromGIS migration applies the embedded payload) and via the
/// admin endpoint (frontend uploads a regenerated payload without a redeploy).
/// </summary>
public interface IBuildingsImportService
{
    /// <summary>
    /// Apply a JSON dataset (same shape as the embedded
    /// <c>Data/buildings_sample_v1.json</c>) against the live database.
    /// UPSERT-style: inserts new rows, updates geometry in place if the BuildingId
    /// already exists, skips rows whose admin hierarchy isn't seeded yet.
    /// Idempotent — re-running with the same payload is a no-op.
    /// </summary>
    /// <param name="json">JSON payload conforming to the BuildingsDataset shape.</param>
    /// <param name="userId">Audit user; usually the authenticated admin's UserId.</param>
    Task<BuildingsImportSummary> ApplyFromJsonAsync(
        string json,
        Guid userId,
        CancellationToken cancellationToken = default);
}

/// <summary>Result counts for a buildings bulk-import.</summary>
public sealed class BuildingsImportSummary
{
    public int Inserted { get; set; }
    public int Updated { get; set; }
    public int Unchanged { get; set; }
    public int Skipped { get; set; }
    public int TotalProcessed => Inserted + Updated + Unchanged + Skipped;
}
