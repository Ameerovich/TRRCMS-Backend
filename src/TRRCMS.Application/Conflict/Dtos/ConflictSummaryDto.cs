namespace TRRCMS.Application.Conflicts.Dtos;

/// <summary>
/// Dashboard summary DTO for the conflict management overview.
/// Provides aggregate counts grouped by type and status
/// to power the queue summary widgets and badge counters.
/// </summary>
public class ConflictSummaryDto
{
    // ==================== TOTALS ====================

    public int TotalConflicts { get; set; }
    public int PendingReviewCount { get; set; }
    public int ResolvedCount { get; set; }
    public int IgnoredCount { get; set; }

    // ==================== BY TYPE ====================

    public int PersonDuplicateCount { get; set; }
    public int PropertyDuplicateCount { get; set; }
    public int ClaimConflictCount { get; set; }

    // ==================== PENDING BY TYPE ====================

    public int PendingPersonDuplicates { get; set; }
    public int PendingPropertyDuplicates { get; set; }
    public int PendingClaimConflicts { get; set; }

    // ==================== PRIORITY & FLAGS ====================

    public int EscalatedCount { get; set; }
    public int OverdueCount { get; set; }
    public int HighPriorityCount { get; set; }
    public int AutoResolvedCount { get; set; }
}
