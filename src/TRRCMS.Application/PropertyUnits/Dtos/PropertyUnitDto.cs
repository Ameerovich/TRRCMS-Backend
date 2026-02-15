namespace TRRCMS.Application.PropertyUnits.Dtos;

/// <summary>
/// Simplified Property Unit DTO - matches frontend form fields
/// Used for API requests and responses
/// </summary>
public class PropertyUnitDto
{
    // ==================== IDENTIFIERS ====================

    /// <summary>
    /// Unique identifier (GUID)
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Parent building ID
    /// </summary>
    public Guid BuildingId { get; set; }

    /// <summary>
    /// Building number (for display purposes)
    /// </summary>
    public string? BuildingNumber { get; set; }

    /// <summary>
    /// Unit identifier within building (رقم الوحدة)
    /// Example: "1", "1A", "Ground-Left"
    /// </summary>
    public string UnitIdentifier { get; set; } = string.Empty;

    // ==================== UNIT ATTRIBUTES ====================

    /// <summary>
    /// Floor number (رقم الطابق)
    /// 0 = Ground, 1 = First, -1 = Basement
    /// </summary>
    public int? FloorNumber { get; set; }

    /// <summary>
    /// Property unit type (نوع الوحدة)
    /// Values: "Apartment", "Shop", "Office", "Warehouse", "Other"
    /// </summary>
    public int UnitType { get; set; }

    /// <summary>
    /// Property unit status (حالة الوحدة)
    /// Values: "Occupied", "Vacant", "Damaged", "UnderRenovation", "Uninhabitable", "Locked", "Unknown"
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// Area in square meters (مساحة القسم)
    /// </summary>
    public decimal? AreaSquareMeters { get; set; }

    /// <summary>
    /// Number of rooms (عدد الغرف)
    /// </summary>
    public int? NumberOfRooms { get; set; }

    /// <summary>
    /// Unit description and notes (وصف مفصل)
    /// </summary>
    public string? Description { get; set; }

    // ==================== AUDIT FIELDS ====================

    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Last modification timestamp
    /// </summary>
    public DateTime? LastModifiedAtUtc { get; set; }
}