namespace TRRCMS.Domain.Enums;

/// <summary>
/// Tenure/Contract type classification (نوع وثيقة الإشغال/العقد)
/// Type of contract or tenure for occupants/tenants
/// Referenced in FSD section 6.1.4
/// </summary>
public enum TenureContractType
{
    /// <summary>
    /// Full ownership - Complete property ownership (ملكية تامة)
    /// </summary>
    [ArabicLabel("ملكية تامة")]
    FullOwnership = 1,

    /// <summary>
    /// Shared ownership - Partial ownership with others (ملكية مشتركة)
    /// </summary>
    [ArabicLabel("ملكية مشتركة")]
    SharedOwnership = 2,

    /// <summary>
    /// Long-term rental - Formal long-term lease (إيجار طويل الأمد)
    /// </summary>
    [ArabicLabel("إيجار طويل الأمد")]
    LongTermRental = 3,

    /// <summary>
    /// Short-term rental - Temporary rental agreement (إيجار قصير الأمد)
    /// </summary>
    [ArabicLabel("إيجار قصير الأمد")]
    ShortTermRental = 4,

    /// <summary>
    /// Informal tenure - No formal documentation (حيازة غير رسمية)
    /// </summary>
    [ArabicLabel("حيازة غير رسمية")]
    InformalTenure = 5,

    /// <summary>
    /// Squatter/Unauthorized occupation (إشغال غير مصرح)
    /// </summary>
    [ArabicLabel("إشغال غير مصرح")]
    UnauthorizedOccupation = 6,

    /// <summary>
    /// Customary/Traditional rights (حقوق عرفية)
    /// </summary>
    [ArabicLabel("حقوق عرفية")]
    CustomaryRights = 7,

    /// <summary>
    /// Inheritance-based - Inherited property (ميراث)
    /// </summary>
    [ArabicLabel("ميراث")]
    InheritanceBased = 8,

    /// <summary>
    /// Hosted/Guest - Staying as guest (استضافة)
    /// </summary>
    [ArabicLabel("استضافة")]
    HostedGuest = 9,

    /// <summary>
    /// Temporary shelter - Emergency or temporary accommodation (مأوى مؤقت)
    /// </summary>
    [ArabicLabel("مأوى مؤقت")]
    TemporaryShelter = 10,

    /// <summary>
    /// Government allocation (تخصيص حكومي)
    /// </summary>
    [ArabicLabel("تخصيص حكومي")]
    GovernmentAllocation = 11,

    /// <summary>
    /// Usufruct - Right to use property (حق الانتفاع)
    /// </summary>
    [ArabicLabel("حق الانتفاع")]
    Usufruct = 12,

    /// <summary>
    /// Other type not listed
    /// </summary>
    [ArabicLabel("أخرى")]
    Other = 99
}