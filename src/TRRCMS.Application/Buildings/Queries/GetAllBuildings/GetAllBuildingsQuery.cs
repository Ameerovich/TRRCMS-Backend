using MediatR;
using TRRCMS.Application.Buildings.Dtos;
using TRRCMS.Application.Common.Models;

namespace TRRCMS.Application.Buildings.Queries.GetAllBuildings;

/// <summary>
/// Query to get all buildings with pagination
/// </summary>
public class GetAllBuildingsQuery : PagedQuery, IRequest<PagedResult<BuildingDto>> { }
