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
                    snapshot.Person = _mapper.Map<PersonDto>(row);
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

    public async Task PopulatePersonLocationsAsync(
        ConflictEntitySnapshotDto first,
        ConflictEntitySnapshotDto second,
        CancellationToken cancellationToken)
    {
        var firstUnits = await TryLoadPersonUnitsAsync(first, cancellationToken);
        var secondUnits = await TryLoadPersonUnitsAsync(second, cancellationToken);

        // A unit both persons are linked to is the strongest evidence for the
        // reviewer, so it wins over each side's own priority order.
        foreach (var unit in firstUnits)
        {
            var match = secondUnits.FirstOrDefault(other => IsSameUnit(unit, other));
            if (match is not null)
            {
                first.PersonLocation = unit;
                second.PersonLocation = match;
                return;
            }
        }

        first.PersonLocation = firstUnits.FirstOrDefault();
        second.PersonLocation = secondUnits.FirstOrDefault();
    }

    /// <summary>
    /// Staging and production units have different ids, so the same physical unit
    /// is matched on building code + unit identifier.
    /// </summary>
    private static bool IsSameUnit(PersonLocationDto a, PersonLocationDto b)
        => !string.IsNullOrWhiteSpace(a.BuildingCode)
           && !string.IsNullOrWhiteSpace(a.UnitIdentifier)
           && string.Equals(a.BuildingCode.Trim(), b.BuildingCode?.Trim(), StringComparison.OrdinalIgnoreCase)
           && string.Equals(a.UnitIdentifier.Trim(), b.UnitIdentifier?.Trim(), StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Location is supplementary: a failure to resolve it must never fail the conflict
    /// details response, so errors are logged and the person gets no units.
    /// </summary>
    private async Task<List<PersonLocationDto>> TryLoadPersonUnitsAsync(
        ConflictEntitySnapshotDto snapshot,
        CancellationToken ct)
    {
        var person = snapshot.Person;
        if (person is null)
            return new List<PersonLocationDto>();

        try
        {
            return snapshot.Source == SourceStaging
                ? await LoadStagingPersonUnitsAsync(snapshot.ImportPackageId, person, ct)
                : await LoadProductionPersonUnitsAsync(person, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Could not resolve units for person {PersonId}; location left empty.",
                person.Id);
            return new List<PersonLocationDto>();
        }
    }

    /// <summary>
    /// Units of a staging person in priority order: household unit first, then the
    /// person's relations in the same package (mirrors CommitService, which infers
    /// household membership from relations when the mobile package does not set
    /// persons.household_id). For staging DTOs, Id/HouseholdId are the original ids.
    /// </summary>
    private async Task<List<PersonLocationDto>> LoadStagingPersonUnitsAsync(
        Guid? importPackageId,
        PersonDto person,
        CancellationToken ct)
    {
        var locations = new List<PersonLocationDto>();
        if (importPackageId is not Guid packageId)
            return locations;

        var unitIds = new List<Guid>();
        if (person.HouseholdId is Guid householdId)
        {
            var household = await _stagingHouseholds.GetByPackageAndOriginalIdAsync(
                packageId, householdId, ct);
            if (household is not null)
                unitIds.Add(household.OriginalPropertyUnitId);
        }
        unitIds.AddRange((await _stagingRelations.GetByPackageIdAsync(packageId, ct))
            .Where(r => r.OriginalPersonId == person.Id)
            .Select(r => r.OriginalPropertyUnitId));

        foreach (var unitId in unitIds.Distinct())
        {
            var unit = await _stagingPropertyUnits.GetByPackageAndOriginalIdAsync(
                packageId, unitId, ct);
            if (unit is null)
                continue;

            var building = await _stagingBuildings.GetByPackageAndOriginalIdAsync(
                packageId, unit.OriginalBuildingId, ct);

            locations.Add(new PersonLocationDto
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
                FloorNumber = unit.FloorNumber
            });
        }

        return locations;
    }

    /// <summary>
    /// Units of a production person in priority order: household unit first, then
    /// active relations, most recent first.
    /// </summary>
    private async Task<List<PersonLocationDto>> LoadProductionPersonUnitsAsync(
        PersonDto person,
        CancellationToken ct)
    {
        var unitIds = new List<Guid>();
        if (person.HouseholdId is Guid householdId)
        {
            var household = await _unitOfWork.Households.GetByIdAsync(householdId, ct);
            if (household is not null)
                unitIds.Add(household.PropertyUnitId);
        }
        unitIds.AddRange((await _unitOfWork.PersonPropertyRelations
                .GetActiveRelationsByPersonIdAsync(person.Id, ct))
            .OrderByDescending(r => r.CreatedAtUtc)
            .Select(r => r.PropertyUnitId));

        var locations = new List<PersonLocationDto>();
        foreach (var unitId in unitIds.Distinct())
        {
            var unit = await _unitOfWork.PropertyUnits.GetByIdAsync(unitId, ct);
            if (unit is null)
                continue;

            var building = await _unitOfWork.Buildings.GetByIdAsync(unit.BuildingId, ct);

            locations.Add(new PersonLocationDto
            {
                BuildingCode = building?.BuildingId,
                CommunityName = NameOrCode(building?.CommunityName, building?.CommunityCode),
                NeighborhoodName = NameOrCode(building?.NeighborhoodName, building?.NeighborhoodCode),
                UnitIdentifier = unit.UnitIdentifier,
                UnitType = (int)unit.UnitType,
                FloorNumber = unit.FloorNumber
            });
        }

        return locations;
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
