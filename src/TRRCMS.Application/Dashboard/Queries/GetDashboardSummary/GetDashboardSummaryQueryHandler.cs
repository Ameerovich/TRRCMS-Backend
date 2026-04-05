using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Dashboard.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Dashboard.Queries.GetDashboardSummary;

/// <summary>
/// Handler for <see cref="GetDashboardSummaryQuery"/>.
/// Aggregates statistics from Claims, Surveys, ImportPackages, and Buildings
/// to populate the desktop dashboard.
/// </summary>
public sealed class GetDashboardSummaryQueryHandler
    : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
{
    private readonly IUnitOfWork _uow;

    public GetDashboardSummaryQueryHandler(IUnitOfWork uow)
    {
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
    }

    public async Task<DashboardSummaryDto> Handle(
        GetDashboardSummaryQuery request,
        CancellationToken cancellationToken)
    {
        return new DashboardSummaryDto
        {
            Cases = await BuildCaseStatisticsAsync(cancellationToken),
            Claims = await BuildClaimStatisticsAsync(cancellationToken),
            Surveys = await BuildSurveyStatisticsAsync(cancellationToken),
            Imports = await BuildImportStatisticsAsync(cancellationToken),
            Buildings = await BuildBuildingStatisticsAsync(cancellationToken),
            GeneratedAtUtc = DateTime.UtcNow
        };
    }

    private async Task<CaseStatisticsDto> BuildCaseStatisticsAsync(CancellationToken ct)
    {
        var statusCounts = await _uow.Cases.GetStatusCountsAsync(ct);

        return new CaseStatisticsDto
        {
            TotalCases = await _uow.Cases.GetTotalCountAsync(ct),
            ByStatus = statusCounts.ToDictionary(
                kvp => kvp.Key.ToString(), kvp => kvp.Value)
        };
    }

    private async Task<ClaimStatisticsDto> BuildClaimStatisticsAsync(CancellationToken ct)
    {
        var statusCounts = await _uow.Claims.GetCaseStatusCountsAsync(ct);

        return new ClaimStatisticsDto
        {
            TotalClaims = await _uow.Claims.GetTotalCountAsync(ct),
            ByStatus = statusCounts.ToDictionary(
                kvp => kvp.Key.ToString(), kvp => kvp.Value)
        };
    }

    private async Task<SurveyStatisticsDto> BuildSurveyStatisticsAsync(CancellationToken ct)
    {
        var statusCounts = await _uow.Surveys.GetStatusCountsAsync(ct);
        var typeCounts = await _uow.Surveys.GetTypeCountsAsync(ct);
        var now = DateTime.UtcNow;

        return new SurveyStatisticsDto
        {
            TotalSurveys = await _uow.Surveys.GetTotalCountAsync(ct),
            ByStatus = statusCounts.ToDictionary(
                kvp => kvp.Key.ToString(), kvp => kvp.Value),
            FieldSurveyCount = typeCounts.GetValueOrDefault(SurveyType.Field, 0),
            OfficeSurveyCount = typeCounts.GetValueOrDefault(SurveyType.Office, 0),
            CompletedLast7Days = await _uow.Surveys.GetFinalizedCountSinceAsync(
                now.AddDays(-7), ct),
            CompletedLast30Days = await _uow.Surveys.GetFinalizedCountSinceAsync(
                now.AddDays(-30), ct)
        };
    }

    private async Task<ImportStatisticsDto> BuildImportStatisticsAsync(CancellationToken ct)
    {
        var statusCounts = await _uow.ImportPackages.GetStatusCountsAsync(ct);
        var unresolvedConflicts = await _uow.ImportPackages.GetWithUnresolvedConflictsAsync(ct);

        var activeStatuses = new[]
        {
            ImportStatus.Pending, ImportStatus.Validating, ImportStatus.Staging,
            ImportStatus.ReviewingConflicts, ImportStatus.ReadyToCommit, ImportStatus.Committing
        };
        var activeCount = activeStatuses
            .Sum(s => statusCounts.GetValueOrDefault(s, 0));

        var contentTotals = await _uow.ImportPackages.GetCompletedContentTotalsAsync(ct);

        return new ImportStatisticsDto
        {
            TotalPackages = await _uow.ImportPackages.GetTotalCountAsync(ct),
            ByStatus = statusCounts.ToDictionary(
                kvp => kvp.Key.ToString(), kvp => kvp.Value),
            ActiveCount = activeCount,
            WithUnresolvedConflicts = unresolvedConflicts.Count,
            TotalSurveysImported = contentTotals.Surveys,
            TotalBuildingsImported = contentTotals.Buildings,
            TotalPersonsImported = contentTotals.Persons
        };
    }

    private async Task<BuildingStatisticsDto> BuildBuildingStatisticsAsync(CancellationToken ct)
    {
        var (totalBuildings, totalPropertyUnits) =
            await _uow.Buildings.GetBuildingAndUnitCountsAsync(ct);

        var byStatus = await _uow.Buildings.GetStatusCountsAsync(ct);

        return new BuildingStatisticsDto
        {
            TotalBuildings = totalBuildings,
            TotalPropertyUnits = totalPropertyUnits,
            ByStatus = byStatus.ToDictionary(
                kvp => kvp.Key.ToString(), kvp => kvp.Value),
            AverageUnitsPerBuilding = totalBuildings > 0
                ? Math.Round((double)totalPropertyUnits / totalBuildings, 2)
                : 0
        };
    }
}
