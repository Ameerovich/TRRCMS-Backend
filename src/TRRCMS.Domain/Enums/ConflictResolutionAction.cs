namespace TRRCMS.Domain.Enums;

/// <summary>
/// Conflict resolution action for duplicate detection and resolution
/// </summary>
public enum ConflictResolutionAction
{
    /// <summary>
    /// Pending review - Conflict detected, awaiting human decision (قيد المراجعة)
    /// </summary>
    PendingReview = 1,

    /// <summary>
    /// Keep both - Records are not duplicates, keep both (الاحتفاظ بكليهما)
    /// </summary>
    KeepBoth = 2,

    /// <summary>
    /// Merge - Merge the duplicate records into one (دمج)
    /// </summary>
    Merge = 3,

    /// <summary>
    /// Keep first - Keep the first record, discard second (الاحتفاظ بالأول)
    /// </summary>
    KeepFirst = 4,

    /// <summary>
    /// Keep second - Keep the second record, discard first (الاحتفاظ بالثاني)
    /// </summary>
    KeepSecond = 5,

    /// <summary>
    /// Mark as duplicate - Flag as duplicate but no action yet (وضع علامة كمكرر)
    /// </summary>
    MarkAsDuplicate = 6,

    /// <summary>
    /// Escalate - Escalate to supervisor or manager (تصعيد)
    /// </summary>
    Escalate = 7,

    /// <summary>
    /// Resolved - Conflict resolved (تم الحل)
    /// </summary>
    Resolved = 8,

    /// <summary>
    /// Ignored - Conflict ignored/dismissed (تم التجاهل)
    /// </summary>
    Ignored = 9
}