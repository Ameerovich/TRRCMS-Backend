using MediatR;
using TRRCMS.Application.PropertyUnits.Dtos;

namespace TRRCMS.Application.PropertyUnits.Queries.GetPropertyUnit;

/// <summary>
/// Query to get a single property unit by ID
/// </summary>
public record GetPropertyUnitQuery(Guid Id) : IRequest<PropertyUnitDto?>;