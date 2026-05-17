using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Reporting.Dtos;

namespace TRRCMS.Application.Reporting.Queries.BuildingInventory;

/// <summary>
/// Builds the building &amp; property inventory report by reusing repository
/// aggregates from the geographic dashboard, optionally filtered to one neighborhood.
/// </summary>
public sealed class GetBuildingInventoryReportQueryHandler
    : IRequestHandler<GetBuildingInventoryReportQuery, BuildingInventoryReportDto>
{
    private readonly IUnitOfWork _uow;

    public GetBuildingInventoryReportQueryHandler(IUnitOfWork uow)
    {
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
    }

    public async Task<BuildingInventoryReportDto> Handle(
        GetBuildingInventoryReportQuery request,
        CancellationToken cancellationToken)
    {
        var neighborhoods = await _uow.Neighborhoods.GetAllAsync(cancellationToken: cancellationToken);
        var countsList = await _uow.Buildings.GetCountsByNeighborhoodAsync(cancellationToken);
        var totalNeighborhoods = await _uow.Neighborhoods.GetTotalCountAsync(cancellationToken);

        var countsMap = countsList.ToDictionary(x => x.NeighborhoodCode,
            x => (x.BuildingCount, x.PropertyUnitCount));

        var rows = neighborhoods
            .Where(n => request.NeighborhoodCode is null ||
                        string.Equals(n.FullCode, request.NeighborhoodCode, StringComparison.OrdinalIgnoreCase))
            .Select(n =>
            {
                var has = countsMap.TryGetValue(n.FullCode, out var c);
                return new BuildingInventoryRow
                {
                    NeighborhoodCode = n.FullCode,
                    NameArabic = n.NameArabic,
                    NameEnglish = n.NameEnglish,
                    BuildingCount = has ? c.BuildingCount : 0,
                    PropertyUnitCount = has ? c.PropertyUnitCount : 0
                };
            })
            .OrderByDescending(r => r.BuildingCount)
            .ToList();

        return new BuildingInventoryReportDto
        {
            NeighborhoodCodeFilter = request.NeighborhoodCode,
            GeneratedAtUtc = DateTime.UtcNow,
            TotalNeighborhoods = totalNeighborhoods,
            NeighborhoodsWithBuildings = rows.Count(r => r.BuildingCount > 0),
            TotalBuildings = rows.Sum(r => r.BuildingCount),
            TotalPropertyUnits = rows.Sum(r => r.PropertyUnitCount),
            Rows = rows
        };
    }
}
