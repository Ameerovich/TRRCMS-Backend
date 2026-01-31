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
    /// Relation type (نوع العلاقة) - Owner, Occupant, Tenant, Guest, Heir, Other
    /// </summary>
    public RelationType RelationType { get; private set; }
    public string? RelationTypeOtherDesc { get; private set; }

    /// <summary>
    /// Contract/Tenure type (نوع العقد)
    /// </summary>
    public TenureContractType? ContractType { get; private set; }
    public string? ContractTypeOtherDesc { get; private set; }

    public decimal? OwnershipShare { get; private set; }
    public string? ContractDetails { get; private set; }
    public DateTime? StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public string? Notes { get; private set; }
    public bool IsActive { get; private set; }

    // Navigation properties
    public virtual Person Person { get; private set; } = null!;
    public virtual PropertyUnit PropertyUnit { get; private set; } = null!;
    public virtual ICollection<Evidence> Evidences { get; private set; }

    private PersonPropertyRelation() : base()
    {
        RelationType = RelationType.Other;
        IsActive = true;
        Evidences = new List<Evidence>();
    }

    public static PersonPropertyRelation Create(
        Guid personId,
        Guid propertyUnitId,
        RelationType relationType,
        Guid createdByUserId)
    {
        var relation = new PersonPropertyRelation
        {
            PersonId = personId,
            PropertyUnitId = propertyUnitId,
            RelationType = relationType,
            IsActive = true
        };
        relation.MarkAsCreated(createdByUserId);
        return relation;
    }

    public void UpdateRelationDetails(
        RelationType relationType,
        string? relationTypeOtherDesc,
        TenureContractType? contractType,
        string? contractTypeOtherDesc,
        decimal? ownershipShare,
        string? contractDetails,
        DateTime? startDate,
        DateTime? endDate,
        string? notes,
        Guid modifiedByUserId)
    {
        RelationType = relationType;
        RelationTypeOtherDesc = relationTypeOtherDesc;
        ContractType = contractType;
        ContractTypeOtherDesc = contractTypeOtherDesc;
        OwnershipShare = ownershipShare;
        ContractDetails = contractDetails;
        StartDate = startDate;
        EndDate = endDate;
        Notes = notes;
        MarkAsModified(modifiedByUserId);
    }

    public void PartialUpdate(
        RelationType? relationType,
        string? relationTypeOtherDesc,
        TenureContractType? contractType,
        string? contractTypeOtherDesc,
        decimal? ownershipShare,
        string? contractDetails,
        DateTime? startDate,
        DateTime? endDate,
        string? notes,
        bool clearRelationTypeOtherDesc,
        bool clearContractType,
        bool clearContractTypeOtherDesc,
        bool clearOwnershipShare,
        bool clearContractDetails,
        bool clearStartDate,
        bool clearEndDate,
        bool clearNotes,
        Guid modifiedByUserId)
    {
        if (relationType.HasValue) RelationType = relationType.Value;
        if (clearRelationTypeOtherDesc) RelationTypeOtherDesc = null;
        else if (relationTypeOtherDesc != null) RelationTypeOtherDesc = relationTypeOtherDesc;
        if (clearContractType) ContractType = null;
        else if (contractType.HasValue) ContractType = contractType.Value;
        if (clearContractTypeOtherDesc) ContractTypeOtherDesc = null;
        else if (contractTypeOtherDesc != null) ContractTypeOtherDesc = contractTypeOtherDesc;
        if (clearOwnershipShare) OwnershipShare = null;
        else if (ownershipShare.HasValue) OwnershipShare = ownershipShare;
        if (clearContractDetails) ContractDetails = null;
        else if (contractDetails != null) ContractDetails = contractDetails;
        if (clearStartDate) StartDate = null;
        else if (startDate.HasValue) StartDate = startDate;
        if (clearEndDate) EndDate = null;
        else if (endDate.HasValue) EndDate = endDate;
        if (clearNotes) Notes = null;
        else if (notes != null) Notes = notes;
        MarkAsModified(modifiedByUserId);
    }

    public void Terminate(DateTime endDate, Guid modifiedByUserId)
    {
        IsActive = false;
        EndDate = endDate;
        MarkAsModified(modifiedByUserId);
    }

    public void Reactivate(Guid modifiedByUserId)
    {
        IsActive = true;
        EndDate = null;
        MarkAsModified(modifiedByUserId);
    }
}
