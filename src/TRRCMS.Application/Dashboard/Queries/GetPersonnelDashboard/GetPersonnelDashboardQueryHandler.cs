using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Dashboard.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Dashboard.Queries.GetPersonnelDashboard;

public sealed class GetPersonnelDashboardQueryHandler
    : IRequestHandler<GetPersonnelDashboardQuery, PersonnelDashboardDto>
{
    private readonly IUnitOfWork _uow;

    public GetPersonnelDashboardQueryHandler(IUnitOfWork uow)
    {
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
    }

    public async Task<PersonnelDashboardDto> Handle(
        GetPersonnelDashboardQuery request,
        CancellationToken cancellationToken)
    {
        var from = request.From;
        var to = request.To;

        var fieldCollectors = await _uow.Users.GetUsersByRoleAsync(UserRole.FieldCollector, activeOnly: true, cancellationToken);
        var officeClerks = await _uow.Users.GetUsersByRoleAsync(UserRole.OfficeClerk, activeOnly: true, cancellationToken);
        var surveyCounts = await _uow.Surveys.GetCountsByCollectorAsync(from, to, cancellationToken);
        var buildingAssignmentCounts = await _uow.BuildingAssignments.GetCountsByFieldCollectorAsync(from, to, cancellationToken);

        var surveyCountsByUser = surveyCounts.ToDictionary(x => x.UserId);
        var buildingCountsByUser = buildingAssignmentCounts.ToDictionary(x => x.UserId);

        var fieldCollectorWorkloads = fieldCollectors.Select(user =>
        {
            var hasSurveys = surveyCountsByUser.TryGetValue(user.Id, out var surveyCounts);
            var hasBuildings = buildingCountsByUser.TryGetValue(user.Id, out var buildingCounts);

            return new UserWorkloadDto
            {
                UserId = user.Id,
                Username = user.Username,
                FullName = user.FullNameArabic,
                SurveysCompleted = hasSurveys ? surveyCounts.Completed : 0,
                SurveysDraft = hasSurveys ? surveyCounts.Draft : 0,
                TotalSurveys = hasSurveys ? surveyCounts.Total : 0,
                AssignedBuildings = hasBuildings ? buildingCounts.Assigned : 0,
                CompletedBuildings = hasBuildings ? buildingCounts.Completed : 0
            };
        }).ToList();

        var officeClerkWorkloads = officeClerks.Select(user =>
        {
            var hasSurveys = surveyCountsByUser.TryGetValue(user.Id, out var surveyCounts);

            return new UserWorkloadDto
            {
                UserId = user.Id,
                Username = user.Username,
                FullName = user.FullNameArabic,
                SurveysCompleted = hasSurveys ? surveyCounts.Completed : 0,
                SurveysDraft = hasSurveys ? surveyCounts.Draft : 0,
                TotalSurveys = hasSurveys ? surveyCounts.Total : 0
            };
        }).ToList();

        return new PersonnelDashboardDto
        {
            FieldCollectors = fieldCollectorWorkloads,
            OfficeClerks = officeClerkWorkloads,
            GeneratedAtUtc = DateTime.UtcNow
        };
    }
}
