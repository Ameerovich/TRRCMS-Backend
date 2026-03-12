namespace TRRCMS.Application.Claims.Dtos;

/// <summary>
/// Response DTO for the composite UpdateClaim endpoint.
/// Wraps the updated ClaimDto with source relation and evidence summary.
/// </summary>
public class UpdateClaimResultDto
{
    /// <summary>
    /// The updated claim.
    /// </summary>
    public ClaimDto Claim { get; set; } = null!;

    /// <summary>
    /// ID of the source PersonPropertyRelation that backs this claim.
    /// </summary>
    public Guid SourceRelationId { get; set; }

    /// <summary>
    /// Current relation type (Owner=1, Occupant=2, Tenant=3, Guest=4, Heir=5, Other=99).
    /// </summary>
    public int RelationType { get; set; }

    /// <summary>
    /// Whether the source relation has active evidence links.
    /// </summary>
    public bool HasEvidence { get; set; }

    /// <summary>
    /// Count of active evidence links on the source relation.
    /// </summary>
    public int ActiveEvidenceLinkCount { get; set; }
}
