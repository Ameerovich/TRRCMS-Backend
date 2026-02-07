using System.Text.Json;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Conflicts.Dtos;

namespace TRRCMS.Infrastructure.Services.Merge;

/// <summary>
/// Merges duplicate Building or PropertyUnit records as part of conflict resolution (UC-007 S06–S07).
/// 
/// Merge strategy:
/// 1. Master entity keeps its own non-null fields; gaps filled from discarded.
/// 2. PropertyUnits under discarded building → re-parented to master building.
/// 3. PersonPropertyRelations referencing discarded units → re-pointed to master units.
/// 4. Surveys referencing discarded building/unit → re-pointed to master.
/// 5. Claims referencing discarded units → re-pointed to master.
/// 6. Discarded entity is soft-deleted.
/// 7. Merge mapping JSON is built for full audit trail.
///
/// </summary>
public class PropertyMergeService : IMergeService
{
    private readonly IBuildingRepository _buildingRepository;
    private readonly IPropertyUnitRepository _propertyUnitRepository;
    private readonly IPersonPropertyRelationRepository _relationRepository;
    private readonly ISurveyRepository _surveyRepository;
    private readonly IClaimRepository _claimRepository;

    public PropertyMergeService(
        IBuildingRepository buildingRepository,
        IPropertyUnitRepository propertyUnitRepository,
        IPersonPropertyRelationRepository relationRepository,
        ISurveyRepository surveyRepository,
        IClaimRepository claimRepository)
    {
        _buildingRepository = buildingRepository
            ?? throw new ArgumentNullException(nameof(buildingRepository));
        _propertyUnitRepository = propertyUnitRepository
            ?? throw new ArgumentNullException(nameof(propertyUnitRepository));
        _relationRepository = relationRepository
            ?? throw new ArgumentNullException(nameof(relationRepository));
        _surveyRepository = surveyRepository
            ?? throw new ArgumentNullException(nameof(surveyRepository));
        _claimRepository = claimRepository
            ?? throw new ArgumentNullException(nameof(claimRepository));
    }

    /// <inheritdoc />
    public string EntityType => "Building";

    /// <inheritdoc />
    public async Task<MergeResultDto> MergeAsync(
        Guid masterEntityId,
        Guid discardedEntityId,
        CancellationToken cancellationToken = default)
    {
        var result = new MergeResultDto
        {
            MasterEntityId = masterEntityId,
            DiscardedEntityId = discardedEntityId
        };

        try
        {
            // 1. Load both buildings
            var master = await _buildingRepository.GetByIdAsync(masterEntityId, cancellationToken)
                ?? throw new InvalidOperationException(
                    $"Master building with ID {masterEntityId} not found.");

            var discarded = await _buildingRepository.GetByIdAsync(discardedEntityId, cancellationToken)
                ?? throw new InvalidOperationException(
                    $"Discarded building with ID {discardedEntityId} not found.");

            var mergeMapping = new Dictionary<string, string>();
            MergeBuildingFields(master, discarded, mergeMapping);

            // 2. Re-parent property units from discarded → master building
            var discardedUnits = (await _propertyUnitRepository
                .GetByBuildingIdAsync(discardedEntityId, cancellationToken)).ToList();
            var unitsUpdated = 0;

            foreach (var unit in discardedUnits)
            {
                unit.ReParentToBuilding(masterEntityId, masterEntityId);
                await _propertyUnitRepository.UpdateAsync(unit, cancellationToken);
                unitsUpdated++;

                // 2a. Re-point relations for each re-parented unit
                await RePointRelationsForUnitAsync(unit.Id, cancellationToken);
            }

            // 3. Re-point surveys referencing discarded building
            var discardedSurveys = (await _surveyRepository
                .GetByBuildingAsync(discardedEntityId, cancellationToken)).ToList();
            var surveysUpdated = 0;

            foreach (var survey in discardedSurveys)
            {
                survey.UpdateBuildingId(masterEntityId, masterEntityId);
                await _surveyRepository.UpdateAsync(survey, cancellationToken);
                surveysUpdated++;
            }

            // 4. Soft-delete discarded building
            discarded.MarkAsDeleted(masterEntityId);
            await _buildingRepository.UpdateAsync(discarded, cancellationToken);

            // 5. Save master with merged fields
            await _buildingRepository.UpdateAsync(master, cancellationToken);
            await _buildingRepository.SaveChangesAsync(cancellationToken);

            // 6. Build result
            result.Success = true;
            result.MergeMappingJson = JsonSerializer.Serialize(mergeMapping);
            result.ReferencesUpdated = unitsUpdated + surveysUpdated;
            result.ReferencesByType = new Dictionary<string, int>
            {
                ["PropertyUnit"] = unitsUpdated,
                ["Survey"] = surveysUpdated
            };
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    /// <summary>
    /// Re-points PersonPropertyRelations that reference a given property unit.
    /// This is a no-op for FK propagation since the unit ID itself stays the same
    /// (the unit is re-parented, not merged). Included as an extension point
    /// for PropertyUnit-level merges if two units collapse into one.
    /// </summary>
    private Task RePointRelationsForUnitAsync(
        Guid unitId, CancellationToken cancellationToken)
    {
        // Unit-level merge: if needed, extend here to consolidate duplicate units
        // under the same building. For building-level merge, unit IDs remain stable.
        return Task.CompletedTask;
    }

    /// <summary>
    /// Fills master's null/empty fields from discarded entity.
    /// Prefer master's non-null values; fill gaps from discarded.
    /// Uses Building.UpdateDetails for batched field updates.
    /// </summary>
    private static void MergeBuildingFields(
        Domain.Entities.Building master,
        Domain.Entities.Building discarded,
        Dictionary<string, string> mergeMapping)
    {
        // BuildingId (the 17-digit code) — always keep master
        mergeMapping["BuildingId"] = "master";

        // Determine which fields to take from discarded
        var mergedAddress = master.Address;
        var mergedLandmark = master.Landmark;
        var mergedFloors = master.NumberOfFloors;
        var mergedYear = master.YearOfConstruction;
        var mergedLocationDesc = master.LocationDescription;
        var mergedNotes = master.Notes;

        mergeMapping["Address"] = "master";
        mergeMapping["Landmark"] = "master";
        mergeMapping["NumberOfFloors"] = "master";
        mergeMapping["YearOfConstruction"] = "master";
        mergeMapping["BuildingGeometry"] = "master";

        if (string.IsNullOrWhiteSpace(mergedAddress) &&
            !string.IsNullOrWhiteSpace(discarded.Address))
        {
            mergedAddress = discarded.Address;
            mergeMapping["Address"] = "discarded";
        }

        if (string.IsNullOrWhiteSpace(mergedLandmark) &&
            !string.IsNullOrWhiteSpace(discarded.Landmark))
        {
            mergedLandmark = discarded.Landmark;
            mergeMapping["Landmark"] = "discarded";
        }

        if ((!mergedFloors.HasValue || mergedFloors == 0) &&
            discarded.NumberOfFloors.HasValue && discarded.NumberOfFloors > 0)
        {
            mergedFloors = discarded.NumberOfFloors;
            mergeMapping["NumberOfFloors"] = "discarded";
        }

        if (!mergedYear.HasValue && discarded.YearOfConstruction.HasValue)
        {
            mergedYear = discarded.YearOfConstruction;
            mergeMapping["YearOfConstruction"] = "discarded";
        }

        // Apply merged fields via single domain method call
        master.UpdateDetails(
            mergedFloors,
            mergedYear,
            mergedAddress,
            mergedLandmark,
            mergedLocationDesc,
            mergedNotes,
            master.Id);

        // Geometry — prefer master if present, else take discarded's WKT
        if (master.BuildingGeometry is null && discarded.BuildingGeometry is not null)
        {
            master.SetGeometry(discarded.BuildingGeometry.AsText(), master.Id);
            mergeMapping["BuildingGeometry"] = "discarded";
        }
    }
}
