using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Claims.Dtos;

/// <summary>
/// Data Transfer Object for Claim entity
/// Includes all entity properties plus computed properties for UI/API consumption
/// </summary>
public class ClaimDto
{
    // ==================== IDENTIFIERS ====================
    
    public Guid Id { get; set; }
    public string ClaimNumber { get; set; } = string.Empty;
    public Guid PropertyUnitId { get; set; }
    public Guid? PrimaryClaimantId { get; set; }
    
    // ==================== CLAIM CLASSIFICATION ====================
    
    public string ClaimType { get; set; } = string.Empty;
    public ClaimSource ClaimSource { get; set; }
    public CasePriority Priority { get; set; }
    
    // ==================== LIFECYCLE MANAGEMENT ====================
    
    public LifecycleStage LifecycleStage { get; set; }
    public ClaimStatus Status { get; set; }
    public DateTime? SubmittedDate { get; set; }
    public Guid? SubmittedByUserId { get; set; }
    public DateTime? DecisionDate { get; set; }
    public Guid? DecisionByUserId { get; set; }
    
    // ==================== ASSIGNMENT & WORKFLOW ====================
    
    public Guid? AssignedToUserId { get; set; }
    public DateTime? AssignedDate { get; set; }
    public DateTime? TargetCompletionDate { get; set; }
    
    // ==================== TENURE DETAILS ====================
    
    public TenureContractType? TenureContractType { get; set; }
    public int? OwnershipShare { get; set; }
    public DateTime? TenureStartDate { get; set; }
    public DateTime? TenureEndDate { get; set; }
    
    // ==================== CLAIM DETAILS ====================
    
    public string? ClaimDescription { get; set; }
    public string? LegalBasis { get; set; }
    public string? SupportingNarrative { get; set; }
    
    // ==================== CONFLICT & DISPUTES ====================
    
    public bool HasConflicts { get; set; }
    public int ConflictCount { get; set; }
    public string? ConflictResolutionStatus { get; set; }
    
    // ==================== EVIDENCE & DOCUMENTATION ====================
    
    public int EvidenceCount { get; set; }
    public bool AllRequiredDocumentsSubmitted { get; set; }
    public string? MissingDocuments { get; set; }
    
    // ==================== REVIEW & VERIFICATION ====================
    
    public VerificationStatus VerificationStatus { get; set; }
    public DateTime? VerificationDate { get; set; }
    public Guid? VerifiedByUserId { get; set; }
    public string? VerificationNotes { get; set; }
    
    // ==================== DECISION & OUTCOME ====================
    
    public string? FinalDecision { get; set; }
    public string? DecisionReason { get; set; }
    public string? DecisionNotes { get; set; }
    
    // ==================== CERTIFICATE ====================
    
    public CertificateStatus CertificateStatus { get; set; }
    
    // ==================== NOTES & HISTORY ====================
    
    public string? ProcessingNotes { get; set; }
    public string? PublicRemarks { get; set; }
    
    // ==================== AUDIT FIELDS ====================
    
    public DateTime CreatedAtUtc { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? LastModifiedAtUtc { get; set; }
    public Guid? LastModifiedBy { get; set; }
    
    // ==================== COMPUTED PROPERTIES ====================
    
    /// <summary>
    /// Indicates if the claim is overdue (past target completion date without decision)
    /// </summary>
    public bool IsOverdue { get; set; }
    
    /// <summary>
    /// Number of days until deadline (negative if overdue)
    /// </summary>
    public int? DaysUntilDeadline { get; set; }
    
    /// <summary>
    /// Number of days since submission
    /// </summary>
    public int? DaysSinceSubmission { get; set; }
    
    /// <summary>
    /// Indicates if claim has any evidence attached
    /// </summary>
    public bool HasEvidence { get; set; }
    
    /// <summary>
    /// Indicates if verification is pending
    /// </summary>
    public bool IsPendingVerification { get; set; }
    
    /// <summary>
    /// Indicates if claim requires immediate action
    /// </summary>
    public bool RequiresAction { get; set; }
    
    // ==================== NAVIGATION PROPERTIES (OPTIONAL) ====================
    
    /// <summary>
    /// Property unit basic info (optional for list views)
    /// </summary>
    public string? PropertyUnitCode { get; set; }
    
    /// <summary>
    /// Primary claimant name (optional for list views)
    /// </summary>
    public string? PrimaryClaimantName { get; set; }
    
    /// <summary>
    /// Assigned officer name (optional for list views)
    /// </summary>
    public string? AssignedToUserName { get; set; }
}
