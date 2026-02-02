using MediatR;
using TRRCMS.Application.BuildingAssignments.Dtos;

namespace TRRCMS.Application.BuildingAssignments.Queries.GetPropertyUnitsForRevisit;

/// <summary>
/// Query to get property units for a building for revisit selection
/// UC-012: S04-S05 - Review property units and select for revisit
/// </summary>
public record GetPropertyUnitsForRevisitQuery : IRequest<List<PropertyUnitForRevisitDto>>
{
    /// <summary>
    /// Building ID to get property units for
    /// </summary>
    public Guid BuildingId { get; init; }
    
    /// <summary>
    /// Only include units that have completed surveys
    /// </summary>
    public bool OnlyWithCompletedSurveys { get; init; } = false;
}
