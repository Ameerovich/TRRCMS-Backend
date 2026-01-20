namespace TRRCMS.Application.PropertyUnits.Dtos;

/// <summary>
/// Data transfer object for Property Unit
/// </summary>
public class PropertyUnitDto
{
    // ==================== IDENTIFIERS ====================

    public Guid Id { get; set; }
    public Guid BuildingId { get; set; }
    public string? BuildingNumber { get; set; } // Added for enrichment in handlers
    public string UnitIdentifier { get; set; } = string.Empty;

    // ==================== UNIT ATTRIBUTES ====================

    public int? FloorNumber { get; set; }
    public string? PositionOnFloor { get; set; } // Added for Day 2
    public string UnitType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? OccupancyStatus { get; set; } // Added for Day 2 (flexible field survey data)
    public string? DamageLevel { get; set; }
    public decimal? AreaSquareMeters { get; set; }
    public decimal? EstimatedAreaSqm { get; set; } // Added for Day 2 (field survey estimates)
    public int? NumberOfRooms { get; set; }
    public int? NumberOfBathrooms { get; set; }
    public bool? HasBalcony { get; set; }

    // ==================== OCCUPANCY INFORMATION ====================

    public string? OccupancyType { get; set; }
    public string? OccupancyNature { get; set; }
    public int? NumberOfHouseholds { get; set; }
    public int? TotalOccupants { get; set; }

    // ==================== UTILITIES (Added for Day 2) ====================

    public bool HasElectricity { get; set; }
    public bool HasWater { get; set; }
    public bool HasSewage { get; set; }
    public string? UtilitiesNotes { get; set; }

    // ==================== ADDITIONAL INFORMATION ====================

    public string? Description { get; set; }
    public string? SpecialFeatures { get; set; }

    // ==================== AUDIT FIELDS ====================

    public DateTime CreatedAtUtc { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? LastModifiedAtUtc { get; set; }
    public Guid? LastModifiedBy { get; set; }
}