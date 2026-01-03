using TRRCMS.Domain.Common;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Domain.Entities;

/// <summary>
/// Referral entity - tracks claim lifecycle referrals and redirections between roles
/// Referenced in FSD section 6.1.8: Referral & Lifecycle Management
/// Supports UC workflows for claim routing and reassignment
/// </summary>
public class Referral : BaseAuditableEntity
{
    // ==================== REFERRAL IDENTIFICATION ====================

    /// <summary>
    /// Referral number for tracking (رقم الإحالة)
    /// Format: REF-YYYY-NNNN
    /// </summary>
    public string ReferralNumber { get; private set; }

    // ==================== CLAIM RELATIONSHIP ====================

    /// <summary>
    /// Foreign key to Claim being referred
    /// </summary>
    public Guid ClaimId { get; private set; }

    // ==================== REFERRAL PARTIES ====================

    /// <summary>
    /// Role referring the claim (الجهة المحيلة)
    /// </summary>
    public ReferralRole FromRole { get; private set; }

    /// <summary>
    /// User who initiated the referral
    /// </summary>
    public Guid FromUserId { get; private set; }

    /// <summary>
    /// Role receiving the claim (الجهة المستقبلة)
    /// </summary>
    public ReferralRole ToRole { get; private set; }

    /// <summary>
    /// Specific user assigned to handle the referral (optional)
    /// If null, any user with ToRole can handle it
    /// </summary>
    public Guid? ToUserId { get; private set; }

    // ==================== REFERRAL DETAILS ====================

    /// <summary>
    /// Referral type/action
    /// Examples: "Refer Back to Field Team", "Refer to Municipality Clerk", "Escalate to Supervisor"
    /// </summary>
    public string ReferralType { get; private set; }

    /// <summary>
    /// Reason for referral (سبب الإحالة)
    /// </summary>
    public string ReferralReason { get; private set; }

    /// <summary>
    /// Detailed notes about the referral
    /// </summary>
    public string? ReferralNotes { get; private set; }

    /// <summary>
    /// Priority level (Normal, High, Urgent)
    /// </summary>
    public string Priority { get; private set; }

    /// <summary>
    /// Date when referral was created (تاريخ الإحالة)
    /// </summary>
    public DateTime ReferralDate { get; private set; }

    /// <summary>
    /// Expected response/completion date
    /// </summary>
    public DateTime? ExpectedResponseDate { get; private set; }

    // ==================== REFERRAL STATUS ====================

    /// <summary>
    /// Referral status
    /// Values: Pending, Acknowledged, InProgress, Completed, Rejected, Cancelled
    /// </summary>
    public string Status { get; private set; }

    /// <summary>
    /// Date when referral was acknowledged by recipient
    /// </summary>
    public DateTime? AcknowledgedDate { get; private set; }

    /// <summary>
    /// User who acknowledged the referral
    /// </summary>
    public Guid? AcknowledgedByUserId { get; private set; }

    /// <summary>
    /// Date when work started on the referral
    /// </summary>
    public DateTime? StartedDate { get; private set; }

    /// <summary>
    /// Date when referral was completed/resolved
    /// </summary>
    public DateTime? CompletedDate { get; private set; }

    /// <summary>
    /// Response or resolution notes
    /// </summary>
    public string? ResponseNotes { get; private set; }

    /// <summary>
    /// Outcome of the referral
    /// Examples: "Issue Resolved", "Claim Updated", "Referred Further", "Rejected"
    /// </summary>
    public string? Outcome { get; private set; }

    // ==================== ESCALATION TRACKING ====================

    /// <summary>
    /// Indicates if this is an escalation
    /// </summary>
    public bool IsEscalation { get; private set; }

    /// <summary>
    /// Reference to previous referral (if this is a follow-up or escalation)
    /// </summary>
    public Guid? PreviousReferralId { get; private set; }

    /// <summary>
    /// Level of escalation (1, 2, 3, etc.)
    /// </summary>
    public int? EscalationLevel { get; private set; }

    // ==================== ACTIONS REQUIRED ====================

    /// <summary>
    /// Specific actions requested from recipient
    /// Stored as JSON array: ["Action 1", "Action 2", ...]
    /// </summary>
    public string? ActionsRequired { get; private set; }

    /// <summary>
    /// Documents or evidence required
    /// </summary>
    public string? DocumentsRequired { get; private set; }

    // ==================== SLA TRACKING ====================

    /// <summary>
    /// Target resolution time in hours (Service Level Agreement)
    /// </summary>
    public int? TargetResolutionHours { get; private set; }

    /// <summary>
    /// Indicates if referral is overdue
    /// </summary>
    public bool IsOverdue { get; private set; }

    // ==================== NAVIGATION PROPERTIES ====================

    /// <summary>
    /// Claim being referred
    /// </summary>
    public virtual Claim Claim { get; private set; } = null!;

    /// <summary>
    /// Previous referral in the chain (if applicable)
    /// </summary>
    public virtual Referral? PreviousReferral { get; private set; }

    // Note: FromUser, ToUser, AcknowledgedByUser would be User entities (to be created)
    // public virtual User FromUser { get; private set; } = null!;
    // public virtual User? ToUser { get; private set; }
    // public virtual User? AcknowledgedByUser { get; private set; }

    // ==================== CONSTRUCTORS ====================

    /// <summary>
    /// EF Core constructor
    /// </summary>
    private Referral() : base()
    {
        ReferralNumber = string.Empty;
        ReferralType = string.Empty;
        ReferralReason = string.Empty;
        Priority = "Normal";
        Status = "Pending";
        IsEscalation = false;
        IsOverdue = false;
    }

    /// <summary>
    /// Create new referral
    /// </summary>
    public static Referral Create(
        Guid claimId,
        ReferralRole fromRole,
        Guid fromUserId,
        ReferralRole toRole,
        string referralType,
        string referralReason,
        string? referralNotes,
        string priority,
        DateTime? expectedResponseDate,
        int? targetResolutionHours,
        Guid createdByUserId)
    {
        var referral = new Referral
        {
            ClaimId = claimId,
            FromRole = fromRole,
            FromUserId = fromUserId,
            ToRole = toRole,
            ReferralType = referralType,
            ReferralReason = referralReason,
            ReferralNotes = referralNotes,
            Priority = priority,
            ReferralDate = DateTime.UtcNow,
            ExpectedResponseDate = expectedResponseDate,
            TargetResolutionHours = targetResolutionHours,
            Status = "Pending",
            IsEscalation = false,
            IsOverdue = false
        };

        // Generate referral number
        referral.ReferralNumber = GenerateReferralNumber();

        referral.MarkAsCreated(createdByUserId);

        return referral;
    }

    /// <summary>
    /// Create escalation referral
    /// </summary>
    public static Referral CreateEscalation(
        Guid claimId,
        ReferralRole fromRole,
        Guid fromUserId,
        ReferralRole toRole,
        string escalationReason,
        Guid previousReferralId,
        int escalationLevel,
        Guid createdByUserId)
    {
        var referral = new Referral
        {
            ClaimId = claimId,
            FromRole = fromRole,
            FromUserId = fromUserId,
            ToRole = toRole,
            ReferralType = "Escalation",
            ReferralReason = escalationReason,
            Priority = "High", // Escalations are high priority
            ReferralDate = DateTime.UtcNow,
            Status = "Pending",
            IsEscalation = true,
            PreviousReferralId = previousReferralId,
            EscalationLevel = escalationLevel,
            IsOverdue = false
        };

        referral.ReferralNumber = GenerateReferralNumber();
        referral.MarkAsCreated(createdByUserId);

        return referral;
    }

    // ==================== DOMAIN METHODS ====================

    /// <summary>
    /// Acknowledge referral receipt
    /// </summary>
    public void Acknowledge(Guid acknowledgedByUserId, Guid modifiedByUserId)
    {
        Status = "Acknowledged";
        AcknowledgedDate = DateTime.UtcNow;
        AcknowledgedByUserId = acknowledgedByUserId;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Start working on referral
    /// </summary>
    public void Start(Guid modifiedByUserId)
    {
        Status = "InProgress";
        StartedDate = DateTime.UtcNow;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Complete referral with outcome
    /// </summary>
    public void Complete(string outcome, string? responseNotes, Guid modifiedByUserId)
    {
        Status = "Completed";
        CompletedDate = DateTime.UtcNow;
        Outcome = outcome;
        ResponseNotes = responseNotes;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Reject referral
    /// </summary>
    public void Reject(string rejectionReason, Guid modifiedByUserId)
    {
        Status = "Rejected";
        CompletedDate = DateTime.UtcNow;
        Outcome = "Rejected";
        ResponseNotes = rejectionReason;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Cancel referral
    /// </summary>
    public void Cancel(string cancellationReason, Guid modifiedByUserId)
    {
        Status = "Cancelled";
        CompletedDate = DateTime.UtcNow;
        ResponseNotes = cancellationReason;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Assign to specific user
    /// </summary>
    public void AssignToUser(Guid userId, Guid modifiedByUserId)
    {
        ToUserId = userId;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Update priority
    /// </summary>
    public void UpdatePriority(string newPriority, Guid modifiedByUserId)
    {
        Priority = newPriority;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Set actions required
    /// </summary>
    public void SetActionsRequired(string actionsRequired, string? documentsRequired, Guid modifiedByUserId)
    {
        ActionsRequired = actionsRequired;
        DocumentsRequired = documentsRequired;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Mark as overdue
    /// </summary>
    public void MarkAsOverdue()
    {
        IsOverdue = true;
    }

    /// <summary>
    /// Check if referral is currently overdue
    /// </summary>
    public bool CheckIfOverdue()
    {
        if (ExpectedResponseDate.HasValue && Status != "Completed" && Status != "Cancelled")
        {
            return DateTime.UtcNow > ExpectedResponseDate.Value;
        }

        if (TargetResolutionHours.HasValue && ReferralDate != default && Status != "Completed" && Status != "Cancelled")
        {
            var targetDate = ReferralDate.AddHours(TargetResolutionHours.Value);
            return DateTime.UtcNow > targetDate;
        }

        return false;
    }

    /// <summary>
    /// Calculate time elapsed since referral
    /// </summary>
    public TimeSpan GetElapsedTime()
    {
        var endDate = CompletedDate ?? DateTime.UtcNow;
        return endDate - ReferralDate;
    }

    // ==================== HELPER METHODS ====================

    /// <summary>
    /// Generate unique referral number
    /// Format: REF-YYYY-NNNN
    /// </summary>
    private static string GenerateReferralNumber()
    {
        var year = DateTime.UtcNow.Year;
        var random = new Random();
        var sequence = random.Next(1000, 9999);
        return $"REF-{year}-{sequence:D4}";
    }
}