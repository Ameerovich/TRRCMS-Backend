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
}
