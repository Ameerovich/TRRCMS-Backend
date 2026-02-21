using TRRCMS.Domain.Common;

namespace TRRCMS.Domain.Entities;

/// <summary>
/// EvidenceRelation entity — many-to-many join between Evidence and PersonPropertyRelation.
/// One evidence document can support multiple person-property relations.
/// Follows the UserPermission pattern: BaseAuditableEntity, GUID PK, IsActive, factory Create().
/// </summary>
public class EvidenceRelation : BaseAuditableEntity
{
    // ==================== RELATIONSHIP KEYS ====================

    /// <summary>
    /// Foreign key to Evidence
    /// </summary>
    public Guid EvidenceId { get; private set; }

    /// <summary>
    /// Foreign key to PersonPropertyRelation
    /// </summary>
    public Guid PersonPropertyRelationId { get; private set; }

    // ==================== METADATA ====================

    /// <summary>
    /// Why this evidence was linked to this relation
    /// </summary>
    public string? LinkReason { get; private set; }

    /// <summary>
    /// When the link was created (UTC)
    /// </summary>
    public DateTime LinkedAtUtc { get; private set; }

    /// <summary>
    /// User who created this link
    /// </summary>
    public Guid LinkedBy { get; private set; }

    /// <summary>
    /// Whether this link is currently active
    /// </summary>
    public bool IsActive { get; private set; }

    // ==================== NAVIGATION PROPERTIES ====================

    /// <summary>
    /// Evidence being linked
    /// </summary>
    public virtual Evidence Evidence { get; private set; } = null!;

    /// <summary>
    /// Person-property relation being linked
    /// </summary>
    public virtual PersonPropertyRelation PersonPropertyRelation { get; private set; } = null!;

    // ==================== CONSTRUCTORS ====================

    /// <summary>
    /// EF Core constructor
    /// </summary>
    private EvidenceRelation() : base()
    {
        IsActive = true;
        LinkedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Create new evidence-relation link
    /// </summary>
    public static EvidenceRelation Create(
        Guid evidenceId,
        Guid personPropertyRelationId,
        Guid linkedBy,
        string? linkReason = null)
    {
        var evidenceRelation = new EvidenceRelation
        {
            EvidenceId = evidenceId,
            PersonPropertyRelationId = personPropertyRelationId,
            LinkedBy = linkedBy,
            LinkReason = linkReason,
            LinkedAtUtc = DateTime.UtcNow,
            IsActive = true
        };

        evidenceRelation.MarkAsCreated(linkedBy);

        return evidenceRelation;
    }

    // ==================== DOMAIN METHODS ====================

    /// <summary>
    /// Deactivate this link
    /// </summary>
    public void Deactivate(Guid deactivatedBy, string reason)
    {
        IsActive = false;
        LinkReason = string.IsNullOrWhiteSpace(LinkReason)
            ? $"[Deactivated]: {reason}"
            : $"{LinkReason}\n[Deactivated]: {reason}";
        MarkAsModified(deactivatedBy);
    }

    /// <summary>
    /// Reactivate this link
    /// </summary>
    public void Reactivate(Guid reactivatedBy, string reason)
    {
        IsActive = true;
        LinkReason = string.IsNullOrWhiteSpace(LinkReason)
            ? $"[Reactivated]: {reason}"
            : $"{LinkReason}\n[Reactivated]: {reason}";
        MarkAsModified(reactivatedBy);
    }
}
