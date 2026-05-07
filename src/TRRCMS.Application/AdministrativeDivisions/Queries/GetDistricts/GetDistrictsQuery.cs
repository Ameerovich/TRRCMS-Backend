using MediatR;
using TRRCMS.Application.AdministrativeDivisions.Dtos;

namespace TRRCMS.Application.AdministrativeDivisions.Queries.GetDistricts;

/// <summary>
/// Query to get districts, optionally filtered by governorate
/// </summary>
public record GetDistrictsQuery : IRequest<List<DistrictDto>>
{
    /// <summary>
    /// Filter by governorate code (optional, raw 2-digit numeric)
    /// </summary>
    public string? GovernorateCode { get; init; }

    /// <summary>
    /// Filter by governorate OCHA P-Code (optional, e.g. "SY02").
    /// Takes precedence over GovernorateCode when both are provided.
    /// </summary>
    public string? GovernoratePCode { get; init; }
}
