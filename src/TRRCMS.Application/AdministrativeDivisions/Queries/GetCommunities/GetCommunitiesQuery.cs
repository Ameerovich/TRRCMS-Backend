using MediatR;
using TRRCMS.Application.AdministrativeDivisions.Dtos;

namespace TRRCMS.Application.AdministrativeDivisions.Queries.GetCommunities;

/// <summary>
/// Query to get communities, optionally filtered by governorate, district, and sub-district
/// </summary>
public record GetCommunitiesQuery : IRequest<List<CommunityDto>>
{
    /// <summary>
    /// Filter by governorate code (optional)
    /// </summary>
    public string? GovernorateCode { get; init; }

    /// <summary>
    /// Filter by district code (optional)
    /// </summary>
    public string? DistrictCode { get; init; }

    /// <summary>
    /// Filter by sub-district code (optional)
    /// </summary>
    public string? SubDistrictCode { get; init; }
}
