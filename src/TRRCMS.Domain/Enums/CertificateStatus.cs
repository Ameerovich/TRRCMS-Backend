namespace TRRCMS.Domain.Enums;

/// <summary>
/// Certificate status for tenure rights certificate issuance
/// Tracks the lifecycle of certificate generation and issuance
/// Referenced in FSD section 6.1.8: Lifecycle Management - Certificate Issued stage
/// </summary>
public enum CertificateStatus
{
    /// <summary>
    /// Not applicable - Certificate not required for this case (غير قابل للتطبيق)
    /// </summary>
    NotApplicable = 0,

    /// <summary>
    /// Pending generation - Certificate not yet generated (قيد الانتظار)
    /// </summary>
    PendingGeneration = 1,

    /// <summary>
    /// Generating - Certificate is being generated (قيد الإنشاء)
    /// </summary>
    Generating = 2,

    /// <summary>
    /// Generated - Certificate created but not issued (منشأ)
    /// </summary>
    Generated = 3,

    /// <summary>
    /// Pending approval - Certificate awaiting approval before issuance (بانتظار الموافقة)
    /// </summary>
    PendingApproval = 4,

    /// <summary>
    /// Approved for issuance - Certificate approved, ready to issue (موافق عليه للإصدار)
    /// </summary>
    ApprovedForIssuance = 5,

    /// <summary>
    /// Issued - Certificate issued to beneficiary (صادر)
    /// </summary>
    Issued = 6,

    /// <summary>
    /// Collected - Certificate collected by beneficiary (مستلم)
    /// </summary>
    Collected = 7,

    /// <summary>
    /// Pending collection - Certificate ready but not collected (بانتظار الاستلام)
    /// </summary>
    PendingCollection = 8,

    /// <summary>
    /// Voided - Certificate cancelled/invalidated (ملغى)
    /// </summary>
    Voided = 9,

    /// <summary>
    /// Reissued - Certificate was reissued due to errors or loss (معاد إصداره)
    /// </summary>
    Reissued = 10,

    /// <summary>
    /// Expired - Certificate validity period expired (منتهي الصلاحية)
    /// </summary>
    Expired = 11,

    /// <summary>
    /// On hold - Certificate issuance suspended (معلق)
    /// </summary>
    OnHold = 12
}