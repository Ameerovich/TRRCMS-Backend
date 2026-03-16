using MediatR;
using TRRCMS.Application.Import.Dtos;

namespace TRRCMS.Application.Import.Commands.QuarantinePackage;

/// <summary>
/// Command to quarantine a suspicious import package.
/// </summary>
public class QuarantinePackageCommand : IRequest<ImportPackageDto>
{
    /// <summary>ImportPackage.Id (set from route parameter).</summary>
    public Guid ImportPackageId { get; set; }

    /// <summary>Mandatory reason for quarantine (audit trail).</summary>
    public string Reason { get; set; } = string.Empty;
}
