using TRRCMS.Domain.Common;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Domain.Entities;

/// <summary>
/// Claim entity - represents tenure rights claims
/// Created via office survey processing or .uhc import pipeline
/// </summary>
public class Claim : BaseAuditableEntity
{
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
    /// Foreign key to primary claimant (Person) — required
    /// </summary>
    public Guid PrimaryClaimantId { get; private set; }

    /// <summary>
    /// Foreign key to the Survey that originated this claim (optional).
    /// Set during office survey finalization or .uhc import commit.
    /// </summary>
    public Guid? OriginatingSurveyId { get; private set; }

    /// <summary>
    /// Foreign key to the Case this claim belongs to (set automatically)
    /// </summary>
    public Guid? CaseId { get; private set; }
    /// <summary>
    /// Claim type classification: OwnershipClaim=1, OccupancyClaim=2 (نوع المطالبة)
    /// </summary>
    public ClaimType ClaimType { get; private set; }

    /// <summary>
    /// Claim source - how the claim entered the system
    /// </summary>
    public ClaimSource ClaimSource { get; private set; }
    /// <summary>
    /// Case status — Open (non-owner claim) or Closed (ownership/heir claim).
    /// Determined by RelationType of the generating PersonPropertyRelation.
    /// </summary>
    public CaseStatus CaseStatus { get; private set; }

    /// <summary>
    /// Date when claim was submitted
    /// </summary>
    public DateTime? SubmittedDate { get; private set; }

    /// <summary>
    /// User who submitted the claim
    /// </summary>
    public Guid? SubmittedByUserId { get; private set; }
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
    /// Detailed description of the claim
    /// </summary>
    public string? ClaimDescription { get; private set; }
    /// <summary>
    /// Property unit this claim is for
    /// </summary>
    public virtual PropertyUnit PropertyUnit { get; private set; } = null!;

    /// <summary>
    /// Primary claimant person
    /// </summary>
    public virtual Person PrimaryClaimant { get; private set; } = null!;

    /// <summary>
    /// Case this claim belongs to
    /// </summary>
    public virtual Case? Case { get; private set; }

    /// <summary>
    /// Evidence supporting this claim
    /// </summary>
    public virtual ICollection<Evidence> Evidences { get; private set; }
    /// <summary>
    /// EF Core constructor
    /// </summary>
    private Claim() : base()
    {
        ClaimNumber = string.Empty;
        ClaimType = ClaimType.OwnershipClaim;
        CaseStatus = CaseStatus.Open;
        ClaimSource = ClaimSource.FieldCollection;
        Evidences = new List<Evidence>();
    }

    /// <summary>
    /// Create new claim
    /// </summary>
    public static Claim Create(
        string claimNumber,  // Sequential claim number from ClaimNumberGenerator
        Guid propertyUnitId,
        Guid primaryClaimantId,
        ClaimType claimType,
        ClaimSource claimSource,
        Guid createdByUserId,
        Guid? originatingSurveyId = null)
    {
        var claim = new Claim
        {
            ClaimNumber = claimNumber,
            PropertyUnitId = propertyUnitId,
            PrimaryClaimantId = primaryClaimantId,
            ClaimType = claimType,
            ClaimSource = claimSource,
            CaseStatus = CaseStatus.Open,
            OriginatingSurveyId = originatingSurveyId
        };

        claim.MarkAsCreated(createdByUserId);

        return claim;
    }
    /// <summary>
    /// Submit claim for processing (sets submission timestamp)
    /// </summary>
    public void Submit(Guid submittedByUserId, Guid modifiedByUserId)
    {
        SubmittedDate = DateTime.UtcNow;
        SubmittedByUserId = submittedByUserId;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Update primary claimant
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
    /// Update claim type classification
    /// </summary>
    public void UpdateClassification(ClaimType claimType, Guid modifiedByUserId)
    {
        ClaimType = claimType;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Link claim to a case
    /// </summary>
    public void LinkToCase(Guid caseId, Guid modifiedByUserId)
    {
        if (CaseId.HasValue)
            return;

        CaseId = caseId;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Close the case — ownership/heir claim registered.
    /// </summary>
    public void CloseCase(Guid modifiedByUserId)
    {
        CaseStatus = CaseStatus.Closed;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Reopen the case — revert to Open status.
    /// </summary>
    public void ReopenCase(Guid modifiedByUserId)
    {
        CaseStatus = CaseStatus.Open;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Re-derive ClaimType and CaseStatus from the source relation's RelationType.
    /// Mirrors the creation logic: Owner/Heir → OwnershipClaim + Closed, others → OccupancyClaim + Open.
    /// </summary>
    public void DeriveStateFromRelation(RelationType relationType, Guid modifiedByUserId)
    {
        var isOwnership = relationType == RelationType.Owner || relationType == RelationType.Heir;
        var newClaimType = isOwnership ? ClaimType.OwnershipClaim : ClaimType.OccupancyClaim;
        var newCaseStatus = isOwnership ? CaseStatus.Closed : CaseStatus.Open;

        if (ClaimType != newClaimType)
            ClaimType = newClaimType;

        if (CaseStatus != newCaseStatus)
            CaseStatus = newCaseStatus;

        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Update tenure contract details
    /// </summary>
    public void UpdateTenureContract(TenureContractType tenureContractType, string? contractDetails, Guid modifiedByUserId)
    {
        TenureContractType = tenureContractType;
        if (!string.IsNullOrWhiteSpace(contractDetails))
        {
            ClaimDescription = contractDetails;
        }
        MarkAsModified(modifiedByUserId);
    }

    public void UpdateOwnershipShare(int? ownershipShare, Guid modifiedByUserId)
    {
        OwnershipShare = ownershipShare;
        MarkAsModified(modifiedByUserId);
    }

    public void UpdateClaimDescription(string? claimDescription, Guid modifiedByUserId)
    {
        ClaimDescription = claimDescription;
        MarkAsModified(modifiedByUserId);
    }

}
