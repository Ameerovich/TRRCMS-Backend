namespace TRRCMS.Application.PropertyUnits.Dtos;

/// <summary>
/// Response DTO for grouped property units query
/// Contains property units grouped by building with summary statistics
/// </summary>
public class GroupedPropertyUnitsResponseDto
{
    /// <summary>
    /// Property units grouped by building
    /// Each building contains its associated property units
    /// </summary>
    public List<BuildingWithUnitsDto> GroupedByBuilding { get; set; } = new();

    /// <summary>
    /// Total count of property units in response
    /// </summary>
    public int TotalUnits { get; set; }

    /// <summary>
    /// Total count of buildings containing property units
    /// </summary>
    public int TotalBuildings { get; set; }
}

/// <summary>
/// Building DTO with its associated property units
/// Used in grouped property units response
/// </summary>
public class BuildingWithUnitsDto
{
    /// <summary>
    /// Building unique identifier (GUID)
    /// </summary>
    public Guid BuildingId { get; set; }

    /// <summary>
    /// Building code (17-digit formatted: GG-DD-SS-CCC-NNN-BBBBB)
    /// </summary>
    public string BuildingNumber { get; set; } = string.Empty;

    /// <summary>
    /// Number of property units in this building
    /// </summary>
    public int UnitCount { get; set; }

    /// <summary>
    /// Property units within this building
    /// Ordered by FloorNumber, then UnitIdentifier
    /// </summary>
    public List<PropertyUnitDto> PropertyUnits { get; set; } = new();
}
