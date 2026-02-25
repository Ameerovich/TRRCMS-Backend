using System.Text.Json;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Conflicts.Dtos;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Entities.Staging;

namespace TRRCMS.Infrastructure.Services.Merge;

/// <summary>
/// Merges duplicate PropertyUnit records as part of conflict resolution (UC-007 S06–S07).
///
/// Handles both cross-batch (staging vs production) and within-batch (staging vs staging):
///
/// Cross-batch:
///   - Updates the production PropertyUnit with merged field values.
///   - Marks the staging PropertyUnit as Skipped and sets CommittedEntityId.
///   - Re-points production FK references only if the production entity is discarded.
///
/// Within-batch:
///   - Marks the discarded staging entity as Skipped.
///   - The master staging entity proceeds to commit normally.
///   - CommitService handles FK redirect via merge mapping.
///
/// FSD Reference: FR-D-7 (Conflict Resolution), FR-D-6 (Property Matching).
/// </summary>
public class PropertyMergeService : IMergeService
{
    private readonly IPropertyUnitRepository _propertyUnitRepository;
    private readonly IPersonPropertyRelationRepository _relationRepository;
    private readonly IClaimRepository _claimRepository;
    private readonly ISurveyRepository _surveyRepository;
    private readonly IHouseholdRepository _householdRepository;
    private readonly IStagingRepository<StagingPropertyUnit> _stagingPropertyUnitRepo;

    public PropertyMergeService(
        IPropertyUnitRepository propertyUnitRepository,
        IPersonPropertyRelationRepository relationRepository,
        IClaimRepository claimRepository,
        ISurveyRepository surveyRepository,
        IHouseholdRepository householdRepository,
        IStagingRepository<StagingPropertyUnit> stagingPropertyUnitRepo)
    {
        _propertyUnitRepository = propertyUnitRepository
            ?? throw new ArgumentNullException(nameof(propertyUnitRepository));
        _relationRepository = relationRepository
            ?? throw new ArgumentNullException(nameof(relationRepository));
        _claimRepository = claimRepository
            ?? throw new ArgumentNullException(nameof(claimRepository));
        _surveyRepository = surveyRepository
            ?? throw new ArgumentNullException(nameof(surveyRepository));
        _householdRepository = householdRepository
            ?? throw new ArgumentNullException(nameof(householdRepository));
        _stagingPropertyUnitRepo = stagingPropertyUnitRepo
            ?? throw new ArgumentNullException(nameof(stagingPropertyUnitRepo));
    }

    /// <inheritdoc />
    public string EntityType => "PropertyUnit";

    /// <inheritdoc />
    public async Task<MergeResultDto> MergeAsync(
        Guid masterEntityId,
        Guid discardedEntityId,
        Guid? importPackageId,
        CancellationToken cancellationToken = default)
    {
        var result = new MergeResultDto
        {
            MasterEntityId = masterEntityId,
            DiscardedEntityId = discardedEntityId
        };

        try
        {
            var (masterProd, masterStaging) = await ResolveEntityAsync(
                masterEntityId, importPackageId, cancellationToken);
            var (discardedProd, discardedStaging) = await ResolveEntityAsync(
                discardedEntityId, importPackageId, cancellationToken);

            var mergeMapping = new Dictionary<string, string>();
            var refsUpdated = 0;

            // ============================================================
            // Case 1: Both in production (standard merge — rare in import flow)
            // ============================================================
            if (masterProd != null && discardedProd != null)
            {
                MergePropertyUnitFields(masterProd, discardedProd, mergeMapping);
                refsUpdated = await RePointProductionReferencesAsync(
                    masterProd.Id, discardedProd.Id, cancellationToken);
                discardedProd.MarkAsDeleted(masterProd.Id);
                await _propertyUnitRepository.UpdateAsync(discardedProd, cancellationToken);
                await _propertyUnitRepository.UpdateAsync(masterProd, cancellationToken);
                await _propertyUnitRepository.SaveChangesAsync(cancellationToken);

                mergeMapping["_merge_type"] = "production_production";
                result.MasterEntityId = masterProd.Id;
                result.DiscardedEntityId = discardedProd.Id;
            }
            // ============================================================
            // Case 2: Cross-batch — master is production, discarded is staging
            // ============================================================
            else if (masterProd != null && discardedStaging != null)
            {
                FillProductionGapsFromStaging(masterProd, discardedStaging, mergeMapping);
                await _propertyUnitRepository.UpdateAsync(masterProd, cancellationToken);
                await _propertyUnitRepository.SaveChangesAsync(cancellationToken);

                discardedStaging.MarkAsSkipped("Merged into existing production record");
                discardedStaging.SetCommittedEntityId(masterProd.Id);
                await _stagingPropertyUnitRepo.UpdateAsync(discardedStaging, cancellationToken);
                await _stagingPropertyUnitRepo.SaveChangesAsync(cancellationToken);

                mergeMapping["_merge_type"] = "cross_batch_master_production";
                result.MasterEntityId = masterProd.Id;
                result.DiscardedEntityId = discardedEntityId;
            }
            // ============================================================
            // Case 3: Cross-batch — master is staging, discarded is production
            // ============================================================
            else if (masterStaging != null && discardedProd != null)
            {
                UpdateProductionFromStaging(discardedProd, masterStaging, mergeMapping);
                await _propertyUnitRepository.UpdateAsync(discardedProd, cancellationToken);
                await _propertyUnitRepository.SaveChangesAsync(cancellationToken);

                masterStaging.MarkAsSkipped("Data applied to existing production record");
                masterStaging.SetCommittedEntityId(discardedProd.Id);
                await _stagingPropertyUnitRepo.UpdateAsync(masterStaging, cancellationToken);
                await _stagingPropertyUnitRepo.SaveChangesAsync(cancellationToken);

                mergeMapping["_merge_type"] = "cross_batch_master_staging";
                result.MasterEntityId = discardedProd.Id;
                result.DiscardedEntityId = masterEntityId;
            }
            // ============================================================
            // Case 4: Within-batch — both are staging
            // ============================================================
            else if (masterStaging != null && discardedStaging != null)
            {
                discardedStaging.MarkAsSkipped("Within-batch duplicate — merged into master staging record");
                await _stagingPropertyUnitRepo.UpdateAsync(discardedStaging, cancellationToken);
                await _stagingPropertyUnitRepo.SaveChangesAsync(cancellationToken);

                mergeMapping["_merge_type"] = "within_batch";
                mergeMapping["master_staging_original_id"] = masterEntityId.ToString();
                mergeMapping["discarded_staging_original_id"] = discardedEntityId.ToString();
            }
            else
            {
                throw new InvalidOperationException(
                    $"Could not locate master ({masterEntityId}) or discarded ({discardedEntityId}) " +
                    $"entity in either production or staging tables.");
            }

            result.Success = true;
            result.MergeMappingJson = JsonSerializer.Serialize(mergeMapping);
            result.ReferencesUpdated = refsUpdated;
            result.ReferencesByType = new Dictionary<string, int>
            {
                ["PersonPropertyRelation"] = 0,
                ["Claim"] = 0,
                ["Survey"] = 0,
                ["Household"] = 0
            };
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    // ==================== ENTITY RESOLUTION ====================

    private async Task<(PropertyUnit? Production, StagingPropertyUnit? Staging)> ResolveEntityAsync(
        Guid entityId, Guid? importPackageId, CancellationToken ct)
    {
        var production = await _propertyUnitRepository.GetByIdAsync(entityId, ct);
        if (production != null)
            return (production, null);

        if (importPackageId.HasValue)
        {
            var staging = await _stagingPropertyUnitRepo
                .GetByPackageAndOriginalIdAsync(importPackageId.Value, entityId, ct);
            if (staging != null)
                return (null, staging);
        }

        return (null, null);
    }

    // ==================== PRODUCTION ↔ PRODUCTION MERGE ====================

    private static void MergePropertyUnitFields(
        PropertyUnit master, PropertyUnit discarded, Dictionary<string, string> mapping)
    {
        mapping["UnitIdentifier"] = "master";
        mapping["BuildingId"] = "master";

        if (master.Status == Domain.Enums.PropertyUnitStatus.Unknown &&
            discarded.Status != Domain.Enums.PropertyUnitStatus.Unknown)
        {
            master.UpdateStatus(discarded.Status, master.DamageLevel, master.Id);
            mapping["Status"] = "discarded";
        }
        else mapping["Status"] = "master";

        if (!master.DamageLevel.HasValue && discarded.DamageLevel.HasValue)
        {
            master.UpdateStatus(master.Status, discarded.DamageLevel, master.Id);
            mapping["DamageLevel"] = "discarded";
        }
        else mapping["DamageLevel"] = "master";

        if (!master.FloorNumber.HasValue && discarded.FloorNumber.HasValue)
        {
            master.UpdateLocation(discarded.FloorNumber, master.PositionOnFloor, master.Id);
            mapping["FloorNumber"] = "discarded";
        }
        else mapping["FloorNumber"] = "master";

        if (!master.NumberOfRooms.HasValue && discarded.NumberOfRooms.HasValue)
        {
            master.UpdateRoomCount(discarded.NumberOfRooms.Value, master.Id);
            mapping["NumberOfRooms"] = "discarded";
        }
        else mapping["NumberOfRooms"] = "master";

        if (!master.AreaSquareMeters.HasValue && discarded.AreaSquareMeters.HasValue)
        {
            master.UpdateArea(discarded.AreaSquareMeters.Value, master.Id);
            mapping["AreaSquareMeters"] = "discarded";
        }
        else mapping["AreaSquareMeters"] = "master";

        if (string.IsNullOrWhiteSpace(master.Description) &&
            !string.IsNullOrWhiteSpace(discarded.Description))
        {
            master.UpdateDescription(discarded.Description, master.Id);
            mapping["Description"] = "discarded";
        }
        else mapping["Description"] = "master";
    }

    // ==================== CROSS-BATCH: PRODUCTION ← STAGING GAP FILL ====================

    private static void FillProductionGapsFromStaging(
        PropertyUnit production, StagingPropertyUnit staging, Dictionary<string, string> mapping)
    {
        mapping["UnitIdentifier"] = "production";

        if (production.Status == Domain.Enums.PropertyUnitStatus.Unknown &&
            staging.Status != Domain.Enums.PropertyUnitStatus.Unknown)
        {
            production.UpdateStatus(staging.Status, production.DamageLevel, production.Id);
            mapping["Status"] = "staging";
        }
        else mapping["Status"] = "production";

        if (!production.DamageLevel.HasValue && staging.DamageLevel.HasValue)
        {
            production.UpdateStatus(production.Status, staging.DamageLevel, production.Id);
            mapping["DamageLevel"] = "staging";
        }
        else mapping["DamageLevel"] = "production";

        if (!production.FloorNumber.HasValue && staging.FloorNumber.HasValue)
        {
            production.UpdateLocation(staging.FloorNumber, production.PositionOnFloor, production.Id);
            mapping["FloorNumber"] = "staging";
        }
        else mapping["FloorNumber"] = "production";

        if (!production.NumberOfRooms.HasValue && staging.NumberOfRooms.HasValue)
        {
            production.UpdateRoomCount(staging.NumberOfRooms.Value, production.Id);
            mapping["NumberOfRooms"] = "staging";
        }
        else mapping["NumberOfRooms"] = "production";

        if (!production.AreaSquareMeters.HasValue && staging.AreaSquareMeters.HasValue)
        {
            production.UpdateArea(staging.AreaSquareMeters.Value, production.Id);
            mapping["AreaSquareMeters"] = "staging";
        }
        else mapping["AreaSquareMeters"] = "production";

        if (string.IsNullOrWhiteSpace(production.Description) &&
            !string.IsNullOrWhiteSpace(staging.Description))
        {
            production.UpdateDescription(staging.Description, production.Id);
            mapping["Description"] = "staging";
        }
        else mapping["Description"] = "production";
    }

    // ==================== CROSS-BATCH: STAGING → PRODUCTION OVERWRITE ====================

    private static void UpdateProductionFromStaging(
        PropertyUnit production, StagingPropertyUnit staging, Dictionary<string, string> mapping)
    {
        // Staging data takes priority
        if (staging.Status != Domain.Enums.PropertyUnitStatus.Unknown)
        {
            production.UpdateStatus(staging.Status, staging.DamageLevel ?? production.DamageLevel, production.Id);
            mapping["Status"] = "staging";
        }
        else mapping["Status"] = "production";

        if (staging.FloorNumber.HasValue)
        {
            production.UpdateLocation(staging.FloorNumber, staging.PositionOnFloor, production.Id);
            mapping["FloorNumber"] = "staging";
        }
        else mapping["FloorNumber"] = "production";

        if (staging.NumberOfRooms.HasValue)
        {
            production.UpdateRoomCount(staging.NumberOfRooms.Value, production.Id);
            mapping["NumberOfRooms"] = "staging";
        }
        else mapping["NumberOfRooms"] = "production";

        if (staging.AreaSquareMeters.HasValue)
        {
            production.UpdateArea(staging.AreaSquareMeters.Value, production.Id);
            mapping["AreaSquareMeters"] = "staging";
        }
        else mapping["AreaSquareMeters"] = "production";

        if (!string.IsNullOrWhiteSpace(staging.Description))
        {
            production.UpdateDescription(staging.Description, production.Id);
            mapping["Description"] = "staging";
        }
        else mapping["Description"] = "production";

        mapping["_data_source"] = "staging_priority";
    }

    // ==================== PRODUCTION FK RE-POINTING ====================

    private async Task<int> RePointProductionReferencesAsync(
        Guid masterUnitId, Guid discardedUnitId, CancellationToken ct)
    {
        var count = 0;

        var relations = (await _relationRepository
            .GetByPropertyUnitIdAsync(discardedUnitId, ct)).ToList();
        foreach (var relation in relations)
        {
            var existing = await _relationRepository
                .GetByPersonAndPropertyUnitAsync(relation.PersonId, masterUnitId, ct);
            if (existing is null)
            {
                relation.UpdatePropertyUnitId(masterUnitId, masterUnitId);
                await _relationRepository.UpdateAsync(relation, ct);
                count++;
            }
            else
            {
                await _relationRepository.DeleteAsync(relation, ct);
            }
        }

        var claims = (await _claimRepository
            .GetAllByPropertyUnitIdAsync(discardedUnitId, ct));
        foreach (var claim in claims)
        {
            claim.UpdatePropertyUnit(masterUnitId, masterUnitId);
            await _claimRepository.UpdateAsync(claim, ct);
            count++;
        }

        var surveys = (await _surveyRepository
            .GetByPropertyUnitAsync(discardedUnitId, ct));
        foreach (var survey in surveys)
        {
            survey.LinkToPropertyUnit(masterUnitId, masterUnitId);
            await _surveyRepository.UpdateAsync(survey, ct);
            count++;
        }

        var households = (await _householdRepository
            .GetByPropertyUnitIdAsync(discardedUnitId, ct)).ToList();
        foreach (var household in households)
        {
            household.UpdatePropertyUnit(masterUnitId, masterUnitId);
            await _householdRepository.UpdateAsync(household, ct);
            count++;
        }

        return count;
    }
}
