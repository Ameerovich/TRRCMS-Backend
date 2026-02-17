using System.Diagnostics;
using System.Text.Json;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Common;
using TRRCMS.Domain.Entities.Staging;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Infrastructure.Services.Validators;

/// <summary>لآ
/// Level 1: Data Consistency Validator (FR-D-4).
/// Checks required fields, data type validity, string length constraints,
/// and enum value ranges for all staging entities.
/// </summary>
public class DataConsistencyValidator : IStagingValidator
{
    public string Name => "DataConsistencyValidator";
    public int Level => 1;

    private readonly IStagingRepository<StagingBuilding> _buildingRepo;
    private readonly IStagingRepository<StagingPropertyUnit> _unitRepo;
    private readonly IStagingRepository<StagingPerson> _personRepo;
    private readonly IStagingRepository<StagingHousehold> _householdRepo;
    private readonly IStagingRepository<StagingPersonPropertyRelation> _relationRepo;
    private readonly IStagingRepository<StagingEvidence> _evidenceRepo;
    private readonly IStagingRepository<StagingClaim> _claimRepo;
    private readonly IStagingRepository<StagingSurvey> _surveyRepo;
    private readonly IVocabularyValidationService _vocabService;

    public DataConsistencyValidator(
        IStagingRepository<StagingBuilding> buildingRepo,
        IStagingRepository<StagingPropertyUnit> unitRepo,
        IStagingRepository<StagingPerson> personRepo,
        IStagingRepository<StagingHousehold> householdRepo,
        IStagingRepository<StagingPersonPropertyRelation> relationRepo,
        IStagingRepository<StagingEvidence> evidenceRepo,
        IStagingRepository<StagingClaim> claimRepo,
        IStagingRepository<StagingSurvey> surveyRepo,
        IVocabularyValidationService vocabService)
    {
        _buildingRepo = buildingRepo;
        _unitRepo = unitRepo;
        _personRepo = personRepo;
        _householdRepo = householdRepo;
        _relationRepo = relationRepo;
        _evidenceRepo = evidenceRepo;
        _claimRepo = claimRepo;
        _surveyRepo = surveyRepo;
        _vocabService = vocabService;
    }

    public async Task<ValidatorResult> ValidateAsync(
        Guid importPackageId, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        int totalErrors = 0, totalWarnings = 0, totalChecked = 0;

        var r1 = await ValidateEntitiesAsync(_buildingRepo, importPackageId, ValidateBuilding, ct);
        totalChecked += r1.Checked; totalErrors += r1.Errors; totalWarnings += r1.Warnings;

        var r2 = await ValidateEntitiesAsync(_unitRepo, importPackageId, ValidatePropertyUnit, ct);
        totalChecked += r2.Checked; totalErrors += r2.Errors; totalWarnings += r2.Warnings;

        var r3 = await ValidateEntitiesAsync(_personRepo, importPackageId, ValidatePerson, ct);
        totalChecked += r3.Checked; totalErrors += r3.Errors; totalWarnings += r3.Warnings;

        var r4 = await ValidateEntitiesAsync(_householdRepo, importPackageId, ValidateHousehold, ct);
        totalChecked += r4.Checked; totalErrors += r4.Errors; totalWarnings += r4.Warnings;

        var r5 = await ValidateEntitiesAsync(_relationRepo, importPackageId, ValidateRelation, ct);
        totalChecked += r5.Checked; totalErrors += r5.Errors; totalWarnings += r5.Warnings;

        var r6 = await ValidateEntitiesAsync(_evidenceRepo, importPackageId, ValidateEvidence, ct);
        totalChecked += r6.Checked; totalErrors += r6.Errors; totalWarnings += r6.Warnings;

        var r7 = await ValidateEntitiesAsync(_claimRepo, importPackageId, ValidateClaim, ct);
        totalChecked += r7.Checked; totalErrors += r7.Errors; totalWarnings += r7.Warnings;

        var r8 = await ValidateEntitiesAsync(_surveyRepo, importPackageId, ValidateSurvey, ct);
        totalChecked += r8.Checked; totalErrors += r8.Errors; totalWarnings += r8.Warnings;

        return new ValidatorResult
        {
            ValidatorName = Name,
            Level = Level,
            ErrorCount = totalErrors,
            WarningCount = totalWarnings,
            RecordsChecked = totalChecked,
            Duration = sw.Elapsed
        };
    }

    private async Task<(int Checked, int Errors, int Warnings)> ValidateEntitiesAsync<T>(
        IStagingRepository<T> repo, Guid packageId,
        Func<T, (List<string> errors, List<string> warnings)> validateFunc,
        CancellationToken ct)
        where T : BaseStagingEntity
    {
        var entities = await repo.GetByPackageIdAsync(packageId, ct);
        var modified = new List<T>();
        int errors = 0, warnings = 0;

        foreach (var entity in entities)
        {
            var (errs, warns) = validateFunc(entity);

            if (errs.Count > 0)
            {
                entity.MarkAsInvalid(
                    JsonSerializer.Serialize(errs),
                    warns.Count > 0 ? JsonSerializer.Serialize(warns) : null);
                errors += errs.Count;
                warnings += warns.Count;
                modified.Add(entity);
            }
            else if (warns.Count > 0)
            {
                entity.MarkAsValid(JsonSerializer.Serialize(warns));
                warnings += warns.Count;
                modified.Add(entity);
            }
        }

        if (modified.Count > 0)
        {
            await repo.UpdateRangeAsync(modified, ct);
            await repo.SaveChangesAsync(ct);
        }

        return (entities.Count, errors, warnings);
    }

    // ==================== PER-ENTITY VALIDATION RULES ====================

    private (List<string>, List<string>) ValidateBuilding(StagingBuilding b)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        if (string.IsNullOrWhiteSpace(b.GovernorateCode)) errors.Add("GovernorateCode is required");
        else if (b.GovernorateCode.Length != 2) errors.Add("GovernorateCode must be 2 digits");

        if (string.IsNullOrWhiteSpace(b.DistrictCode)) errors.Add("DistrictCode is required");
        else if (b.DistrictCode.Length != 2) errors.Add("DistrictCode must be 2 digits");

        if (string.IsNullOrWhiteSpace(b.SubDistrictCode)) errors.Add("SubDistrictCode is required");
        else if (b.SubDistrictCode.Length != 2) errors.Add("SubDistrictCode must be 2 digits");

        if (string.IsNullOrWhiteSpace(b.CommunityCode)) errors.Add("CommunityCode is required");
        else if (b.CommunityCode.Length != 3) errors.Add("CommunityCode must be 3 digits");

        if (string.IsNullOrWhiteSpace(b.NeighborhoodCode)) errors.Add("NeighborhoodCode is required");
        else if (b.NeighborhoodCode.Length != 3) errors.Add("NeighborhoodCode must be 3 digits");

        if (string.IsNullOrWhiteSpace(b.BuildingNumber)) errors.Add("BuildingNumber is required");
        else if (b.BuildingNumber.Length != 5) errors.Add("BuildingNumber must be 5 digits");

        if (!_vocabService.IsValidCode("building_type", (int)b.BuildingType)) errors.Add($"Invalid BuildingType: {b.BuildingType}");
        if (!_vocabService.IsValidCode("building_status", (int)b.Status)) errors.Add($"Invalid BuildingStatus: {b.Status}");

        if (b.NumberOfPropertyUnits < 0) errors.Add("NumberOfPropertyUnits cannot be negative");
        if (b.NumberOfApartments < 0) errors.Add("NumberOfApartments cannot be negative");
        if (b.NumberOfShops < 0) errors.Add("NumberOfShops cannot be negative");

        if (b.NumberOfApartments + b.NumberOfShops > b.NumberOfPropertyUnits && b.NumberOfPropertyUnits > 0)
            warnings.Add("Apartments + Shops exceeds total PropertyUnits");

        // Syria bounds check (basic — detailed in Level 5)
        if (b.Latitude.HasValue && (b.Latitude < 32.0m || b.Latitude > 37.5m))
            warnings.Add($"Latitude {b.Latitude} outside Syria bounds (32.0-37.5)");
        if (b.Longitude.HasValue && (b.Longitude < 35.5m || b.Longitude > 42.5m))
            warnings.Add($"Longitude {b.Longitude} outside Syria bounds (35.5-42.5)");

        return (errors, warnings);
    }

    private (List<string>, List<string>) ValidatePropertyUnit(StagingPropertyUnit u)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        if (u.OriginalBuildingId == Guid.Empty) errors.Add("OriginalBuildingId is required");
        if (string.IsNullOrWhiteSpace(u.UnitIdentifier)) errors.Add("UnitIdentifier is required");
        if (!_vocabService.IsValidCode("property_unit_type", (int)u.UnitType)) errors.Add($"Invalid UnitType: {u.UnitType}");
        if (!_vocabService.IsValidCode("property_unit_status", (int)u.Status)) errors.Add($"Invalid PropertyUnitStatus: {u.Status}");
        if (u.AreaSquareMeters.HasValue && u.AreaSquareMeters <= 0)
            warnings.Add("AreaSquareMeters should be positive");

        return (errors, warnings);
    }

    private (List<string>, List<string>) ValidatePerson(StagingPerson p)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        if (string.IsNullOrWhiteSpace(p.FamilyNameArabic)) errors.Add("FamilyNameArabic is required");
        if (string.IsNullOrWhiteSpace(p.FirstNameArabic)) errors.Add("FirstNameArabic is required");
        if (string.IsNullOrWhiteSpace(p.FatherNameArabic)) errors.Add("FatherNameArabic is required");

        if (!string.IsNullOrWhiteSpace(p.NationalId) && p.NationalId.Length > 20)
            warnings.Add($"NationalId length ({p.NationalId.Length}) exceeds expected maximum");

        if (p.YearOfBirth.HasValue && (p.YearOfBirth < 1900 || p.YearOfBirth > DateTime.UtcNow.Year))
            warnings.Add($"YearOfBirth {p.YearOfBirth} seems invalid");

        return (errors, warnings);
    }

    private (List<string>, List<string>) ValidateHousehold(StagingHousehold h)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        if (h.OriginalPropertyUnitId == Guid.Empty) errors.Add("OriginalPropertyUnitId is required");
        if (string.IsNullOrWhiteSpace(h.HeadOfHouseholdName)) errors.Add("HeadOfHouseholdName is required");
        if (h.HouseholdSize <= 0) errors.Add("HouseholdSize must be > 0");
        if (h.MaleCount < 0 || h.FemaleCount < 0) errors.Add("Gender counts cannot be negative");

        return (errors, warnings);
    }

    private (List<string>, List<string>) ValidateRelation(StagingPersonPropertyRelation r)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        if (r.OriginalPersonId == Guid.Empty) errors.Add("OriginalPersonId is required");
        if (r.OriginalPropertyUnitId == Guid.Empty) errors.Add("OriginalPropertyUnitId is required");
        if (!_vocabService.IsValidCode("relation_type", (int)r.RelationType)) errors.Add($"Invalid RelationType: {r.RelationType}");

        if (r.OwnershipShare.HasValue && (r.OwnershipShare < 0 || r.OwnershipShare > 100))
            errors.Add($"OwnershipShare must be 0-100, got {r.OwnershipShare}");

        return (errors, warnings);
    }

    private (List<string>, List<string>) ValidateEvidence(StagingEvidence e)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        if (!_vocabService.IsValidCode("evidence_type", (int)e.EvidenceType)) errors.Add($"Invalid EvidenceType: {e.EvidenceType}");
        if (string.IsNullOrWhiteSpace(e.OriginalFileName)) errors.Add("OriginalFileName is required");
        if (e.FileSizeBytes <= 0) warnings.Add("FileSizeBytes is 0 or negative");

        // At least one parent reference should be present
        if (e.OriginalPersonId == null && e.OriginalPersonPropertyRelationId == null && e.OriginalClaimId == null)
            warnings.Add("Evidence has no linked Person, Relation, or Claim");

        return (errors, warnings);
    }

    private (List<string>, List<string>) ValidateClaim(StagingClaim c)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        if (c.OriginalPropertyUnitId == Guid.Empty) errors.Add("OriginalPropertyUnitId is required");
        if (string.IsNullOrWhiteSpace(c.ClaimType)) errors.Add("ClaimType is required");
        if (!_vocabService.IsValidCode("claim_source", (int)c.ClaimSource)) errors.Add($"Invalid ClaimSource: {c.ClaimSource}");

        return (errors, warnings);
    }

    private (List<string>, List<string>) ValidateSurvey(StagingSurvey s)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        if (s.OriginalBuildingId == Guid.Empty) errors.Add("OriginalBuildingId is required");
        if (s.SurveyDate == default) errors.Add("SurveyDate is required");
        if (s.SurveyDate > DateTime.UtcNow.AddDays(1))
            warnings.Add($"SurveyDate {s.SurveyDate:yyyy-MM-dd} is in the future");

        return (errors, warnings);
    }
}
