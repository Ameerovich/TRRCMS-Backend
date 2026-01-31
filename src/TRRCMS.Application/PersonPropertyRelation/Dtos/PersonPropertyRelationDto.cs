using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.PersonPropertyRelations.Dtos;

/// <summary>
/// DTO for PersonPropertyRelation entity
/// </summary>
public class PersonPropertyRelationDto
{
    public Guid Id { get; set; }
    public Guid PersonId { get; set; }
    public Guid PropertyUnitId { get; set; }

    /// <summary>
    /// نوع العلاقة - Owner=1, Occupant=2, Tenant=3, Guest=4, Heir=5, Other=99
    /// </summary>
    public RelationType RelationType { get; set; }
    public string? RelationTypeOtherDesc { get; set; }

    /// <summary>
    /// نوع العقد - FullOwnership=1, SharedOwnership=2, LongTermRental=3, etc.
    /// </summary>
    public TenureContractType? ContractType { get; set; }
    public string? ContractTypeOtherDesc { get; set; }

    /// <summary>
    /// حصة الملكية - 0.0 to 1.0
    /// </summary>
    public decimal? OwnershipShare { get; set; }
    public string? ContractDetails { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }

    // Audit fields
    public DateTime CreatedAtUtc { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? LastModifiedAtUtc { get; set; }
    public Guid? LastModifiedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public Guid? DeletedBy { get; set; }

    // Computed fields
    public int? DurationInDays { get; set; }
    public bool IsOngoing { get; set; }
    public int EvidenceCount { get; set; }
}
