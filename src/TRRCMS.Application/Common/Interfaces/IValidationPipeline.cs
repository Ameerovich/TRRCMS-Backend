using TRRCMS.Domain.Common;

namespace TRRCMS.Application.Common.Interfaces;

/// <summary>
/// Orchestrates the 8-level validation pipeline for staged import data (FR-D-4).
/// Each level runs independently and writes results to the staging entities
/// via <see cref="BaseStagingEntity.MarkAsValid"/> / <see cref="BaseStagingEntity.MarkAsInvalid"/>.
///
/// Levels:
///   1. DataConsistencyValidator       — required fields, types, lengths, enum values
///   2. CrossEntityRelationValidator   — intra-batch FK integrity (Person→Unit, Claim→Unit, etc.)
///   3. OwnershipEvidenceValidator     — ownership relations have supporting evidence
///   4. HouseholdStructureValidator    — male+female=total, head of household exists
///   5. SpatialGeometryValidator       — coordinates within Syria bounds, valid geometry
///   6. ClaimLifecycleValidator        — valid status transitions, correct lifecycle stage
///   7. VocabularyVersionValidator     — all enum/code values exist in active vocabulary
///   8. BuildingUnitCodeValidator      — 17-digit building_id pattern, unique unit codes
///
/// UC-003 Stage 2 — S14 (Detect Anomalies).
/// </summary>
public interface IValidationPipeline
{
    /// <summary>
    /// Run all 8 validation levels against all staging records for a given import package.
    /// Each validator writes errors/warnings directly to the staging entities.
    /// Returns an aggregate summary of validation results.
    /// </summary>
    /// <param name="importPackageId">The import package whose staging data to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Aggregate validation summary.</returns>
    Task<ValidationSummary> ValidateAsync(Guid importPackageId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Individual validator interface. Each of the 8 validation levels implements this.
/// </summary>
public interface IStagingValidator
{
    /// <summary>Validator display name (for logging and error attribution).</summary>
    string Name { get; }

    /// <summary>Validation level (1–8). Validators run in order.</summary>
    int Level { get; }

    /// <summary>
    /// Run this validator against all staged records for the given package.
    /// Returns the count of errors and warnings added.
    /// </summary>
    Task<ValidatorResult> ValidateAsync(Guid importPackageId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result from a single validator level.
/// </summary>
public class ValidatorResult
{
    public string ValidatorName { get; set; } = string.Empty;
    public int Level { get; set; }
    public int ErrorCount { get; set; }
    public int WarningCount { get; set; }
    public int RecordsChecked { get; set; }
    public TimeSpan Duration { get; set; }
}

/// <summary>
/// Aggregate validation summary across all 8 levels.
/// </summary>
public class ValidationSummary
{
    public Guid ImportPackageId { get; set; }
    public int TotalRecords { get; set; }
    public int ValidCount { get; set; }
    public int InvalidCount { get; set; }
    public int WarningCount { get; set; }
    public int SkippedCount { get; set; }
    public int PendingCount { get; set; }
    public List<ValidatorResult> LevelResults { get; set; } = new();
    public TimeSpan TotalDuration { get; set; }

    /// <summary>True if no blocking errors were found.</summary>
    public bool IsClean => InvalidCount == 0;
}
