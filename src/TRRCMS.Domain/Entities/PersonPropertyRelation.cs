using TRRCMS.Domain.Common;

namespace TRRCMS.Domain.Entities;

/// <summary>
/// Person-Property Relation entity
/// Links a Person to a PropertyUnit with a specific relation type and evidence
/// Relation types include owner, occupant, tenant, guest, heir
/// </summary>
public class PersonPropertyRelation : BaseAuditableEntity
{
    // ==================== RELATIONSHIP IDENTIFIERS ====================

    /// <summary>
    /// Foreign key to Person
    /// </summary>
    public Guid PersonId { get; private set; }

    /// <summary>
    /// Foreign key to PropertyUnit
    /// </summary>
    public Guid PropertyUnitId { get; private set; }

    // ==================== RELATION ATTRIBUTES ====================

    /// <summary>
    /// Relation type (controlled vocabulary)
    /// </summary>
    public string RelationType { get; private set; }

   
    /// <summary>
    /// Description in case the chosen type is "Other"
    /// </summary>

    public string? RelationTypeOtherDesc { get; set; }

    /// <summary>
    /// Ownership or occupancy share (if applicable)
    /// </summary>

    public decimal? OwnershipShare { get; private set; }

    /// <summary>
    /// Contract or agreement details (if applicable)
    /// </summary>
    public string? ContractDetails { get; private set; }

    /// <summary>
    /// Start date of the relation (when did ownership/tenancy begin)
    /// </summary>
    public DateTime? StartDate { get; private set; }

    /// <summary>
    /// End date of the relation (for terminated tenancies, etc.)
    /// </summary>
    public DateTime? EndDate { get; private set; }

    /// <summary>
    /// Additional notes about this relation
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// Indicates if this relation is currently active
    /// </summary>
    public bool IsActive { get; private set; }

    // ==================== NAVIGATION PROPERTIES ====================

    /// <summary>
    /// The person in this relation
    /// </summary>
    public virtual Person Person { get; private set; } = null!;

    /// <summary>
    /// The property unit in this relation
    /// </summary>
    public virtual PropertyUnit PropertyUnit { get; private set; } = null!;

    /// <summary>
    /// Evidence supporting this relation
    /// </summary>
    public virtual ICollection<Evidence> Evidences { get; private set; }

    // ==================== CONSTRUCTORS ====================

    /// <summary>
    /// EF Core constructor
    /// </summary>
    private PersonPropertyRelation() : base()
    {
        RelationType = string.Empty;
        IsActive = true;
        Evidences = new List<Evidence>();
    }

    /// <summary>
    /// Create new person-property relation
    /// </summary>
    public static PersonPropertyRelation Create(
        Guid personId,
        Guid propertyUnitId,
        string relationType,
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

    // ==================== DOMAIN METHODS ====================

    /// <summary>
    /// Update relation details
    /// </summary>
    public void UpdateRelationDetails(
        string relationType,
        string? relationTypeOtherDesc,  
        decimal? ownershipShare,
        string? contractDetails,
        DateTime? startDate,
        DateTime? endDate,
        string? notes,
        Guid modifiedByUserId)
    {
        RelationType = relationType;
        RelationTypeOtherDesc = relationTypeOtherDesc; 
        OwnershipShare = ownershipShare;
        ContractDetails = contractDetails;
        StartDate = startDate;
        EndDate = endDate;
        Notes = notes;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Terminate this relation (mark as inactive)
    /// </summary>
    public void Terminate(DateTime endDate, Guid modifiedByUserId)
    {
        IsActive = false;
        EndDate = endDate;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Reactivate this relation
    /// </summary>
    public void Reactivate(Guid modifiedByUserId)
    {
        IsActive = true;
        EndDate = null;
        MarkAsModified(modifiedByUserId);
    }
}