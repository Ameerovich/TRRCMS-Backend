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
    Intact = 1,

    /// <summary>
    /// Minor damage - Building has minor damage but is habitable (أضرار طفيفة)
    /// </summary>
    MinorDamage = 2,

    /// <summary>
    /// Moderate damage - Building has moderate damage (أضرار متوسطة)
    /// </summary>
    ModerateDamage = 3,

    /// <summary>
    /// Major damage - Building has major structural damage (أضرار كبيرة)
    /// </summary>
    MajorDamage = 4,

    /// <summary>
    /// Severely damaged - Building is severely damaged (أضرار شديدة)
    /// </summary>
    SeverelyDamaged = 5,

    /// <summary>
    /// Destroyed - Building is completely destroyed (مدمر)
    /// </summary>
    Destroyed = 6,

    /// <summary>
    /// Under construction - Building is being built (قيد الإنشاء)
    /// </summary>
    UnderConstruction = 7,

    /// <summary>
    /// Abandoned - Building is abandoned (مهجور)
    /// </summary>
    Abandoned = 8,

    /// <summary>
    /// Unknown status (غير معروف)
    /// </summary>
    Unknown = 99
}