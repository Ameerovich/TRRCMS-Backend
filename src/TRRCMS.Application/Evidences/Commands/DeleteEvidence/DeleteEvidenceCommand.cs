using MediatR;
using TRRCMS.Application.Common.Models;

namespace TRRCMS.Application.Evidences.Commands.DeleteEvidence;

/// <summary>
/// Command to soft delete an evidence document
/// Validates survey status is Draft before deletion
/// </summary>
public class DeleteEvidenceCommand : IRequest<DeleteResultDto>
{
    public Guid EvidenceId { get; set; }
}
