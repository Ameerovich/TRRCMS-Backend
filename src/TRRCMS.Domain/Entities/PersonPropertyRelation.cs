using TRRCMS.Domain.Common;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Domain.Entities;

/// <summary>
/// Person-Property Relation entity
/// Links a Person to a PropertyUnit with a specific relation type and evidence
/// </summary>
public class PersonPropertyRelation : BaseAuditableEntity
{
    public Guid PersonId { get; private set; }
    public Guid PropertyUnitId { get; private set; }

    /// <summary>
    /// The survey that created this relation. Used to scope claim creation
    /// to only relations created within a specific survey context.
    /// </summary>
    public Guid? SurveyId { get; private set; }

    /// <summary>
    /// Relation type (نوع العلاقة) - Owner, Occupant, Tenant, Guest, Heir, Other
    /// </summary>
    public RelationType RelationType { get; private set; }

    // ==================== NEW FIELDS FOR OFFICE SURVEY ====================

    /// <summary>
    /// Occupancy type (نوع الإشغال) - OwnerOccupied, TenantOccupied, FamilyOccupied, etc.
    /// </summary>
    public OccupancyType? OccupancyType { get; private set; }

    /// <summary>
    /// Indicates if evidence documents are available/attached (هل يوجد دليل؟)
    /// </summary>
    public bool HasEvidence { get; private set; }

    // ==================== OTHER FIELDS ====================

    public decimal? OwnershipShare { get; private set; }
    public string? ContractDetails { get; private set; }
    public string? Notes { get; private set; }
    public bool IsActive { get; private set; }

    // Navigation properties
    public virtual Person Person { get; private set; } = null!;
    public virtual PropertyUnit PropertyUnit { get; private set; } = null!;
    public virtual Survey? Survey { get; private set; }
    /// <summary>
    /// Many-to-many links to Evidence via EvidenceRelation join entity
    /// </summary>
    public virtual ICollection<EvidenceRelation> EvidenceRelations { get; private set; }

    private PersonPropertyRelation() : base()
    {
        RelationType = RelationType.Other;
        IsActive = true;
        EvidenceRelations = new List<EvidenceRelation>();
    }

    public static PersonPropertyRelation Create(
        Guid personId,
        Guid propertyUnitId,
        RelationType relationType,
        OccupancyType? occupancyType,
        bool hasEvidence,
        Guid createdByUserId,
        Guid? surveyId = null)
    {
        var relation = new PersonPropertyRelation
        {
            PersonId = personId,
            PropertyUnitId = propertyUnitId,
            RelationType = relationType,
            OccupancyType = occupancyType,
            HasEvidence = hasEvidence,
            SurveyId = surveyId,
            IsActive = true
        };
        relation.MarkAsCreated(createdByUserId);
        return relation;
    }

    public void UpdateRelationDetails(
        RelationType relationType,
        OccupancyType? occupancyType,
        bool hasEvidence,
        decimal? ownershipShare,
        string? contractDetails,
        string? notes,
        Guid modifiedByUserId)
    {
        RelationType = relationType;
        OccupancyType = occupancyType;
        HasEvidence = hasEvidence;
        OwnershipShare = ownershipShare;
        ContractDetails = contractDetails;
        Notes = notes;
        MarkAsModified(modifiedByUserId);
    }

    public void UpdatePersonId(Guid newPersonId, Guid modifiedByUserId)
    {
        PersonId = newPersonId;
        MarkAsModified(modifiedByUserId);
    }

    public void UpdatePropertyUnitId(Guid newPropertyUnitId, Guid modifiedByUserId)
    {
        PropertyUnitId = newPropertyUnitId;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Set HasEvidence flag explicitly
    /// </summary>
    public void SetHasEvidence(bool hasEvidence, Guid modifiedByUserId)
    {
        HasEvidence = hasEvidence;
        MarkAsModified(modifiedByUserId);
    }
}
