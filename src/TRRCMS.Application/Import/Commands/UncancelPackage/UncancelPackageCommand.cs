using MediatR;
using TRRCMS.Application.Import.Dtos;

namespace TRRCMS.Application.Import.Commands.UncancelPackage;

/// <summary>
/// Restores a Cancelled import package to its pre-cancellation status so the
/// import pipeline can resume from where it left off.
/// If PreviousStatus was not recorded, the package is restored to Pending.
/// </summary>
public class UncancelPackageCommand : IRequest<ImportPackageDto>
{
    /// <summary>ImportPackage.Id (set from route parameter).</summary>
    public Guid ImportPackageId { get; set; }

    /// <summary>Optional reason for uncancellation (audit trail).</summary>
    public string? Reason { get; set; }
}
