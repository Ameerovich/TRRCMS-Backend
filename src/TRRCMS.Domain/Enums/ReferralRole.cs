namespace TRRCMS.Domain.Enums;

/// <summary>
/// Referral role classification
/// Roles involved in claim referral and redirection workflow
/// Referenced in FSD section 6.1.8 - Referral & Lifecycle Management
/// </summary>
public enum ReferralRole
{
    /// <summary>
    /// Field team - Field data collectors (الفريق الميداني)
    /// </summary>
    [ArabicLabel("الفريق الميداني")]
    FieldTeam = 1,

    /// <summary>
    /// Field supervisor - Supervises field operations (مشرف ميداني)
    /// </summary>
    [ArabicLabel("مشرف ميداني")]
    FieldSupervisor = 2,

    /// <summary>
    /// Municipality clerk - Front desk officer (موظف بلدي)
    /// </summary>
    [ArabicLabel("موظف بلدي")]
    MunicipalityClerk = 3,

    /// <summary>
    /// Office clerk - Back office data entry (موظف مكتب)
    /// </summary>
    [ArabicLabel("موظف مكتب")]
    OfficeClerk = 4,

    /// <summary>
    /// Data manager - Data validation and quality control (مدير البيانات)
    /// </summary>
    [ArabicLabel("مدير البيانات")]
    DataManager = 5,

    /// <summary>
    /// Case officer - Reviews and processes claims (موظف الحالات)
    /// </summary>
    [ArabicLabel("موظف الحالات")]
    CaseOfficer = 6,

    /// <summary>
    /// Adjudication panel - Resolves conflicts (لجنة التحكيم)
    /// </summary>
    [ArabicLabel("لجنة التحكيم")]
    AdjudicationPanel = 7,

    /// <summary>
    /// Legal team - Legal review and advice (الفريق القانوني)
    /// </summary>
    [ArabicLabel("الفريق القانوني")]
    LegalTeam = 8,

    /// <summary>
    /// Manager - Approval authority (المدير)
    /// </summary>
    [ArabicLabel("المدير")]
    Manager = 9,

    /// <summary>
    /// Technical specialist - Technical assessment (أخصائي فني)
    /// </summary>
    [ArabicLabel("أخصائي فني")]
    TechnicalSpecialist = 10,

    /// <summary>
    /// System administrator - System maintenance (مدير النظام)
    /// </summary>
    [ArabicLabel("مدير النظام")]
    SystemAdministrator = 99
}