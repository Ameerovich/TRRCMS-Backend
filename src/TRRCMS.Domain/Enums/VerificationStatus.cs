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
    Pending = 1,

    /// <summary>
    /// Under review - currently being verified (قيد المراجعة)
    /// </summary>
    UnderReview = 2,

    /// <summary>
    /// Verified - validated and accepted (موثق)
    /// </summary>
    Verified = 3,

    /// <summary>
    /// Rejected - not valid or rejected (مرفوض)
    /// </summary>
    Rejected = 4,

    /// <summary>
    /// Requires additional information (يتطلب معلومات إضافية)
    /// </summary>
    RequiresAdditionalInfo = 5,

    /// <summary>
    /// Expired - document or evidence has expired (منتهي الصلاحية)
    /// </summary>
    Expired = 6
}