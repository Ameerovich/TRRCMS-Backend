using MediatR;
using TRRCMS.Application.BuildingAssignments.Dtos;

namespace TRRCMS.Application.BuildingAssignments.Commands.AssignBuildings;

/// <summary>
/// Command to assign multiple buildings to a field collector
/// UC-012: Assign Buildings to Field Collectors (S06-S07)
/// </summary>
public record AssignBuildingsCommand : IRequest<AssignBuildingsResult>
{
    /// <summary>
    /// Field collector to assign buildings to
    /// </summary>
    public Guid FieldCollectorId { get; init; }
    
    /// <summary>
    /// List of buildings to assign
    /// </summary>
    public List<BuildingAssignmentItem> Buildings { get; init; } = new();
    
    /// <summary>
    /// Target completion date for all assignments (optional)
    /// </summary>
    public DateTime? TargetCompletionDate { get; init; }
    
    /// <summary>
    /// Assignment priority (Normal, High, Urgent)
    /// </summary>
    public string Priority { get; init; } = "Normal";
    
    /// <summary>
    /// Notes/instructions for the field collector
    /// </summary>
    public string? AssignmentNotes { get; init; }
}

/// <summary>
/// Individual building assignment item
/// </summary>
public class BuildingAssignmentItem
{
    /// <summary>
    /// Building ID to assign
    /// </summary>
    public Guid BuildingId { get; set; }
    
    /// <summary>
    /// Property unit IDs for revisit (optional)
    /// If provided, this becomes a revisit assignment
    /// </summary>
    public List<Guid>? PropertyUnitIdsForRevisit { get; set; }
    
    /// <summary>
    /// Reason for revisit (required if PropertyUnitIdsForRevisit is provided)
    /// </summary>
    public string? RevisitReason { get; set; }
    
    /// <summary>
    /// Individual notes for this building (optional)
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// Result of the assign buildings command
/// </summary>
public class AssignBuildingsResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Number of buildings successfully assigned
    /// </summary>
    public int AssignedCount { get; set; }
    
    /// <summary>
    /// Number of buildings that failed to assign
    /// </summary>
    public int FailedCount { get; set; }
    
    /// <summary>
    /// Created assignment IDs
    /// </summary>
    public List<Guid> CreatedAssignmentIds { get; set; } = new();
    
    /// <summary>
    /// Details of created assignments
    /// </summary>
    public List<BuildingAssignmentSummaryDto> Assignments { get; set; } = new();
    
    /// <summary>
    /// Errors for failed assignments
    /// </summary>
    public List<AssignmentError> Errors { get; set; } = new();
}

/// <summary>
/// Error details for failed assignment
/// </summary>
public class AssignmentError
{
    public Guid BuildingId { get; set; }
    public string BuildingCode { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}
