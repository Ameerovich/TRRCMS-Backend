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

    // ──────────── OCHA pCode filter aliases ────────────
    /// <summary>OCHA governorate P-Code, e.g. "SY02" (optional, takes precedence).</summary>
    public string? GovernoratePCode { get; set; }
    /// <summary>OCHA district P-Code, e.g. "SY0200" (optional, populates Gov+Dist).</summary>
    public string? DistrictPCode { get; set; }
    /// <summary>OCHA sub-district P-Code, e.g. "SY020000" (optional, populates Gov+Dist+SubDist).</summary>
    public string? SubDistrictPCode { get; set; }
    /// <summary>OCHA community P-Code, e.g. "C1007" (optional, resolved via Community.ExternalPCode).</summary>
    public string? CommunityPCode { get; set; }
}
