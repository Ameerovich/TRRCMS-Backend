using TRRCMS.Domain.Common;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Domain.Entities.Staging;

/// <summary>
/// Staging entity for PersonPropertyRelation records from .uhc packages.
/// Mirrors the <see cref="PersonPropertyRelation"/> production entity in an isolated staging area.
/// Subject to cross-entity relation validation (FR-D-4 Level 2) and
/// ownership evidence validation (FR-D-4 Level 3):
/// - OriginalPersonId must exist in StagingPersons for the same package
/// - OriginalPropertyUnitId must exist in StagingPropertyUnits for the same package
/// - Ownership relations should have supporting StagingEvidence records
/// 
/// Referenced in UC-003 Stage 2 (S13).
/// </summary>
public class StagingPersonPropertyRelation : BaseStagingEntity
{
    // ==================== RELATIONSHIPS (original UUIDs from .uhc) ====================

    /// <summary>
    /// Original Person UUID from .uhc — not a FK to production Persons.
    /// Must exist in StagingPersons for the same ImportPackageId.
    /// </summary>
    public Guid OriginalPersonId { get; private set; }

    /// <summary>
    /// Original PropertyUnit UUID from .uhc — not a FK to production PropertyUnits.
    /// Must exist in StagingPropertyUnits for the same ImportPackageId.
    /// </summary>
    public Guid OriginalPropertyUnitId { get; private set; }

    // ==================== RELATION DETAILS ====================

    /// <summary>Type of person-property relation (Owner, Tenant, Occupant, etc.).</summary>
    public RelationType RelationType { get; private set; }

    /// <summary>Description when RelationType is "Other".</summary>
    public string? RelationTypeOtherDesc { get; private set; }

    /// <summary>Type of tenure/contract.</summary>
    public TenureContractType? ContractType { get; private set; }

    /// <summary>Description when ContractType is "Other".</summary>
    public string? ContractTypeOtherDesc { get; private set; }

    /// <summary>Ownership share percentage (0-100).</summary>
    public decimal? OwnershipShare { get; private set; }

    /// <summary>Additional contract or arrangement details.</summary>
    public string? ContractDetails { get; private set; }

    /// <summary>Start date of the relation/contract — from command, optional.</summary>
    public DateTime? StartDate { get; private set; }

    /// <summary>End date of the relation/contract — from command, optional.</summary>
    public DateTime? EndDate { get; private set; }

    /// <summary>Additional notes about the relation.</summary>
    public string? Notes { get; private set; }

    /// <summary>Whether this relation is currently active.</summary>
    public bool IsActive { get; private set; }

    // ==================== CONSTRUCTORS ====================

    /// <summary>EF Core constructor.</summary>
    private StagingPersonPropertyRelation() : base()
    {
        IsActive = true;
    }

    // ==================== FACTORY METHOD ====================

    /// <summary>
    /// Create a new StagingPersonPropertyRelation record from .uhc package data.
    /// </summary>
    public static StagingPersonPropertyRelation Create(
        Guid importPackageId,
        Guid originalEntityId,
        Guid originalPersonId,
        Guid originalPropertyUnitId,
        RelationType relationType,
        // --- optional: from command ---
        string? relationTypeOtherDesc = null,
        TenureContractType? contractType = null,
        string? contractTypeOtherDesc = null,
        decimal? ownershipShare = null,
        string? contractDetails = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? notes = null,
        bool isActive = true)
    {
        var entity = new StagingPersonPropertyRelation
        {
            OriginalPersonId = originalPersonId,
            OriginalPropertyUnitId = originalPropertyUnitId,
            RelationType = relationType,
            RelationTypeOtherDesc = relationTypeOtherDesc,
            ContractType = contractType,
            ContractTypeOtherDesc = contractTypeOtherDesc,
            OwnershipShare = ownershipShare,
            ContractDetails = contractDetails,
            StartDate = startDate,
            EndDate = endDate,
            Notes = notes,
            IsActive = isActive
        };

        entity.InitializeStagingMetadata(importPackageId, originalEntityId);
        return entity;
    }
}
