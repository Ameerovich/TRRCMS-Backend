using TRRCMS.Application.Import.Dtos;

namespace TRRCMS.Application.Common.Interfaces;

/// <summary>
/// Service interface for committing approved staging records to production tables.
///
/// Orchestrates the atomic commit pipeline:
///   1. Verify all pre-conditions (status, conflicts resolved, records approved)
///   2. Map staging entities → production entities (resolve Original*Id → production FK)
///   3. Deduplicate attachment files by SHA-256 hash (FR-D-9)
///   4. Generate Record IDs for new production entities (FR-D-8)
///   5. Insert production entities within a single DB transaction
///   6. Update staging records with CommittedEntityId for traceability
///   7. Archive the original .uhc package to immutable store
///   8. Generate commit report
///
/// The entire commit is wrapped in a transaction:
///   - Success → ImportPackage.Status = Completed
///   - Partial failure → PartiallyCompleted (some entities committed)
///   - Total failure → Failed + rollback
///
/// FSD: FR-D-8 (Record ID Generation), FR-D-9 (Attachment Deduplication).
/// UC-003 Stage 4 — S16 (Approve), S17 (Commit), S11 (Archive).
/// Delivery Plan Task: TRRCMS-IMP-05.
/// </summary>
public interface ICommitService
{
    /// <summary>
    /// Commit all approved staging records for a package to production.
    /// Runs within a single database transaction for atomicity.
    /// </summary>
    /// <param name="importPackageId">The ImportPackage.Id whose approved staging data to commit.</param>
    /// <param name="committedByUserId">User performing the commit (Data Manager).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Commit report with per-entity-type counts and any errors.</returns>
    Task<CommitReportDto> CommitAsync(
        Guid importPackageId,
        Guid committedByUserId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Archive the original .uhc package file to the immutable archive store.
    /// Called after successful commit. Path: archives/YYYY/MM/[packageId].uhc.
    /// </summary>
    /// <param name="importPackageId">The ImportPackage.Id to archive.</param>
    /// <param name="committedByUserId">User performing the archive.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The archive file path.</returns>
    Task<string> ArchivePackageAsync(
        Guid importPackageId,
        Guid committedByUserId,
        CancellationToken cancellationToken = default);
}
