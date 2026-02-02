using TRRCMS.Domain.Common;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Domain.Entities;

/// <summary>
/// Building Assignment entity - tracks assignment of buildings to field collectors
/// UC-012: Assign Buildings to Field Collectors
/// </summary>
public class BuildingAssignment : BaseAuditableEntity
{
    // ==================== RELATIONSHIPS ====================

    /// <summary>
    /// Foreign key to Building being assigned
    /// </summary>
    public Guid BuildingId { get; private set; }

    /// <summary>
    /// Foreign key to field collector (User) assigned to survey this building
    /// </summary>
    public Guid FieldCollectorId { get; private set; }

    /// <summary>
    /// Foreign key to supervisor who made the assignment (optional)
    /// </summary>
    public Guid? AssignedByUserId { get; private set; }

    // ==================== ASSIGNMENT DETAILS ====================

    /// <summary>
    /// Date when building was assigned (تاريخ التعيين)
    /// </summary>
    public DateTime AssignedDate { get; private set; }

    /// <summary>
    /// Target completion date (optional) (تاريخ الاستحقاق)
    /// </summary>
    public DateTime? TargetCompletionDate { get; private set; }

    /// <summary>
    /// Actual completion date (when field work completed)
    /// </summary>
    public DateTime? ActualCompletionDate { get; private set; }

    /// <summary>
    /// Transfer status (Pending, InProgress, Transferred, Failed, etc.)
    /// </summary>
    public TransferStatus TransferStatus { get; private set; }

    /// <summary>
    /// Date when data was transferred to tablet
    /// </summary>
    public DateTime? TransferredToTabletDate { get; private set; }

    /// <summary>
    /// Date when data was synchronized back from tablet
    /// </summary>
    public DateTime? SynchronizedFromTabletDate { get; private set; }

    // ==================== REVISIT TRACKING ====================

    /// <summary>
    /// Array of property unit IDs selected for revisit
    /// Stored as JSON array: ["uuid1", "uuid2", ...]
    /// </summary>
    public string? UnitsForRevisit { get; private set; }

    /// <summary>
    /// Reason for revisit (if applicable)
    /// </summary>
    public string? RevisitReason { get; private set; }

    /// <summary>
    /// Indicates if this is a revisit assignment
    /// </summary>
    public bool IsRevisit { get; private set; }

    /// <summary>
    /// Reference to original assignment (if this is a revisit)
    /// NULLABLE: May be null if this is the first assignment or original was deleted
    /// </summary>
    public Guid? OriginalAssignmentId { get; private set; }

    // ==================== ASSIGNMENT STATUS ====================

    /// <summary>
    /// Assignment priority (Normal, High, Urgent)
    /// </summary>
    public string Priority { get; private set; }

    /// <summary>
    /// Assignment notes/instructions from supervisor
    /// </summary>
    public string? AssignmentNotes { get; private set; }

    /// <summary>
    /// Indicates if assignment is currently active
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Number of property units in this building (for tracking progress)
    /// </summary>
    public int TotalPropertyUnits { get; private set; }

    /// <summary>
    /// Number of property units surveyed so far
    /// </summary>
    public int CompletedPropertyUnits { get; private set; }

    // ==================== TRANSFER ERROR TRACKING ====================

    /// <summary>
    /// Error message if transfer failed
    /// </summary>
    public string? TransferErrorMessage { get; private set; }

    /// <summary>
    /// Number of transfer retry attempts
    /// </summary>
    public int TransferRetryCount { get; private set; }

    /// <summary>
    /// Last transfer attempt date
    /// </summary>
    public DateTime? LastTransferAttemptDate { get; private set; }

    // ==================== NAVIGATION PROPERTIES ====================

    /// <summary>
    /// Building being assigned
    /// </summary>
    public virtual Building Building { get; private set; } = null!;

    /// <summary>
    /// Original assignment (if this is a revisit)
    /// </summary>
    public virtual BuildingAssignment? OriginalAssignment { get; private set; }

    // ==================== CONSTRUCTORS ====================

    /// <summary>
    /// EF Core constructor
    /// </summary>
    private BuildingAssignment() : base()
    {
        Priority = "Normal";
        IsActive = true;
    }

    /// <summary>
    /// Create new building assignment
    /// </summary>
    public static BuildingAssignment Create(
        Guid buildingId,
        Guid fieldCollectorId,
        Guid? assignedByUserId,
        int totalPropertyUnits,
        DateTime? targetCompletionDate,
        string? assignmentNotes,
        Guid createdByUserId)
    {
        var assignment = new BuildingAssignment
        {
            BuildingId = buildingId,
            FieldCollectorId = fieldCollectorId,
            AssignedByUserId = assignedByUserId,
            AssignedDate = DateTime.UtcNow,
            TargetCompletionDate = targetCompletionDate,
            AssignmentNotes = assignmentNotes,
            TotalPropertyUnits = totalPropertyUnits,
            CompletedPropertyUnits = 0,
            TransferStatus = TransferStatus.Pending,
            TransferRetryCount = 0,
            Priority = "Normal",
            IsActive = true,
            IsRevisit = false
        };

        assignment.MarkAsCreated(createdByUserId);

        return assignment;
    }

    /// <summary>
    /// Create revisit assignment for specific units
    /// </summary>
    /// <param name="buildingId">Building to revisit</param>
    /// <param name="fieldCollectorId">Field collector to assign</param>
    /// <param name="originalAssignmentId">Original assignment ID (CAN BE NULL if no prior assignment exists)</param>
    /// <param name="unitsForRevisit">JSON array of property unit IDs</param>
    /// <param name="revisitReason">Reason for revisit</param>
    /// <param name="totalPropertyUnits">Number of units to revisit</param>
    /// <param name="createdByUserId">User creating the assignment</param>
    public static BuildingAssignment CreateRevisit(
        Guid buildingId,
        Guid fieldCollectorId,
        Guid? originalAssignmentId,  // FIX: Changed from Guid to Guid? - can be null!
        string unitsForRevisit,
        string revisitReason,
        int totalPropertyUnits,
        Guid createdByUserId)
    {
        var assignment = new BuildingAssignment
        {
            BuildingId = buildingId,
            FieldCollectorId = fieldCollectorId,
            AssignedDate = DateTime.UtcNow,
            UnitsForRevisit = unitsForRevisit,
            RevisitReason = revisitReason,
            IsRevisit = true,
            OriginalAssignmentId = originalAssignmentId,  // Can be null - FK allows null
            TransferStatus = TransferStatus.Pending,
            Priority = "High", // Revisits typically have higher priority
            IsActive = true,
            TotalPropertyUnits = totalPropertyUnits,
            CompletedPropertyUnits = 0
        };

        assignment.MarkAsCreated(createdByUserId);

        return assignment;
    }

    // ==================== DOMAIN METHODS ====================

    /// <summary>
    /// Mark assignment as transferred to tablet
    /// </summary>
    public void MarkAsTransferred(Guid modifiedByUserId)
    {
        TransferStatus = TransferStatus.Transferred;
        TransferredToTabletDate = DateTime.UtcNow;
        TransferErrorMessage = null;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Mark transfer as failed with error message
    /// </summary>
    public void MarkTransferFailed(string errorMessage, Guid modifiedByUserId)
    {
        TransferStatus = TransferStatus.Failed;
        TransferErrorMessage = errorMessage;
        TransferRetryCount++;
        LastTransferAttemptDate = DateTime.UtcNow;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Mark transfer as in progress
    /// </summary>
    public void MarkTransferInProgress(Guid modifiedByUserId)
    {
        TransferStatus = TransferStatus.InProgress;
        LastTransferAttemptDate = DateTime.UtcNow;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Mark data as synchronized back from tablet
    /// </summary>
    public void MarkAsSynchronized(Guid modifiedByUserId)
    {
        TransferStatus = TransferStatus.Synchronized;
        SynchronizedFromTabletDate = DateTime.UtcNow;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Update progress (number of units completed)
    /// </summary>
    public void UpdateProgress(int completedUnits, Guid modifiedByUserId)
    {
        CompletedPropertyUnits = completedUnits;

        // Check if assignment is completed
        if (CompletedPropertyUnits >= TotalPropertyUnits && TotalPropertyUnits > 0)
        {
            ActualCompletionDate = DateTime.UtcNow;
        }

        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Mark assignment as completed
    /// </summary>
    public void MarkAsCompleted(Guid modifiedByUserId)
    {
        ActualCompletionDate = DateTime.UtcNow;
        IsActive = false;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Cancel assignment
    /// </summary>
    public void Cancel(string cancellationReason, Guid modifiedByUserId)
    {
        IsActive = false;
        TransferStatus = TransferStatus.Cancelled;
        AssignmentNotes = string.IsNullOrWhiteSpace(AssignmentNotes)
            ? $"[Cancelled]: {cancellationReason}"
            : $"{AssignmentNotes}\n[Cancelled]: {cancellationReason}";
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Reassign to different field collector
    /// </summary>
    public void Reassign(Guid newFieldCollectorId, string reassignmentReason, Guid modifiedByUserId)
    {
        FieldCollectorId = newFieldCollectorId;
        TransferStatus = TransferStatus.Pending; // Need to transfer to new tablet
        AssignmentNotes = string.IsNullOrWhiteSpace(AssignmentNotes)
            ? $"[Reassigned]: {reassignmentReason}"
            : $"{AssignmentNotes}\n[Reassigned]: {reassignmentReason}";
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Set priority level
    /// </summary>
    public void SetPriority(string priority, Guid modifiedByUserId)
    {
        Priority = priority;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Check if assignment is overdue
    /// </summary>
    public bool IsOverdue()
    {
        return TargetCompletionDate.HasValue
            && !ActualCompletionDate.HasValue
            && DateTime.UtcNow > TargetCompletionDate.Value;
    }

    /// <summary>
    /// Calculate completion percentage
    /// </summary>
    public decimal GetCompletionPercentage()
    {
        if (TotalPropertyUnits == 0)
            return 0;

        return (decimal)CompletedPropertyUnits / TotalPropertyUnits * 100;
    }
}
