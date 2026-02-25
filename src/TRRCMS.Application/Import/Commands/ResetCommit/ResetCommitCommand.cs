using MediatR;
using TRRCMS.Application.Import.Dtos;

namespace TRRCMS.Application.Import.Commands.ResetCommit;

/// <summary>
/// Command to reset a stuck or failed package back to ReadyToCommit status.
/// Valid only when package is in Committing or Failed status (i.e. a commit
/// that crashed mid-way or failed with an error).
///
/// Clears error state but preserves all staging data, conflict resolutions,
/// and approvals so the package can be re-committed.
/// </summary>
public class ResetCommitCommand : IRequest<ImportPackageDto>
{
    /// <summary>ImportPackage.Id (set from route parameter).</summary>
    public Guid ImportPackageId { get; set; }

    /// <summary>Mandatory reason for reset (audit trail).</summary>
    public string Reason { get; set; } = string.Empty;
}
