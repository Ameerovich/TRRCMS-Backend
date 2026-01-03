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
    FieldTeam = 1,

    /// <summary>
    /// Field supervisor - Supervises field operations (مشرف ميداني)
    /// </summary>
    FieldSupervisor = 2,

    /// <summary>
    /// Municipality clerk - Front desk officer (موظف بلدي)
    /// </summary>
    MunicipalityClerk = 3,

    /// <summary>
    /// Office clerk - Back office data entry (موظف مكتب)
    /// </summary>
    OfficeClerk = 4,

    /// <summary>
    /// Data manager - Data validation and quality control (مدير البيانات)
    /// </summary>
    DataManager = 5,

    /// <summary>
    /// Case officer - Reviews and processes claims (موظف الحالات)
    /// </summary>
    CaseOfficer = 6,

    /// <summary>
    /// Adjudication panel - Resolves conflicts (لجنة التحكيم)
    /// </summary>
    AdjudicationPanel = 7,

    /// <summary>
    /// Legal team - Legal review and advice (الفريق القانوني)
    /// </summary>
    LegalTeam = 8,

    /// <summary>
    /// Manager - Approval authority (المدير)
    /// </summary>
    Manager = 9,

    /// <summary>
    /// Technical specialist - Technical assessment (أخصائي فني)
    /// </summary>
    TechnicalSpecialist = 10,

    /// <summary>
    /// System administrator - System maintenance (مدير النظام)
    /// </summary>
    SystemAdministrator = 99
}