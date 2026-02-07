namespace TRRCMS.Application.Common.Interfaces;

/// <summary>
/// Service interface for detecting duplicate persons and properties
/// during the import pipeline.
///
/// Compares staged records (from .uhc package) against:
///   1. Production data (existing Persons, Buildings, PropertyUnits)
///   2. Within-batch duplicates (staging records referencing the same real-world entity)
///
/// Creates <see cref="Domain.Entities.ConflictResolution"/> entities for each detected pair.
/// Transitions the <see cref="Domain.Entities.ImportPackage"/> to ReviewingConflicts or ReadyToCommit.
///
/// FSD: FR-D-5 (Person Matching), FR-D-6 (Property Matching), FR-D-7 (Conflict Resolution).
/// UC-003 Stage 2 — S14 (Detect Anomalies and Potential Duplicates).
/// UC-007 (Resolve Duplicate Properties), UC-008 (Resolve Person Duplicates).
/// </summary>
public interface IDuplicateDetectionService
{
    /// <summary>
    /// Run person and property duplicate detection for all valid/warning staging
    /// records in the specified import package.
    /// </summary>
    /// <param name="importPackageId">The ImportPackage.Id whose staging data to scan.</param>
    /// <param name="detectedByUserId">The user initiating the detection (for audit trail).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Summary of duplicates found and conflicts created.</returns>
    Task<DuplicateDetectionResult> DetectAsync(
        Guid importPackageId,
        Guid detectedByUserId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result returned by <see cref="IDuplicateDetectionService.DetectAsync"/>.
/// Contains counts and the IDs of all <see cref="Domain.Entities.ConflictResolution"/>
/// records created during detection.
/// </summary>
public class DuplicateDetectionResult
{
    /// <summary>Number of person-duplicate conflict pairs detected.</summary>
    public int PersonDuplicatesFound { get; set; }

    /// <summary>Number of property-duplicate conflict pairs detected (building or unit level).</summary>
    public int PropertyDuplicatesFound { get; set; }

    /// <summary>Total ConflictResolution records created.</summary>
    public int TotalConflictsCreated { get; set; }

    /// <summary>IDs of the created ConflictResolution records.</summary>
    public List<Guid> ConflictIds { get; set; } = new();

    /// <summary>Total staging persons scanned.</summary>
    public int PersonsScanned { get; set; }

    /// <summary>Total staging buildings scanned.</summary>
    public int BuildingsScanned { get; set; }

    /// <summary>Total duration of the detection run.</summary>
    public TimeSpan Duration { get; set; }
}
