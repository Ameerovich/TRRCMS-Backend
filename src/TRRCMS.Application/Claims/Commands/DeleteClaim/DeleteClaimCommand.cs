using MediatR;
using TRRCMS.Application.Common.Models;

namespace TRRCMS.Application.Claims.Commands.DeleteClaim;

/// <summary>
/// Soft delete a claim.
/// Does NOT delete the source PersonPropertyRelation or its evidence —
/// those remain intact and can generate a new claim if needed.
/// </summary>
public class DeleteClaimCommand : IRequest<DeleteResultDto>
{
    public Guid ClaimId { get; set; }

    /// <summary>
    /// Reason for deletion (audit requirement).
    /// </summary>
    public string? DeletionReason { get; set; }
}
