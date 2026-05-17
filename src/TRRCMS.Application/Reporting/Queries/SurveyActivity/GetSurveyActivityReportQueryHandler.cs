using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Reporting.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Reporting.Queries.SurveyActivity;

/// <summary>
/// Builds the survey-activity report payload by reusing the same repository
/// aggregates that feed GET /api/v1/dashboard/personnel.
/// </summary>
public sealed class GetSurveyActivityReportQueryHandler
    : IRequestHandler<GetSurveyActivityReportQuery, SurveyActivityReportDto>
{
    private readonly IUnitOfWork _uow;

    public GetSurveyActivityReportQueryHandler(IUnitOfWork uow)
    {
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
    }

    public async Task<SurveyActivityReportDto> Handle(
        GetSurveyActivityReportQuery request,
        CancellationToken cancellationToken)
    {
        var fieldCollectors = await _uow.Users.GetUsersByRoleAsync(UserRole.FieldCollector, activeOnly: true, cancellationToken);
        var officeClerks = await _uow.Users.GetUsersByRoleAsync(UserRole.OfficeClerk, activeOnly: true, cancellationToken);
        var surveyCounts = await _uow.Surveys.GetCountsByCollectorAsync(request.From, request.To, cancellationToken);
        var buildingCounts = await _uow.BuildingAssignments.GetCountsByFieldCollectorAsync(request.From, request.To, cancellationToken);

        var surveyMap = surveyCounts.ToDictionary(x => x.UserId);
        var buildingMap = buildingCounts.ToDictionary(x => x.UserId);

        var fcRows = fieldCollectors.Select(u =>
        {
            var hasS = surveyMap.TryGetValue(u.Id, out var s);
            var hasB = buildingMap.TryGetValue(u.Id, out var b);
            return new SurveyActivityRow
            {
                UserId = u.Id,
                Username = u.Username,
                FullName = u.FullNameArabic,
                SurveysCompleted = hasS ? s.Completed : 0,
                SurveysDraft = hasS ? s.Draft : 0,
                TotalSurveys = hasS ? s.Total : 0,
                AssignedBuildings = hasB ? b.Assigned : 0,
                CompletedBuildings = hasB ? b.Completed : 0
            };
        }).ToList();

        var ocRows = officeClerks.Select(u =>
        {
            var hasS = surveyMap.TryGetValue(u.Id, out var s);
            return new SurveyActivityRow
            {
                UserId = u.Id,
                Username = u.Username,
                FullName = u.FullNameArabic,
                SurveysCompleted = hasS ? s.Completed : 0,
                SurveysDraft = hasS ? s.Draft : 0,
                TotalSurveys = hasS ? s.Total : 0
            };
        }).ToList();

        return new SurveyActivityReportDto
        {
            FromUtc = request.From,
            ToUtc = request.To,
            GeneratedAtUtc = DateTime.UtcNow,
            FieldCollectors = fcRows,
            OfficeClerks = ocRows
        };
    }
}
