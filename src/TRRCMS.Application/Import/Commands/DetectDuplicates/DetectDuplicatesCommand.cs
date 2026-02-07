using MediatR;
using TRRCMS.Application.Import.Dtos;

namespace TRRCMS.Application.Import.Commands.DetectDuplicates;

/// <summary>
/// Command to trigger duplicate detection for a staged import package.
/// Runs person matching (FR-D-5) and property matching (FR-D-6) against
/// production data and within-batch records.
///
/// Prerequisites:
///   - Package must exist
///   - Package status must be Staging (validation passed, data is staged)
///     OR ReviewingConflicts (allow re-run after resolving some conflicts)
///
/// UC-003 Stage 2 — S14 (Detect Anomalies and Potential Duplicates).
/// </summary>
public record DetectDuplicatesCommand : IRequest<DuplicateDetectionResultDto>
{
    /// <summary>The ImportPackage.Id to run detection against.</summary>
    public Guid ImportPackageId { get; init; }
}
