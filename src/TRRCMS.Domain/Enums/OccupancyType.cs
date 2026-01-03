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
    OwnerOccupied = 1,

    /// <summary>
    /// Tenant-occupied - Property occupied by renters (يشغله مستأجر)
    /// </summary>
    TenantOccupied = 2,

    /// <summary>
    /// Family-occupied - Occupied by family members (يشغله أفراد العائلة)
    /// </summary>
    FamilyOccupied = 3,

    /// <summary>
    /// Mixed occupancy - Both owners and tenants (إشغال مختلط)
    /// </summary>
    MixedOccupancy = 4,

    /// <summary>
    /// Vacant - Not currently occupied (شاغر)
    /// </summary>
    Vacant = 5,

    /// <summary>
    /// Temporary/Seasonal - Used temporarily (موسمي/مؤقت)
    /// </summary>
    TemporarySeasonal = 6,

    /// <summary>
    /// Commercial use - Used for business purposes (استخدام تجاري)
    /// </summary>
    CommercialUse = 7,

    /// <summary>
    /// Abandoned - No longer occupied (مهجور)
    /// </summary>
    Abandoned = 8,

    /// <summary>
    /// Disputed occupancy - Contested or unclear (متنازع عليه)
    /// </summary>
    Disputed = 9,

    /// <summary>
    /// Unknown occupancy type
    /// </summary>
    Unknown = 99
}