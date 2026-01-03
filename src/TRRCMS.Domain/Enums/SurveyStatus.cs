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
    Draft = 1,

    /// <summary>
    /// Completed - Survey completed but not finalized (مكتمل)
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Finalized - Survey finalized and ready for export (نهائي)
    /// Per UC-001 S25, UC-004 S21: "mark as finalized"
    /// </summary>
    Finalized = 3,

    /// <summary>
    /// Exported - Survey exported to .uhc container (مُصدّر)
    /// </summary>
    Exported = 4,

    /// <summary>
    /// Imported - Survey imported to desktop system (مُستورد)
    /// </summary>
    Imported = 5,

    /// <summary>
    /// Validated - Survey data validated by data manager (مُدقق)
    /// </summary>
    Validated = 6,

    /// <summary>
    /// Requires revision - Survey needs corrections (يتطلب مراجعة)
    /// </summary>
    RequiresRevision = 7,

    /// <summary>
    /// Cancelled - Survey cancelled (ملغى)
    /// </summary>
    Cancelled = 8,

    /// <summary>
    /// Archived - Survey archived (مؤرشف)
    /// </summary>
    Archived = 99
}