using MediatR;
using TRRCMS.Application.AdministrativeDivisions.Dtos;

namespace TRRCMS.Application.AdministrativeDivisions.Queries.GetGovernorates;

/// <summary>
/// Query to get all governorates
/// </summary>
public record GetGovernoratesQuery : IRequest<List<GovernorateDto>>;
