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
    [ArabicLabel("غير قابل للتطبيق")]
    NotApplicable = 0,

    /// <summary>
    /// Pending generation - Certificate not yet generated (قيد الانتظار)
    /// </summary>
    [ArabicLabel("قيد الانتظار")]
    PendingGeneration = 1,

    /// <summary>
    /// Generating - Certificate is being generated (قيد الإنشاء)
    /// </summary>
    [ArabicLabel("قيد الإنشاء")]
    Generating = 2,

    /// <summary>
    /// Generated - Certificate created but not issued (منشأ)
    /// </summary>
    [ArabicLabel("منشأ")]
    Generated = 3,

    /// <summary>
    /// Pending approval - Certificate awaiting approval before issuance (بانتظار الموافقة)
    /// </summary>
    [ArabicLabel("بانتظار الموافقة")]
    PendingApproval = 4,

    /// <summary>
    /// Approved for issuance - Certificate approved, ready to issue (موافق عليه للإصدار)
    /// </summary>
    [ArabicLabel("موافق عليه للإصدار")]
    ApprovedForIssuance = 5,

    /// <summary>
    /// Issued - Certificate issued to beneficiary (صادر)
    /// </summary>
    [ArabicLabel("صادر")]
    Issued = 6,

    /// <summary>
    /// Collected - Certificate collected by beneficiary (مستلم)
    /// </summary>
    [ArabicLabel("مستلم")]
    Collected = 7,

    /// <summary>
    /// Pending collection - Certificate ready but not collected (بانتظار الاستلام)
    /// </summary>
    [ArabicLabel("بانتظار الاستلام")]
    PendingCollection = 8,

    /// <summary>
    /// Voided - Certificate cancelled/invalidated (ملغى)
    /// </summary>
    [ArabicLabel("ملغى")]
    Voided = 9,

    /// <summary>
    /// Reissued - Certificate was reissued due to errors or loss (معاد إصداره)
    /// </summary>
    [ArabicLabel("معاد إصداره")]
    Reissued = 10,

    /// <summary>
    /// Expired - Certificate validity period expired (منتهي الصلاحية)
    /// </summary>
    [ArabicLabel("منتهي الصلاحية")]
    Expired = 11,

    /// <summary>
    /// On hold - Certificate issuance suspended (معلق)
    /// </summary>
    [ArabicLabel("معلق")]
    OnHold = 12
}