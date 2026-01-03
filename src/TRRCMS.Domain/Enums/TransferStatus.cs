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
    Pending = 1,

    /// <summary>
    /// In progress - Transfer is currently in progress (قيد التنفيذ)
    /// </summary>
    InProgress = 2,

    /// <summary>
    /// Transferred - Successfully transferred to tablet (منقول)
    /// </summary>
    Transferred = 3,

    /// <summary>
    /// Failed - Transfer failed (فشل)
    /// </summary>
    Failed = 4,

    /// <summary>
    /// Cancelled - Transfer cancelled (ملغى)
    /// </summary>
    Cancelled = 5,

    /// <summary>
    /// Partial transfer - Some data transferred, some failed (نقل جزئي)
    /// </summary>
    PartialTransfer = 6,

    /// <summary>
    /// Synchronized - Data synced back to server (متزامن)
    /// </summary>
    Synchronized = 7
}