using MediatR;
using TRRCMS.Application.Buildings.Dtos;

namespace TRRCMS.Application.Buildings.Queries.GetBuildingDocumentsByBuilding;

/// <summary>
/// Query to get all building documents linked to a specific building.
/// </summary>
public record GetBuildingDocumentsByBuildingQuery(Guid BuildingId) : IRequest<List<BuildingDocumentDto>>;
