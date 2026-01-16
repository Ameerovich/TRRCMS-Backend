namespace TRRCMS.Application.Buildings.Dtos;

/// <summary>
/// Lightweight DTO for map rendering
/// Contains only essential fields needed for building markers
/// Optimized for performance when loading thousands of buildings
/// </summary>
public class BuildingMapDto
{
    /// <summary>
    /// Building UUID (internal)
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 17-digit Building ID (business identifier)
    /// </summary>
    public string BuildingId { get; set; } = string.Empty;

    /// <summary>
    /// GPS Latitude
    /// </summary>
    public decimal? Latitude { get; set; }

    /// <summary>
    /// GPS Longitude
    /// </summary>
    public decimal? Longitude { get; set; }

    /// <summary>
    /// Building status for color coding on map
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Building type for icon selection
    /// </summary>
    public string BuildingType { get; set; } = string.Empty;

    /// <summary>
    /// Damage level for visual indicator
    /// </summary>
    public string? DamageLevel { get; set; }

    /// <summary>
    /// Building address for tooltip/popup
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Number of property units (for badge display)
    /// </summary>
    public int NumberOfPropertyUnits { get; set; }

    /// <summary>
    /// Administrative location for grouping
    /// </summary>
    public string NeighborhoodName { get; set; } = string.Empty;
}