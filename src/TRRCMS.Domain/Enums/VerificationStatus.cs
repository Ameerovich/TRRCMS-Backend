namespace TRRCMS.Domain.Enums;

/// <summary>
/// Verification status for evidence, documents, and claims
/// Used throughout the system for validation workflows
/// </summary>
public enum VerificationStatus
{
    /// <summary>
    /// Pending verification - not yet reviewed (قيد الانتظار)
    /// </summary>
    [ArabicLabel("قيد الانتظار")]
    Pending = 1,

    /// <summary>
    /// Under review - currently being verified (قيد المراجعة)
    /// </summary>
    [ArabicLabel("قيد المراجعة")]
    UnderReview = 2,

    /// <summary>
    /// Verified - validated and accepted (موثق)
    /// </summary>
    [ArabicLabel("موثق")]
    Verified = 3,

    /// <summary>
    /// Rejected - not valid or rejected (مرفوض)
    /// </summary>
    [ArabicLabel("مرفوض")]
    Rejected = 4,

    /// <summary>
    /// Requires additional information (يتطلب معلومات إضافية)
    /// </summary>
    [ArabicLabel("يتطلب معلومات إضافية")]
    RequiresAdditionalInfo = 5,

    /// <summary>
    /// Expired - document or evidence has expired (منتهي الصلاحية)
    /// </summary>
    [ArabicLabel("منتهي الصلاحية")]
    Expired = 6
}