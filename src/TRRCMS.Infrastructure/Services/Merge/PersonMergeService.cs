using System.Text.Json;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Conflicts.Dtos;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Entities.Staging;

namespace TRRCMS.Infrastructure.Services.Merge;

/// <summary>
/// Merges duplicate Person records as part of conflict resolution (UC-008 S06–S07).
///
/// Handles both cross-batch (staging vs production) and within-batch (staging vs staging):
///
/// Cross-batch:
///   - Updates the production Person with merged field values.
///   - Marks the staging Person as Skipped and sets CommittedEntityId = production.Id.
///   - Re-points production FK references (Relations, Claims) if the production entity
///     is the one being discarded (master = staging data).
///
/// Within-batch:
///   - Marks the discarded staging entity as Skipped.
///   - The master staging entity proceeds to commit normally.
///   - CommitService handles FK redirect via merge mapping.
///
/// FSD Reference: FR-D-7 (Conflict Resolution), FR-D-5 (Person Matching).
/// </summary>
public class PersonMergeService : IMergeService
{
    private readonly IPersonRepository _personRepository;
    private readonly IPersonPropertyRelationRepository _relationRepository;
    private readonly IClaimRepository _claimRepository;
    private readonly IHouseholdRepository _householdRepository;
    private readonly IStagingRepository<StagingPerson> _stagingPersonRepo;

    public PersonMergeService(
        IPersonRepository personRepository,
        IPersonPropertyRelationRepository relationRepository,
        IClaimRepository claimRepository,
        IHouseholdRepository householdRepository,
        IStagingRepository<StagingPerson> stagingPersonRepo)
    {
        _personRepository = personRepository
            ?? throw new ArgumentNullException(nameof(personRepository));
        _relationRepository = relationRepository
            ?? throw new ArgumentNullException(nameof(relationRepository));
        _claimRepository = claimRepository
            ?? throw new ArgumentNullException(nameof(claimRepository));
        _householdRepository = householdRepository
            ?? throw new ArgumentNullException(nameof(householdRepository));
        _stagingPersonRepo = stagingPersonRepo
            ?? throw new ArgumentNullException(nameof(stagingPersonRepo));
    }

    /// <inheritdoc />
    public string EntityType => "Person";

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
            // Resolve each entity from production or staging
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
                MergePersonFields(masterProd, discardedProd, mergeMapping);
                refsUpdated = await RePointProductionReferencesAsync(
                    masterProd.Id, discardedProd.Id, cancellationToken);
                discardedProd.MarkAsDeleted(masterProd.Id);
                await _personRepository.UpdateAsync(discardedProd, cancellationToken);
                await _personRepository.UpdateAsync(masterProd, cancellationToken);
                await _personRepository.SaveChangesAsync(cancellationToken);

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
                await _personRepository.UpdateAsync(masterProd, cancellationToken);
                await _personRepository.SaveChangesAsync(cancellationToken);

                // Mark staging as skipped with traceability link
                discardedStaging.MarkAsSkipped("Merged into existing production record");
                discardedStaging.SetCommittedEntityId(masterProd.Id);
                await _stagingPersonRepo.UpdateAsync(discardedStaging, cancellationToken);
                await _stagingPersonRepo.SaveChangesAsync(cancellationToken);

                mergeMapping["_merge_type"] = "cross_batch_master_production";
                result.MasterEntityId = masterProd.Id;
                result.DiscardedEntityId = discardedEntityId; // staging OriginalEntityId
            }
            // ============================================================
            // Case 3: Cross-batch — master is staging, discarded is production
            // ============================================================
            else if (masterStaging != null && discardedProd != null)
            {
                UpdateProductionFromStaging(discardedProd, masterStaging, mergeMapping);
                await _personRepository.UpdateAsync(discardedProd, cancellationToken);
                await _personRepository.SaveChangesAsync(cancellationToken);

                // Mark staging as skipped — its data has been applied to production
                masterStaging.MarkAsSkipped("Data applied to existing production record");
                masterStaging.SetCommittedEntityId(discardedProd.Id);
                await _stagingPersonRepo.UpdateAsync(masterStaging, cancellationToken);
                await _stagingPersonRepo.SaveChangesAsync(cancellationToken);

                mergeMapping["_merge_type"] = "cross_batch_master_staging";
                // The surviving production entity is the "master" from data perspective
                result.MasterEntityId = discardedProd.Id;
                result.DiscardedEntityId = masterEntityId; // staging OriginalEntityId
            }
            // ============================================================
            // Case 4: Within-batch — both are staging
            // ============================================================
            else if (masterStaging != null && discardedStaging != null)
            {
                discardedStaging.MarkAsSkipped("Within-batch duplicate — merged into master staging record");
                await _stagingPersonRepo.UpdateAsync(discardedStaging, cancellationToken);
                await _stagingPersonRepo.SaveChangesAsync(cancellationToken);

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
                ["PersonPropertyRelation"] = refsUpdated,
                ["Claim"] = 0
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

    /// <summary>
    /// Try to load an entity from production, then from staging.
    /// Returns (productionEntity, stagingEntity) — exactly one will be non-null.
    /// </summary>
    private async Task<(Person? Production, StagingPerson? Staging)> ResolveEntityAsync(
        Guid entityId, Guid? importPackageId, CancellationToken ct)
    {
        // Try production first
        var production = await _personRepository.GetByIdAsync(entityId, ct);
        if (production != null)
            return (production, null);

        // Try staging (entityId is the OriginalEntityId in staging)
        if (importPackageId.HasValue)
        {
            var staging = await _stagingPersonRepo
                .GetByPackageAndOriginalIdAsync(importPackageId.Value, entityId, ct);
            if (staging != null)
                return (null, staging);
        }

        return (null, null);
    }

    // ==================== PRODUCTION ↔ PRODUCTION MERGE ====================

    private static void MergePersonFields(
        Person master, Person discarded, Dictionary<string, string> mapping)
    {
        if (string.IsNullOrWhiteSpace(master.NationalId) &&
            !string.IsNullOrWhiteSpace(discarded.NationalId))
        {
            master.UpdateIdentification(discarded.NationalId, master.DateOfBirth,
                master.Gender, master.Nationality, master.Id);
            mapping["NationalId"] = "discarded";
        }
        else mapping["NationalId"] = "master";

        if (string.IsNullOrWhiteSpace(master.MobileNumber) &&
            !string.IsNullOrWhiteSpace(discarded.MobileNumber))
        {
            master.UpdateContactInfo(master.Email, discarded.MobileNumber,
                master.PhoneNumber, master.Id);
            mapping["MobileNumber"] = "discarded";
        }
        else mapping["MobileNumber"] = "master";

        if (string.IsNullOrWhiteSpace(master.PhoneNumber) &&
            !string.IsNullOrWhiteSpace(discarded.PhoneNumber))
        {
            master.UpdateContactInfo(master.Email, master.MobileNumber,
                discarded.PhoneNumber, master.Id);
            mapping["PhoneNumber"] = "discarded";
        }
        else mapping["PhoneNumber"] = "master";

        if (string.IsNullOrWhiteSpace(master.Email) &&
            !string.IsNullOrWhiteSpace(discarded.Email))
        {
            master.UpdateContactInfo(discarded.Email, master.MobileNumber,
                master.PhoneNumber, master.Id);
            mapping["Email"] = "discarded";
        }
        else mapping["Email"] = "master";

        if (!master.DateOfBirth.HasValue && discarded.DateOfBirth.HasValue)
        {
            master.UpdateIdentification(master.NationalId, discarded.DateOfBirth,
                master.Gender, master.Nationality, master.Id);
            mapping["DateOfBirth"] = "discarded";
        }
        else mapping["DateOfBirth"] = "master";

        mapping["FirstNameArabic"] = "master";
        mapping["FatherNameArabic"] = "master";
        mapping["FamilyNameArabic"] = "master";
    }

    // ==================== CROSS-BATCH: PRODUCTION ← STAGING GAP FILL ====================

    private static void FillProductionGapsFromStaging(
        Person production, StagingPerson staging, Dictionary<string, string> mapping)
    {
        if (string.IsNullOrWhiteSpace(production.NationalId) &&
            !string.IsNullOrWhiteSpace(staging.NationalId))
        {
            production.UpdateIdentification(staging.NationalId, production.DateOfBirth,
                production.Gender, production.Nationality, production.Id);
            mapping["NationalId"] = "staging";
        }
        else mapping["NationalId"] = "production";

        if (string.IsNullOrWhiteSpace(production.MobileNumber) &&
            !string.IsNullOrWhiteSpace(staging.MobileNumber))
        {
            production.UpdateContactInfo(production.Email, staging.MobileNumber,
                production.PhoneNumber, production.Id);
            mapping["MobileNumber"] = "staging";
        }
        else mapping["MobileNumber"] = "production";

        if (string.IsNullOrWhiteSpace(production.PhoneNumber) &&
            !string.IsNullOrWhiteSpace(staging.PhoneNumber))
        {
            production.UpdateContactInfo(production.Email, production.MobileNumber,
                staging.PhoneNumber, production.Id);
            mapping["PhoneNumber"] = "staging";
        }
        else mapping["PhoneNumber"] = "production";

        if (string.IsNullOrWhiteSpace(production.Email) &&
            !string.IsNullOrWhiteSpace(staging.Email))
        {
            production.UpdateContactInfo(staging.Email, production.MobileNumber,
                production.PhoneNumber, production.Id);
            mapping["Email"] = "staging";
        }
        else mapping["Email"] = "production";

        mapping["Names"] = "production";
    }

    // ==================== CROSS-BATCH: STAGING → PRODUCTION OVERWRITE ====================

    private static void UpdateProductionFromStaging(
        Person production, StagingPerson staging, Dictionary<string, string> mapping)
    {
        // Staging data takes priority — update production with staging values
        // Names from staging take priority
        production.UpdateBasicInfo(
            staging.FamilyNameArabic,
            staging.FirstNameArabic,
            staging.FatherNameArabic,
            staging.MotherNameArabic,
            staging.NationalId ?? production.NationalId,
            staging.YearOfBirth.HasValue
                ? new DateTime(staging.YearOfBirth.Value, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                : production.DateOfBirth,
            production.Gender,
            production.Nationality,
            production.Id);

        production.UpdateContactInfo(
            staging.Email ?? production.Email,
            staging.MobileNumber ?? production.MobileNumber,
            staging.PhoneNumber ?? production.PhoneNumber,
            production.Id);

        mapping["_data_source"] = "staging_priority";
        mapping["NationalId"] = !string.IsNullOrWhiteSpace(staging.NationalId) ? "staging" : "production";
        mapping["MobileNumber"] = !string.IsNullOrWhiteSpace(staging.MobileNumber) ? "staging" : "production";
        mapping["Names"] = "staging";
    }

    // ==================== PRODUCTION FK RE-POINTING ====================

    private async Task<int> RePointProductionReferencesAsync(
        Guid masterPersonId, Guid discardedPersonId, CancellationToken ct)
    {
        var count = 0;

        var relations = (await _relationRepository
            .GetByPersonIdAsync(discardedPersonId, ct)).ToList();
        foreach (var relation in relations)
        {
            var existing = await _relationRepository
                .GetByPersonAndPropertyUnitAsync(masterPersonId, relation.PropertyUnitId, ct);
            if (existing is null)
            {
                relation.UpdatePersonId(masterPersonId, masterPersonId);
                await _relationRepository.UpdateAsync(relation, ct);
                count++;
            }
            else
            {
                await _relationRepository.DeleteAsync(relation, ct);
            }
        }

        var claims = (await _claimRepository
            .GetByPrimaryClaimantIdAsync(discardedPersonId, ct)).ToList();
        foreach (var claim in claims)
        {
            claim.UpdatePrimaryClaimant(masterPersonId, masterPersonId);
            await _claimRepository.UpdateAsync(claim, ct);
            count++;
        }

        return count;
    }
}
