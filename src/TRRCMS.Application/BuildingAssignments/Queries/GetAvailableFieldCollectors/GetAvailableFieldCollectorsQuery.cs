using MediatR;
using TRRCMS.Application.BuildingAssignments.Dtos;

namespace TRRCMS.Application.BuildingAssignments.Queries.GetAvailableFieldCollectors;

/// <summary>
/// Query to get available field collectors for building assignment
/// UC-012: Select field collector for assignment
/// </summary>
public record GetAvailableFieldCollectorsQuery : IRequest<List<AvailableFieldCollectorDto>>
{
    /// <summary>
    /// Filter by availability status
    /// - null: All field collectors
    /// - true: Only available collectors
    /// - false: Only unavailable collectors
    /// </summary>
    public bool? IsAvailable { get; init; }
    
    /// <summary>
    /// Filter by team name
    /// </summary>
    public string? TeamName { get; init; }
    
    /// <summary>
    /// Search by name or device ID
    /// </summary>
    public string? SearchTerm { get; init; }
    
    /// <summary>
    /// Only include collectors with assigned tablets
    /// </summary>
    public bool? HasAssignedTablet { get; init; }
    
    /// <summary>
    /// Sort by workload (active assignments count)
    /// </summary>
    public bool SortByWorkloadAscending { get; init; } = true;
}
