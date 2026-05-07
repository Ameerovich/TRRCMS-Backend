using MediatR;
using TRRCMS.Application.Common.Interfaces;

namespace TRRCMS.Application.Buildings.Commands.ImportBuildings;

/// <summary>
/// Bulk-import command for sample-buildings reference data. The payload mirrors the
/// embedded <c>Data/buildings_sample_v1.json</c> produced by
/// <c>tools/SeedBuildingsFromShapefile</c>.
///
/// Behavior is identical to the SeedSampleBuildingsFromGIS migration: UPSERT by
/// 17-digit BuildingId, skip rows whose admin hierarchy isn't seeded yet.
/// Idempotent.
/// </summary>
public record ImportBuildingsCommand : IRequest<BuildingsImportSummary>
{
    /// <summary>
    /// JSON payload conforming to the BuildingsDataset shape:
    /// <code>
    /// {
    ///   "version": 1,
    ///   "source": "buidings.shp",
    ///   "crs": "EPSG:4326",
    ///   "items": [
    ///     {
    ///       "buildingNumber": "00051",
    ///       "governoratePCode": "SY02",
    ///       "districtPCode": "SY0200",
    ///       "subDistrictPCode": "SY020000",
    ///       "communityPCode": "C1007",
    ///       "neighborhoodPCode": "N0225",
    ///       "geometryWkt": "POINT(37.14983584 36.18613900)",
    ///       "notes": null
    ///     }
    ///   ]
    /// }
    /// </code>
    /// </summary>
    public string JsonPayload { get; init; } = string.Empty;
}
