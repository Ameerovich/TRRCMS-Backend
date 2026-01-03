namespace TRRCMS.Domain.Enums;

/// <summary>
/// Claim lifecycle stage classification (مسار حياة الحالة)
/// Complete workflow stages for claim processing
/// Referenced in FSD section 6.1.8 and throughout the document
/// </summary>
public enum LifecycleStage
{
    /// <summary>
    /// Draft / Pending Submission - Claim created but not submitted (مسودة / قيد الإعداد)
    /// Case created on tablet or office but not yet submitted
    /// </summary>
    DraftPendingSubmission = 1,

    /// <summary>
    /// Submitted - Claim sent to municipality office (مُقدّم)
    /// Case sent to municipality office for initial verification
    /// </summary>
    Submitted = 2,

    /// <summary>
    /// Initial Screening - Municipality clerk checks completeness (التدقيق الأولي)
    /// Municipality clerk checks completeness, may redirect back to field team
    /// </summary>
    InitialScreening = 3,

    /// <summary>
    /// Under Review - Case officer examining evidence (قيد المراجعة)
    /// Case officer examines evidence, verifies data, and prepares file
    /// </summary>
    UnderReview = 4,

    /// <summary>
    /// Awaiting Documents - Waiting for missing evidence (في انتظار الوثائق)
    /// System places case on hold pending missing evidence
    /// </summary>
    AwaitingDocuments = 5,

    /// <summary>
    /// Conflict Detected - Multiple claims on same unit (تعارض مُكتشف)
    /// System auto-detects multiple claims on same unit → routed to Adjudication
    /// </summary>
    ConflictDetected = 6,

    /// <summary>
    /// In Adjudication - Conflict resolution in progress (قيد التحكيم)
    /// Multiple conflicting claims being resolved
    /// </summary>
    InAdjudication = 7,

    /// <summary>
    /// Pending Approval - Ready for final approval (بانتظار الموافقة)
    /// All requirements met, awaiting manager approval
    /// </summary>
    PendingApproval = 8,

    /// <summary>
    /// Approved - Claim approved (موافق عليه)
    /// Claim has been validated and approved
    /// </summary>
    Approved = 9,

    /// <summary>
    /// Rejected - Claim rejected (مرفوض)
    /// Claim was reviewed and rejected
    /// </summary>
    Rejected = 10,

    /// <summary>
    /// On Hold - Temporarily suspended (معلق)
    /// Processing temporarily paused
    /// </summary>
    OnHold = 11,

    /// <summary>
    /// Reassigned - Transferred to another team/officer (مُعاد تعيينه)
    /// Claim reassigned for processing
    /// </summary>
    Reassigned = 12,

    /// <summary>
    /// Certificate Issued - Final certificate issued (شهادة صادرة)
    /// Final tenure certificate has been issued
    /// </summary>
    CertificateIssued = 13,

    /// <summary>
    /// Archived - Closed and archived (مؤرشف)
    /// Claim processing complete and archived
    /// </summary>
    Archived = 99
}