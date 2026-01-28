using MediatR;
using TRRCMS.Application.PropertyUnits.Dtos;

namespace TRRCMS.Application.PropertyUnits.Queries.GetPropertyUnitsByBuilding;

/// <summary>
/// Query to get all property units for a specific building
/// </summary>
public record GetPropertyUnitsByBuildingQuery(Guid BuildingId) : IRequest<List<PropertyUnitDto>>;
