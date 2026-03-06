namespace TRRCMS.Domain.Enums;

/// <summary>
/// Case status — Open (حالة مفتوحة) or Closed (حالة مغلقة).
/// Determined by the RelationType of the PersonPropertyRelation that generated the claim:
/// - Owner (1) or Heir (5) → Closed
/// - Occupant (2), Tenant (3), Guest (4), Other (99) → Open
/// Independent of LifecycleStage (processing workflow).
/// </summary>
public enum CaseStatus
{
    /// <summary>
    /// Open — occupants are non-owners; waiting for ownership claimant (حالة مفتوحة)
    /// </summary>
    [ArabicLabel("حالة مفتوحة")]
    Open = 1,

    /// <summary>
    /// Closed — ownership claim registered (حالة مغلقة)
    /// </summary>
    [ArabicLabel("حالة مغلقة")]
    Closed = 2
}
