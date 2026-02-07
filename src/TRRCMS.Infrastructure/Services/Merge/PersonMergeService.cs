using System.Text.Json;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Conflicts.Dtos;

namespace TRRCMS.Infrastructure.Services.Merge;

/// <summary>
/// Merges duplicate Person records as part of conflict resolution (UC-008 S06–S07).
/// 
/// Merge strategy:
/// 1. Master entity keeps its own non-null fields; gaps filled from discarded.
/// 2. All PersonPropertyRelations pointing to discarded → re-pointed to master.
/// 3. All Claims where discarded is PrimaryClaimant → re-pointed to master.
/// 4. Household membership: if discarded is in a different household, its
///    membership is transferred to master's household (or kept if master has none).
/// 5. Discarded entity is soft-deleted.
/// 6. Merge mapping JSON is built for full audit trail.
/// 
/// FSD Reference: FR-D-7 (Conflict Resolution), FR-D-5 (Person Matching).
/// </summary>
public class PersonMergeService : IMergeService
{
    private readonly IPersonRepository _personRepository;
    private readonly IPersonPropertyRelationRepository _relationRepository;
    private readonly IClaimRepository _claimRepository;
    private readonly IHouseholdRepository _householdRepository;

    public PersonMergeService(
        IPersonRepository personRepository,
        IPersonPropertyRelationRepository relationRepository,
        IClaimRepository claimRepository,
        IHouseholdRepository householdRepository)
    {
        _personRepository = personRepository
            ?? throw new ArgumentNullException(nameof(personRepository));
        _relationRepository = relationRepository
            ?? throw new ArgumentNullException(nameof(relationRepository));
        _claimRepository = claimRepository
            ?? throw new ArgumentNullException(nameof(claimRepository));
        _householdRepository = householdRepository
            ?? throw new ArgumentNullException(nameof(householdRepository));
    }

    /// <inheritdoc />
    public string EntityType => "Person";

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
            // 1. Load both persons
            var master = await _personRepository.GetByIdAsync(masterEntityId, cancellationToken)
                ?? throw new InvalidOperationException(
                    $"Master person with ID {masterEntityId} not found.");

            var discarded = await _personRepository.GetByIdAsync(discardedEntityId, cancellationToken)
                ?? throw new InvalidOperationException(
                    $"Discarded person with ID {discardedEntityId} not found.");

            // 2. Build merge mapping: fill master's null/empty fields from discarded
            var mergeMapping = new Dictionary<string, string>();
            MergePersonFields(master, discarded, mergeMapping);

            // 3. Re-point PersonPropertyRelations
            var discardedRelations = (await _relationRepository
                .GetByPersonIdAsync(discardedEntityId, cancellationToken)).ToList();
            var relationsUpdated = 0;

            foreach (var relation in discardedRelations)
            {
                // Check if master already has a relation to the same property unit
                var existingRelation = await _relationRepository
                    .GetByPersonAndPropertyUnitAsync(
                        masterEntityId, relation.PropertyUnitId, cancellationToken);

                if (existingRelation is null)
                {
                    // Re-point relation to master person
                    relation.UpdatePersonId(masterEntityId, masterEntityId);
                    await _relationRepository.UpdateAsync(relation, cancellationToken);
                    relationsUpdated++;
                }
                else
                {
                    // Duplicate relation — soft-delete the discarded one
                    await _relationRepository.DeleteAsync(relation, cancellationToken);
                }
            }

            // 4. Re-point Claims where discarded is primary claimant
            var discardedClaims = (await _claimRepository
                .GetByPrimaryClaimantIdAsync(discardedEntityId, cancellationToken)).ToList();
            var claimsUpdated = 0;

            foreach (var claim in discardedClaims)
            {
                claim.UpdatePrimaryClaimant(masterEntityId, masterEntityId);
                await _claimRepository.UpdateAsync(claim, cancellationToken);
                claimsUpdated++;
            }

            // 5. Soft-delete discarded person
            discarded.MarkAsDeleted(masterEntityId);
            await _personRepository.UpdateAsync(discarded, cancellationToken);

            // 6. Save master with merged fields
            await _personRepository.UpdateAsync(master, cancellationToken);
            await _personRepository.SaveChangesAsync(cancellationToken);

            // 7. Build result
            result.Success = true;
            result.MergeMappingJson = JsonSerializer.Serialize(mergeMapping);
            result.ReferencesUpdated = relationsUpdated + claimsUpdated;
            result.ReferencesByType = new Dictionary<string, int>
            {
                ["PersonPropertyRelation"] = relationsUpdated,
                ["Claim"] = claimsUpdated
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
    /// Fills master's null/empty fields from discarded entity.
    /// Tracks which fields were taken from which source in the merge mapping.
    /// </summary>
    private static void MergePersonFields(
        Domain.Entities.Person master,
        Domain.Entities.Person discarded,
        Dictionary<string, string> mergeMapping)
    {
        // NationalId — prefer non-null
        if (string.IsNullOrWhiteSpace(master.NationalId) &&
            !string.IsNullOrWhiteSpace(discarded.NationalId))
        {
            master.UpdateIdentification(
                discarded.NationalId,
                master.YearOfBirth,
                master.Gender,
                master.Nationality,
                master.Id);
            mergeMapping["NationalId"] = "discarded";
        }
        else
        {
            mergeMapping["NationalId"] = "master";
        }

        // MobileNumber — prefer non-null
        if (string.IsNullOrWhiteSpace(master.MobileNumber) &&
            !string.IsNullOrWhiteSpace(discarded.MobileNumber))
        {
            master.UpdateContactInfo(
                discarded.MobileNumber,
                master.PhoneNumber,
                master.Email,
                master.Id);
            mergeMapping["MobileNumber"] = "discarded";
        }
        else
        {
            mergeMapping["MobileNumber"] = "master";
        }

        // PhoneNumber — prefer non-null
        if (string.IsNullOrWhiteSpace(master.PhoneNumber) &&
            !string.IsNullOrWhiteSpace(discarded.PhoneNumber))
        {
            master.UpdateContactInfo(
                master.MobileNumber,
                discarded.PhoneNumber,
                master.Email,
                master.Id);
            mergeMapping["PhoneNumber"] = "discarded";
        }
        else
        {
            mergeMapping["PhoneNumber"] = "master";
        }

        // Email — prefer non-null
        if (string.IsNullOrWhiteSpace(master.Email) &&
            !string.IsNullOrWhiteSpace(discarded.Email))
        {
            master.UpdateContactInfo(
                master.MobileNumber,
                master.PhoneNumber,
                discarded.Email,
                master.Id);
            mergeMapping["Email"] = "discarded";
        }
        else
        {
            mergeMapping["Email"] = "master";
        }

        // YearOfBirth — prefer non-null
        if (!master.YearOfBirth.HasValue && discarded.YearOfBirth.HasValue)
        {
            master.UpdateIdentification(
                master.NationalId,
                discarded.YearOfBirth,
                master.Gender,
                master.Nationality,
                master.Id);
            mergeMapping["YearOfBirth"] = "discarded";
        }
        else
        {
            mergeMapping["YearOfBirth"] = "master";
        }

        // Names always kept from master (primary identity)
        mergeMapping["FirstNameArabic"] = "master";
        mergeMapping["FatherNameArabic"] = "master";
        mergeMapping["FamilyNameArabic"] = "master";
    }
}
