using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Dashboard.Dtos;

namespace TRRCMS.Application.Dashboard.Queries.GetDashboardTrends;

public sealed class GetDashboardTrendsQueryHandler
    : IRequestHandler<GetDashboardTrendsQuery, DashboardTrendsDto>
{
    private readonly IUnitOfWork _uow;

    public GetDashboardTrendsQueryHandler(IUnitOfWork uow)
    {
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
    }

    public async Task<DashboardTrendsDto> Handle(
        GetDashboardTrendsQuery request,
        CancellationToken cancellationToken)
    {
        var from = request.From;
        var to = request.To;

        var claims = await _uow.Claims.GetMonthlyCreationCountsAsync(from, to, cancellationToken);
        var surveys = await _uow.Surveys.GetMonthlyCreationCountsAsync(from, to, cancellationToken);
        var buildings = await _uow.Buildings.GetMonthlyCreationCountsAsync(from, to, cancellationToken);
        var persons = await _uow.Persons.GetMonthlyCreationCountsAsync(from, to, cancellationToken);
        var imports = await _uow.ImportPackages.GetMonthlyCreationCountsAsync(from, to, cancellationToken);

        return new DashboardTrendsDto
        {
            Claims = ToTrendPoints(claims),
            Surveys = ToTrendPoints(surveys),
            Buildings = ToTrendPoints(buildings),
            Persons = ToTrendPoints(persons),
            Imports = ToTrendPoints(imports),
            GeneratedAtUtc = DateTime.UtcNow
        };
    }

    private static List<MonthlyTrendPointDto> ToTrendPoints(List<(int Year, int Month, int Count)> data)
    {
        return data.Select(d => new MonthlyTrendPointDto
        {
            Year = d.Year,
            Month = d.Month,
            Label = $"{d.Year}-{d.Month:D2}",
            Count = d.Count
        }).ToList();
    }
}
