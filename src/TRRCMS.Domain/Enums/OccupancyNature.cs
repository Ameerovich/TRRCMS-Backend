namespace TRRCMS.Domain.Enums;

/// <summary>
/// Occupancy nature classification (طبيعة الإشغال)
/// Describes the nature/basis of the occupancy
/// Referenced in FSD section 6.1.6
/// </summary>
public enum OccupancyNature
{
    /// <summary>
    /// Legal/Formal - With proper legal documentation (قانوني/رسمي)
    /// </summary>
    LegalFormal = 1,

    /// <summary>
    /// Informal - Without formal legal documentation (غير رسمي)
    /// </summary>
    Informal = 2,

    /// <summary>
    /// Customary - Based on traditional/customary rights (عرفي)
    /// </summary>
    Customary = 3,

    /// <summary>
    /// Temporary/Emergency - Due to displacement or emergency (طوارئ/مؤقت)
    /// </summary>
    TemporaryEmergency = 4,

    /// <summary>
    /// Authorized - With government authorization (مصرح به)
    /// </summary>
    Authorized = 5,

    /// <summary>
    /// Unauthorized - Without authorization (غير مصرح به)
    /// </summary>
    Unauthorized = 6,

    /// <summary>
    /// Pending regularization - Being formalized (قيد التسوية)
    /// </summary>
    PendingRegularization = 7,

    /// <summary>
    /// Contested/Disputed - Disputed occupancy (متنازع عليه)
    /// </summary>
    Contested = 8,

    /// <summary>
    /// Unknown nature
    /// </summary>
    Unknown = 99
}