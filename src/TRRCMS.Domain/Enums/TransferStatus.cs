namespace TRRCMS.Domain.Enums;

/// <summary>
/// Transfer status for building assignments to field collectors
/// Used when assigning buildings to tablets
/// Referenced in FSD section 6.2.3
/// </summary>
public enum TransferStatus
{
    /// <summary>
    /// Pending transfer - Assignment created but not yet transferred (قيد الانتظار)
    /// </summary>
    [ArabicLabel("قيد الانتظار")]
    Pending = 1,

    /// <summary>
    /// In progress - Transfer is currently in progress (قيد التنفيذ)
    /// </summary>
    [ArabicLabel("قيد التنفيذ")]
    InProgress = 2,

    /// <summary>
    /// Transferred - Successfully transferred to tablet (منقول)
    /// </summary>
    [ArabicLabel("منقول")]
    Transferred = 3,

    /// <summary>
    /// Failed - Transfer failed (فشل)
    /// </summary>
    [ArabicLabel("فشل")]
    Failed = 4,

    /// <summary>
    /// Cancelled - Transfer cancelled (ملغى)
    /// </summary>
    [ArabicLabel("ملغى")]
    Cancelled = 5,

    /// <summary>
    /// Partial transfer - Some data transferred, some failed (نقل جزئي)
    /// </summary>
    [ArabicLabel("نقل جزئي")]
    PartialTransfer = 6,

    /// <summary>
    /// Synchronized - Data synced back to server (متزامن)
    /// </summary>
    [ArabicLabel("متزامن")]
    Synchronized = 7
}