using TRRCMS.Application.Common.Mappings;

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
    /// نوع العلاقة - Returned as integer: Owner=1, Occupant=2, Tenant=3, Guest=4, Heir=5, Other=99
    /// </summary>
    public int RelationType { get; set; }

    /// <summary>
    /// نوع الادعاء - claim type derived from <see cref="RelationType"/>: OwnershipClaim=1 (Owner, Heir),
    /// OccupancyClaim=2 (every other relation type, including admin-added ones).
    /// Bind the desktop "نوع الادعاء" dropdown to this field and the claim_type vocabulary (2 options);
    /// send it back as ClaimType on link/update.
    /// </summary>
    public int ClaimType => (int)ClaimTypeRelationMapper.ToClaimType((Domain.Enums.RelationType)RelationType);

    // ==================== NEW FIELDS FOR OFFICE SURVEY ====================

    /// <summary>
    /// نوع الإشغال - Returned as integer: OwnerOccupied=1, TenantOccupied=2, FamilyOccupied=3, etc.
    /// </summary>
    public int? OccupancyType { get; set; }

    /// <summary>
    /// هل يوجد دليل؟ - Indicates if evidence documents are available
    /// </summary>
    public bool HasEvidence { get; set; }

    // ==================== OTHER FIELDS ====================

    /// <summary>
    /// حصة الملكية - 0.0 to 1.0
    /// </summary>
    public decimal? OwnershipShare { get; set; }
    public string? ContractDetails { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }

    /// <summary>
    /// جهة الاتصال - true when this relation's person is the survey's contact point.
    /// Only populated by survey-scoped endpoints; the person is always a Rights Claimant (has this relation).
    /// </summary>
    public bool IsContactPoint { get; set; }

    // Audit fields
    public DateTime CreatedAtUtc { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? LastModifiedAtUtc { get; set; }
    public Guid? LastModifiedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public Guid? DeletedBy { get; set; }

    // Computed fields
    public bool IsOngoing { get; set; }
    public int EvidenceCount { get; set; }
}
