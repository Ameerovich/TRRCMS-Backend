using MediatR;

namespace TRRCMS.Application.Claims.Commands.ApproveClaim;

/// <summary>
/// Command to approve a claim
/// Approves claim and triggers certificate generation
/// </summary>
public class ApproveClaimCommand : IRequest<Unit>
{
    /// <summary>
    /// Claim ID to approve
    /// </summary>
    public Guid ClaimId { get; set; }
    
    /// <summary>
    /// Reason for approval (optional)
    /// </summary>
    public string? DecisionReason { get; set; }
    
    /// <summary>
    /// Additional decision notes (optional)
    /// </summary>
    public string? DecisionNotes { get; set; }
    
    /// <summary>
    /// User ID making the decision
    /// </summary>
    public Guid DecisionByUserId { get; set; }
}
