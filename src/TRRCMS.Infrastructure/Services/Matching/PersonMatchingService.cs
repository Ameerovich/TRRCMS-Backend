using Microsoft.Extensions.Logging;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Entities.Staging;
using TRRCMS.Domain.Enums;
using TRRCMS.Domain.ValueObjects;

namespace TRRCMS.Infrastructure.Services.Matching;

/// <summary>
/// Compares staging persons against production persons and within-batch staging persons
/// to detect duplicates.
///
/// Scoring algorithm (FR-D-5):
///   - National ID exact match:       100 points (deterministic — short-circuit)
///   - Phone number exact match:       +30 points
///   - Arabic name Levenshtein:         0–40 points (weighted composite of 3 name parts)
///   - Year of birth exact match:      +15 points
///   - Gender exact match:             +15 points
///   Max composite score = 100 (capped).
///
/// Thresholds:
///   ≥ 90 → High confidence
///   70–89 → Medium confidence
///   &lt; 70 → below threshold (not flagged)
///
/// UC-008 (Resolve Person Duplicates).
/// </summary>
public class PersonMatchingService
{
    private const decimal NationalIdScore = 100m;
    private const decimal PhoneScore = 30m;
    private const decimal MaxNameScore = 40m;
    private const decimal YearOfBirthScore = 15m;
    private const decimal GenderScore = 15m;
    private const decimal MaxCompositeScore = 100m;
    private const decimal MinimumThreshold = 70m;

    private readonly IPersonRepository _personRepository;
    private readonly ILogger<PersonMatchingService> _logger;

    public PersonMatchingService(
        IPersonRepository personRepository,
        ILogger<PersonMatchingService> logger)
    {
        _personRepository = personRepository;
        _logger = logger;
    }

    /// <summary>
    /// Run person matching for all valid/warning staging persons in a package.
    /// </summary>
    /// <param name="stagingPersons">Staging persons to check (already filtered to Valid/Warning).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of match results above the minimum threshold.</returns>
    public async Task<List<PersonMatchResult>> DetectDuplicatesAsync(
        List<StagingPerson> stagingPersons,
        CancellationToken cancellationToken = default)
    {
        var results = new List<PersonMatchResult>();

        if (stagingPersons.Count == 0)
            return results;

        // ============================================================
        // Phase 1: National ID exact matches (production)
        // ============================================================
        var stagingWithNationalId = stagingPersons
            .Where(p => !string.IsNullOrWhiteSpace(p.NationalId))
            .ToList();

        foreach (var staging in stagingWithNationalId)
        {
            var productionMatch = await _personRepository
                .GetByNationalIdAsync(staging.NationalId!, cancellationToken);

            if (productionMatch != null)
            {
                results.Add(BuildResult(staging, productionMatch, isWithinBatch: false));
                _logger.LogDebug(
                    "National ID exact match: staging {StagingId} ↔ production {ProductionId}",
                    staging.Id, productionMatch.Id);
            }
        }

        // ============================================================
        // Phase 2: Composite matching — production persons
        // ============================================================
        // For persons without a National ID match, search production by name similarity.
        // This is an expensive operation — we limit by searching only persons with
        // matching family name prefix (first 3 chars) to reduce the comparison space.
        var alreadyMatchedStagingIds = results
            .Select(r => r.StagingPersonId)
            .ToHashSet();

        var unmatchedStaging = stagingPersons
            .Where(p => !alreadyMatchedStagingIds.Contains(p.Id))
            .ToList();

        foreach (var staging in unmatchedStaging)
        {
            var candidates = await _personRepository.SearchByNameAsync(
                staging.FirstNameArabic,
                staging.FatherNameArabic,
                staging.FamilyNameArabic,
                cancellationToken);

            foreach (var candidate in candidates)
            {
                var matchResult = ComputeCompositeScore(staging, candidate);

                if (matchResult.SimilarityScore >= MinimumThreshold)
                {
                    results.Add(matchResult);
                    _logger.LogDebug(
                        "Composite match ({Score}): staging {StagingId} ↔ production {ProductionId}",
                        matchResult.SimilarityScore, staging.Id, candidate.Id);
                }
            }
        }

        // ============================================================
        // Phase 3: Within-batch duplicates
        // ============================================================
        var withinBatchResults = DetectWithinBatchDuplicates(stagingPersons);
        results.AddRange(withinBatchResults);

        // Deduplicate: if the same pair appears multiple times (e.g. NationalID + composite),
        // keep the one with the highest score.
        results = results
            .GroupBy(r => new { r.StagingPersonId, r.MatchedEntityId })
            .Select(g => g.OrderByDescending(r => r.SimilarityScore).First())
            .ToList();

        _logger.LogInformation(
            "Person matching complete: {Scanned} scanned, {Matches} matches found",
            stagingPersons.Count, results.Count);

        return results;
    }

    /// <summary>
    /// Detect within-batch duplicates (staging person vs staging person in same package).
    /// Only checks National ID exact matches and high composite scores.
    /// </summary>
    private List<PersonMatchResult> DetectWithinBatchDuplicates(List<StagingPerson> stagingPersons)
    {
        var results = new List<PersonMatchResult>();
        var processed = new HashSet<(Guid, Guid)>();

        for (int i = 0; i < stagingPersons.Count; i++)
        {
            for (int j = i + 1; j < stagingPersons.Count; j++)
            {
                var a = stagingPersons[i];
                var b = stagingPersons[j];

                // Skip if already processed (order-independent)
                var pairKey = a.Id.CompareTo(b.Id) < 0
                    ? (a.Id, b.Id)
                    : (b.Id, a.Id);
                if (!processed.Add(pairKey))
                    continue;

                // National ID exact match within batch
                if (!string.IsNullOrWhiteSpace(a.NationalId) &&
                    !string.IsNullOrWhiteSpace(b.NationalId) &&
                    string.Equals(a.NationalId, b.NationalId, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(BuildWithinBatchResult(a, b, NationalIdScore, nationalIdMatched: true));
                    continue;
                }

                // Composite score within batch
                var score = ComputeCompositeScoreBetweenStaging(a, b);
                if (score >= MinimumThreshold)
                {
                    results.Add(BuildWithinBatchResult(a, b, score, nationalIdMatched: false));
                }
            }
        }

        return results;
    }

    // ==================== SCORING ====================

    /// <summary>
    /// Compute composite score between a staging person and a production person.
    /// </summary>
    private static PersonMatchResult ComputeCompositeScore(StagingPerson staging, Person production)
    {
        decimal score = 0;

        // National ID
        bool nationalIdMatched = !string.IsNullOrWhiteSpace(staging.NationalId) &&
                                 !string.IsNullOrWhiteSpace(production.NationalId) &&
                                 string.Equals(staging.NationalId, production.NationalId,
                                     StringComparison.OrdinalIgnoreCase);
        if (nationalIdMatched)
            return BuildResult(staging, production, isWithinBatch: false);

        // Phone
        bool phoneMatched = PhoneNumbersMatch(staging.MobileNumber, production.MobileNumber);
        if (phoneMatched) score += PhoneScore;

        // Arabic name similarity (0–40)
        decimal nameSimilarity = ArabicNameSimilarityHelper.ComputeFullNameSimilarity(
            staging.FirstNameArabic, staging.FatherNameArabic, staging.FamilyNameArabic,
            production.FirstNameArabic, production.FatherNameArabic, production.FamilyNameArabic);
        score += nameSimilarity / 100m * MaxNameScore;

        // Year of birth (staging has int YearOfBirth, production has DateTime DateOfBirth)
        bool yearMatched = staging.YearOfBirth.HasValue &&
                           production.DateOfBirth.HasValue &&
                           staging.YearOfBirth.Value == production.DateOfBirth.Value.Year;
        if (yearMatched) score += YearOfBirthScore;

        // Gender (staging has string, production has enum)
        bool genderMatched = GendersMatch(staging.Gender,
            production.Gender.HasValue ? production.Gender.Value.ToString() : null);
        if (genderMatched) score += GenderScore;

        score = Math.Min(score, MaxCompositeScore);

        return new PersonMatchResult
        {
            StagingPersonId = staging.Id,
            StagingOriginalEntityId = staging.OriginalEntityId,
            MatchedEntityId = production.Id,
            StagingPersonIdentifier = BuildPersonIdentifier(
                staging.FirstNameArabic, staging.FatherNameArabic,
                staging.FamilyNameArabic, staging.NationalId),
            MatchedPersonIdentifier = BuildPersonIdentifier(
                production.FirstNameArabic, production.FatherNameArabic,
                production.FamilyNameArabic, production.NationalId),
            IsWithinBatchMatch = false,
            SimilarityScore = Math.Round(score, 1),
            NationalIdMatched = false,
            PhoneMatched = phoneMatched,
            NameSimilarityScore = nameSimilarity,
            YearOfBirthMatched = yearMatched,
            GenderMatched = genderMatched
        };
    }

    /// <summary>
    /// Compute composite score between two staging persons (within-batch).
    /// </summary>
    private static decimal ComputeCompositeScoreBetweenStaging(StagingPerson a, StagingPerson b)
    {
        decimal score = 0;

        if (PhoneNumbersMatch(a.MobileNumber, b.MobileNumber)) score += PhoneScore;

        decimal nameSimilarity = ArabicNameSimilarityHelper.ComputeFullNameSimilarity(
            a.FirstNameArabic, a.FatherNameArabic, a.FamilyNameArabic,
            b.FirstNameArabic, b.FatherNameArabic, b.FamilyNameArabic);
        score += nameSimilarity / 100m * MaxNameScore;

        // Both are StagingPerson entities, so they both have YearOfBirth (int?) and Gender (string?)
        if (a.YearOfBirth.HasValue && b.YearOfBirth.HasValue &&
            a.YearOfBirth.Value == b.YearOfBirth.Value)
            score += YearOfBirthScore;

        if (GendersMatch(a.Gender, b.Gender))
            score += GenderScore;

        return Math.Min(score, MaxCompositeScore);
    }

    // ==================== HELPER BUILDERS ====================

    private static PersonMatchResult BuildResult(
        StagingPerson staging, Person production, bool isWithinBatch)
    {
        return new PersonMatchResult
        {
            StagingPersonId = staging.Id,
            StagingOriginalEntityId = staging.OriginalEntityId,
            MatchedEntityId = production.Id,
            StagingPersonIdentifier = BuildPersonIdentifier(
                staging.FirstNameArabic, staging.FatherNameArabic,
                staging.FamilyNameArabic, staging.NationalId),
            MatchedPersonIdentifier = BuildPersonIdentifier(
                production.FirstNameArabic, production.FatherNameArabic,
                production.FamilyNameArabic, production.NationalId),
            IsWithinBatchMatch = isWithinBatch,
            SimilarityScore = NationalIdScore,
            NationalIdMatched = true,
            PhoneMatched = PhoneNumbersMatch(staging.MobileNumber, production.MobileNumber),
            NameSimilarityScore = ArabicNameSimilarityHelper.ComputeFullNameSimilarity(
                staging.FirstNameArabic, staging.FatherNameArabic, staging.FamilyNameArabic,
                production.FirstNameArabic, production.FatherNameArabic, production.FamilyNameArabic),
            YearOfBirthMatched = staging.YearOfBirth.HasValue && production.DateOfBirth.HasValue &&
                                 staging.YearOfBirth.Value == production.DateOfBirth.Value.Year,
            GenderMatched = GendersMatch(staging.Gender,
                production.Gender.HasValue ? production.Gender.Value.ToString() : null)
        };
    }

    private static PersonMatchResult BuildWithinBatchResult(
        StagingPerson a, StagingPerson b, decimal score, bool nationalIdMatched)
    {
        return new PersonMatchResult
        {
            StagingPersonId = a.Id,
            StagingOriginalEntityId = a.OriginalEntityId,
            MatchedEntityId = b.Id,
            StagingPersonIdentifier = BuildPersonIdentifier(
                a.FirstNameArabic, a.FatherNameArabic, a.FamilyNameArabic, a.NationalId),
            MatchedPersonIdentifier = BuildPersonIdentifier(
                b.FirstNameArabic, b.FatherNameArabic, b.FamilyNameArabic, b.NationalId),
            IsWithinBatchMatch = true,
            SimilarityScore = score,
            NationalIdMatched = nationalIdMatched,
            PhoneMatched = PhoneNumbersMatch(a.MobileNumber, b.MobileNumber),
            NameSimilarityScore = ArabicNameSimilarityHelper.ComputeFullNameSimilarity(
                a.FirstNameArabic, a.FatherNameArabic, a.FamilyNameArabic,
                b.FirstNameArabic, b.FatherNameArabic, b.FamilyNameArabic),
            YearOfBirthMatched = a.YearOfBirth.HasValue && b.YearOfBirth.HasValue &&
                                 a.YearOfBirth.Value == b.YearOfBirth.Value,
            GenderMatched = GendersMatch(a.Gender, b.Gender)
        };
    }

    // ==================== NORMALIZATION HELPERS ====================

    /// <summary>
    /// Normalize and compare phone numbers.
    /// Strips spaces, dashes, country code prefix (+963 for Syria).
    /// </summary>
    private static bool PhoneNumbersMatch(string? phone1, string? phone2)
    {
        if (string.IsNullOrWhiteSpace(phone1) || string.IsNullOrWhiteSpace(phone2))
            return false;

        return NormalizePhone(phone1) == NormalizePhone(phone2);
    }

    private static string NormalizePhone(string phone)
    {
        // Strip all non-digit characters
        var digits = new string(phone.Where(char.IsDigit).ToArray());

        // Remove Syria country code prefix (963)
        if (digits.StartsWith("963") && digits.Length > 9)
            digits = digits[3..];

        // Remove leading zero (local format)
        if (digits.StartsWith('0') && digits.Length > 9)
            digits = digits[1..];

        return digits;
    }

    /// <summary>
    /// Normalize and compare gender values.
    /// Handles Arabic (ذكر/أنثى), English (M/F/Male/Female), and mixed variations.
    /// </summary>
    private static bool GendersMatch(string? gender1, string? gender2)
    {
        if (string.IsNullOrWhiteSpace(gender1) || string.IsNullOrWhiteSpace(gender2))
            return false;

        return NormalizeGender(gender1) == NormalizeGender(gender2);
    }

    private static string NormalizeGender(string gender)
    {
        var trimmed = gender.Trim().ToUpperInvariant();
        return trimmed switch
        {
            "M" or "MALE" or "ذكر" => "M",
            "F" or "FEMALE" or "أنثى" or "انثى" => "F",
            _ => trimmed
        };
    }

    private static string BuildPersonIdentifier(
        string? firstName, string? fatherName, string? familyName, string? nationalId)
    {
        var name = string.Join(" ",
            new[] { firstName, fatherName, familyName }
                .Where(s => !string.IsNullOrWhiteSpace(s)));

        return string.IsNullOrWhiteSpace(nationalId)
            ? name
            : $"{name} (NID: {nationalId})";
    }
}
