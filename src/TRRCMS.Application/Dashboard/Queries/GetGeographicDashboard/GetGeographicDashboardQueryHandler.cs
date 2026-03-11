using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Dashboard.Dtos;

namespace TRRCMS.Application.Dashboard.Queries.GetGeographicDashboard;

public sealed class GetGeographicDashboardQueryHandler
    : IRequestHandler<GetGeographicDashboardQuery, GeographicDashboardDto>
{
    private readonly IUnitOfWork _uow;

    public GetGeographicDashboardQueryHandler(IUnitOfWork uow)
    {
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
    }

    public async Task<GeographicDashboardDto> Handle(
        GetGeographicDashboardQuery request,
        CancellationToken cancellationToken)
    {
        var neighborhoods = await _uow.Neighborhoods.GetAllAsync(cancellationToken: cancellationToken);
        var buildingCountsList = await _uow.Buildings.GetCountsByNeighborhoodAsync(cancellationToken);
        var totalNeighborhoods = await _uow.Neighborhoods.GetTotalCountAsync(cancellationToken);

        var buildingCounts = buildingCountsList
            .ToDictionary(x => x.NeighborhoodCode, x => (x.BuildingCount, x.PropertyUnitCount));

        var coverageList = neighborhoods.Select(n =>
        {
            var hasCounts = buildingCounts.TryGetValue(n.FullCode, out var counts);
            return new NeighborhoodCoverageDto
            {
                Code = n.FullCode,
                NameArabic = n.NameArabic,
                NameEnglish = n.NameEnglish,
                BuildingCount = hasCounts ? counts.BuildingCount : 0,
                PropertyUnitCount = hasCounts ? counts.PropertyUnitCount : 0
            };
        }).ToList();

        return new GeographicDashboardDto
        {
            TotalNeighborhoods = totalNeighborhoods,
            NeighborhoodsWithBuildings = coverageList.Count(n => n.BuildingCount > 0),
            Neighborhoods = coverageList,
            GeneratedAtUtc = DateTime.UtcNow
        };
    }
}
