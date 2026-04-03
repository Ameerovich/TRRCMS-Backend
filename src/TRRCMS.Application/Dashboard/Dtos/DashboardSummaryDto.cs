namespace TRRCMS.Application.Dashboard.Dtos;

/// <summary>
/// Top-level dashboard statistics DTO.
/// Returned by GET /api/v1/dashboard/summary.
/// </summary>
public sealed class DashboardSummaryDto
{
    public CaseStatisticsDto Cases { get; set; } = new();
    public ClaimStatisticsDto Claims { get; set; } = new();
    public SurveyStatisticsDto Surveys { get; set; } = new();
    public ImportStatisticsDto Imports { get; set; } = new();
    public BuildingStatisticsDto Buildings { get; set; } = new();
    public DateTime GeneratedAtUtc { get; set; }
}

/// <summary>
/// Case statistics: total cases, counts by lifecycle status (Open/Closed).
/// </summary>
public sealed class CaseStatisticsDto
{
    public int TotalCases { get; set; }

    /// <summary>
    /// Count per CaseLifecycleStatus enum name (e.g. "Open": 10, "Closed": 5).
    /// </summary>
    public Dictionary<string, int> ByStatus { get; set; } = new();
}

/// <summary>
/// Claim statistics: counts by status, lifecycle stage, and special flags.
/// </summary>
public sealed class ClaimStatisticsDto
{
    public int TotalClaims { get; set; }

    /// <summary>
    /// Count per CaseStatus enum name (e.g. "Open": 5, "Closed": 12).
    /// </summary>
    public Dictionary<string, int> ByStatus { get; set; } = new();
}

/// <summary>
/// Survey statistics: counts by status, type split, and recent completions.
/// </summary>
public sealed class SurveyStatisticsDto
{
    public int TotalSurveys { get; set; }

    /// <summary>
    /// Count per SurveyStatus enum name (e.g. "Draft": 10, "Completed": 25).
    /// </summary>
    public Dictionary<string, int> ByStatus { get; set; } = new();

    public int FieldSurveyCount { get; set; }
    public int OfficeSurveyCount { get; set; }
    public int CompletedLast7Days { get; set; }
    public int CompletedLast30Days { get; set; }
}

/// <summary>
/// Import pipeline statistics: package counts by status, active pipeline, content totals.
/// </summary>
public sealed class ImportStatisticsDto
{
    public int TotalPackages { get; set; }

    /// <summary>
    /// Count per ImportStatus enum name (e.g. "Pending": 2, "Completed": 8).
    /// </summary>
    public Dictionary<string, int> ByStatus { get; set; } = new();

    /// <summary>
    /// In-flight pipeline count (Pending + Validating + Staging + ReviewingConflicts + ReadyToCommit).
    /// </summary>
    public int ActiveCount { get; set; }

    public int WithUnresolvedConflicts { get; set; }
    public int TotalSurveysImported { get; set; }
    public int TotalBuildingsImported { get; set; }
    public int TotalPersonsImported { get; set; }
}

/// <summary>
/// Building coverage statistics: totals, property units, damage breakdown.
/// </summary>
public sealed class BuildingStatisticsDto
{
    public int TotalBuildings { get; set; }
    public int TotalPropertyUnits { get; set; }

    /// <summary>
    /// Count per BuildingStatus enum name (e.g. "Occupied": 50, "Damaged": 12).
    /// </summary>
    public Dictionary<string, int> ByStatus { get; set; } = new();

    public double AverageUnitsPerBuilding { get; set; }
}
