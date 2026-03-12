using MediatR;
using TRRCMS.Application.Claims.Dtos;

namespace TRRCMS.Application.Claims.Commands.UpdateClaim;

/// <summary>
/// Composite update: modifies the source PersonPropertyRelation + manages evidence links,
/// then re-derives claim state (ClaimType, CaseStatus) from the updated relation.
/// The claim keeps its ID and ClaimNumber.
/// </summary>
public class UpdateClaimCommand : IRequest<UpdateClaimResultDto>
{
    public Guid ClaimId { get; set; }

    // ==================== SOURCE RELATION FIELDS (partial update) ====================

    /// <summary>
    /// Relation type: Owner=1, Occupant=2, Tenant=3, Guest=4, Heir=5, Other=99.
    /// Changing this auto-derives ClaimType and CaseStatus.
    /// </summary>
    public int? RelationType { get; set; }

    /// <summary>
    /// Occupancy type for non-owner relations.
    /// </summary>
    public int? OccupancyType { get; set; }

    /// <summary>
    /// Ownership share (fraction out of 2400, e.g., 1200 = 50%).
    /// </summary>
    public decimal? OwnershipShare { get; set; }

    /// <summary>
    /// Contract details text.
    /// </summary>
    public string? ContractDetails { get; set; }

    /// <summary>
    /// Free-text notes on the relation.
    /// </summary>
    public string? Notes { get; set; }

    // Clear flags for nullable relation fields
    public bool ClearOccupancyType { get; set; }
    public bool ClearOwnershipShare { get; set; }
    public bool ClearContractDetails { get; set; }
    public bool ClearNotes { get; set; }

    // ==================== CLAIM-LEVEL FIELDS (directly set) ====================

    /// <summary>
    /// Tenure contract type (claim-level legal classification).
    /// </summary>
    public int? TenureContractType { get; set; }

    /// <summary>
    /// Tenure contract details text.
    /// </summary>
    public string? TenureContractDetails { get; set; }

    // ==================== EVIDENCE OPERATIONS ====================

    /// <summary>
    /// Create new evidence records and link them to the source relation.
    /// </summary>
    public List<CreateAndLinkEvidenceDto>? NewEvidence { get; set; }

    /// <summary>
    /// IDs of existing Evidence records to link to the source relation.
    /// </summary>
    public List<Guid>? LinkExistingEvidenceIds { get; set; }

    /// <summary>
    /// IDs of EvidenceRelation records to deactivate (unlink from relation).
    /// </summary>
    public List<Guid>? UnlinkEvidenceRelationIds { get; set; }

    // ==================== AUDIT ====================

    /// <summary>
    /// Mandatory reason for modification (audit requirement, 10-500 chars).
    /// </summary>
    public string ReasonForModification { get; set; } = string.Empty;
}
