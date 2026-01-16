namespace TRRCMS.Application.Buildings.Dtos;

public class BuildingDto
{
    public Guid Id { get; set; }
    public string BuildingId { get; set; } = string.Empty;

    // Administrative Codes
    public string GovernorateCode { get; set; } = string.Empty;
    public string DistrictCode { get; set; } = string.Empty;
    public string SubDistrictCode { get; set; } = string.Empty;
    public string CommunityCode { get; set; } = string.Empty;
    public string NeighborhoodCode { get; set; } = string.Empty;
    public string BuildingNumber { get; set; } = string.Empty;

    // Location Names (Arabic)
    public string GovernorateName { get; set; } = string.Empty;
    public string DistrictName { get; set; } = string.Empty;
    public string SubDistrictName { get; set; } = string.Empty;
    public string CommunityName { get; set; } = string.Empty;
    public string NeighborhoodName { get; set; } = string.Empty;

    // Attributes
    public string BuildingType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? DamageLevel { get; set; }
    public int NumberOfPropertyUnits { get; set; }
    public int NumberOfApartments { get; set; }
    public int NumberOfShops { get; set; }
    public int? NumberOfFloors { get; set; }
    public int? YearOfConstruction { get; set; }

    // Location
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? BuildingGeometryWkt { get; set; }

    // Additional Information - ADDED THESE!
    public string? Address { get; set; }
    public string? Landmark { get; set; }
    public string? Notes { get; set; }

    // Audit
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? LastModifiedAtUtc { get; set; }
}