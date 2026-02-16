namespace TRRCMS.Domain.Enums;

/// <summary>
/// Property unit status classification (حالة المقسم/الوحدة)
/// Describes the condition and occupancy of individual property units
/// </summary>
public enum PropertyUnitStatus
{
    /// <summary>
    /// Occupied - Unit is currently occupied (مشغول)
    /// </summary>
    [ArabicLabel("مشغول")]
    Occupied = 1,

    /// <summary>
    /// Vacant - Unit is empty/unoccupied (شاغر)
    /// </summary>
    [ArabicLabel("شاغر")]
    Vacant = 2,

    /// <summary>
    /// Damaged - Unit is damaged (متضرر)
    /// </summary>
    [ArabicLabel("متضرر")]
    Damaged = 3,

    /// <summary>
    /// Under renovation - Unit is being renovated (قيد الترميم)
    /// </summary>
    [ArabicLabel("قيد الترميم")]
    UnderRenovation = 4,

    /// <summary>
    /// Uninhabitable - Unit cannot be occupied due to damage (غير صالح للسكن)
    /// </summary>
    [ArabicLabel("غير صالح للسكن")]
    Uninhabitable = 5,

    /// <summary>
    /// Locked/Sealed - Unit is locked or sealed by authorities (مغلق)
    /// </summary>
    [ArabicLabel("مغلق")]
    Locked = 6,

    /// <summary>
    /// Unknown status (غير معروف)
    /// </summary>
    [ArabicLabel("غير معروف")]
    Unknown = 99
}