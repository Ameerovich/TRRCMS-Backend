using MediatR;

namespace TRRCMS.Application.Claims.Commands.RejectClaim;

/// <summary>
/// Command to reject a claim
/// Rejects claim with required reason
/// </summary>
public class RejectClaimCommand : IRequest<Unit>
{
    /// <summary>
    /// Claim ID to reject
    /// </summary>
    public Guid ClaimId { get; set; }
    
    /// <summary>
    /// Reason for rejection (required)
    /// </summary>
    public string DecisionReason { get; set; } = string.Empty;
    
    /// <summary>
    /// Additional decision notes (optional)
    /// </summary>
    public string? DecisionNotes { get; set; }
    
    /// <summary>
    /// User ID making the decision
    /// </summary>
    public Guid DecisionByUserId { get; set; }
}
