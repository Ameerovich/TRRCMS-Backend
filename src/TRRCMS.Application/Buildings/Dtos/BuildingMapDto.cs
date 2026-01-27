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
    /// Building ID stored in database (رمز البناء)
    /// Format: GGDDSSCCNCNNBBBBB (17 digits)
    /// </summary>
    public string BuildingId { get; set; } = string.Empty;

    /// <summary>
    /// Formatted Building ID for display
    /// Format: GG-DD-SS-CCC-NNN-BBBBB
    /// </summary>
    public string BuildingIdFormatted => FormatBuildingId(BuildingId);

    private static string FormatBuildingId(string buildingId)
    {
        if (string.IsNullOrEmpty(buildingId) || buildingId.Length != 17)
            return buildingId;

        return $"{buildingId[..2]}-{buildingId[2..4]}-{buildingId[4..6]}-" +
               $"{buildingId[6..9]}-{buildingId[9..12]}-{buildingId[12..17]}";
    }

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
    /// Number of property units (عدد الوحدات)
    /// For badge display on map marker
    /// </summary>
    public int NumberOfPropertyUnits { get; set; }

    /// <summary>
    /// Number of apartments (عدد المقاسم)
    /// </summary>
    public int NumberOfApartments { get; set; }

    /// <summary>
    /// Number of shops (عدد المحلات)
    /// </summary>
    public int NumberOfShops { get; set; }
}