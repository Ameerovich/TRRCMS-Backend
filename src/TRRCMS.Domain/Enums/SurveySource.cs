namespace TRRCMS.Domain.Enums;

/// <summary>
/// Survey source classification
/// Tracks how survey data entered the system
/// </summary>
public enum SurveySource
{
    /// <summary>
    /// Field Collection - Data collected on-site via tablet (جمع ميداني)
    /// </summary>
    [ArabicLabel("جمع ميداني")]
    FieldCollection = 1,

    /// <summary>
    /// Office Submission - Data entered at office via desktop (إدخال مكتبي)
    /// </summary>
    [ArabicLabel("إدخال مكتبي")]
    OfficeSubmission = 2,

    /// <summary>
    /// Data Migration - Imported from legacy system (ترحيل بيانات)
    /// </summary>
    [ArabicLabel("ترحيل بيانات")]
    DataMigration = 3,

    /// <summary>
    /// Bulk Import - Imported via .uhc package (استيراد جماعي)
    /// </summary>
    [ArabicLabel("استيراد جماعي")]
    BulkImport = 4
}
