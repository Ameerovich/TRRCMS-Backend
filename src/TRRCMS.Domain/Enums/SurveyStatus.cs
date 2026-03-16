namespace TRRCMS.Domain.Enums;

/// <summary>
/// Survey status classification
/// Status of field surveys conducted by field collectors
/// </summary>
public enum SurveyStatus
{
    /// <summary>
    /// Draft - Survey in progress, not completed (مسودة)
    /// </summary>
    [ArabicLabel("مسودة")]
    Draft = 1,

    /// <summary>
    /// Finalized - Survey finalized and ready for claim creation (نهائي)
    /// Per"mark as finalized"
    /// </summary>
    [ArabicLabel("نهائي")]
    Finalized = 3,

    /// <summary>
    /// Cancelled - Survey cancelled (ملغى)
    /// </summary>
    [ArabicLabel("ملغى")]
    Cancelled = 8,

    /// <summary>
    /// Archived - Survey archived (مؤرشف)
    /// </summary>
    [ArabicLabel("مؤرشف")]
    Archived = 99
}