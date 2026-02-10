using MediatR;
using TRRCMS.Application.Import.Dtos;

namespace TRRCMS.Application.Import.Commands.CancelPackage;

/// <summary>
/// Command to cancel an active import package.
/// Pre-conditions: package exists and is NOT in a terminal state.
/// </summary>
public class CancelPackageCommand : IRequest<ImportPackageDto>
{
    /// <summary>ImportPackage.Id (set from route parameter).</summary>
    public Guid ImportPackageId { get; set; }

    /// <summary>Mandatory reason for cancellation (audit trail).</summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>Whether to purge staging data immediately (default: true).</summary>
    public bool CleanupStaging { get; set; } = true;
}
