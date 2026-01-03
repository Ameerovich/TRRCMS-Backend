using MediatR;
using TRRCMS.Application.Buildings.Dtos;

namespace TRRCMS.Application.Buildings.Queries.GetAllBuildings;

public record GetAllBuildingsQuery : IRequest<List<BuildingDto>>;