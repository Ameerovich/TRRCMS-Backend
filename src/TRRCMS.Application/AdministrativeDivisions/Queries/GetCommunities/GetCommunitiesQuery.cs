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

    /// <summary>OCHA governorate P-Code, e.g. "SY02" (optional, takes precedence).</summary>
    public string? GovernoratePCode { get; init; }

    /// <summary>OCHA district P-Code, e.g. "SY0200" (optional, populates Gov+Dist).</summary>
    public string? DistrictPCode { get; init; }

    /// <summary>OCHA sub-district P-Code, e.g. "SY020000" (optional, populates Gov+Dist+SubDist).</summary>
    public string? SubDistrictPCode { get; init; }
}
