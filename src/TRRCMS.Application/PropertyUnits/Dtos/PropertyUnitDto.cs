namespace TRRCMS.Application.PropertyUnits.Dtos;

/// <summary>
/// Data transfer object for Property Unit
/// </summary>
public class PropertyUnitDto
{
    // ==================== IDENTIFIERS ====================

    public Guid Id { get; set; }
    public Guid BuildingId { get; set; }
    public string UnitIdentifier { get; set; } = string.Empty;

    // ==================== UNIT ATTRIBUTES ====================

    public int? FloorNumber { get; set; }
    public string UnitType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? DamageLevel { get; set; }
    public decimal? AreaSquareMeters { get; set; }
    public int? NumberOfRooms { get; set; }
    public int? NumberOfBathrooms { get; set; }
    public bool? HasBalcony { get; set; }

    // ==================== OCCUPANCY INFORMATION ====================

    public string? OccupancyType { get; set; }
    public string? OccupancyNature { get; set; }
    public int? NumberOfHouseholds { get; set; }
    public int? TotalOccupants { get; set; }

    // ==================== ADDITIONAL INFORMATION ====================

    public string? Description { get; set; }
    public string? SpecialFeatures { get; set; }

    // ==================== AUDIT FIELDS ====================

    public DateTime CreatedAtUtc { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? LastModifiedAtUtc { get; set; }
    public Guid? LastModifiedBy { get; set; }
}