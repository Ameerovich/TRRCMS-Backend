using MediatR;
using TRRCMS.Application.Neighborhoods.Dtos;

namespace TRRCMS.Application.Neighborhoods.Queries.GetNeighborhoodByCode;

/// <summary>
/// Query to get a single neighborhood by its administrative hierarchy codes.
/// Used for "fly-to" navigation and boundary rendering.
/// </summary>
public class GetNeighborhoodByCodeQuery : IRequest<NeighborhoodDto?>
{
    /// <summary>
    /// Full 12-digit composite code (GGDDSSCCCCNNN).
    /// If provided, other code fields are ignored.
    /// </summary>
    public string? FullCode { get; set; }

    /// <summary>
    /// Governorate code — 2 digits
    /// </summary>
    public string? GovernorateCode { get; set; }

    /// <summary>
    /// District code — 2 digits
    /// </summary>
    public string? DistrictCode { get; set; }

    /// <summary>
    /// Sub-district code — 2 digits
    /// </summary>
    public string? SubDistrictCode { get; set; }

    /// <summary>
    /// Community code — 3 digits
    /// </summary>
    public string? CommunityCode { get; set; }

    /// <summary>
    /// Neighborhood code — 3 digits
    /// </summary>
    public string? NeighborhoodCode { get; set; }
}
