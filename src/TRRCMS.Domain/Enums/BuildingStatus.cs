namespace TRRCMS.Domain.Enums;

/// <summary>
/// Building status classification (حالة البناء)
/// Describes the physical condition and occupancy status of the building
/// </summary>
public enum BuildingStatus
{
    /// <summary>
    /// Intact/Undamaged - Building is in good condition (سليم)
    /// </summary>
    [ArabicLabel("سليم")]
    Intact = 1,

    /// <summary>
    /// Minor damage - Building has minor damage but is habitable (أضرار طفيفة)
    /// </summary>
    [ArabicLabel("أضرار طفيفة")]
    MinorDamage = 2,

    /// <summary>
    /// Moderate damage - Building has moderate damage (أضرار متوسطة)
    /// </summary>
    [ArabicLabel("أضرار متوسطة")]
    ModerateDamage = 3,

    /// <summary>
    /// Major damage - Building has major structural damage (أضرار كبيرة)
    /// </summary>
    [ArabicLabel("أضرار كبيرة")]
    MajorDamage = 4,

    /// <summary>
    /// Severely damaged - Building is severely damaged (أضرار شديدة)
    /// </summary>
    [ArabicLabel("أضرار شديدة")]
    SeverelyDamaged = 5,

    /// <summary>
    /// Destroyed - Building is completely destroyed (مدمر)
    /// </summary>
    [ArabicLabel("مدمر")]
    Destroyed = 6,

    /// <summary>
    /// Under construction - Building is being built (قيد الإنشاء)
    /// </summary>
    [ArabicLabel("قيد الإنشاء")]
    UnderConstruction = 7,

    /// <summary>
    /// Abandoned - Building is abandoned (مهجور)
    /// </summary>
    [ArabicLabel("مهجور")]
    Abandoned = 8,

    /// <summary>
    /// Unknown status (غير معروف)
    /// </summary>
    [ArabicLabel("غير معروف")]
    Unknown = 99
}