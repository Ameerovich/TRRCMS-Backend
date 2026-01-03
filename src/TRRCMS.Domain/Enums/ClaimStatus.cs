namespace TRRCMS.Domain.Enums;

/// <summary>
/// Claim status classification
/// </summary>
public enum ClaimStatus
{
    /// <summary>
    /// Draft - not yet finalized (مسودة)
    /// </summary>
    Draft = 1,

    /// <summary>
    /// Finalized - ready for review (نهائي)
    /// Per UC-001 S25, UC-004 S21: "mark as finalized"
    /// </summary>
    Finalized = 2,

    /// <summary>
    /// Under review (قيد المراجعة)
    /// </summary>
    UnderReview = 3,

    /// <summary>
    /// Approved (موافق عليه)
    /// </summary>
    Approved = 4,

    /// <summary>
    /// Rejected (مرفوض)
    /// </summary>
    Rejected = 5,

    /// <summary>
    /// Pending additional evidence (بانتظار مستندات إضافية)
    /// </summary>
    PendingEvidence = 6,

    /// <summary>
    /// Disputed - conflicting claims exist (متنازع عليه)
    /// </summary>
    Disputed = 7,

    /// <summary>
    /// Archived (مؤرشف)
    /// </summary>
    Archived = 99
}