using MediatR;
using TRRCMS.Application.PropertyUnits.Dtos;

namespace TRRCMS.Application.PropertyUnits.Queries.GetAllPropertyUnits;

/// <summary>
/// Query to get all property units with optional filtering and grouping
/// Returns property units grouped by building by default
/// All filter parameters are optional (AND-combined if multiple provided)
/// </summary>
public record GetAllPropertyUnitsQuery : IRequest<GroupedPropertyUnitsResponseDto>
{
    /// <summary>
    /// Filter by building ID (optional)
    /// If provided, only returns units from this building
    /// </summary>
    public Guid? BuildingId { get; init; }

    /// <summary>
    /// Filter by property unit type (optional)
    /// Values: Apartment=1, Shop=2, Office=3, Warehouse=4, Other=5
    /// </summary>
    public int? UnitType { get; init; }

    /// <summary>
    /// Filter by property unit status (optional)
    /// Values: Occupied=1, Vacant=2, Damaged=3, UnderRenovation=4, Uninhabitable=5, Locked=6, Unknown=99
    /// </summary>
    public int? Status { get; init; }

    /// <summary>
    /// Group results by building (default: true)
    /// When true, results are returned grouped by building with hierarchy
    /// When false, results are returned as flat list with all statistics
    /// </summary>
    public bool GroupByBuilding { get; init; } = true;
}