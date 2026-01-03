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
    FullOwnership = 1,

    /// <summary>
    /// Shared ownership - Partial ownership with others (ملكية مشتركة)
    /// </summary>
    SharedOwnership = 2,

    /// <summary>
    /// Long-term rental - Formal long-term lease (إيجار طويل الأمد)
    /// </summary>
    LongTermRental = 3,

    /// <summary>
    /// Short-term rental - Temporary rental agreement (إيجار قصير الأمد)
    /// </summary>
    ShortTermRental = 4,

    /// <summary>
    /// Informal tenure - No formal documentation (حيازة غير رسمية)
    /// </summary>
    InformalTenure = 5,

    /// <summary>
    /// Squatter/Unauthorized occupation (إشغال غير مصرح)
    /// </summary>
    UnauthorizedOccupation = 6,

    /// <summary>
    /// Customary/Traditional rights (حقوق عرفية)
    /// </summary>
    CustomaryRights = 7,

    /// <summary>
    /// Inheritance-based - Inherited property (ميراث)
    /// </summary>
    InheritanceBased = 8,

    /// <summary>
    /// Hosted/Guest - Staying as guest (استضافة)
    /// </summary>
    HostedGuest = 9,

    /// <summary>
    /// Temporary shelter - Emergency or temporary accommodation (مأوى مؤقت)
    /// </summary>
    TemporaryShelter = 10,

    /// <summary>
    /// Government allocation (تخصيص حكومي)
    /// </summary>
    GovernmentAllocation = 11,

    /// <summary>
    /// Usufruct - Right to use property (حق الانتفاع)
    /// </summary>
    Usufruct = 12,

    /// <summary>
    /// Other type not listed
    /// </summary>
    Other = 99
}