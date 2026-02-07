namespace TRRCMS.Application.Common.Interfaces;

/// <summary>
/// Service interface for unpacking .uhc SQLite containers into staging tables
/// and managing staging data lifecycle.
///
/// UC-003 Stage 2 — S13 (Load to Staging).
/// FSD: FR-D-4 (Staging & Validation).
///
/// The .uhc file is a renamed SQLite database containing data tables:
///   surveys, buildings, property_units, persons, households,
///   person_property_relations, evidences, claims
///
/// Each table maps to a corresponding Staging* entity via factory methods.
/// Attachment blobs are extracted and saved via IFileStorageService.
/// </summary>
public interface IStagingService
{
    /// <summary>
    /// Open the .uhc SQLite file, read all data tables, map to staging entities,
    /// bulk-insert into staging tables, and extract attachment files.
    /// 
    /// Flow:
    ///   1. Open .uhc as read-only SQLite
    ///   2. Read each data table (surveys, buildings, etc.)
    ///   3. Map rows → staging entities via factory Create methods
    ///   4. Bulk insert (AddRangeAsync) per entity type
    ///   5. Extract attachment blobs → IFileStorageService
    ///   6. Return staging result with record counts
    /// </summary>
    /// <param name="importPackageId">The ImportPackage.Id (already created by UploadPackageCommand).</param>
    /// <param name="uhcFilePath">File system path to the .uhc SQLite file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result with per-entity-type record counts.</returns>
    Task<StagingResult> UnpackAndStageAsync(
        Guid importPackageId,
        string uhcFilePath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete all staging data for a given import package.
    /// Used for cleanup after commit, or when cancelling/retrying an import.
    /// Cascading delete via ImportPackageId FK handles this at the DB level,
    /// but this method provides explicit cleanup + file deletion.
    /// </summary>
    /// <param name="importPackageId">The import package whose staging data to purge.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task CleanupStagingAsync(Guid importPackageId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of the unpack-and-stage operation, with record counts per entity type.
/// </summary>
public class StagingResult
{
    public Guid ImportPackageId { get; set; }
    public int SurveyCount { get; set; }
    public int BuildingCount { get; set; }
    public int PropertyUnitCount { get; set; }
    public int PersonCount { get; set; }
    public int HouseholdCount { get; set; }
    public int PersonPropertyRelationCount { get; set; }
    public int EvidenceCount { get; set; }
    public int ClaimCount { get; set; }
    public int AttachmentFilesExtracted { get; set; }
    public long AttachmentBytesExtracted { get; set; }

    /// <summary>Total records across all entity types.</summary>
    public int TotalRecordCount =>
        SurveyCount + BuildingCount + PropertyUnitCount + PersonCount +
        HouseholdCount + PersonPropertyRelationCount + EvidenceCount + ClaimCount;
}
