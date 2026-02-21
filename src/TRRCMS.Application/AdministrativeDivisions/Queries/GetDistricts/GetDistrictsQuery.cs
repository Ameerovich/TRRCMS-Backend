using MediatR;
using TRRCMS.Application.AdministrativeDivisions.Dtos;

namespace TRRCMS.Application.AdministrativeDivisions.Queries.GetDistricts;

/// <summary>
/// Query to get districts, optionally filtered by governorate
/// </summary>
public record GetDistrictsQuery : IRequest<List<DistrictDto>>
{
    /// <summary>
    /// Filter by governorate code (optional)
    /// </summary>
    public string? GovernorateCode { get; init; }
}
