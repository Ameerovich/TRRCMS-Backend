namespace TRRCMS.Domain.Enums;

/// <summary>
/// Survey type classification
/// Distinguishes between field and office survey workflows
/// </summary>
public enum SurveyType
{
    /// <summary>
    /// Field Survey - Conducted on-site using tablet by field collectors (استطلاع ميداني)
    /// </summary>
    [ArabicLabel("استطلاع ميداني")]
    Field = 1,

    /// <summary>
    /// Office Survey - Conducted at office using desktop by office clerks (استطلاع مكتبي)
    /// </summary>
    [ArabicLabel("استطلاع مكتبي")]
    Office = 2
}
