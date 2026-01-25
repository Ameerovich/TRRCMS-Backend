namespace TRRCMS.Domain.Enums;

/// <summary>
/// Survey type classification
/// Distinguishes between field and office survey workflows
/// Referenced in UC-001 (Field Survey) and UC-004 (Office Survey)
/// </summary>
public enum SurveyType
{
    /// <summary>
    /// Field Survey - Conducted on-site using tablet by field collectors (استطلاع ميداني)
    /// UC-001: Field Survey workflow
    /// </summary>
    Field = 1,

    /// <summary>
    /// Office Survey - Conducted at office using desktop by office clerks (استطلاع مكتبي)
    /// UC-004: Office Survey workflow
    /// </summary>
    Office = 2
}
