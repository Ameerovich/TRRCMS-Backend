namespace TRRCMS.Application.Common.Interfaces;

/// <summary>
/// Bulk-import service for Aleppo neighborhood reference data sourced from
/// the GIS team's shapefile (city_neighbourhoods.shp). Used both at startup
/// (seed migration applies the embedded payload) and via the admin endpoint
/// (frontend uploads an updated payload without a redeploy).
/// </summary>
public interface IAleppoNeighborhoodsImportService
{
    /// <summary>
    /// Apply a JSON dataset (same shape as the embedded
    /// <c>Data/aleppo_neighborhoods_v1.json</c>) against the live database.
    /// UPSERT-style: inserts new rows, updates existing ones (matched by FullCode),
    /// soft-deletes legacy placeholders that aren't in the payload.
    /// Idempotent — re-running with the same payload is a no-op.
    /// </summary>
    /// <param name="json">JSON payload conforming to the AleppoNeighborhoodsDataset shape.</param>
    /// <param name="userId">Audit user; usually the authenticated admin's UserId.</param>
    /// <returns>Counts of inserted / updated / restored / soft-deleted rows.</returns>
    Task<NeighborhoodImportSummary> ApplyFromJsonAsync(
        string json,
        Guid userId,
        CancellationToken cancellationToken = default);
}

/// <summary>Result counts for an Aleppo-neighborhoods import.</summary>
public sealed class NeighborhoodImportSummary
{
    public int Inserted { get; set; }
    public int Updated { get; set; }
    public int Restored { get; set; }
    public int SoftDeletedPlaceholders { get; set; }
    public int TotalProcessed => Inserted + Updated + Restored;
}
