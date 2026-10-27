using AutoMapper;
using Microsoft.Extensions.Logging;
using TRRCMS.Application.Buildings.Dtos;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Conflicts.Dtos;
using TRRCMS.Application.Persons.Dtos;
using TRRCMS.Application.PropertyUnits.Dtos;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Entities.Staging;

namespace TRRCMS.Application.Conflicts.Services;

/// <summary>
/// Default implementation of <see cref="IConflictEntityLoader"/>. Encapsulates the
/// staging-vs-production branching so query handlers stay focused on orchestration.
/// </summary>
public class ConflictEntityLoader : IConflictEntityLoader
{
    private const string SourceStaging = "Staging";
    private const string SourceProduction = "Production";

    private const string EntityPerson = "Person";
    private const string EntityPropertyUnit = "PropertyUnit";
    private const string EntityBuilding = "Building";

    private const string WithinBatchSuffix = "_WithinBatch";

    private const string LocationSourceHousehold = "Household";
    private const string LocationSourceRelation = "Relation";

    private readonly IUnitOfWork _unitOfWork;
    private readonly IStagingRepository<StagingPerson> _stagingPersons;
    private readonly IStagingRepository<StagingPropertyUnit> _stagingPropertyUnits;
    private readonly IStagingRepository<StagingBuilding> _stagingBuildings;
    private readonly IStagingRepository<StagingHousehold> _stagingHouseholds;
    private readonly IStagingRepository<StagingPersonPropertyRelation> _stagingRelations;
    private readonly IMapper _mapper;
    private readonly ILogger<ConflictEntityLoader> _logger;

    public ConflictEntityLoader(
        IUnitOfWork unitOfWork,
        IStagingRepository<StagingPerson> stagingPersons,
        IStagingRepository<StagingPropertyUnit> stagingPropertyUnits,
        IStagingRepository<StagingBuilding> stagingBuildings,
        IStagingRepository<StagingHousehold> stagingHouseholds,
        IStagingRepository<StagingPersonPropertyRelation> stagingRelations,
        IMapper mapper,
        ILogger<ConflictEntityLoader> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _stagingPersons = stagingPersons ?? throw new ArgumentNullException(nameof(stagingPersons));
        _stagingPropertyUnits = stagingPropertyUnits ?? throw new ArgumentNullException(nameof(stagingPropertyUnits));
        _stagingBuildings = stagingBuildings ?? throw new ArgumentNullException(nameof(stagingBuildings));
        _stagingHouseholds = stagingHouseholds ?? throw new ArgumentNullException(nameof(stagingHouseholds));
        _stagingRelations = stagingRelations ?? throw new ArgumentNullException(nameof(stagingRelations));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<ConflictEntitySnapshotDto> LoadFirstEntityAsync(
        ConflictResolution conflict,
        CancellationToken cancellationToken)
    {
        if (conflict is null) throw new ArgumentNullException(nameof(conflict));

        return LoadStagingSnapshotAsync(
            conflict.EntityType,
            conflict.ImportPackageId,
            conflict.FirstEntityId,
            cancellationToken);
    }

    public Task<ConflictEntitySnapshotDto> LoadSecondEntityAsync(
        ConflictResolution conflict,
        CancellationToken cancellationToken)
    {
        if (conflict is null) throw new ArgumentNullException(nameof(conflict));

        var isWithinBatch = !string.IsNullOrEmpty(conflict.ConflictType)
            && conflict.ConflictType.EndsWith(WithinBatchSuffix, StringComparison.OrdinalIgnoreCase);

        if (isWithinBatch)
        {
            return LoadStagingSnapshotAsync(
                conflict.EntityType,
                conflict.ImportPackageId,
                conflict.SecondEntityId,
                cancellationToken);
        }

        return LoadProductionSnapshotAsync(
            conflict.EntityType,
            conflict.SecondEntityId,
            cancellationToken);
    }

    private async Task<ConflictEntitySnapshotDto> LoadStagingSnapshotAsync(
        string entityType,
        Guid? importPackageId,
        Guid originalEntityId,
        CancellationToken ct)
    {
        var snapshot = new ConflictEntitySnapshotDto
        {
            Source = SourceStaging,
            EntityType = entityType,
            ImportPackageId = importPackageId
        };

        if (importPackageId is null || importPackageId == Guid.Empty)
        {
            _logger.LogWarning(
                "Cannot load staging snapshot for {EntityType} {OriginalEntityId}: " +
                "ConflictResolution.ImportPackageId is null. Returning empty wrapper.",
                entityType, originalEntityId);
            return snapshot;
        }

        switch (entityType)
        {
            case EntityPerson:
            {
                var row = await _stagingPersons.GetByPackageAndOriginalIdAsync(
                    importPackageId.Value, originalEntityId, ct);
                if (row is not null)
                {
                    snapshot.Person = _mapper.Map<PersonDto>(row);
                    PopulateStagingMetadata(snapshot, row);
                    snapshot.PersonLocation = await TryLoadLocationAsync(
                        () => LoadStagingPersonLocationAsync(importPackageId.Value, row, ct),
                        originalEntityId);
                }
                break;
            }
            case EntityPropertyUnit:
            {
                var row = await _stagingPropertyUnits.GetByPackageAndOriginalIdAsync(
                    importPackageId.Value, originalEntityId, ct);
                if (row is not null)
                {
                    snapshot.PropertyUnit = _mapper.Map<PropertyUnitDto>(row);
                    PopulateStagingMetadata(snapshot, row);
                }
                break;
            }
            case EntityBuilding:
            {
                var row = await _stagingBuildings.GetByPackageAndOriginalIdAsync(
                    importPackageId.Value, originalEntityId, ct);
                if (row is not null)
                {
                    snapshot.Building = _mapper.Map<BuildingDto>(row);
                    PopulateStagingMetadata(snapshot, row);
                }
                break;
            }
            default:
                _logger.LogWarning(
                    "Unsupported EntityType '{EntityType}' on conflict; staging snapshot left empty.",
                    entityType);
                break;
        }

        return snapshot;
    }

    private async Task<ConflictEntitySnapshotDto> LoadProductionSnapshotAsync(
        string entityType,
        Guid entityId,
        CancellationToken ct)
    {
        var snapshot = new ConflictEntitySnapshotDto
        {
            Source = SourceProduction,
            EntityType = entityType
        };

        switch (entityType)
        {
            case EntityPerson:
            {
                var row = await _unitOfWork.Persons.GetByIdAsync(entityId, ct);
                if (row is not null)
                {
                    snapshot.Person = _mapper.Map<PersonDto>(row);
                    snapshot.PersonLocation = await TryLoadLocationAsync(
                        () => LoadProductionPersonLocationAsync(row, ct),
                        entityId);
                }
                break;
            }
            case EntityPropertyUnit:
            {
                var row = await _unitOfWork.PropertyUnits.GetByIdAsync(entityId, ct);
                if (row is not null)
                    snapshot.PropertyUnit = _mapper.Map<PropertyUnitDto>(row);
                break;
            }
            case EntityBuilding:
            {
                var row = await _unitOfWork.Buildings.GetByIdAsync(entityId, ct);
                if (row is not null)
                    snapshot.Building = _mapper.Map<BuildingDto>(row);
                break;
            }
            default:
                _logger.LogWarning(
                    "Unsupported EntityType '{EntityType}' on conflict; production snapshot left empty.",
                    entityType);
                break;
        }

        return snapshot;
    }

    /// <summary>
    /// Location is supplementary: a failure to resolve it must never fail the conflict
    /// details response, so errors are logged and the location is left null.
    /// </summary>
    private async Task<PersonLocationDto?> TryLoadLocationAsync(
        Func<Task<PersonLocationDto?>> load,
        Guid personId)
    {
        try
        {
            return await load();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Could not resolve unit/building for person {PersonId}; location left empty.",
                personId);
            return null;
        }
    }

    /// <summary>
    /// Household unit first; falls back to the person's relations in the same package
    /// (mirrors CommitService, which infers household membership from relations when the
    /// mobile package does not set persons.household_id).
    /// </summary>
    private async Task<PersonLocationDto?> LoadStagingPersonLocationAsync(
        Guid importPackageId,
        StagingPerson person,
        CancellationToken ct)
    {
        var relatedUnitIds = (await _stagingRelations.GetByPackageIdAsync(importPackageId, ct))
            .Where(r => r.OriginalPersonId == person.OriginalEntityId)
            .Select(r => r.OriginalPropertyUnitId)
            .Distinct()
            .ToList();

        Guid? unitId = null;
        var source = LocationSourceHousehold;
        if (person.OriginalHouseholdId is Guid householdId)
        {
            var household = await _stagingHouseholds.GetByPackageAndOriginalIdAsync(
                importPackageId, householdId, ct);
            unitId = household?.OriginalPropertyUnitId;
        }
        if (unitId is null && relatedUnitIds.Count > 0)
        {
            unitId = relatedUnitIds[0];
            source = LocationSourceRelation;
        }
        if (unitId is null)
            return null;

        var unit = await _stagingPropertyUnits.GetByPackageAndOriginalIdAsync(
            importPackageId, unitId.Value, ct);
        if (unit is null)
            return null;

        var building = await _stagingBuildings.GetByPackageAndOriginalIdAsync(
            importPackageId, unit.OriginalBuildingId, ct);

        return new PersonLocationDto
        {
            BuildingCode = building is null
                ? null
                : building.BuildingId ?? $"{building.GovernorateCode}{building.DistrictCode}" +
                  $"{building.SubDistrictCode}{building.CommunityCode}" +
                  $"{building.NeighborhoodCode}{building.BuildingNumber}",
            CommunityName = NameOrCode(building?.CommunityName, building?.CommunityCode),
            NeighborhoodName = NameOrCode(building?.NeighborhoodName, building?.NeighborhoodCode),
            UnitIdentifier = unit.UnitIdentifier,
            UnitType = (int)unit.UnitType,
            FloorNumber = unit.FloorNumber,
            Source = source,
            OtherUnitsCount = relatedUnitIds.Count(id => id != unitId.Value)
        };
    }

    /// <summary>
    /// Household unit first; falls back to the person's most recent active relation.
    /// </summary>
    private async Task<PersonLocationDto?> LoadProductionPersonLocationAsync(
        Person person,
        CancellationToken ct)
    {
        var relatedUnitIds = (await _unitOfWork.PersonPropertyRelations
                .GetActiveRelationsByPersonIdAsync(person.Id, ct))
            .OrderByDescending(r => r.CreatedAtUtc)
            .Select(r => r.PropertyUnitId)
            .Distinct()
            .ToList();

        Guid? unitId = null;
        var source = LocationSourceHousehold;
        if (person.HouseholdId is Guid householdId)
        {
            var household = await _unitOfWork.Households.GetByIdAsync(householdId, ct);
            unitId = household?.PropertyUnitId;
        }
        if (unitId is null && relatedUnitIds.Count > 0)
        {
            unitId = relatedUnitIds[0];
            source = LocationSourceRelation;
        }
        if (unitId is null)
            return null;

        var unit = await _unitOfWork.PropertyUnits.GetByIdAsync(unitId.Value, ct);
        if (unit is null)
            return null;

        var building = await _unitOfWork.Buildings.GetByIdAsync(unit.BuildingId, ct);

        return new PersonLocationDto
        {
            BuildingCode = building?.BuildingId,
            CommunityName = NameOrCode(building?.CommunityName, building?.CommunityCode),
            NeighborhoodName = NameOrCode(building?.NeighborhoodName, building?.NeighborhoodCode),
            UnitIdentifier = unit.UnitIdentifier,
            UnitType = (int)unit.UnitType,
            FloorNumber = unit.FloorNumber,
            Source = source,
            OtherUnitsCount = relatedUnitIds.Count(id => id != unitId.Value)
        };
    }

    private static string? NameOrCode(string? name, string? code)
        => string.IsNullOrWhiteSpace(name) ? code : name;

    private static void PopulateStagingMetadata(
        ConflictEntitySnapshotDto snapshot,
        Domain.Common.BaseStagingEntity row)
    {
        snapshot.StagingRowId = row.Id;
        snapshot.StagingValidationStatus = row.ValidationStatus.ToString();
        snapshot.IsApprovedForCommit = row.IsApprovedForCommit;
    }
}
