using System.Diagnostics;
using System.Text.Json;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Common;
using TRRCMS.Domain.Entities.Staging;

namespace TRRCMS.Infrastructure.Services.Validators;

/// <summary>
/// Level 2: Cross-Entity Relation Validator (FR-D-4).
/// Checks that intra-batch FK references are valid:
///   - PropertyUnit.OriginalBuildingId → exists in StagingBuilding
///   - Household.OriginalPropertyUnitId → exists in StagingPropertyUnit
///   - Relation.OriginalPersonId → exists in StagingPerson
///   - Relation.OriginalPropertyUnitId → exists in StagingPropertyUnit
///   - Evidence.OriginalPersonId → exists in StagingPerson (if present)
///   - Evidence.OriginalClaimId → exists in StagingClaim (if present)
///   - Claim.OriginalPropertyUnitId → exists in StagingPropertyUnit
///   - Survey.OriginalBuildingId → exists in StagingBuilding
/// No orphan records allowed within the batch.
/// </summary>
public class CrossEntityRelationValidator : IStagingValidator
{
    public string Name => "CrossEntityRelationValidator";
    public int Level => 2;

    private readonly IStagingRepository<StagingBuilding> _buildingRepo;
    private readonly IStagingRepository<StagingPropertyUnit> _unitRepo;
    private readonly IStagingRepository<StagingPerson> _personRepo;
    private readonly IStagingRepository<StagingHousehold> _householdRepo;
    private readonly IStagingRepository<StagingPersonPropertyRelation> _relationRepo;
    private readonly IStagingRepository<StagingEvidence> _evidenceRepo;
    private readonly IStagingRepository<StagingClaim> _claimRepo;
    private readonly IStagingRepository<StagingSurvey> _surveyRepo;

    public CrossEntityRelationValidator(
        IStagingRepository<StagingBuilding> buildingRepo,
        IStagingRepository<StagingPropertyUnit> unitRepo,
        IStagingRepository<StagingPerson> personRepo,
        IStagingRepository<StagingHousehold> householdRepo,
        IStagingRepository<StagingPersonPropertyRelation> relationRepo,
        IStagingRepository<StagingEvidence> evidenceRepo,
        IStagingRepository<StagingClaim> claimRepo,
        IStagingRepository<StagingSurvey> surveyRepo)
    {
        _buildingRepo = buildingRepo;
        _unitRepo = unitRepo;
        _personRepo = personRepo;
        _householdRepo = householdRepo;
        _relationRepo = relationRepo;
        _evidenceRepo = evidenceRepo;
        _claimRepo = claimRepo;
        _surveyRepo = surveyRepo;
    }

    public async Task<ValidatorResult> ValidateAsync(
        Guid importPackageId, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        int totalErrors = 0, totalWarnings = 0, totalChecked = 0;

        // Build lookup sets of OriginalEntityIds for each entity type
        var buildings = await _buildingRepo.GetByPackageIdAsync(importPackageId, ct);
        var units = await _unitRepo.GetByPackageIdAsync(importPackageId, ct);
        var persons = await _personRepo.GetByPackageIdAsync(importPackageId, ct);
        var households = await _householdRepo.GetByPackageIdAsync(importPackageId, ct);
        var relations = await _relationRepo.GetByPackageIdAsync(importPackageId, ct);
        var evidences = await _evidenceRepo.GetByPackageIdAsync(importPackageId, ct);
        var claims = await _claimRepo.GetByPackageIdAsync(importPackageId, ct);
        var surveys = await _surveyRepo.GetByPackageIdAsync(importPackageId, ct);

        var buildingIds = buildings.Select(b => b.OriginalEntityId).ToHashSet();
        var unitIds = units.Select(u => u.OriginalEntityId).ToHashSet();
        var personIds = persons.Select(p => p.OriginalEntityId).ToHashSet();
        var claimIds = claims.Select(c => c.OriginalEntityId).ToHashSet();

        // Validate PropertyUnit → Building
        totalChecked += CheckReferences(units, u => u.OriginalBuildingId, buildingIds,
            "PropertyUnit", "Building", ref totalErrors);

        // Validate Household → PropertyUnit
        totalChecked += CheckReferences(households, h => h.OriginalPropertyUnitId, unitIds,
            "Household", "PropertyUnit", ref totalErrors);

        // Validate Relation → Person
        totalChecked += CheckReferences(relations, r => r.OriginalPersonId, personIds,
            "PersonPropertyRelation.PersonId", "Person", ref totalErrors);

        // Validate Relation → PropertyUnit
        totalChecked += CheckReferences(relations, r => r.OriginalPropertyUnitId, unitIds,
            "PersonPropertyRelation.PropertyUnitId", "PropertyUnit", ref totalErrors);

        // Validate Claim → PropertyUnit
        totalChecked += CheckReferences(claims, c => c.OriginalPropertyUnitId, unitIds,
            "Claim", "PropertyUnit", ref totalErrors);

        // Validate Survey → Building
        totalChecked += CheckReferences(surveys, s => s.OriginalBuildingId, buildingIds,
            "Survey", "Building", ref totalErrors);

        // Validate Evidence → Person (optional FK)
        foreach (var e in evidences)
        {
            var errors = new List<string>();
            if (e.OriginalPersonId.HasValue && !personIds.Contains(e.OriginalPersonId.Value))
                errors.Add($"Referenced Person {e.OriginalPersonId} not found in batch");
            if (e.OriginalClaimId.HasValue && !claimIds.Contains(e.OriginalClaimId.Value))
                errors.Add($"Referenced Claim {e.OriginalClaimId} not found in batch");

            if (errors.Count > 0)
            {
                AppendErrors(e, errors);
                totalErrors += errors.Count;
            }
            totalChecked++;
        }

        // Save all modified entities
        await SaveModifiedAsync(_unitRepo, units, ct);
        await SaveModifiedAsync(_householdRepo, households, ct);
        await SaveModifiedAsync(_relationRepo, relations, ct);
        await SaveModifiedAsync(_claimRepo, claims, ct);
        await SaveModifiedAsync(_surveyRepo, surveys, ct);
        await SaveModifiedAsync(_evidenceRepo, evidences, ct);

        return new ValidatorResult
        {
            ValidatorName = Name, Level = Level,
            ErrorCount = totalErrors, WarningCount = totalWarnings,
            RecordsChecked = totalChecked, Duration = sw.Elapsed
        };
    }

    private static int CheckReferences<T>(
        List<T> entities, Func<T, Guid> fkSelector, HashSet<Guid> validIds,
        string childType, string parentType, ref int errorCount)
        where T : BaseStagingEntity
    {
        foreach (var entity in entities)
        {
            var fkValue = fkSelector(entity);
            if (fkValue != Guid.Empty && !validIds.Contains(fkValue))
            {
                AppendErrors(entity, new List<string>
                {
                    $"{childType} references {parentType} {fkValue} which does not exist in batch"
                });
                errorCount++;
            }
        }
        return entities.Count;
    }

    private static void AppendErrors(BaseStagingEntity entity, List<string> newErrors)
    {
        // Merge with existing errors if entity was already marked invalid by Level 1
        var existingErrors = new List<string>();
        if (!string.IsNullOrWhiteSpace(entity.ValidationErrors))
        {
            try { existingErrors = JsonSerializer.Deserialize<List<string>>(entity.ValidationErrors) ?? new(); }
            catch { /* ignore parse failures */ }
        }
        existingErrors.AddRange(newErrors);
        entity.MarkAsInvalid(JsonSerializer.Serialize(existingErrors), entity.ValidationWarnings);
    }

    private static async Task SaveModifiedAsync<T>(
        IStagingRepository<T> repo, List<T> entities, CancellationToken ct)
        where T : BaseStagingEntity
    {
        var modified = entities.Where(e =>
            e.ValidationStatus == Domain.Enums.StagingValidationStatus.Invalid).ToList();
        if (modified.Count > 0)
        {
            await repo.UpdateRangeAsync(modified, ct);
            await repo.SaveChangesAsync(ct);
        }
    }
}
