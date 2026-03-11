namespace TRRCMS.Application.Dashboard.Dtos;

/// <summary>
/// Monthly time-series trends for key entities.
/// Returned by GET /api/v1/dashboard/trends.
/// </summary>
public sealed class DashboardTrendsDto
{
    public List<MonthlyTrendPointDto> Claims { get; set; } = new();
    public List<MonthlyTrendPointDto> Surveys { get; set; } = new();
    public List<MonthlyTrendPointDto> Buildings { get; set; } = new();
    public List<MonthlyTrendPointDto> Persons { get; set; } = new();
    public List<MonthlyTrendPointDto> Imports { get; set; } = new();
    public DateTime GeneratedAtUtc { get; set; }
}

/// <summary>
/// A single data point in a monthly time-series.
/// </summary>
public sealed class MonthlyTrendPointDto
{
    public int Year { get; set; }
    public int Month { get; set; }

    /// <summary>
    /// Human-readable label, e.g. "2026-03"
    /// </summary>
    public string Label { get; set; } = string.Empty;

    public int Count { get; set; }
}
