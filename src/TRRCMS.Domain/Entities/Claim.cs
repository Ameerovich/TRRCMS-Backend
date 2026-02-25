using TRRCMS.Domain.Common;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Domain.Entities;

/// <summary>
/// Claim entity - represents tenure rights claims
/// Core workflow entity with complete lifecycle management
/// </summary>
public class Claim : BaseAuditableEntity
{
    // ==================== IDENTIFIERS ====================

    /// <summary>
    /// Unique claim identifier (17-digit Record ID)
    /// Format: CLM-YYYY-NNNNNNNNN
    /// </summary>
    public string ClaimNumber { get; private set; }

    /// <summary>
    /// Foreign key to PropertyUnit this claim is for
    /// </summary>
    public Guid PropertyUnitId { get; private set; }

    /// <summary>
    /// Foreign key to primary claimant (Person)
    /// </summary>
    public Guid? PrimaryClaimantId { get; private set; }

    // ==================== CLAIM CLASSIFICATION ====================

    /// <summary>
    /// Claim type (controlled vocabulary)
    /// Example: "Ownership Claim", "Occupancy Claim", "Rental Claim"
    /// </summary>
    public string ClaimType { get; private set; }

    /// <summary>
    /// Claim source - how the claim entered the system
    /// </summary>
    public ClaimSource ClaimSource { get; private set; }

    /// <summary>
    /// Claim priority level
    /// </summary>
    public CasePriority Priority { get; private set; }

    // ==================== LIFECYCLE MANAGEMENT ====================

    /// <summary>
    /// Current lifecycle stage
    /// </summary>
    public LifecycleStage LifecycleStage { get; private set; }

    /// <summary>
    /// Legacy claim status (kept for backward compatibility)
    /// </summary>
    public ClaimStatus Status { get; private set; }

    /// <summary>
    /// Date when claim was submitted
    /// </summary>
    public DateTime? SubmittedDate { get; private set; }

    /// <summary>
    /// User who submitted the claim
    /// </summary>
    public Guid? SubmittedByUserId { get; private set; }

    /// <summary>
    /// Date when claim reached final decision
    /// </summary>
    public DateTime? DecisionDate { get; private set; }

    /// <summary>
    /// User who made final decision
    /// </summary>
    public Guid? DecisionByUserId { get; private set; }

    // ==================== ASSIGNMENT & WORKFLOW ====================

    /// <summary>
    /// Currently assigned case officer
    /// </summary>
    public Guid? AssignedToUserId { get; private set; }

    /// <summary>
    /// Date when assigned to current officer
    /// </summary>
    public DateTime? AssignedDate { get; private set; }

    /// <summary>
    /// Target completion/decision date
    /// </summary>
    public DateTime? TargetCompletionDate { get; private set; }

    // ==================== TENURE DETAILS ====================

    /// <summary>
    /// Type of tenure contract
    /// </summary>
    public TenureContractType? TenureContractType { get; private set; }

    /// <summary>
    /// Ownership share (if shared ownership)
    /// Format: fraction out of 2400 (e.g., 1200 = 50%)
    /// </summary>
    public int? OwnershipShare { get; private set; }

    /// <summary>
    /// Date from which tenure/occupancy started
    /// </summary>
    public DateTime? TenureStartDate { get; private set; }

    /// <summary>
    /// Date when tenure/occupancy ended (if applicable)
    /// </summary>
    public DateTime? TenureEndDate { get; private set; }

    // ==================== CLAIM DETAILS ====================

    /// <summary>
    /// Detailed description of the claim
    /// </summary>
    public string? ClaimDescription { get; private set; }

    /// <summary>
    /// Legal basis for the claim
    /// </summary>
    public string? LegalBasis { get; private set; }

    /// <summary>
    /// Supporting narrative or story
    /// </summary>
    public string? SupportingNarrative { get; private set; }

    // ==================== CONFLICT & DISPUTES ====================

    /// <summary>
    /// Indicates if there are conflicting claims
    /// </summary>
    public bool HasConflicts { get; private set; }

    /// <summary>
    /// Number of conflicting claims detected
    /// </summary>
    public int ConflictCount { get; private set; }

    /// <summary>
    /// Conflict resolution status
    /// </summary>
    public string? ConflictResolutionStatus { get; private set; }

    // ==================== EVIDENCE & DOCUMENTATION ====================

    /// <summary>
    /// Number of evidence items attached
    /// </summary>
    public int EvidenceCount { get; private set; }

    /// <summary>
    /// Indicates if all required documents are submitted
    /// </summary>
    public bool AllRequiredDocumentsSubmitted { get; private set; }

    /// <summary>
    /// List of missing document types (stored as JSON)
    /// </summary>
    public string? MissingDocuments { get; private set; }

    // ==================== REVIEW & VERIFICATION ====================

    /// <summary>
    /// Overall verification status
    /// </summary>
    public VerificationStatus VerificationStatus { get; private set; }

    /// <summary>
    /// Date when verification was completed
    /// </summary>
    public DateTime? VerificationDate { get; private set; }

    /// <summary>
    /// User who verified the claim
    /// </summary>
    public Guid? VerifiedByUserId { get; private set; }

    /// <summary>
    /// Verification notes
    /// </summary>
    public string? VerificationNotes { get; private set; }

    // ==================== DECISION & OUTCOME ====================

    /// <summary>
    /// Final decision on the claim
    /// </summary>
    public string? FinalDecision { get; private set; }

    /// <summary>
    /// Reason for approval or rejection
    /// </summary>
    public string? DecisionReason { get; private set; }

    /// <summary>
    /// Decision notes
    /// </summary>
    public string? DecisionNotes { get; private set; }

    // ==================== CERTIFICATE ====================

    /// <summary>
    /// Certificate status for this claim
    /// </summary>
    public CertificateStatus CertificateStatus { get; private set; }

    /// <summary>
    /// Foreign key to issued certificate (if any)
    /// </summary>
    public Guid? CertificateId { get; private set; }

    // ==================== NOTES & HISTORY ====================

    /// <summary>
    /// Internal processing notes
    /// </summary>
    public string? ProcessingNotes { get; private set; }

    /// <summary>
    /// Public remarks visible to claimant
    /// </summary>
    public string? PublicRemarks { get; private set; }

    // ==================== NAVIGATION PROPERTIES ====================

    /// <summary>
    /// Property unit this claim is for
    /// </summary>
    public virtual PropertyUnit PropertyUnit { get; private set; } = null!;

    /// <summary>
    /// Primary claimant person
    /// </summary>
    public virtual Person? PrimaryClaimant { get; private set; }

    /// <summary>
    /// Evidence supporting this claim
    /// </summary>
    public virtual ICollection<Evidence> Evidences { get; private set; }

    /// <summary>
    /// Documents supporting this claim
    /// </summary>
    public virtual ICollection<Document> Documents { get; private set; }

    /// <summary>
    /// Referrals for this claim
    /// </summary>
    public virtual ICollection<Referral> Referrals { get; private set; }

    /// <summary>
    /// Issued certificate (if any)
    /// </summary>
    public virtual Certificate? Certificate { get; private set; }

    // Note: AssignedToUser, SubmittedByUser, etc. would be User entities
    // public virtual User? AssignedToUser { get; private set; }
    // public virtual User? SubmittedByUser { get; private set; }
    // public virtual User? DecisionByUser { get; private set; }
    // public virtual User? VerifiedByUser { get; private set; }

    // ==================== CONSTRUCTORS ====================

    /// <summary>
    /// EF Core constructor
    /// </summary>
    private Claim() : base()
    {
        ClaimNumber = string.Empty;
        ClaimType = string.Empty;
        Status = ClaimStatus.Draft;
        LifecycleStage = LifecycleStage.DraftPendingSubmission;
        ClaimSource = ClaimSource.FieldCollection;
        Priority = CasePriority.Normal;
        VerificationStatus = VerificationStatus.Pending;
        CertificateStatus = CertificateStatus.NotApplicable;
        HasConflicts = false;
        ConflictCount = 0;
        EvidenceCount = 0;
        AllRequiredDocumentsSubmitted = false;
        Evidences = new List<Evidence>();
        Documents = new List<Document>();
        Referrals = new List<Referral>();
    }

    /// <summary>
    /// Create new claim
    /// </summary>
    public static Claim Create(
        string claimNumber,  // Sequential claim number from ClaimNumberGenerator
        Guid propertyUnitId,
        Guid? primaryClaimantId,
        string claimType,
        ClaimSource claimSource,
        Guid createdByUserId)
    {
        var claim = new Claim
        {
            ClaimNumber = claimNumber,  // Set from parameter (sequential)
            PropertyUnitId = propertyUnitId,
            PrimaryClaimantId = primaryClaimantId,
            ClaimType = claimType,
            ClaimSource = claimSource,
            Status = ClaimStatus.Draft,
            LifecycleStage = LifecycleStage.DraftPendingSubmission,
            Priority = CasePriority.Normal,
            VerificationStatus = VerificationStatus.Pending,
            CertificateStatus = CertificateStatus.NotApplicable,
            HasConflicts = false,
            ConflictCount = 0,
            EvidenceCount = 0,
            AllRequiredDocumentsSubmitted = false
        };

        claim.MarkAsCreated(createdByUserId);

        return claim;
    }

    // ==================== DOMAIN METHODS ====================

    /// <summary>
    /// Submit claim for processing
    /// </summary>
    public void Submit(Guid submittedByUserId, Guid modifiedByUserId)
    {
        Status = ClaimStatus.Finalized;
        LifecycleStage = LifecycleStage.Submitted;
        SubmittedDate = DateTime.UtcNow;
        SubmittedByUserId = submittedByUserId;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Move to lifecycle stage
    /// </summary>
    public void MoveToStage(LifecycleStage newStage, Guid modifiedByUserId)
    {
        LifecycleStage = newStage;

        // Update legacy status based on lifecycle stage
        Status = newStage switch
        {
            LifecycleStage.DraftPendingSubmission => ClaimStatus.Draft,
            LifecycleStage.Submitted => ClaimStatus.Finalized,
            LifecycleStage.InitialScreening => ClaimStatus.UnderReview,
            LifecycleStage.UnderReview => ClaimStatus.UnderReview,
            LifecycleStage.AwaitingDocuments => ClaimStatus.PendingEvidence,
            LifecycleStage.ConflictDetected => ClaimStatus.Disputed,
            LifecycleStage.InAdjudication => ClaimStatus.Disputed,
            LifecycleStage.Approved => ClaimStatus.Approved,
            LifecycleStage.Rejected => ClaimStatus.Rejected,
            LifecycleStage.Archived => ClaimStatus.Archived,
            _ => Status
        };

        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Assign to case officer
    /// </summary>
    public void AssignTo(Guid userId, DateTime? targetCompletionDate, Guid modifiedByUserId)
    {
        AssignedToUserId = userId;
        AssignedDate = DateTime.UtcNow;
        TargetCompletionDate = targetCompletionDate;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Set priority
    /// </summary>
    public void SetPriority(CasePriority priority, Guid modifiedByUserId)
    {
        Priority = priority;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Update tenure details
    /// </summary>
    public void UpdateTenureDetails(
        TenureContractType? tenureType,
        int? ownershipShare,
        DateTime? tenureStartDate,
        DateTime? tenureEndDate,
        Guid modifiedByUserId)
    {
        TenureContractType = tenureType;
        OwnershipShare = ownershipShare;
        TenureStartDate = tenureStartDate;
        TenureEndDate = tenureEndDate;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Update claim description
    /// </summary>
    public void UpdateDescription(
        string? claimDescription,
        string? legalBasis,
        string? supportingNarrative,
        Guid modifiedByUserId)
    {
        ClaimDescription = claimDescription;
        LegalBasis = legalBasis;
        SupportingNarrative = supportingNarrative;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Mark conflicts detected
    /// </summary>
    public void MarkConflictsDetected(int conflictCount, Guid modifiedByUserId)
    {
        HasConflicts = true;
        ConflictCount = conflictCount;
        ConflictResolutionStatus = "Pending";
        LifecycleStage = LifecycleStage.ConflictDetected;
        Status = ClaimStatus.Disputed;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Mark conflicts resolved
    /// </summary>
    public void MarkConflictsResolved(Guid modifiedByUserId)
    {
        ConflictResolutionStatus = "Resolved";
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Update evidence count
    /// </summary>
    public void UpdateEvidenceCount(int count, Guid modifiedByUserId)
    {
        EvidenceCount = count;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Mark missing documents
    /// </summary>
    public void MarkMissingDocuments(string missingDocumentsJson, Guid modifiedByUserId)
    {
        MissingDocuments = missingDocumentsJson;
        AllRequiredDocumentsSubmitted = false;
        LifecycleStage = LifecycleStage.AwaitingDocuments;
        Status = ClaimStatus.PendingEvidence;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Mark all required documents submitted
    /// </summary>
    public void MarkAllDocumentsSubmitted(Guid modifiedByUserId)
    {
        AllRequiredDocumentsSubmitted = true;
        MissingDocuments = null;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Verify claim
    /// </summary>
    public void Verify(
        Guid verifiedByUserId,
        string? verificationNotes,
        Guid modifiedByUserId)
    {
        VerificationStatus = VerificationStatus.Verified;
        VerificationDate = DateTime.UtcNow;
        VerifiedByUserId = verifiedByUserId;
        VerificationNotes = verificationNotes;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Approve claim
    /// </summary>
    public void Approve(
        string? decisionReason,
        string? decisionNotes,
        Guid decisionByUserId,
        Guid modifiedByUserId)
    {
        Status = ClaimStatus.Approved;
        LifecycleStage = LifecycleStage.Approved;
        FinalDecision = "Approved";
        DecisionReason = decisionReason;
        DecisionNotes = decisionNotes;
        DecisionDate = DateTime.UtcNow;
        DecisionByUserId = decisionByUserId;
        CertificateStatus = CertificateStatus.PendingGeneration;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Reject claim
    /// </summary>
    public void Reject(
        string decisionReason,
        string? decisionNotes,
        Guid decisionByUserId,
        Guid modifiedByUserId)
    {
        Status = ClaimStatus.Rejected;
        LifecycleStage = LifecycleStage.Rejected;
        FinalDecision = "Rejected";
        DecisionReason = decisionReason;
        DecisionNotes = decisionNotes;
        DecisionDate = DateTime.UtcNow;
        DecisionByUserId = decisionByUserId;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Link certificate to claim
    /// </summary>
    public void LinkCertificate(Guid certificateId, Guid modifiedByUserId)
    {
        CertificateId = certificateId;
        CertificateStatus = CertificateStatus.Issued;
        LifecycleStage = LifecycleStage.CertificateIssued;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Add processing notes
    /// </summary>
    public void AddProcessingNotes(string notes, Guid modifiedByUserId)
    {
        ProcessingNotes = string.IsNullOrWhiteSpace(ProcessingNotes)
            ? notes
            : $"{ProcessingNotes}\n{notes}";
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Add public remarks
    /// </summary>
    public void AddPublicRemarks(string remarks, Guid modifiedByUserId)
    {
        PublicRemarks = string.IsNullOrWhiteSpace(PublicRemarks)
            ? remarks
            : $"{PublicRemarks}\n{remarks}";
        MarkAsModified(modifiedByUserId);
    }


    /// <summary>
    /// Check if claim is overdue
    /// </summary>
    public bool IsOverdue()
    {
        return TargetCompletionDate.HasValue
            && !DecisionDate.HasValue
            && DateTime.UtcNow > TargetCompletionDate.Value;
    }
    /// <summary>
    /// Update primary claimant
    /// UC-006: Update Existing Claim
    /// </summary>
    public void UpdatePrimaryClaimant(Guid primaryClaimantId, Guid modifiedByUserId)
    {
        PrimaryClaimantId = primaryClaimantId;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Re-point this claim to a different property unit (used during PropertyUnit merge).
    /// </summary>
    public void UpdatePropertyUnit(Guid propertyUnitId, Guid modifiedByUserId)
    {
        PropertyUnitId = propertyUnitId;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Update claim classification (type and priority)
    /// UC-006: Update Existing Claim
    /// </summary>
    public void UpdateClassification(string claimType, CasePriority priority, Guid modifiedByUserId)
    {
        ClaimType = claimType;
        Priority = priority;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Update tenure contract details (simplified version for UpdateClaim)
    /// UC-006: Update Existing Claim
    /// </summary>
    public void UpdateTenureContract(TenureContractType tenureContractType, string? contractDetails, Guid modifiedByUserId)
    {
        TenureContractType = tenureContractType;
        // Store contract details in ClaimDescription or add a new field if needed
        if (!string.IsNullOrWhiteSpace(contractDetails))
        {
            ClaimDescription = contractDetails;
        }
        MarkAsModified(modifiedByUserId);
    }

}