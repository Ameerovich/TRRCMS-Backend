namespace TRRCMS.Domain.Enums;

/// <summary>
/// Damage level classification for buildings and property units
/// Used for damage assessment and reporting
/// Referenced throughout the FSD for damage tracking
/// </summary>
public enum DamageLevel
{
    /// <summary>
    /// No damage - Property is intact (بدون أضرار)
    /// </summary>
    NoDamage = 0,

    /// <summary>
    /// Minor damage - Cosmetic or easily repairable damage (أضرار طفيفة)
    /// Examples: Broken windows, minor cracks, paint damage
    /// </summary>
    Minor = 1,

    /// <summary>
    /// Moderate damage - Significant but repairable damage (أضرار متوسطة)
    /// Examples: Structural cracks, damaged roof, broken utilities
    /// </summary>
    Moderate = 2,

    /// <summary>
    /// Major damage - Extensive damage requiring major repairs (أضرار كبيرة)
    /// Examples: Partial collapse, major structural damage
    /// </summary>
    Major = 3,

    /// <summary>
    /// Severe damage - Building is uninhabitable (أضرار شديدة)
    /// Building cannot be used without extensive reconstruction
    /// </summary>
    Severe = 4,

    /// <summary>
    /// Complete destruction - Building is destroyed (دمار كامل)
    /// Building is completely destroyed or unsafe for any use
    /// </summary>
    CompleteDestruction = 5,

    /// <summary>
    /// Assessment pending - Damage not yet assessed (قيد التقييم)
    /// </summary>
    AssessmentPending = 98,

    /// <summary>
    /// Not applicable or unknown (غير معروف)
    /// </summary>
    Unknown = 99
}