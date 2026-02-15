namespace TRRCMS.Domain.Enums;

/// <summary>
/// Claim source classification
/// Indicates how the claim was created in the system
/// </summary>
public enum ClaimSource
{
    /// <summary>
    /// Field collection - Collected via mobile app by field teams (جمع ميداني)
    /// </summary>
    [ArabicLabel("جمع ميداني")]
    FieldCollection = 1,

    /// <summary>
    /// Office submission - Registered at municipality office (تقديم مكتبي)
    /// </summary>
    [ArabicLabel("تقديم مكتبي")]
    OfficeSubmission = 2,

    /// <summary>
    /// System import - Imported from external system/file (استيراد من النظام)
    /// </summary>
    [ArabicLabel("استيراد من النظام")]
    SystemImport = 3,

    /// <summary>
    /// Migration - Migrated from legacy system (ترحيل من نظام قديم)
    /// </summary>
    [ArabicLabel("ترحيل من نظام قديم")]
    Migration = 4,

    /// <summary>
    /// Online portal - Submitted through web portal (بوابة إلكترونية)
    /// </summary>
    [ArabicLabel("بوابة إلكترونية")]
    OnlinePortal = 5,

    /// <summary>
    /// API integration - Created via API (تكامل API)
    /// </summary>
    [ArabicLabel("تكامل API")]
    ApiIntegration = 6,

    /// <summary>
    /// Manual entry - Manually entered by data manager (إدخال يدوي)
    /// </summary>
    [ArabicLabel("إدخال يدوي")]
    ManualEntry = 7,

    /// <summary>
    /// Other source
    /// </summary>
    [ArabicLabel("أخرى")]
    Other = 99
}