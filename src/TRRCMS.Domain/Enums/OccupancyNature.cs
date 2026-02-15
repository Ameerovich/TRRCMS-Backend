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
    [ArabicLabel("قانوني/رسمي")]
    LegalFormal = 1,

    /// <summary>
    /// Informal - Without formal legal documentation (غير رسمي)
    /// </summary>
    [ArabicLabel("غير رسمي")]
    Informal = 2,

    /// <summary>
    /// Customary - Based on traditional/customary rights (عرفي)
    /// </summary>
    [ArabicLabel("عرفي")]
    Customary = 3,

    /// <summary>
    /// Temporary/Emergency - Due to displacement or emergency (طوارئ/مؤقت)
    /// </summary>
    [ArabicLabel("طوارئ/مؤقت")]
    TemporaryEmergency = 4,

    /// <summary>
    /// Authorized - With government authorization (مصرح به)
    /// </summary>
    [ArabicLabel("مصرح به")]
    Authorized = 5,

    /// <summary>
    /// Unauthorized - Without authorization (غير مصرح به)
    /// </summary>
    [ArabicLabel("غير مصرح به")]
    Unauthorized = 6,

    /// <summary>
    /// Pending regularization - Being formalized (قيد التسوية)
    /// </summary>
    [ArabicLabel("قيد التسوية")]
    PendingRegularization = 7,

    /// <summary>
    /// Contested/Disputed - Disputed occupancy (متنازع عليه)
    /// </summary>
    [ArabicLabel("متنازع عليه")]
    Contested = 8,

    /// <summary>
    /// Unknown nature
    /// </summary>
    [ArabicLabel("غير معروف")]
    Unknown = 99
}