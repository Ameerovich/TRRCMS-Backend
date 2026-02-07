using MediatR;
using TRRCMS.Application.Import.Dtos;

namespace TRRCMS.Application.Import.Commands.StagePackage;

/// <summary>
/// Command to trigger staging (unpack .uhc → staging tables) and validation
/// for an already-uploaded import package.
///
/// UC-003 Stage 2 — S13 (Load to Staging), S14 (Detect Anomalies).
///
/// Prerequisites:
///   - Package must exist (created by UploadPackageCommand)
///   - Package status must be Validating (integrity checks passed)
///     OR ValidationFailed (allow retry after fixing issues)
/// </summary>
public record StagePackageCommand : IRequest<StagingSummaryDto>
{
    /// <summary>
    /// The ImportPackage.Id to stage.
    /// </summary>
    public Guid ImportPackageId { get; init; }
}
