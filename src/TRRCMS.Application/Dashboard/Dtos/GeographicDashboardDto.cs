namespace TRRCMS.Application.Dashboard.Dtos;

/// <summary>
/// Geographic coverage dashboard: building and property unit coverage by neighborhood.
/// Returned by GET /api/v1/dashboard/geographic.
/// </summary>
public sealed class GeographicDashboardDto
{
    public int TotalNeighborhoods { get; set; }
    public int NeighborhoodsWithBuildings { get; set; }
    public List<NeighborhoodCoverageDto> Neighborhoods { get; set; } = new();
    public DateTime GeneratedAtUtc { get; set; }
}

/// <summary>
/// Coverage metrics for a single neighborhood.
/// </summary>
public sealed class NeighborhoodCoverageDto
{
    public string Code { get; set; } = string.Empty;
    public string NameArabic { get; set; } = string.Empty;
    public string? NameEnglish { get; set; }
    public int BuildingCount { get; set; }
    public int PropertyUnitCount { get; set; }
}
