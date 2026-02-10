using MediatR;
using TRRCMS.Application.Import.Dtos;

namespace TRRCMS.Application.Import.Commands.CommitPackage;

/// <summary>
/// Command to commit approved staging records to production tables.
///
/// Pre-conditions:
///   - Package must exist and be in ReadyToCommit status
///   - All conflicts must be resolved
///   - At least one staging record must be approved for commit
///
/// The commit runs within a single database transaction:
///   - On success: ImportPackage → Completed
///   - On partial failure: ImportPackage → PartiallyCompleted
///   - On total failure: rollback + ImportPackage → Failed
///
/// After commit, the original .uhc package is archived to immutable store.
///
/// UC-003 Stage 4 — S17 (Commit to Production).
/// FSD: FR-D-8 (Record IDs), FR-D-9 (Attachment Dedup).
/// </summary>
public class CommitPackageCommand : IRequest<CommitReportDto>
{
    /// <summary>ImportPackage.Id (surrogate key).</summary>
    public Guid ImportPackageId { get; set; }

    /// <summary>
    /// Whether to clean up staging data after successful commit.
    /// Default: false (staging data retained per ImportPipelineSettings.StagingRetentionDays).
    /// </summary>
    public bool CleanupStagingAfterCommit { get; set; } = false;
}
