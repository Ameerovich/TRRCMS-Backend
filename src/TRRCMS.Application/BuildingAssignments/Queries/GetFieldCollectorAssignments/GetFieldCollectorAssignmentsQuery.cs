using MediatR;
using TRRCMS.Application.BuildingAssignments.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.BuildingAssignments.Queries.GetFieldCollectorAssignments;

/// <summary>
/// Query to get assignments for a specific field collector
/// UC-012: View collector's current tasks
/// </summary>
public record GetFieldCollectorAssignmentsQuery : IRequest<FieldCollectorTasksDto>
{
    /// <summary>
    /// Field collector ID
    /// </summary>
    public Guid FieldCollectorId { get; init; }
    
    /// <summary>
    /// Filter by active status
    /// </summary>
    public bool? IsActive { get; init; }
    
    /// <summary>
    /// Filter by transfer status
    /// </summary>
    public TransferStatus? TransferStatus { get; init; }
    
    /// <summary>
    /// Include only revisit assignments
    /// </summary>
    public bool? IsRevisit { get; init; }
}
