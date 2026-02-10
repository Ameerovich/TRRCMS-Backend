using MediatR;
using TRRCMS.Application.Import.Dtos;

namespace TRRCMS.Application.Import.Commands.ApproveForCommit;

/// <summary>
/// Command to approve an import package (or specific records) for commit to production.
///
/// Pre-conditions:
///   - Package must exist and be in ReviewingConflicts or ReadyToCommit status
///   - All conflicts for the package must be resolved
///   - Only Valid or Warning staging records can be approved
///
/// When <see cref="ApproveAllValid"/> is true, all Valid/Warning records are approved.
/// When false, only specific records in <see cref="StagingRecordIds"/> are approved.
///
/// UC-003 Stage 4 — S16 (Approve for Commit).
/// </summary>
public class ApproveForCommitCommand : IRequest<ImportPackageDto>
{
    /// <summary>ImportPackage.Id (surrogate key).</summary>
    public Guid ImportPackageId { get; set; }

    /// <summary>
    /// If true, approve all Valid and Warning staging records for commit.
    /// If false, only approve records specified in <see cref="StagingRecordIds"/>.
    /// </summary>
    public bool ApproveAllValid { get; set; } = true;

    /// <summary>
    /// Optional list of specific staging record IDs to approve.
    /// Only used when <see cref="ApproveAllValid"/> is false.
    /// Allows selective approval for partial commits.
    /// </summary>
    public List<Guid>? StagingRecordIds { get; set; }
}
