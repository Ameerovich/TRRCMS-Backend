using MediatR;

namespace TRRCMS.Application.Claims.Commands.SubmitClaim;

/// <summary>
/// Command to submit a claim for processing
/// Moves claim from Draft to Submitted stage
/// </summary>
public class SubmitClaimCommand : IRequest<Unit>
{
    /// <summary>
    /// Claim ID to submit
    /// </summary>
    public Guid ClaimId { get; set; }
    
    /// <summary>
    /// User ID submitting the claim
    /// </summary>
    public Guid SubmittedByUserId { get; set; }
}
