using MediatR;
using TRRCMS.Application.AdministrativeDivisions.Dtos;

namespace TRRCMS.Application.AdministrativeDivisions.Queries.GetSubDistricts;

/// <summary>
/// Query to get sub-districts, optionally filtered by governorate and district
/// </summary>
public record GetSubDistrictsQuery : IRequest<List<SubDistrictDto>>
{
    /// <summary>
    /// Filter by governorate code (optional)
    /// </summary>
    public string? GovernorateCode { get; init; }

    /// <summary>
    /// Filter by district code (optional)
    /// </summary>
    public string? DistrictCode { get; init; }

    /// <summary>OCHA governorate P-Code, e.g. "SY02" (optional, takes precedence).</summary>
    public string? GovernoratePCode { get; init; }

    /// <summary>OCHA district P-Code, e.g. "SY0200" (optional, populates Gov+Dist).</summary>
    public string? DistrictPCode { get; init; }
}
