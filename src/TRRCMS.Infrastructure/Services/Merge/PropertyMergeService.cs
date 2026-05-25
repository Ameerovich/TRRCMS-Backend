using System.Text.Json;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Conflicts.Dtos;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Entities.Staging;

namespace TRRCMS.Infrastructure.Services.Merge;

/// <summary>
/// Merges duplicate PropertyUnit records as part of conflict resolution.
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
/// </summary>
public class PropertyMergeService : IMergeService
{
    private readonly IPropertyUnitRepository _propertyUnitRepository;
    private readonly IPersonPropertyRelationRepository _relationRepository;
    private readonly IClaimRepository _claimRepository;
    private readonly ISurveyRepository _surveyRepository;
    private readonly IHouseholdRepository _householdRepository;
    private readonly IEvidenceRelationRepository _evidenceRelationRepository;
    private readonly ICaseRepository _caseRepository;
    private readonly IStagingRepository<StagingPropertyUnit> _stagingPropertyUnitRepo;

    public PropertyMergeService(
        IPropertyUnitRepository propertyUnitRepository,
        IPersonPropertyRelationRepository relationRepository,
        IClaimRepository claimRepository,
        ISurveyRepository surveyRepository,
        IHouseholdRepository householdRepository,
        IEvidenceRelationRepository evidenceRelationRepository,
        ICaseRepository caseRepository,
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
        _evidenceRelationRepository = evidenceRelationRepository
            ?? throw new ArgumentNullException(nameof(evidenceRelationRepository));
        _caseRepository = caseRepository
            ?? throw new ArgumentNullException(nameof(caseRepository));
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
            var conflicts = new Dictionary<string, FieldConflictInfo>();
            var refsUpdated = 0;
            var refsByType = new Dictionary<string, int>
            {
                ["PersonPropertyRelation"] = 0,
                ["Claim"] = 0,
                ["Survey"] = 0,
                ["Household"] = 0
            };

            // Case 1: Both in production (standard merge)
            if (masterProd != null && discardedProd != null)
            {
                MergePropertyUnitFields(masterProd, discardedProd, mergeMapping, conflicts);
                (refsUpdated, refsByType) = await RePointProductionReferencesAsync(
                    masterProd.Id, discardedProd.Id, cancellationToken);
                discardedProd.MarkAsDeleted(masterProd.Id);
                await _propertyUnitRepository.UpdateAsync(discardedProd, cancellationToken);
                await _propertyUnitRepository.UpdateAsync(masterProd, cancellationToken);
                await _propertyUnitRepository.SaveChangesAsync(cancellationToken);

                mergeMapping["_merge_type"] = "production_production";
                result.MasterEntityId = masterProd.Id;
                result.DiscardedEntityId = discardedProd.Id;
            }
            // Case 2: Cross-batch — master is production, discarded is staging
            else if (masterProd != null && discardedStaging != null)
            {
                FillProductionGapsFromStaging(masterProd, discardedStaging, mergeMapping, conflicts);
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
            // Case 3: Cross-batch — master is staging, discarded is production
            else if (masterStaging != null && discardedProd != null)
            {
                UpdateProductionFromStaging(discardedProd, masterStaging, mergeMapping, conflicts);
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
            // Case 4: Within-batch — both are staging
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
                // Neither side resolves — the conflict is orphaned. Typical causes:
                //   - The import package was re-staged after this conflict was created
                //     (staging rows replaced with new OriginalEntityIds).
                //   - The package was committed and staging was cleaned up, but the
                //     conflict was not resolved beforehand.
                //   - The conflict references a production entity that has since been
                //     hard-deleted or merged elsewhere.
                throw new InvalidOperationException(
                    $"Conflict references entities that no longer exist " +
                    $"(master={masterEntityId}, discarded={discardedEntityId}). " +
                    "This usually means the import package was re-staged or committed " +
                    "after the conflict was created. Re-run duplicate detection to " +
                    "regenerate conflicts against the current data.");
            }

            result.Success = true;
            result.MergeMappingJson = JsonSerializer.Serialize(mergeMapping);
            result.ConflictingFields = conflicts;
            result.ReferencesUpdated = refsUpdated;
            result.ReferencesByType = refsByType;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

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

    private static void MergePropertyUnitFields(
        PropertyUnit master, PropertyUnit discarded, Dictionary<string, string> mapping,
        Dictionary<string, FieldConflictInfo> conflicts)
    {
        mapping["UnitIdentifier"] = "master";
        mapping["BuildingId"] = "master";

        // Status
        if (master.Status == Domain.Enums.PropertyUnitStatus.Unknown &&
            discarded.Status != Domain.Enums.PropertyUnitStatus.Unknown)
        {
            master.UpdateStatus(discarded.Status, master.Id);
            mapping["Status"] = "discarded";
        }
        else
        {
            mapping["Status"] = "master";
            if (master.Status != Domain.Enums.PropertyUnitStatus.Unknown &&
                discarded.Status != Domain.Enums.PropertyUnitStatus.Unknown &&
                master.Status != discarded.Status)
            {
                conflicts["Status"] = new FieldConflictInfo(
                    master.Status.ToString(), discarded.Status.ToString(), "master");
            }
        }

        // FloorNumber
        if (!master.FloorNumber.HasValue && discarded.FloorNumber.HasValue)
        {
            master.UpdateLocation(discarded.FloorNumber, master.Id);
            mapping["FloorNumber"] = "discarded";
        }
        else
        {
            mapping["FloorNumber"] = "master";
            if (master.FloorNumber.HasValue && discarded.FloorNumber.HasValue &&
                master.FloorNumber != discarded.FloorNumber)
            {
                conflicts["FloorNumber"] = new FieldConflictInfo(
                    master.FloorNumber.ToString(), discarded.FloorNumber.ToString(), "master");
            }
        }

        // NumberOfRooms
        if (!master.NumberOfRooms.HasValue && discarded.NumberOfRooms.HasValue)
        {
            master.UpdateRoomCount(discarded.NumberOfRooms.Value, master.Id);
            mapping["NumberOfRooms"] = "discarded";
        }
        else
        {
            mapping["NumberOfRooms"] = "master";
            if (master.NumberOfRooms.HasValue && discarded.NumberOfRooms.HasValue &&
                master.NumberOfRooms != discarded.NumberOfRooms)
            {
                conflicts["NumberOfRooms"] = new FieldConflictInfo(
                    master.NumberOfRooms.ToString(), discarded.NumberOfRooms.ToString(), "master");
            }
        }

        // AreaSquareMeters
        if (!master.AreaSquareMeters.HasValue && discarded.AreaSquareMeters.HasValue)
        {
            master.UpdateArea(discarded.AreaSquareMeters.Value, master.Id);
            mapping["AreaSquareMeters"] = "discarded";
        }
        else
        {
            mapping["AreaSquareMeters"] = "master";
            if (master.AreaSquareMeters.HasValue && discarded.AreaSquareMeters.HasValue &&
                master.AreaSquareMeters != discarded.AreaSquareMeters)
            {
                conflicts["AreaSquareMeters"] = new FieldConflictInfo(
                    master.AreaSquareMeters.ToString(), discarded.AreaSquareMeters.ToString(), "master");
            }
        }

        // Description
        if (string.IsNullOrWhiteSpace(master.Description) &&
            !string.IsNullOrWhiteSpace(discarded.Description))
        {
            master.UpdateDescription(discarded.Description, master.Id);
            mapping["Description"] = "discarded";
        }
        else
        {
            mapping["Description"] = "master";
            RecordConflictIfDifferent(conflicts, "Description",
                master.Description, discarded.Description, "master");
        }
    }

    private static void FillProductionGapsFromStaging(
        PropertyUnit production, StagingPropertyUnit staging, Dictionary<string, string> mapping,
        Dictionary<string, FieldConflictInfo> conflicts)
    {
        mapping["UnitIdentifier"] = "production";

        if (production.Status == Domain.Enums.PropertyUnitStatus.Unknown &&
            staging.Status != Domain.Enums.PropertyUnitStatus.Unknown)
        {
            production.UpdateStatus(staging.Status, production.Id);
            mapping["Status"] = "staging";
        }
        else
        {
            mapping["Status"] = "production";
            if (production.Status != Domain.Enums.PropertyUnitStatus.Unknown &&
                staging.Status != Domain.Enums.PropertyUnitStatus.Unknown &&
                production.Status != staging.Status)
            {
                conflicts["Status"] = new FieldConflictInfo(
                    production.Status.ToString(), staging.Status.ToString(), "production");
            }
        }

        if (!production.FloorNumber.HasValue && staging.FloorNumber.HasValue)
        {
            production.UpdateLocation(staging.FloorNumber, production.Id);
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
        else
        {
            mapping["Description"] = "production";
            RecordConflictIfDifferent(conflicts, "Description",
                production.Description, staging.Description, "production");
        }
    }

    private static void UpdateProductionFromStaging(
        PropertyUnit production, StagingPropertyUnit staging, Dictionary<string, string> mapping,
        Dictionary<string, FieldConflictInfo> conflicts)
    {
        // Staging data takes priority
        if (staging.Status != Domain.Enums.PropertyUnitStatus.Unknown)
        {
            if (production.Status != Domain.Enums.PropertyUnitStatus.Unknown &&
                production.Status != staging.Status)
            {
                conflicts["Status"] = new FieldConflictInfo(
                    production.Status.ToString(), staging.Status.ToString(), "staging");
            }
            production.UpdateStatus(staging.Status, production.Id);
            mapping["Status"] = "staging";
        }
        else mapping["Status"] = "production";

        if (staging.FloorNumber.HasValue)
        {
            if (production.FloorNumber.HasValue && production.FloorNumber != staging.FloorNumber)
            {
                conflicts["FloorNumber"] = new FieldConflictInfo(
                    production.FloorNumber.ToString(), staging.FloorNumber.ToString(), "staging");
            }
            production.UpdateLocation(staging.FloorNumber, production.Id);
            mapping["FloorNumber"] = "staging";
        }
        else mapping["FloorNumber"] = "production";

        if (staging.NumberOfRooms.HasValue)
        {
            if (production.NumberOfRooms.HasValue && production.NumberOfRooms != staging.NumberOfRooms)
            {
                conflicts["NumberOfRooms"] = new FieldConflictInfo(
                    production.NumberOfRooms.ToString(), staging.NumberOfRooms.ToString(), "staging");
            }
            production.UpdateRoomCount(staging.NumberOfRooms.Value, production.Id);
            mapping["NumberOfRooms"] = "staging";
        }
        else mapping["NumberOfRooms"] = "production";

        if (staging.AreaSquareMeters.HasValue)
        {
            if (production.AreaSquareMeters.HasValue && production.AreaSquareMeters != staging.AreaSquareMeters)
            {
                conflicts["AreaSquareMeters"] = new FieldConflictInfo(
                    production.AreaSquareMeters.ToString(), staging.AreaSquareMeters.ToString(), "staging");
            }
            production.UpdateArea(staging.AreaSquareMeters.Value, production.Id);
            mapping["AreaSquareMeters"] = "staging";
        }
        else mapping["AreaSquareMeters"] = "production";

        if (!string.IsNullOrWhiteSpace(staging.Description))
        {
            RecordConflictIfDifferent(conflicts, "Description",
                production.Description, staging.Description, "staging");
            production.UpdateDescription(staging.Description, production.Id);
            mapping["Description"] = "staging";
        }
        else mapping["Description"] = "production";

        mapping["_data_source"] = "staging_priority";
    }

    private async Task<(int Total, Dictionary<string, int> ByType)> RePointProductionReferencesAsync(
        Guid masterUnitId, Guid discardedUnitId, CancellationToken ct)
    {
        var relCount = 0;
        var claimCount = 0;
        var surveyCount = 0;
        var householdCount = 0;

        var evidenceCount = 0;

        var relations = (await _relationRepository
            .GetByPropertyUnitIdAsync(discardedUnitId, ct)).ToList();
        foreach (var relation in relations)
        {
            var existing = await _relationRepository
                .GetByPersonAndPropertyUnitAsync(relation.PersonId, masterUnitId, ct);
            if (existing is null)
            {
                // No competing relation for this person on the master unit — re-point in place.
                // Any evidence stays attached because the relation row itself survives.
                relation.UpdatePropertyUnitId(masterUnitId, masterUnitId);
                await _relationRepository.UpdateAsync(relation, ct);
                relCount++;
            }
            else
            {
                // The same person already relates to the master unit. Move the discarded
                // relation's evidence links onto the surviving relation before deleting it so
                // attached documents are not orphaned on a soft-deleted relation.
                evidenceCount += await MoveEvidenceLinksAsync(relation.Id, existing, masterUnitId, ct);
                await _relationRepository.DeleteAsync(relation, ct);
            }
        }

        var claims = (await _claimRepository
            .GetAllByPropertyUnitIdAsync(discardedUnitId, ct));
        foreach (var claim in claims)
        {
            claim.UpdatePropertyUnit(masterUnitId, masterUnitId);
            await _claimRepository.UpdateAsync(claim, ct);
            claimCount++;
        }

        var surveys = (await _surveyRepository
            .GetByPropertyUnitAsync(discardedUnitId, ct));
        foreach (var survey in surveys)
        {
            survey.LinkToPropertyUnit(masterUnitId, masterUnitId);
            await _surveyRepository.UpdateAsync(survey, ct);
            surveyCount++;
        }

        var households = (await _householdRepository
            .GetByPropertyUnitIdAsync(discardedUnitId, ct)).ToList();
        foreach (var household in households)
        {
            household.UpdatePropertyUnit(masterUnitId, masterUnitId);
            await _householdRepository.UpdateAsync(household, ct);
            householdCount++;
        }

        // Case is one-to-one with PropertyUnit, and the discarded unit is about to be soft-deleted.
        var caseCount = await ReconcileCaseAsync(masterUnitId, discardedUnitId, ct);

        var total = relCount + claimCount + surveyCount + householdCount + evidenceCount + caseCount;
        var byType = new Dictionary<string, int>
        {
            ["PersonPropertyRelation"] = relCount,
            ["Claim"] = claimCount,
            ["Survey"] = surveyCount,
            ["Household"] = householdCount,
            ["EvidenceRelation"] = evidenceCount,
            ["Case"] = caseCount
        };

        return (total, byType);
    }

    /// <summary>
    /// Moves the (non-deleted) evidence links from a relation that is about to be discarded onto a
    /// surviving relation. Skips links that would duplicate an existing active link on the survivor.
    /// Returns the number of links moved.
    /// </summary>
    private async Task<int> MoveEvidenceLinksAsync(
        Guid fromRelationId, PersonPropertyRelation survivingRelation, Guid modifiedBy, CancellationToken ct)
    {
        var moved = 0;
        var links = (await _evidenceRelationRepository
            .GetByRelationIdAsync(fromRelationId, ct)).ToList();
        foreach (var link in links)
        {
            var alreadyLinked = await _evidenceRelationRepository
                .LinkExistsAsync(link.EvidenceId, survivingRelation.Id, ct);
            if (alreadyLinked)
                continue;

            link.RelinkToRelation(survivingRelation.Id, modifiedBy);
            await _evidenceRelationRepository.UpdateAsync(link, ct);
            moved++;
        }

        if (moved > 0 && !survivingRelation.HasEvidence)
        {
            survivingRelation.SetHasEvidence(true, modifiedBy);
            await _relationRepository.UpdateAsync(survivingRelation, ct);
        }

        return moved;
    }

    /// <summary>
    /// Reconciles the Case (one-to-one with PropertyUnit) when a property-unit merge soft-deletes
    /// the discarded unit. By the time this runs, the discarded unit's surveys/claims/relations have
    /// already had their PropertyUnitId re-pointed to the master unit, but their CaseId still points
    /// at the discarded unit's case.
    ///
    ///   - Discarded has no case            → nothing to do.
    ///   - Master has no case               → re-point the discarded case onto the master unit.
    ///   - Both have a case                 → fold the discarded case into the master case:
    ///                                         move its children's CaseId to the master case,
    ///                                         apply "Closed wins" to the master case status, then
    ///                                         soft-delete the now-empty discarded case.
    ///
    /// Returns the number of Case rows affected.
    /// </summary>
    private async Task<int> ReconcileCaseAsync(Guid masterUnitId, Guid discardedUnitId, CancellationToken ct)
    {
        var discardedCase = await _caseRepository.GetByPropertyUnitIdAsync(discardedUnitId, ct);
        if (discardedCase == null)
            return 0;

        var masterCase = await _caseRepository.GetByPropertyUnitIdAsync(masterUnitId, ct);

        // Master has no case — simply re-point the discarded unit's case onto the surviving unit.
        if (masterCase == null)
        {
            discardedCase.RelinkToPropertyUnit(masterUnitId, masterUnitId);
            await _caseRepository.UpdateAsync(discardedCase, ct);
            return 1;
        }

        // Both units have a case — fold the discarded case into the master case so the surviving
        // unit's case carries all the work and nothing is stranded under a soft-deleted unit.
        foreach (var survey in discardedCase.Surveys.ToList())
            survey.RelinkToCase(masterCase.Id, masterUnitId);
        foreach (var claim in discardedCase.Claims.ToList())
            claim.RelinkToCase(masterCase.Id, masterUnitId);
        foreach (var relation in discardedCase.PersonPropertyRelations.ToList())
            relation.RelinkToCase(masterCase.Id, masterUnitId);

        // "Closed wins": if the discarded case was closed (by an ownership/heir claim) and the
        // master case is still open, carry that closure onto the master case. Case.Close is
        // idempotent, so this is a no-op when the master is already closed.
        if (discardedCase.Status == Domain.Enums.CaseLifecycleStatus.Closed
            && masterCase.Status != Domain.Enums.CaseLifecycleStatus.Closed
            && discardedCase.ClosedByClaimId.HasValue)
        {
            masterCase.Close(discardedCase.ClosedByClaimId.Value, masterUnitId);
            await _caseRepository.UpdateAsync(masterCase, ct);
        }

        discardedCase.MarkAsDeleted(masterUnitId);
        await _caseRepository.UpdateAsync(discardedCase, ct);
        return 1;
    }

    /// <summary>
    /// Records a field conflict when both string values are non-null/non-empty and differ.
    /// </summary>
    private static void RecordConflictIfDifferent(
        Dictionary<string, FieldConflictInfo> conflicts,
        string fieldName, string? masterValue, string? discardedValue, string keptFrom)
    {
        if (!string.IsNullOrWhiteSpace(masterValue) &&
            !string.IsNullOrWhiteSpace(discardedValue) &&
            !string.Equals(masterValue, discardedValue, StringComparison.OrdinalIgnoreCase))
        {
            conflicts[fieldName] = new FieldConflictInfo(masterValue, discardedValue, keptFrom);
        }
    }
}
