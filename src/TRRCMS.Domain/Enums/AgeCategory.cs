namespace TRRCMS.Domain.Enums;

/// <summary>
/// Age category for household composition analysis
/// </summary>
public enum AgeCategory
{
    /// <summary>
    /// Infant - Under 2 years (رضيع)
    /// </summary>
    [ArabicLabel("رضيع")]
    Infant = 1,

    /// <summary>
    /// Child - 2 to 12 years (طفل)
    /// </summary>
    [ArabicLabel("طفل")]
    Child = 2,

    /// <summary>
    /// Minor/Adolescent - 13 to 17 years (قاصر)
    /// Under 18 as defined by UN-Habitat
    /// </summary>
    [ArabicLabel("قاصر")]
    Minor = 3,

    /// <summary>
    /// Adult - 18 to 64 years (بالغ)
    /// </summary>
    [ArabicLabel("بالغ")]
    Adult = 4,

    /// <summary>
    /// Elderly - 65 years and above (مسن)
    /// </summary>
    [ArabicLabel("مسن")]
    Elderly = 5
}