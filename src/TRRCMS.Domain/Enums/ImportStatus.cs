namespace TRRCMS.Domain.Enums;

/// <summary>
/// Import status for .uhc container packages
/// </summary>
public enum ImportStatus
{
    /// <summary>
    /// Pending import - Package received but not yet processed (قيد الانتظار)
    /// </summary>
    [ArabicLabel("قيد الانتظار")]
    Pending = 1,

    /// <summary>
    /// Validating - Package is being validated (signature, schema check) (قيد التحقق)
    /// </summary>
    [ArabicLabel("قيد التحقق")]
    Validating = 2,

    /// <summary>
    /// Staging - Data is being staged for review (قيد التدريج)
    /// </summary>
    [ArabicLabel("قيد التدريج")]
    Staging = 3,

    /// <summary>
    /// Validation failed - Package failed validation checks (فشل التحقق)
    /// </summary>
    [ArabicLabel("فشل التحقق")]
    ValidationFailed = 4,

    /// <summary>
    /// Quarantined - Package quarantined due to errors (معزول)
    /// </summary>
    [ArabicLabel("معزول")]
    Quarantined = 5,

    /// <summary>
    /// Reviewing conflicts - Human review of conflicts required (مراجعة التعارضات)
    /// </summary>
    [ArabicLabel("مراجعة التعارضات")]
    ReviewingConflicts = 6,

    /// <summary>
    /// Ready to commit - Validated and ready for database commit (جاهز للحفظ)
    /// </summary>
    [ArabicLabel("جاهز للحفظ")]
    ReadyToCommit = 7,

    /// <summary>
    /// Committing - Data is being committed to database (قيد الحفظ)
    /// </summary>
    [ArabicLabel("قيد الحفظ")]
    Committing = 8,

    /// <summary>
    /// Completed - Import completed successfully (مكتمل)
    /// </summary>
    [ArabicLabel("مكتمل")]
    Completed = 9,

    /// <summary>
    /// Failed - Import failed (فشل)
    /// </summary>
    [ArabicLabel("فشل")]
    Failed = 10,

    /// <summary>
    /// Partially completed - Some data imported, some failed (مكتمل جزئياً)
    /// </summary>
    [ArabicLabel("مكتمل جزئياً")]
    PartiallyCompleted = 11,

    /// <summary>
    /// Cancelled - Import cancelled by user (ملغى)
    /// </summary>
    [ArabicLabel("ملغى")]
    Cancelled = 12
}