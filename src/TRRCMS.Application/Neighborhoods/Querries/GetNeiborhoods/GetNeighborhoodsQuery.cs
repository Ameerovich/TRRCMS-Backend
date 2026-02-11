using MediatR;
using TRRCMS.Application.Neighborhoods.Dtos;

namespace TRRCMS.Application.Neighborhoods.Queries.GetNeighborhoods;

/// <summary>
/// Query to get all neighborhoods, optionally filtered by parent hierarchy.
/// Used for populating dropdown selectors and loading map boundaries.
/// </summary>
public class GetNeighborhoodsQuery : IRequest<List<NeighborhoodDto>>
{
    /// <summary>
    /// Filter by governorate code (optional)
    /// </summary>
    public string? GovernorateCode { get; set; }

    /// <summary>
    /// Filter by district code (optional)
    /// </summary>
    public string? DistrictCode { get; set; }

    /// <summary>
    /// Filter by sub-district code (optional)
    /// </summary>
    public string? SubDistrictCode { get; set; }

    /// <summary>
    /// Filter by community code (optional)
    /// </summary>
    public string? CommunityCode { get; set; }
}
