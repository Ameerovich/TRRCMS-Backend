using TRRCMS.Domain.Common;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Domain.Entities;

/// <summary>
/// Case entity — aggregates all work done on a PropertyUnit.
/// One-to-one with PropertyUnit. Auto-created when the first survey touches a PropertyUnit.
/// Status is Open by default, Closed when an ownership/heir claim is created.
/// </summary>
public class Case : BaseAuditableEntity
{
    /// <summary>
    /// Unique case number (رقم الحالة)
    /// Format: CASE-YYYY-NNNNN
    /// </summary>
    public string CaseNumber { get; private set; }

    /// <summary>
    /// Foreign key to the PropertyUnit this case covers (one-to-one)
    /// </summary>
    public Guid PropertyUnitId { get; private set; }

    /// <summary>
    /// Case lifecycle status — Open or Closed
    /// </summary>
    public CaseLifecycleStatus Status { get; private set; }

    /// <summary>
    /// Date when the case was opened
    /// </summary>
    public DateTime OpenedDate { get; private set; }

    /// <summary>
    /// Date when the case was closed (null if still open)
    /// </summary>
    public DateTime? ClosedDate { get; private set; }

    /// <summary>
    /// Foreign key to the Claim that caused the case to close (null if still open)
    /// </summary>
    public Guid? ClosedByClaimId { get; private set; }

    /// <summary>
    /// Whether the case data can be edited. Default: true.
    /// </summary>
    public bool IsEditable { get; private set; }

    /// <summary>
    /// Additional notes
    /// </summary>
    public string? Notes { get; private set; }

    // Navigation properties

    /// <summary>
    /// The property unit this case covers
    /// </summary>
    public virtual PropertyUnit PropertyUnit { get; private set; } = null!;

    /// <summary>
    /// The claim that caused the case to close (if any)
    /// </summary>
    public virtual Claim? ClosedByClaim { get; private set; }

    /// <summary>
    /// All surveys linked to this case
    /// </summary>
    public virtual ICollection<Survey> Surveys { get; private set; }

    /// <summary>
    /// All claims linked to this case
    /// </summary>
    public virtual ICollection<Claim> Claims { get; private set; }

    /// <summary>
    /// All person-property relations linked to this case
    /// </summary>
    public virtual ICollection<PersonPropertyRelation> PersonPropertyRelations { get; private set; }

    /// <summary>
    /// EF Core constructor
    /// </summary>
    private Case() : base()
    {
        CaseNumber = string.Empty;
        Status = CaseLifecycleStatus.Open;
        IsEditable = true;
        Surveys = new List<Survey>();
        Claims = new List<Claim>();
        PersonPropertyRelations = new List<PersonPropertyRelation>();
    }

    /// <summary>
    /// Create a new case for a property unit (Factory Method — DDD)
    /// </summary>
    public static Case Create(
        string caseNumber,
        Guid propertyUnitId,
        Guid createdByUserId)
    {
        var entity = new Case
        {
            CaseNumber = caseNumber,
            PropertyUnitId = propertyUnitId,
            Status = CaseLifecycleStatus.Open,
            IsEditable = true,
            OpenedDate = DateTime.UtcNow
        };
        entity.MarkAsCreated(createdByUserId);
        return entity;
    }

    /// <summary>
    /// Close the case when an ownership/heir claim is created.
    /// Idempotent — does nothing if already closed.
    /// </summary>
    public void Close(Guid claimId, Guid modifiedByUserId)
    {
        if (Status == CaseLifecycleStatus.Closed)
            return;

        Status = CaseLifecycleStatus.Closed;
        ClosedDate = DateTime.UtcNow;
        ClosedByClaimId = claimId;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Reopen a previously closed case (Admin/DataManager action)
    /// </summary>
    public void Reopen(Guid modifiedByUserId)
    {
        Status = CaseLifecycleStatus.Open;
        ClosedDate = null;
        ClosedByClaimId = null;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Update notes
    /// </summary>
    public void UpdateNotes(string? notes, Guid modifiedByUserId)
    {
        Notes = notes;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Set the editable flag
    /// </summary>
    public void SetEditable(bool isEditable, Guid modifiedByUserId)
    {
        IsEditable = isEditable;
        MarkAsModified(modifiedByUserId);
    }
}
