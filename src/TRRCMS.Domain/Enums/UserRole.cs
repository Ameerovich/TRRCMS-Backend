namespace TRRCMS.Domain.Enums;

/// <summary>
/// System user roles with different access levels
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Field data collector - mobile app access only
    /// </summary>
    [ArabicLabel("جامع بيانات ميداني")]
    FieldCollector = 1,

    /// <summary>
    /// Field supervisor - read-only desktop access
    /// </summary>
    [ArabicLabel("مشرف ميداني")]
    FieldSupervisor = 2,

    /// <summary>
    /// Office clerk - full desktop access for claim registration
    /// </summary>
    [ArabicLabel("موظف مكتب")]
    OfficeClerk = 3,

    /// <summary>
    /// Data manager - full desktop access for data validation and import
    /// </summary>
    [ArabicLabel("مدير بيانات")]
    DataManager = 4,

    /// <summary>
    /// Analyst - read-only access for reporting and analytics
    /// </summary>
    [ArabicLabel("محلل")]
    Analyst = 5,

    /// <summary>
    /// System administrator - full access to all features
    /// </summary>
    [ArabicLabel("مدير النظام")]
    Administrator = 6
}