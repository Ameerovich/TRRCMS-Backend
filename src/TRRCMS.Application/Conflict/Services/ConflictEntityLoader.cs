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
    private readonly IMapper _mapper;
    private readonly ILogger<ConflictEntityLoader> _logger;

    public ConflictEntityLoader(
        IUnitOfWork unitOfWork,
        IStagingRepository<StagingPerson> stagingPersons,
        IStagingRepository<StagingPropertyUnit> stagingPropertyUnits,
        IStagingRepository<StagingBuilding> stagingBuildings,
        IMapper mapper,
        ILogger<ConflictEntityLoader> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _stagingPersons = stagingPersons ?? throw new ArgumentNullException(nameof(stagingPersons));
        _stagingPropertyUnits = stagingPropertyUnits ?? throw new ArgumentNullException(nameof(stagingPropertyUnits));
        _stagingBuildings = stagingBuildings ?? throw new ArgumentNullException(nameof(stagingBuildings));
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

    private static void PopulateStagingMetadata(
        ConflictEntitySnapshotDto snapshot,
        Domain.Common.BaseStagingEntity row)
    {
        snapshot.StagingRowId = row.Id;
        snapshot.StagingValidationStatus = row.ValidationStatus.ToString();
        snapshot.IsApprovedForCommit = row.IsApprovedForCommit;
    }
}
