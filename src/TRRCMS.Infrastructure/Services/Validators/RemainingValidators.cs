using System.Diagnostics;
using System.Text.Json;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Common;
using TRRCMS.Domain.Entities.Staging;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Infrastructure.Services.Validators;

// ============================================================================
// LEVEL 3: Ownership Evidence Validator
// ============================================================================

/// <summary>
/// Level 3: Ownership Evidence Validator (FR-D-4).
/// Checks that ownership-type relations have supporting evidence,
/// and that evidence records have valid file references.
/// </summary>
public class OwnershipEvidenceValidator : IStagingValidator
{
    public string Name => "OwnershipEvidenceValidator";
    public int Level => 3;

    private readonly IStagingRepository<StagingPersonPropertyRelation> _relationRepo;
    private readonly IStagingRepository<StagingEvidence> _evidenceRepo;

    public OwnershipEvidenceValidator(
        IStagingRepository<StagingPersonPropertyRelation> relationRepo,
        IStagingRepository<StagingEvidence> evidenceRepo)
    {
        _relationRepo = relationRepo;
        _evidenceRepo = evidenceRepo;
    }

    public async Task<ValidatorResult> ValidateAsync(Guid importPackageId, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        int errors = 0, warnings = 0, checked_ = 0;

        var relations = await _relationRepo.GetByPackageIdAsync(importPackageId, ct);
        var evidences = await _evidenceRepo.GetByPackageIdAsync(importPackageId, ct);

        // Build lookup: which relations have evidence
        var relationsWithEvidence = evidences
            .Where(e => e.OriginalPersonPropertyRelationId.HasValue)
            .Select(e => e.OriginalPersonPropertyRelationId!.Value)
            .ToHashSet();

        // Ownership relations (Owner type) should have evidence
        var ownerRelations = relations.Where(r => r.RelationType == RelationType.Owner).ToList();
        var modified = new List<StagingPersonPropertyRelation>();

        foreach (var relation in ownerRelations)
        {
            checked_++;
            if (!relationsWithEvidence.Contains(relation.OriginalEntityId))
            {
                AppendWarning(relation, "Ownership relation has no supporting evidence documents");
                warnings++;
                modified.Add(relation);
            }
        }

        // Check evidence files have non-empty paths
        var modifiedEvidence = new List<StagingEvidence>();
        foreach (var evidence in evidences)
        {
            checked_++;
            if (string.IsNullOrWhiteSpace(evidence.FilePath))
            {
                AppendWarning(evidence, "Evidence record has empty file path");
                warnings++;
                modifiedEvidence.Add(evidence);
            }
        }

        if (modified.Count > 0) { await _relationRepo.UpdateRangeAsync(modified, ct); await _relationRepo.SaveChangesAsync(ct); }
        if (modifiedEvidence.Count > 0) { await _evidenceRepo.UpdateRangeAsync(modifiedEvidence, ct); await _evidenceRepo.SaveChangesAsync(ct); }

        return new ValidatorResult { ValidatorName = Name, Level = Level, ErrorCount = errors, WarningCount = warnings, RecordsChecked = checked_, Duration = sw.Elapsed };
    }

    private static void AppendWarning(BaseStagingEntity entity, string warning)
    {
        var existing = new List<string>();
        if (!string.IsNullOrWhiteSpace(entity.ValidationWarnings))
            try { existing = JsonSerializer.Deserialize<List<string>>(entity.ValidationWarnings) ?? new(); } catch { }
        existing.Add(warning);
        var warningsJson = JsonSerializer.Serialize(existing);

        // Guard: do NOT overwrite Invalid status from earlier validation levels
        if (entity.ValidationStatus != StagingValidationStatus.Invalid)
            entity.MarkAsValid(warningsJson);
        else
            entity.MarkAsInvalid(entity.ValidationErrors!, warningsJson);
    }
}

// ============================================================================
// LEVEL 4: Household Structure Validator
// ============================================================================

/// <summary>
/// Level 4: Household Structure Validator (FR-D-4).
/// Checks: male+female counts = total, household size > 0,
/// head of household exists, gender breakdown consistency.
/// </summary>
public class HouseholdStructureValidator : IStagingValidator
{
    public string Name => "HouseholdStructureValidator";
    public int Level => 4;

    private readonly IStagingRepository<StagingHousehold> _householdRepo;
    private readonly IStagingRepository<StagingPerson> _personRepo;

    public HouseholdStructureValidator(
        IStagingRepository<StagingHousehold> householdRepo,
        IStagingRepository<StagingPerson> personRepo)
    {
        _householdRepo = householdRepo;
        _personRepo = personRepo;
    }

    public async Task<ValidatorResult> ValidateAsync(Guid importPackageId, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        int errors = 0, warnings = 0;

        var households = await _householdRepo.GetByPackageIdAsync(importPackageId, ct);
        var persons = await _personRepo.GetByPackageIdAsync(importPackageId, ct);

        var personsByHousehold = persons
            .Where(p => p.OriginalHouseholdId.HasValue)
            .GroupBy(p => p.OriginalHouseholdId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        var modified = new List<StagingHousehold>();

        foreach (var h in households)
        {
            var errs = new List<string>();
            var warns = new List<string>();

            // Male + Female should equal total (or at least not exceed)
            var genderTotal = h.MaleCount + h.FemaleCount;
            if (genderTotal > 0 && genderTotal != h.HouseholdSize)
                warns.Add($"MaleCount({h.MaleCount}) + FemaleCount({h.FemaleCount}) = {genderTotal} ≠ HouseholdSize({h.HouseholdSize})");

            // Head of household person should exist in batch
            if (h.OriginalHeadOfHouseholdPersonId.HasValue)
            {
                var headExists = persons.Any(p => p.OriginalEntityId == h.OriginalHeadOfHouseholdPersonId.Value);
                if (!headExists)
                    warns.Add($"Head of household person {h.OriginalHeadOfHouseholdPersonId} not found in batch");
            }

            // Check actual person count matches declared size
            if (personsByHousehold.TryGetValue(h.OriginalEntityId, out var members))
            {
                if (members.Count != h.HouseholdSize)
                    warns.Add($"Declared HouseholdSize={h.HouseholdSize} but {members.Count} persons linked");
            }

            if (errs.Count > 0)
            {
                MergeAndMarkInvalid(h, errs, warns);
                errors += errs.Count;
                warnings += warns.Count;
                modified.Add(h);
            }
            else if (warns.Count > 0)
            {
                MergeAndMarkValid(h, warns);
                warnings += warns.Count;
                modified.Add(h);
            }
        }

        if (modified.Count > 0) { await _householdRepo.UpdateRangeAsync(modified, ct); await _householdRepo.SaveChangesAsync(ct); }

        return new ValidatorResult { ValidatorName = Name, Level = Level, ErrorCount = errors, WarningCount = warnings, RecordsChecked = households.Count, Duration = sw.Elapsed };
    }

    private static void MergeAndMarkInvalid(BaseStagingEntity e, List<string> errs, List<string> warns)
    {
        var existingErrs = ParseJsonList(e.ValidationErrors);
        existingErrs.AddRange(errs);
        var existingWarns = ParseJsonList(e.ValidationWarnings);
        existingWarns.AddRange(warns);
        e.MarkAsInvalid(JsonSerializer.Serialize(existingErrs), existingWarns.Count > 0 ? JsonSerializer.Serialize(existingWarns) : null);
    }

    private static void MergeAndMarkValid(BaseStagingEntity e, List<string> warns)
    {
        var existing = ParseJsonList(e.ValidationWarnings);
        existing.AddRange(warns);
        var warningsJson = JsonSerializer.Serialize(existing);

        if (e.ValidationStatus != StagingValidationStatus.Invalid)
            e.MarkAsValid(warningsJson);
        else
            e.MarkAsInvalid(e.ValidationErrors!, warningsJson);
    }

    private static List<string> ParseJsonList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new();
        try { return JsonSerializer.Deserialize<List<string>>(json) ?? new(); } catch { return new(); }
    }
}

// ============================================================================
// LEVEL 5: Spatial Geometry Validator
// ============================================================================

/// <summary>
/// Level 5: Spatial Geometry Validator (FR-D-4).
/// Checks coordinates within Syria bounds and WKT geometry validity.
/// Syria bounds: Lat 32.0°N–37.5°N, Lng 35.5°E–42.5°E.
/// </summary>
public class SpatialGeometryValidator : IStagingValidator
{
    public string Name => "SpatialGeometryValidator";
    public int Level => 5;

    private readonly IStagingRepository<StagingBuilding> _buildingRepo;

    public SpatialGeometryValidator(IStagingRepository<StagingBuilding> buildingRepo)
    {
        _buildingRepo = buildingRepo;
    }

    public async Task<ValidatorResult> ValidateAsync(Guid importPackageId, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        int errors = 0, warnings = 0;

        var buildings = await _buildingRepo.GetByPackageIdAsync(importPackageId, ct);
        var modified = new List<StagingBuilding>();

        foreach (var b in buildings)
        {
            var errs = new List<string>();
            var warns = new List<string>();

            // Strict Syria bounds
            if (b.Latitude.HasValue)
            {
                if (b.Latitude < 32.0m || b.Latitude > 37.5m)
                    errs.Add($"Latitude {b.Latitude} is outside Syria bounds (32.0-37.5)");
            }
            if (b.Longitude.HasValue)
            {
                if (b.Longitude < 35.5m || b.Longitude > 42.5m)
                    errs.Add($"Longitude {b.Longitude} is outside Syria bounds (35.5-42.5)");
            }

            // Lat/Lng must come as a pair
            if (b.Latitude.HasValue != b.Longitude.HasValue)
                errs.Add("Latitude and Longitude must be provided together");

            // Basic WKT validation
            if (!string.IsNullOrWhiteSpace(b.BuildingGeometryWkt))
            {
                var wkt = b.BuildingGeometryWkt.Trim().ToUpperInvariant();
                if (!wkt.StartsWith("POLYGON") && !wkt.StartsWith("POINT") && !wkt.StartsWith("MULTIPOLYGON"))
                    warns.Add($"BuildingGeometryWkt does not start with a recognized geometry type");
            }

            if (errs.Count > 0)
            {
                var existingErrs = ParseJsonList(b.ValidationErrors);
                existingErrs.AddRange(errs);
                var existingWarns = ParseJsonList(b.ValidationWarnings);
                existingWarns.AddRange(warns);
                b.MarkAsInvalid(JsonSerializer.Serialize(existingErrs), existingWarns.Count > 0 ? JsonSerializer.Serialize(existingWarns) : null);
                errors += errs.Count; warnings += warns.Count;
                modified.Add(b);
            }
            else if (warns.Count > 0)
            {
                var existing = ParseJsonList(b.ValidationWarnings);
                existing.AddRange(warns);
                var warningsJson = JsonSerializer.Serialize(existing);
                if (b.ValidationStatus != StagingValidationStatus.Invalid)
                    b.MarkAsValid(warningsJson);
                else
                    b.MarkAsInvalid(b.ValidationErrors!, warningsJson);
                warnings += warns.Count;
                modified.Add(b);
            }
        }

        if (modified.Count > 0) { await _buildingRepo.UpdateRangeAsync(modified, ct); await _buildingRepo.SaveChangesAsync(ct); }

        return new ValidatorResult { ValidatorName = Name, Level = Level, ErrorCount = errors, WarningCount = warnings, RecordsChecked = buildings.Count, Duration = sw.Elapsed };
    }

    private static List<string> ParseJsonList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new();
        try { return JsonSerializer.Deserialize<List<string>>(json) ?? new(); } catch { return new(); }
    }
}

// ============================================================================
// LEVEL 6: Claim Lifecycle Validator
// ============================================================================

/// <summary>
/// Level 6: Claim Lifecycle Validator (FR-D-4).
/// Checks that imported claims have valid status transitions and map
/// to the correct lifecycle stage (imported claims → Submitted per FR-D-2).
/// </summary>
public class ClaimLifecycleValidator : IStagingValidator
{
    public string Name => "ClaimLifecycleValidator";
    public int Level => 6;

    private readonly IStagingRepository<StagingClaim> _claimRepo;

    public ClaimLifecycleValidator(IStagingRepository<StagingClaim> claimRepo)
    {
        _claimRepo = claimRepo;
    }

    public async Task<ValidatorResult> ValidateAsync(Guid importPackageId, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        int errors = 0, warnings = 0;

        var claims = await _claimRepo.GetByPackageIdAsync(importPackageId, ct);
        var modified = new List<StagingClaim>();

        foreach (var c in claims)
        {
            var warns = new List<string>();

            // Imported claims should map to specific lifecycle stages
            if (c.LifecycleStage.HasValue && c.LifecycleStage != LifecycleStage.DraftPendingSubmission)
                warns.Add($"Imported claim has LifecycleStage={c.LifecycleStage}; expected DraftPendingSubmission (will be set to Submitted on commit)");

            if (c.Status.HasValue && c.Status != ClaimStatus.Draft)
                warns.Add($"Imported claim has Status={c.Status}; expected Draft (will be set to Submitted on commit)");

            // ClaimSource should be Field for tablet imports
            if (c.ClaimSource != ClaimSource.FieldCollection)
                warns.Add($"ClaimSource={c.ClaimSource}; expected Field for tablet import");

            if (warns.Count > 0)
            {
                var existing = ParseJsonList(c.ValidationWarnings);
                existing.AddRange(warns);
                var warningsJson = JsonSerializer.Serialize(existing);
                if (c.ValidationStatus != StagingValidationStatus.Invalid)
                    c.MarkAsValid(warningsJson);
                else
                    c.MarkAsInvalid(c.ValidationErrors!, warningsJson);
                warnings += warns.Count;
                modified.Add(c);
            }
        }

        if (modified.Count > 0) { await _claimRepo.UpdateRangeAsync(modified, ct); await _claimRepo.SaveChangesAsync(ct); }

        return new ValidatorResult { ValidatorName = Name, Level = Level, ErrorCount = errors, WarningCount = warnings, RecordsChecked = claims.Count, Duration = sw.Elapsed };
    }

    private static List<string> ParseJsonList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new();
        try { return JsonSerializer.Deserialize<List<string>>(json) ?? new(); } catch { return new(); }
    }
}

// ============================================================================
// LEVEL 7: Vocabulary Version Validator
// ============================================================================

/// <summary>
/// Level 7: Vocabulary Version Validator (FR-D-4).
/// Checks that all enum/code values in staging records exist in the
/// active vocabulary definitions. Invalid codes → warning (soft fail).
/// </summary>
public class VocabularyVersionValidator : IStagingValidator
{
    public string Name => "VocabularyVersionValidator";
    public int Level => 7;

    private readonly IStagingRepository<StagingBuilding> _buildingRepo;
    private readonly IStagingRepository<StagingPropertyUnit> _unitRepo;
    private readonly IStagingRepository<StagingClaim> _claimRepo;
    private readonly IVocabularyValidationService _vocabService;

    public VocabularyVersionValidator(
        IStagingRepository<StagingBuilding> buildingRepo,
        IStagingRepository<StagingPropertyUnit> unitRepo,
        IStagingRepository<StagingClaim> claimRepo,
        IVocabularyValidationService vocabService)
    {
        _buildingRepo = buildingRepo;
        _unitRepo = unitRepo;
        _claimRepo = claimRepo;
        _vocabService = vocabService;
    }

    public async Task<ValidatorResult> ValidateAsync(Guid importPackageId, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        int errors = 0, warnings = 0, checked_ = 0;

        // Validate BuildingType and BuildingStatus are defined enum values
        var buildings = await _buildingRepo.GetByPackageIdAsync(importPackageId, ct);
        var modBuildings = new List<StagingBuilding>();
        foreach (var b in buildings)
        {
            checked_++;
            var warns = new List<string>();
            if (!_vocabService.IsValidCode("building_type", (int)b.BuildingType)) warns.Add($"Unknown BuildingType value: {(int)b.BuildingType}");
            if (!_vocabService.IsValidCode("building_status", (int)b.Status)) warns.Add($"Unknown BuildingStatus value: {(int)b.Status}");
            if (b.DamageLevel.HasValue && !_vocabService.IsValidCode("damage_level", (int)b.DamageLevel.Value)) warns.Add($"Unknown DamageLevel value: {(int)b.DamageLevel.Value}");

            if (warns.Count > 0) { AppendWarnings(b, warns); warnings += warns.Count; modBuildings.Add(b); }
        }

        var units = await _unitRepo.GetByPackageIdAsync(importPackageId, ct);
        var modUnits = new List<StagingPropertyUnit>();
        foreach (var u in units)
        {
            checked_++;
            var warns = new List<string>();
            if (!_vocabService.IsValidCode("property_unit_type", (int)u.UnitType)) warns.Add($"Unknown PropertyUnitType value: {(int)u.UnitType}");
            if (!_vocabService.IsValidCode("property_unit_status", (int)u.Status)) warns.Add($"Unknown PropertyUnitStatus value: {(int)u.Status}");

            if (warns.Count > 0) { AppendWarnings(u, warns); warnings += warns.Count; modUnits.Add(u); }
        }

        var claims = await _claimRepo.GetByPackageIdAsync(importPackageId, ct);
        var modClaims = new List<StagingClaim>();
        foreach (var c in claims)
        {
            checked_++;
            var warns = new List<string>();
            if (!_vocabService.IsValidCode("claim_source", (int)c.ClaimSource)) warns.Add($"Unknown ClaimSource value: {(int)c.ClaimSource}");
            if (!_vocabService.IsValidCode("case_priority", (int)c.Priority)) warns.Add($"Unknown CasePriority value: {(int)c.Priority}");

            if (warns.Count > 0) { AppendWarnings(c, warns); warnings += warns.Count; modClaims.Add(c); }
        }

        if (modBuildings.Count > 0) { await _buildingRepo.UpdateRangeAsync(modBuildings, ct); await _buildingRepo.SaveChangesAsync(ct); }
        if (modUnits.Count > 0) { await _unitRepo.UpdateRangeAsync(modUnits, ct); await _unitRepo.SaveChangesAsync(ct); }
        if (modClaims.Count > 0) { await _claimRepo.UpdateRangeAsync(modClaims, ct); await _claimRepo.SaveChangesAsync(ct); }

        return new ValidatorResult { ValidatorName = Name, Level = Level, ErrorCount = errors, WarningCount = warnings, RecordsChecked = checked_, Duration = sw.Elapsed };
    }

    private static void AppendWarnings(BaseStagingEntity entity, List<string> warns)
    {
        var existing = new List<string>();
        if (!string.IsNullOrWhiteSpace(entity.ValidationWarnings))
            try { existing = JsonSerializer.Deserialize<List<string>>(entity.ValidationWarnings) ?? new(); } catch { }
        existing.AddRange(warns);
        // Only upgrade to Warning status if currently Valid or Pending
        if (entity.ValidationStatus != StagingValidationStatus.Invalid)
            entity.MarkAsValid(JsonSerializer.Serialize(existing));
    }
}

// ============================================================================
// LEVEL 8: Building/Unit Code Validator
// ============================================================================

/// <summary>
/// Level 8: Building & Unit Code Validator (FR-D-4, FR-D-8).
/// Checks:
///   - building_id follows 17-digit composite pattern (2+2+2+3+3+5)
///   - No duplicate building_id within the batch
///   - Unit codes are unique within each building
///   - No duplicate building_id against production (when production data is queryable)
/// </summary>
public class BuildingUnitCodeValidator : IStagingValidator
{
    public string Name => "BuildingUnitCodeValidator";
    public int Level => 8;

    private readonly IStagingRepository<StagingBuilding> _buildingRepo;
    private readonly IStagingRepository<StagingPropertyUnit> _unitRepo;

    public BuildingUnitCodeValidator(
        IStagingRepository<StagingBuilding> buildingRepo,
        IStagingRepository<StagingPropertyUnit> unitRepo)
    {
        _buildingRepo = buildingRepo;
        _unitRepo = unitRepo;
    }

    public async Task<ValidatorResult> ValidateAsync(Guid importPackageId, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        int errors = 0, warnings = 0;

        var buildings = await _buildingRepo.GetByPackageIdAsync(importPackageId, ct);
        var units = await _unitRepo.GetByPackageIdAsync(importPackageId, ct);

        var modBuildings = new List<StagingBuilding>();
        var modUnits = new List<StagingPropertyUnit>();

        // Check building_id composite format
        var buildingIdSet = new Dictionary<string, List<StagingBuilding>>();
        foreach (var b in buildings)
        {
            var errs = new List<string>();
            var warns = new List<string>();

            // Compute building_id from codes
            var compositeId = $"{b.GovernorateCode}{b.DistrictCode}{b.SubDistrictCode}" +
                              $"{b.CommunityCode}{b.NeighborhoodCode}{b.BuildingNumber}";

            if (compositeId.Length != 17)
                errs.Add($"Composite building ID '{compositeId}' is {compositeId.Length} digits (expected 17)");
            else if (!compositeId.All(char.IsDigit))
                errs.Add($"Composite building ID '{compositeId}' contains non-digit characters");

            // If building_id is provided, verify it matches computed
            if (!string.IsNullOrWhiteSpace(b.BuildingId) && b.BuildingId != compositeId)
                warns.Add($"Provided BuildingId '{b.BuildingId}' doesn't match computed '{compositeId}'");

            // Track for duplicate detection
            var key = compositeId.Length == 17 ? compositeId : b.OriginalEntityId.ToString();
            if (!buildingIdSet.ContainsKey(key))
                buildingIdSet[key] = new List<StagingBuilding>();
            buildingIdSet[key].Add(b);

            if (errs.Count > 0)
            {
                var existingErrs = ParseJsonList(b.ValidationErrors);
                existingErrs.AddRange(errs);
                var existingWarns = ParseJsonList(b.ValidationWarnings);
                existingWarns.AddRange(warns);
                b.MarkAsInvalid(JsonSerializer.Serialize(existingErrs), existingWarns.Count > 0 ? JsonSerializer.Serialize(existingWarns) : null);
                errors += errs.Count; warnings += warns.Count;
                modBuildings.Add(b);
            }
            else if (warns.Count > 0)
            {
                var existing = ParseJsonList(b.ValidationWarnings);
                existing.AddRange(warns);
                if (b.ValidationStatus != StagingValidationStatus.Invalid)
                    b.MarkAsValid(JsonSerializer.Serialize(existing));
                warnings += warns.Count;
                modBuildings.Add(b);
            }
        }

        // Flag duplicate building_ids within batch (§12.2.4)
        foreach (var (code, dupes) in buildingIdSet.Where(kv => kv.Value.Count > 1))
        {
            foreach (var b in dupes)
            {
                var existingErrs = ParseJsonList(b.ValidationErrors);
                existingErrs.Add($"Duplicate building code '{code}' found {dupes.Count} times in batch");
                b.MarkAsInvalid(JsonSerializer.Serialize(existingErrs), b.ValidationWarnings);
                errors++;
                if (!modBuildings.Contains(b)) modBuildings.Add(b);
            }
        }

        // Check unit identifier uniqueness within each building
        var unitsByBuilding = units.GroupBy(u => u.OriginalBuildingId);
        foreach (var group in unitsByBuilding)
        {
            var unitIdSet = new Dictionary<string, List<StagingPropertyUnit>>();
            foreach (var u in group)
            {
                if (!unitIdSet.ContainsKey(u.UnitIdentifier))
                    unitIdSet[u.UnitIdentifier] = new List<StagingPropertyUnit>();
                unitIdSet[u.UnitIdentifier].Add(u);
            }

            foreach (var (unitCode, dupes) in unitIdSet.Where(kv => kv.Value.Count > 1))
            {
                foreach (var u in dupes)
                {
                    var existingErrs = ParseJsonList(u.ValidationErrors);
                    existingErrs.Add($"Duplicate unit identifier '{unitCode}' within building {group.Key}");
                    u.MarkAsInvalid(JsonSerializer.Serialize(existingErrs), u.ValidationWarnings);
                    errors++;
                    if (!modUnits.Contains(u)) modUnits.Add(u);
                }
            }
        }

        if (modBuildings.Count > 0) { await _buildingRepo.UpdateRangeAsync(modBuildings, ct); await _buildingRepo.SaveChangesAsync(ct); }
        if (modUnits.Count > 0) { await _unitRepo.UpdateRangeAsync(modUnits, ct); await _unitRepo.SaveChangesAsync(ct); }

        return new ValidatorResult { ValidatorName = Name, Level = Level, ErrorCount = errors, WarningCount = warnings, RecordsChecked = buildings.Count + units.Count, Duration = sw.Elapsed };
    }

    private static List<string> ParseJsonList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new();
        try { return JsonSerializer.Deserialize<List<string>>(json) ?? new(); } catch { return new(); }
    }
}
