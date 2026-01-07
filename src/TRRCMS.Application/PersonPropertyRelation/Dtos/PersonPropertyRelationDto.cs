namespace TRRCMS.Application.PersonPropertyRelations.Dtos;

/// <summary>
/// Data transfer object for PersonPropertyRelation entity
/// </summary>
public class PersonPropertyRelationDto
{
    // ==================== IDENTIFIERS ====================

    public Guid Id { get; set; }
    public Guid PersonId { get; set; }
    public Guid PropertyUnitId { get; set; }

    // ==================== RELATION ATTRIBUTES ====================

    /// <summary>
    /// Relation type (owner, tenant, occupant, guest, heir, other, etc.)
    /// </summary>
    public string RelationType { get; set; } = string.Empty;

    /// <summary>
    /// Description in case the chosen type is "Other"
    /// </summary>
    public string? RelationTypeOtherDesc { get; set; }

    /// <summary>
    /// Ownership or occupancy share (percentage, e.g., 0.5 for 50%)
    /// </summary>
    public decimal? OwnershipShare { get; set; }

    /// <summary>
    /// Contract or agreement details
    /// </summary>
    public string? ContractDetails { get; set; }

    /// <summary>
    /// Start date of the relation
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// End date of the relation (for terminated relations)
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Additional notes
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Indicates if this relation is currently active
    /// </summary>
    public bool IsActive { get; set; }

    // ==================== AUDIT FIELDS ====================

    public DateTime CreatedAtUtc { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? LastModifiedAtUtc { get; set; }
    public Guid? LastModifiedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public Guid? DeletedBy { get; set; }

    // ==================== COMPUTED PROPERTIES ====================

    /// <summary>
    /// Duration of the relation in days (if both StartDate and EndDate exist)
    /// </summary>
    public int? DurationInDays { get; set; }

    /// <summary>
    /// Indicates if the relation is currently ongoing (StartDate exists but no EndDate)
    /// </summary>
    public bool IsOngoing { get; set; }
}