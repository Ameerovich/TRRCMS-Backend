namespace TRRCMS.Domain.Enums;

/// <summary>
/// Survey status classification
/// Status of field surveys conducted by field collectors
/// Referenced in FSD section 6.2.2
/// </summary>
public enum SurveyStatus
{
    /// <summary>
    /// Draft - Survey in progress, not completed (مسودة)
    /// </summary>
    [ArabicLabel("مسودة")]
    Draft = 1,

    /// <summary>
    /// Completed - Survey completed but not finalized (مكتمل)
    /// </summary>
    [ArabicLabel("مكتمل")]
    Completed = 2,

    /// <summary>
    /// Finalized - Survey finalized and ready for export (نهائي)
    /// Per UC-001 S25, UC-004 S21: "mark as finalized"
    /// </summary>
    [ArabicLabel("نهائي")]
    Finalized = 3,

    /// <summary>
    /// Exported - Survey exported to .uhc container (مُصدّر)
    /// </summary>
    [ArabicLabel("مُصدّر")]
    Exported = 4,

    /// <summary>
    /// Imported - Survey imported to desktop system (مُستورد)
    /// </summary>
    [ArabicLabel("مُستورد")]
    Imported = 5,

    /// <summary>
    /// Validated - Survey data validated by data manager (مُدقق)
    /// </summary>
    [ArabicLabel("مُدقق")]
    Validated = 6,

    /// <summary>
    /// Requires revision - Survey needs corrections (يتطلب مراجعة)
    /// </summary>
    [ArabicLabel("يتطلب مراجعة")]
    RequiresRevision = 7,

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