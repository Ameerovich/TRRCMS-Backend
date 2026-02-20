using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.PropertyUnits.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.PropertyUnits.Queries.GetAllPropertyUnits;

/// <summary>
/// Handler for GetAllPropertyUnitsQuery
/// Handles filtering by building, unit type, and status
/// Returns results grouped by building (or as flat list if ungrouped)
/// </summary>
public class GetAllPropertyUnitsQueryHandler
    : IRequestHandler<GetAllPropertyUnitsQuery, GroupedPropertyUnitsResponseDto>
{
    private readonly IPropertyUnitRepository _propertyUnitRepository;
    private readonly IBuildingRepository _buildingRepository;
    private readonly IMapper _mapper;

    public GetAllPropertyUnitsQueryHandler(
        IPropertyUnitRepository propertyUnitRepository,
        IBuildingRepository buildingRepository,
        IMapper mapper)
    {
        _propertyUnitRepository = propertyUnitRepository;
        _buildingRepository = buildingRepository;
        _mapper = mapper;
    }

    public async Task<GroupedPropertyUnitsResponseDto> Handle(
        GetAllPropertyUnitsQuery request,
        CancellationToken cancellationToken)
    {
        // Cast integer filters to enums for repository call
        PropertyUnitType? typeFilter = request.UnitType.HasValue
            ? (PropertyUnitType)request.UnitType.Value
            : null;

        PropertyUnitStatus? statusFilter = request.Status.HasValue
            ? (PropertyUnitStatus)request.Status.Value
            : null;

        // Get filtered property units from repository
        var propertyUnits = await _propertyUnitRepository.GetFilteredAsync(
            request.BuildingId,
            typeFilter,
            statusFilter,
            cancellationToken);

        // Get distinct building IDs for batch loading
        var buildingIds = propertyUnits.Select(p => p.BuildingId).Distinct().ToList();

        // Fetch all buildings in batch (avoid N+1 queries)
        var buildingDict = new Dictionary<Guid, string>();
        foreach (var buildingId in buildingIds)
        {
            var building = await _buildingRepository.GetByIdAsync(buildingId, cancellationToken);
            if (building != null)
            {
                buildingDict[buildingId] = building.BuildingNumber;
            }
        }

        // Map to DTOs and enrich with building numbers
        var unitDtos = propertyUnits.Select(unit =>
        {
            var dto = _mapper.Map<PropertyUnitDto>(unit);
            dto.BuildingNumber = buildingDict.GetValueOrDefault(unit.BuildingId);
            return dto;
        }).ToList();

        // Group by building if requested (default: true)
        if (request.GroupByBuilding)
        {
            var grouped = unitDtos
                .GroupBy(u => u.BuildingId)
                .Select(g => new BuildingWithUnitsDto
                {
                    BuildingId = g.Key,
                    BuildingNumber = g.First().BuildingNumber ?? string.Empty,
                    UnitCount = g.Count(),
                    PropertyUnits = g
                        .OrderBy(u => u.FloorNumber)
                        .ThenBy(u => u.UnitIdentifier)
                        .ToList()
                })
                .OrderBy(b => b.BuildingNumber)
                .ToList();

            return new GroupedPropertyUnitsResponseDto
            {
                GroupedByBuilding = grouped,
                TotalUnits = unitDtos.Count,
                TotalBuildings = grouped.Count
            };
        }
        else
        {
            // Return as single "ungrouped" entry with all units
            return new GroupedPropertyUnitsResponseDto
            {
                GroupedByBuilding = new List<BuildingWithUnitsDto>
                {
                    new BuildingWithUnitsDto
                    {
                        BuildingId = Guid.Empty,
                        BuildingNumber = "All Units",
                        UnitCount = unitDtos.Count,
                        PropertyUnits = unitDtos
                    }
                },
                TotalUnits = unitDtos.Count,
                TotalBuildings = buildingIds.Count
            };
        }
    }
}