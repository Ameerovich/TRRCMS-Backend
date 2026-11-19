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
        // Counts per (user, survey type): each role is measured on the survey type it works
        // on, matching what the desktop lists for that user (an office clerk sees only his
        // office surveys). Surveys of the other type stay on the same row (OtherType*);
        // users outside both lists are reported under Others.
        var surveyCounts = await _uow.Surveys.GetCountsByCollectorAndTypeAsync(from, to, cancellationToken);
        var buildingAssignmentCounts = await _uow.BuildingAssignments.GetSurveyedCountsByFieldCollectorAsync(from, to, cancellationToken);

        var countsByUserAndType = surveyCounts.ToDictionary(x => (x.UserId, x.Type));
        var buildingCountsByUser = buildingAssignmentCounts.ToDictionary(x => x.UserId);
        var counted = new HashSet<(Guid, SurveyType)>();

        (int Completed, int Draft, int Total) Take(Guid userId, SurveyType type)
        {
            counted.Add((userId, type));
            return countsByUserAndType.TryGetValue((userId, type), out var c)
                ? (c.Completed, c.Draft, c.Total)
                : (0, 0, 0);
        }

        var fieldCollectorWorkloads = fieldCollectors.Select(user =>
        {
            var surveys = Take(user.Id, SurveyType.Field);
            var otherType = Take(user.Id, SurveyType.Office);
            var hasBuildings = buildingCountsByUser.TryGetValue(user.Id, out var buildingCounts);

            return new UserWorkloadDto
            {
                UserId = user.Id,
                Username = user.Username,
                FullName = user.FullNameArabic,
                SurveysCompleted = surveys.Completed,
                SurveysDraft = surveys.Draft,
                TotalSurveys = surveys.Total,
                OtherTypeSurveysTotal = otherType.Total,
                OtherTypeSurveysDraft = otherType.Draft,
                AssignedBuildings = hasBuildings ? buildingCounts.Assigned : 0,
                CompletedBuildings = hasBuildings ? buildingCounts.Completed : 0
            };
        }).ToList();

        var officeClerkWorkloads = officeClerks.Select(user =>
        {
            var surveys = Take(user.Id, SurveyType.Office);
            var otherType = Take(user.Id, SurveyType.Field);

            return new UserWorkloadDto
            {
                UserId = user.Id,
                Username = user.Username,
                FullName = user.FullNameArabic,
                SurveysCompleted = surveys.Completed,
                SurveysDraft = surveys.Draft,
                TotalSurveys = surveys.Total,
                OtherTypeSurveysTotal = otherType.Total,
                OtherTypeSurveysDraft = otherType.Draft
            };
        }).ToList();

        var otherWorkloads = new List<UserWorkloadDto>();
        foreach (var group in surveyCounts
                     .Where(c => !counted.Contains((c.UserId, c.Type)))
                     .GroupBy(c => c.UserId))
        {
            var user = await _uow.Users.GetByIdAsync(group.Key, cancellationToken);
            otherWorkloads.Add(new UserWorkloadDto
            {
                UserId = group.Key,
                Username = user?.Username ?? string.Empty,
                FullName = user?.FullNameArabic ?? string.Empty,
                Role = user?.Role.ToString(),
                SurveysCompleted = group.Sum(c => c.Completed),
                SurveysDraft = group.Sum(c => c.Draft),
                TotalSurveys = group.Sum(c => c.Total)
            });
        }

        return new PersonnelDashboardDto
        {
            FieldCollectors = fieldCollectorWorkloads,
            OfficeClerks = officeClerkWorkloads,
            Others = otherWorkloads
                .OrderByDescending(o => o.SurveysDraft)
                .ThenByDescending(o => o.TotalSurveys)
                .ToList(),
            GeneratedAtUtc = DateTime.UtcNow
        };
    }
}
