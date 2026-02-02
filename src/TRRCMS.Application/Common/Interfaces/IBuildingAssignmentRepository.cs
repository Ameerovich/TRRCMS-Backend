using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Common.Interfaces;

/// <summary>
/// Repository interface for BuildingAssignment operations
/// Supports UC-012: Assign Buildings to Field Collectors
/// </summary>
public interface IBuildingAssignmentRepository
{
    // ==================== BASIC CRUD ====================
    
    Task<BuildingAssignment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<BuildingAssignment?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<BuildingAssignment> AddAsync(BuildingAssignment assignment, CancellationToken cancellationToken = default);
    
    Task AddRangeAsync(IEnumerable<BuildingAssignment> assignments, CancellationToken cancellationToken = default);
    
    Task UpdateAsync(BuildingAssignment assignment, CancellationToken cancellationToken = default);
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    // ==================== FIELD COLLECTOR QUERIES ====================
    
    /// <summary>
    /// Get all active assignments for a specific field collector
    /// </summary>
    Task<List<BuildingAssignment>> GetByFieldCollectorAsync(
        Guid fieldCollectorId, 
        bool activeOnly = true,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get assignments by field collector with building details
    /// </summary>
    Task<List<BuildingAssignment>> GetByFieldCollectorWithBuildingsAsync(
        Guid fieldCollectorId,
        bool activeOnly = true,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get assignment count for a field collector
    /// </summary>
    Task<int> GetAssignmentCountAsync(
        Guid fieldCollectorId, 
        bool activeOnly = true,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get assignments by transfer status for a field collector
    /// </summary>
    Task<List<BuildingAssignment>> GetByFieldCollectorAndStatusAsync(
        Guid fieldCollectorId,
        TransferStatus status,
        CancellationToken cancellationToken = default);

    // ==================== BUILDING QUERIES ====================
    
    /// <summary>
    /// Get all assignments for a specific building
    /// </summary>
    Task<List<BuildingAssignment>> GetByBuildingIdAsync(
        Guid buildingId,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get active assignment for a building (if any)
    /// </summary>
    Task<BuildingAssignment?> GetActiveAssignmentForBuildingAsync(
        Guid buildingId,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Check if building has any active assignment
    /// </summary>
    Task<bool> HasActiveAssignmentAsync(
        Guid buildingId,
        CancellationToken cancellationToken = default);

    // ==================== STATUS QUERIES ====================
    
    /// <summary>
    /// Get assignments by transfer status
    /// </summary>
    Task<List<BuildingAssignment>> GetByTransferStatusAsync(
        TransferStatus status,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get pending assignments (not yet transferred to tablet)
    /// </summary>
    Task<List<BuildingAssignment>> GetPendingAssignmentsAsync(
        Guid? fieldCollectorId = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get overdue assignments
    /// </summary>
    Task<List<BuildingAssignment>> GetOverdueAssignmentsAsync(
        CancellationToken cancellationToken = default);

    // ==================== SEARCH WITH FILTERS ====================
    
    /// <summary>
    /// Search assignments with filters and pagination
    /// </summary>
    Task<(List<BuildingAssignment> Assignments, int TotalCount)> SearchAssignmentsAsync(
        Guid? fieldCollectorId = null,
        Guid? buildingId = null,
        TransferStatus? transferStatus = null,
        bool? isActive = null,
        bool? isRevisit = null,
        DateTime? assignedFromDate = null,
        DateTime? assignedToDate = null,
        int page = 1,
        int pageSize = 20,
        string? sortBy = null,
        bool sortDescending = false,
        CancellationToken cancellationToken = default);

    // ==================== REVISIT QUERIES ====================
    
    /// <summary>
    /// Get revisit assignments for a building
    /// </summary>
    Task<List<BuildingAssignment>> GetRevisitAssignmentsAsync(
        Guid buildingId,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get original assignment for a revisit
    /// </summary>
    Task<BuildingAssignment?> GetOriginalAssignmentAsync(
        Guid revisitAssignmentId,
        CancellationToken cancellationToken = default);

    // ==================== STATISTICS ====================
    
    /// <summary>
    /// Get assignment statistics for a field collector
    /// </summary>
    Task<FieldCollectorAssignmentStats> GetFieldCollectorStatsAsync(
        Guid fieldCollectorId,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get overall assignment statistics
    /// </summary>
    Task<AssignmentOverviewStats> GetOverviewStatsAsync(
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Statistics for a field collector's assignments
/// </summary>
public class FieldCollectorAssignmentStats
{
    public int TotalAssignments { get; set; }
    public int ActiveAssignments { get; set; }
    public int PendingTransfer { get; set; }
    public int Transferred { get; set; }
    public int Completed { get; set; }
    public int OverdueAssignments { get; set; }
    public int TotalPropertyUnits { get; set; }
    public int CompletedPropertyUnits { get; set; }
    public decimal CompletionPercentage => TotalPropertyUnits > 0 
        ? (decimal)CompletedPropertyUnits / TotalPropertyUnits * 100 
        : 0;
}

/// <summary>
/// Overall assignment statistics
/// </summary>
public class AssignmentOverviewStats
{
    public int TotalAssignments { get; set; }
    public int TotalActiveAssignments { get; set; }
    public int TotalPendingTransfer { get; set; }
    public int TotalTransferred { get; set; }
    public int TotalCompleted { get; set; }
    public int TotalOverdue { get; set; }
    public int TotalFieldCollectorsWithAssignments { get; set; }
}
