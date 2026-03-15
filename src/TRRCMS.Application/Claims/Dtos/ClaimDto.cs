namespace TRRCMS.Application.Claims.Dtos;

/// <summary>
/// Data Transfer Object for Claim entity
/// </summary>
public class ClaimDto
{
    // ==================== IDENTIFIERS ====================

    public Guid Id { get; set; }
    public string ClaimNumber { get; set; } = string.Empty;
    public Guid PropertyUnitId { get; set; }
    public Guid? PrimaryClaimantId { get; set; }
    public Guid? OriginatingSurveyId { get; set; }

    // ==================== CLAIM CLASSIFICATION ====================

    public int ClaimType { get; set; }
    public int ClaimSource { get; set; }

    // ==================== STATUS & SUBMISSION ====================

    public int CaseStatus { get; set; }
    public DateTime? SubmittedDate { get; set; }
    public Guid? SubmittedByUserId { get; set; }

    // ==================== TENURE DETAILS ====================

    public int? TenureContractType { get; set; }
    public int? OwnershipShare { get; set; }

    // ==================== CLAIM DETAILS ====================

    public string? ClaimDescription { get; set; }

    // ==================== EVIDENCE ====================

    public bool HasEvidence { get; set; }

    /// <summary>
    /// IDs of evidence documents linked to the source PersonPropertyRelation
    /// </summary>
    public List<Guid> EvidenceIds { get; set; } = new();

    /// <summary>
    /// The PersonPropertyRelation that produced this claim
    /// </summary>
    public Guid? SourceRelationId { get; set; }

    // ==================== AUDIT FIELDS ====================

    public DateTime CreatedAtUtc { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? LastModifiedAtUtc { get; set; }
    public Guid? LastModifiedBy { get; set; }

    // ==================== NAVIGATION PROPERTIES (OPTIONAL) ====================

    /// <summary>
    /// Property unit basic info (optional for list views)
    /// </summary>
    public string? PropertyUnitCode { get; set; }

    /// <summary>
    /// Primary claimant name (optional for list views)
    /// </summary>
    public string? PrimaryClaimantName { get; set; }
}
