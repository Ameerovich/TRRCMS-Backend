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
    FieldCollection = 1,

    /// <summary>
    /// Office Submission - Data entered at office via desktop (إدخال مكتبي)
    /// </summary>
    OfficeSubmission = 2,

    /// <summary>
    /// Data Migration - Imported from legacy system (ترحيل بيانات)
    /// </summary>
    DataMigration = 3,

    /// <summary>
    /// Bulk Import - Imported via .uhc package (استيراد جماعي)
    /// </summary>
    BulkImport = 4
}
