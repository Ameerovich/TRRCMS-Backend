using TRRCMS.Domain.Common;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Domain.Entities;

/// <summary>
/// Conflict Resolution entity - tracks duplicate detection and resolution workflow
/// Referenced in UC-007 (Resolve Duplicate Properties) and UC-008 (Resolve Person Duplicates)
/// Supports FSD section FR-D-7: Conflict Resolution
/// </summary>
public class ConflictResolution : BaseAuditableEntity
{
    // ==================== CONFLICT IDENTIFICATION ====================

    /// <summary>
    /// Conflict resolution number for tracking
    /// Format: CNF-YYYY-NNNN
    /// </summary>
    public string ConflictNumber { get; private set; }

    /// <summary>
    /// Type of conflict (PersonDuplicate, PropertyDuplicate, ClaimConflict)
    /// </summary>
    public string ConflictType { get; private set; }

    // ==================== ENTITY REFERENCES ====================

    /// <summary>
    /// Type of entities in conflict (Person, PropertyUnit, Building)
    /// </summary>
    public string EntityType { get; private set; }

    /// <summary>
    /// ID of first entity in conflict
    /// </summary>
    public Guid FirstEntityId { get; private set; }

    /// <summary>
    /// ID of second entity in conflict
    /// </summary>
    public Guid SecondEntityId { get; private set; }

    /// <summary>
    /// Human-readable identifier for first entity
    /// </summary>
    public string? FirstEntityIdentifier { get; private set; }

    /// <summary>
    /// Human-readable identifier for second entity
    /// </summary>
    public string? SecondEntityIdentifier { get; private set; }

    /// <summary>
    /// Import package that triggered this conflict (if applicable)
    /// </summary>
    public Guid? ImportPackageId { get; private set; }

    // ==================== CONFLICT DETAILS ====================

    /// <summary>
    /// Similarity score (0-100%)
    /// Higher score means more similar/likely duplicate
    /// </summary>
    public decimal SimilarityScore { get; private set; }

    /// <summary>
    /// Confidence level (Low, Medium, High)
    /// </summary>
    public string ConfidenceLevel { get; private set; }

    /// <summary>
    /// Detailed explanation of why conflict was detected
    /// </summary>
    public string ConflictDescription { get; private set; }

    /// <summary>
    /// Matching criteria that triggered the conflict (stored as JSON)
    /// Example: {"national_id": "match", "name_similarity": "95%", "phone": "match"}
    /// </summary>
    public string? MatchingCriteria { get; private set; }

    /// <summary>
    /// Data comparison showing differences (stored as JSON)
    /// Side-by-side comparison of field values
    /// </summary>
    public string? DataComparison { get; private set; }

    // ==================== RESOLUTION STATUS ====================

    /// <summary>
    /// Current resolution status (PendingReview, Resolved, Ignored)
    /// </summary>
    public string Status { get; private set; }

    /// <summary>
    /// Resolution action taken (KeepBoth, Merge, KeepFirst, KeepSecond, etc.)
    /// </summary>
    public ConflictResolutionAction? ResolutionAction { get; private set; }

    /// <summary>
    /// Date when conflict was detected (تاريخ الاكتشاف)
    /// </summary>
    public DateTime DetectedDate { get; private set; }

    /// <summary>
    /// System/user who detected the conflict
    /// </summary>
    public Guid? DetectedByUserId { get; private set; }

    /// <summary>
    /// Date when conflict was assigned for review
    /// </summary>
    public DateTime? AssignedDate { get; private set; }

    /// <summary>
    /// User assigned to resolve the conflict
    /// </summary>
    public Guid? AssignedToUserId { get; private set; }

    /// <summary>
    /// Date when resolution was completed (تاريخ الحل)
    /// </summary>
    public DateTime? ResolvedDate { get; private set; }

    /// <summary>
    /// User who resolved the conflict
    /// </summary>
    public Guid? ResolvedByUserId { get; private set; }

    // ==================== RESOLUTION DETAILS ====================

    /// <summary>
    /// Reason for the chosen resolution action
    /// </summary>
    public string? ResolutionReason { get; private set; }

    /// <summary>
    /// Detailed notes about the resolution
    /// </summary>
    public string? ResolutionNotes { get; private set; }

    /// <summary>
    /// If merged, ID of the resulting merged entity
    /// </summary>
    public Guid? MergedEntityId { get; private set; }

    /// <summary>
    /// If merged, ID of the discarded entity
    /// </summary>
    public Guid? DiscardedEntityId { get; private set; }

    /// <summary>
    /// Merge mapping details (which fields came from which entity)
    /// Stored as JSON for audit trail
    /// </summary>
    public string? MergeMapping { get; private set; }

    // ==================== PRIORITY & SLA ====================

    /// <summary>
    /// Priority level (Low, Normal, High, Critical)
    /// </summary>
    public string Priority { get; private set; }

    /// <summary>
    /// Target resolution time in hours
    /// </summary>
    public int? TargetResolutionHours { get; private set; }

    /// <summary>
    /// Indicates if resolution is overdue
    /// </summary>
    public bool IsOverdue { get; private set; }

    // ==================== AUTOMATED VS MANUAL ====================

    /// <summary>
    /// Indicates if conflict was auto-detected by system
    /// </summary>
    public bool IsAutoDetected { get; private set; }

    /// <summary>
    /// Indicates if resolution was automated (auto-merge based on rules)
    /// </summary>
    public bool IsAutoResolved { get; private set; }

    /// <summary>
    /// Auto-resolution rule that was applied (if automated)
    /// </summary>
    public string? AutoResolutionRule { get; private set; }

    // ==================== ESCALATION ====================

    /// <summary>
    /// Indicates if conflict has been escalated
    /// </summary>
    public bool IsEscalated { get; private set; }

    /// <summary>
    /// Escalation reason
    /// </summary>
    public string? EscalationReason { get; private set; }

    /// <summary>
    /// Date when escalated
    /// </summary>
    public DateTime? EscalatedDate { get; private set; }

    /// <summary>
    /// User who escalated
    /// </summary>
    public Guid? EscalatedByUserId { get; private set; }

    // ==================== REVIEW TRACKING ====================

    /// <summary>
    /// Number of review attempts
    /// </summary>
    public int ReviewAttemptCount { get; private set; }

    /// <summary>
    /// Review history (stored as JSON array)
    /// Tracks all review attempts and decisions
    /// </summary>
    public string? ReviewHistory { get; private set; }

    // ==================== NAVIGATION PROPERTIES ====================

    /// <summary>
    /// Import package that triggered this conflict
    /// </summary>
    public virtual ImportPackage? ImportPackage { get; private set; }

    // Note: User entities for DetectedBy, AssignedTo, ResolvedBy
    // public virtual User? DetectedByUser { get; private set; }
    // public virtual User? AssignedToUser { get; private set; }
    // public virtual User? ResolvedByUser { get; private set; }
    // public virtual User? EscalatedByUser { get; private set; }

    // ==================== CONSTRUCTORS ====================

    /// <summary>
    /// EF Core constructor
    /// </summary>
    private ConflictResolution() : base()
    {
        ConflictNumber = string.Empty;
        ConflictType = string.Empty;
        EntityType = string.Empty;
        ConfidenceLevel = string.Empty;
        ConflictDescription = string.Empty;
        Status = "PendingReview";
        Priority = "Normal";
        IsAutoDetected = false;
        IsAutoResolved = false;
        IsEscalated = false;
        IsOverdue = false;
        ReviewAttemptCount = 0;
    }

    /// <summary>
    /// Create new conflict resolution
    /// </summary>
    public static ConflictResolution Create(
        string conflictType,
        string entityType,
        Guid firstEntityId,
        Guid secondEntityId,
        string? firstEntityIdentifier,
        string? secondEntityIdentifier,
        decimal similarityScore,
        string confidenceLevel,
        string conflictDescription,
        string? matchingCriteriaJson,
        string? dataComparisonJson,
        bool isAutoDetected,
        Guid? importPackageId,
        Guid createdByUserId)
    {
        var conflict = new ConflictResolution
        {
            ConflictType = conflictType,
            EntityType = entityType,
            FirstEntityId = firstEntityId,
            SecondEntityId = secondEntityId,
            FirstEntityIdentifier = firstEntityIdentifier,
            SecondEntityIdentifier = secondEntityIdentifier,
            SimilarityScore = similarityScore,
            ConfidenceLevel = confidenceLevel,
            ConflictDescription = conflictDescription,
            MatchingCriteria = matchingCriteriaJson,
            DataComparison = dataComparisonJson,
            DetectedDate = DateTime.UtcNow,
            ImportPackageId = importPackageId,
            Status = "PendingReview",
            Priority = DeterminePriority(similarityScore, confidenceLevel),
            IsAutoDetected = isAutoDetected,
            IsAutoResolved = false,
            IsEscalated = false,
            IsOverdue = false,
            ReviewAttemptCount = 0
        };

        conflict.ConflictNumber = GenerateConflictNumber();
        conflict.MarkAsCreated(createdByUserId);

        return conflict;
    }

    // ==================== DOMAIN METHODS ====================

    /// <summary>
    /// Assign conflict to user for resolution
    /// </summary>
    public void AssignTo(Guid userId, int? targetHours, Guid modifiedByUserId)
    {
        AssignedToUserId = userId;
        AssignedDate = DateTime.UtcNow;
        TargetResolutionHours = targetHours;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Resolve conflict with chosen action
    /// </summary>
    public void Resolve(
        ConflictResolutionAction action,
        string resolutionReason,
        string? resolutionNotes,
        Guid? mergedEntityId,
        Guid? discardedEntityId,
        string? mergeMappingJson,
        Guid resolvedByUserId,
        Guid modifiedByUserId)
    {
        Status = "Resolved";
        ResolutionAction = action;
        ResolutionReason = resolutionReason;
        ResolutionNotes = resolutionNotes;
        MergedEntityId = mergedEntityId;
        DiscardedEntityId = discardedEntityId;
        MergeMapping = mergeMappingJson;
        ResolvedDate = DateTime.UtcNow;
        ResolvedByUserId = resolvedByUserId;
        IsOverdue = false;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Auto-resolve conflict using system rules
    /// </summary>
    public void AutoResolve(
        ConflictResolutionAction action,
        string autoResolutionRule,
        Guid? mergedEntityId,
        Guid modifiedByUserId)
    {
        Status = "Resolved";
        ResolutionAction = action;
        ResolutionReason = $"Auto-resolved using rule: {autoResolutionRule}";
        AutoResolutionRule = autoResolutionRule;
        MergedEntityId = mergedEntityId;
        ResolvedDate = DateTime.UtcNow;
        IsAutoResolved = true;
        IsOverdue = false;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Ignore conflict (not a duplicate)
    /// </summary>
    public void Ignore(string reason, Guid modifiedByUserId)
    {
        Status = "Ignored";
        ResolutionAction = ConflictResolutionAction.Ignored;
        ResolutionReason = reason;
        ResolvedDate = DateTime.UtcNow;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Escalate conflict to supervisor
    /// </summary>
    public void Escalate(string escalationReason, Guid escalatedByUserId, Guid modifiedByUserId)
    {
        IsEscalated = true;
        EscalationReason = escalationReason;
        EscalatedDate = DateTime.UtcNow;
        EscalatedByUserId = escalatedByUserId;
        Priority = "High"; // Escalated conflicts get high priority
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Record review attempt
    /// </summary>
    public void RecordReviewAttempt(string reviewNotes, Guid modifiedByUserId)
    {
        ReviewAttemptCount++;

        var reviewEntry = new
        {
            AttemptNumber = ReviewAttemptCount,
            Date = DateTime.UtcNow,
            Notes = reviewNotes
        };

        // Append to review history (simplified - would use JSON serialization)
        var historyEntry = System.Text.Json.JsonSerializer.Serialize(reviewEntry);
        ReviewHistory = string.IsNullOrWhiteSpace(ReviewHistory)
            ? $"[{historyEntry}]"
            : ReviewHistory.TrimEnd(']') + $",{historyEntry}]";

        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Mark as overdue
    /// </summary>
    public void MarkAsOverdue()
    {
        IsOverdue = true;
    }

    /// <summary>
    /// Update priority
    /// </summary>
    public void UpdatePriority(string newPriority, Guid modifiedByUserId)
    {
        Priority = newPriority;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Check if conflict is overdue
    /// </summary>
    public bool CheckIfOverdue()
    {
        if (!TargetResolutionHours.HasValue || DetectedDate == default || Status == "Resolved" || Status == "Ignored")
            return false;

        var targetDate = DetectedDate.AddHours(TargetResolutionHours.Value);
        return DateTime.UtcNow > targetDate;
    }

    /// <summary>
    /// Calculate time elapsed since detection
    /// </summary>
    public TimeSpan GetElapsedTime()
    {
        var endDate = ResolvedDate ?? DateTime.UtcNow;
        return endDate - DetectedDate;
    }

    // ==================== HELPER METHODS ====================

    /// <summary>
    /// Generate conflict number
    /// Format: CNF-YYYY-NNNN
    /// </summary>
    private static string GenerateConflictNumber()
    {
        var year = DateTime.UtcNow.Year;
        var random = new Random();
        var sequence = random.Next(1000, 9999);
        return $"CNF-{year}-{sequence:D4}";
    }

    /// <summary>
    /// Determine priority based on similarity and confidence
    /// </summary>
    private static string DeterminePriority(decimal similarityScore, string confidenceLevel)
    {
        if (confidenceLevel == "High" && similarityScore >= 90)
            return "High";

        if (confidenceLevel == "Medium" || similarityScore >= 70)
            return "Normal";

        return "Low";
    }
}