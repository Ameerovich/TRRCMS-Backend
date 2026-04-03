namespace TRRCMS.Domain.Enums;

/// <summary>
/// Lifecycle status of a Case entity (not to be confused with CaseStatus on Claim).
/// A Case is Open by default and becomes Closed when an ownership/heir claim is created.
/// </summary>
public enum CaseLifecycleStatus
{
    /// <summary>
    /// Open — no ownership claim yet; case is active (حالة مفتوحة)
    /// </summary>
    [ArabicLabel("مفتوحة")]
    Open = 1,

    /// <summary>
    /// Closed — an ownership/heir claim was registered (حالة مغلقة)
    /// </summary>
    [ArabicLabel("مغلقة")]
    Closed = 2
}
