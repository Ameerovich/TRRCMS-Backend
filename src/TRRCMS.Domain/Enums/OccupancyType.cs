namespace TRRCMS.Domain.Enums;

/// <summary>
/// Occupancy type classification (نوع الإشغال)
/// Describes the type of occupancy arrangement
/// Referenced in FSD section 6.1.6
/// </summary>
public enum OccupancyType
{
    /// <summary>
    /// Owner-occupied - Property occupied by the owner (يشغله المالك)
    /// </summary>
    [ArabicLabel("يشغله المالك")]
    OwnerOccupied = 1,

    /// <summary>
    /// Tenant-occupied - Property occupied by renters (يشغله مستأجر)
    /// </summary>
    [ArabicLabel("يشغله مستأجر")]
    TenantOccupied = 2,

    /// <summary>
    /// Family-occupied - Occupied by family members (يشغله أفراد العائلة)
    /// </summary>
    [ArabicLabel("يشغله أفراد العائلة")]
    FamilyOccupied = 3,

    /// <summary>
    /// Mixed occupancy - Both owners and tenants (إشغال مختلط)
    /// </summary>
    [ArabicLabel("إشغال مختلط")]
    MixedOccupancy = 4,

    /// <summary>
    /// Vacant - Not currently occupied (شاغر)
    /// </summary>
    [ArabicLabel("شاغر")]
    Vacant = 5,

    /// <summary>
    /// Temporary/Seasonal - Used temporarily (موسمي/مؤقت)
    /// </summary>
    [ArabicLabel("موسمي/مؤقت")]
    TemporarySeasonal = 6,

    /// <summary>
    /// Commercial use - Used for business purposes (استخدام تجاري)
    /// </summary>
    [ArabicLabel("استخدام تجاري")]
    CommercialUse = 7,

    /// <summary>
    /// Abandoned - No longer occupied (مهجور)
    /// </summary>
    [ArabicLabel("مهجور")]
    Abandoned = 8,

    /// <summary>
    /// Disputed occupancy - Contested or unclear (متنازع عليه)
    /// </summary>
    [ArabicLabel("متنازع عليه")]
    Disputed = 9,

    /// <summary>
    /// Unknown occupancy type
    /// </summary>
    [ArabicLabel("غير معروف")]
    Unknown = 99
}