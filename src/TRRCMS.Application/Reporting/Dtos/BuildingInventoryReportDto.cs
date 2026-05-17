namespace TRRCMS.Application.Reporting.Dtos;

public sealed class BuildingInventoryReportDto
{
    public string? NeighborhoodCodeFilter { get; set; }
    public DateTime GeneratedAtUtc { get; set; }

    public int TotalNeighborhoods { get; set; }
    public int NeighborhoodsWithBuildings { get; set; }
    public int TotalBuildings { get; set; }
    public int TotalPropertyUnits { get; set; }

    public List<BuildingInventoryRow> Rows { get; set; } = new();
}

public sealed class BuildingInventoryRow
{
    public string NeighborhoodCode { get; set; } = string.Empty;
    public string NameArabic { get; set; } = string.Empty;
    public string NameEnglish { get; set; } = string.Empty;
    public int BuildingCount { get; set; }
    public int PropertyUnitCount { get; set; }
}
