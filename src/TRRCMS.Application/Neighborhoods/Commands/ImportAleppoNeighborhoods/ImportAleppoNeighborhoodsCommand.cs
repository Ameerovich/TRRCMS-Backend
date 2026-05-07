using MediatR;
using TRRCMS.Application.Common.Interfaces;

namespace TRRCMS.Application.Neighborhoods.Commands.ImportAleppoNeighborhoods;

/// <summary>
/// Bulk-import command for Aleppo neighborhood reference data.
/// The payload mirrors the embedded <c>Data/aleppo_neighborhoods_v1.json</c>
/// produced from the GIS team's city_neighbourhoods.shp.
///
/// Behavior is identical to the SeedAleppoNeighborhoodsFromGIS migration:
/// UPSERTs by FullCode, soft-deletes legacy placeholders not in the payload.
/// Idempotent.
/// </summary>
public record ImportAleppoNeighborhoodsCommand : IRequest<NeighborhoodImportSummary>
{
    /// <summary>
    /// JSON payload conforming to the AleppoNeighborhoodsDataset shape:
    /// <code>
    /// {
    ///   "version": 1,
    ///   "governorateCode": "02",
    ///   "districtCode": "00",
    ///   "subDistrictCode": "00",
    ///   "communityCode": "001",
    ///   "items": [
    ///     {
    ///       "pCode": "N0160",
    ///       "neighborhoodCode": "160",
    ///       "nameArabic": "العزيزية",
    ///       "nameEnglish": "A'aziziyeh",
    ///       "centerLatitude": 36.208007,
    ///       "centerLongitude": 37.151975,
    ///       "boundaryWkt": "POLYGON((37.13 36.20, ...))",
    ///       "areaSquareKm": 0.10
    ///     }
    ///   ]
    /// }
    /// </code>
    /// </summary>
    public string JsonPayload { get; init; } = string.Empty;
}
