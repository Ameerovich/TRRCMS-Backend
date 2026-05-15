using MediatR;
using TRRCMS.Application.Reporting.Dtos;

namespace TRRCMS.Application.Reporting.Queries.BuildingInventory;

public sealed record GetBuildingInventoryReportQuery(
    string? NeighborhoodCode = null
) : IRequest<BuildingInventoryReportDto>;
