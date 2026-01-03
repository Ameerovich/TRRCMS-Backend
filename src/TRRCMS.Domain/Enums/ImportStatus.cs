namespace TRRCMS.Domain.Enums;

/// <summary>
/// Import status for .uhc container packages
/// </summary>
public enum ImportStatus
{
    /// <summary>
    /// Pending import - Package received but not yet processed (قيد الانتظار)
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Validating - Package is being validated (signature, schema check) (قيد التحقق)
    /// </summary>
    Validating = 2,

    /// <summary>
    /// Staging - Data is being staged for review (قيد التدريج)
    /// </summary>
    Staging = 3,

    /// <summary>
    /// Validation failed - Package failed validation checks (فشل التحقق)
    /// </summary>
    ValidationFailed = 4,

    /// <summary>
    /// Quarantined - Package quarantined due to errors (معزول)
    /// </summary>
    Quarantined = 5,

    /// <summary>
    /// Reviewing conflicts - Human review of conflicts required (مراجعة التعارضات)
    /// </summary>
    ReviewingConflicts = 6,

    /// <summary>
    /// Ready to commit - Validated and ready for database commit (جاهز للحفظ)
    /// </summary>
    ReadyToCommit = 7,

    /// <summary>
    /// Committing - Data is being committed to database (قيد الحفظ)
    /// </summary>
    Committing = 8,

    /// <summary>
    /// Completed - Import completed successfully (مكتمل)
    /// </summary>
    Completed = 9,

    /// <summary>
    /// Failed - Import failed (فشل)
    /// </summary>
    Failed = 10,

    /// <summary>
    /// Partially completed - Some data imported, some failed (مكتمل جزئياً)
    /// </summary>
    PartiallyCompleted = 11,

    /// <summary>
    /// Cancelled - Import cancelled by user (ملغى)
    /// </summary>
    Cancelled = 12
}