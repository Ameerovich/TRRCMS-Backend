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
    /// Obstructed - Survey could not be conducted due to non-cooperation (معرقل)
    /// Owner/occupant refused to cooperate or access was blocked
    /// </summary>
    [ArabicLabel("معرقل")]
    Obstructed = 4,

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