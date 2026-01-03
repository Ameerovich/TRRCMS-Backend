using MediatR;
using TRRCMS.Application.Buildings.Dtos;

namespace TRRCMS.Application.Buildings.Queries.GetBuilding;

public record GetBuildingQuery : IRequest<BuildingDto?>
{
    public Guid Id { get; init; }
}