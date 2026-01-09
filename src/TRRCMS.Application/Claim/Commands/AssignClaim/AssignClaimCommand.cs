using MediatR;

namespace TRRCMS.Application.Claims.Commands.AssignClaim;

/// <summary>
/// Command to assign a claim to a case officer
/// </summary>
public class AssignClaimCommand : IRequest<Unit>
{
    /// <summary>
    /// Claim ID to assign
    /// </summary>
    public Guid ClaimId { get; set; }
    
    /// <summary>
    /// User ID to assign the claim to
    /// </summary>
    public Guid AssignToUserId { get; set; }
    
    /// <summary>
    /// Target completion date (optional)
    /// </summary>
    public DateTime? TargetCompletionDate { get; set; }
    
    /// <summary>
    /// User ID performing the assignment
    /// </summary>
    public Guid ModifiedByUserId { get; set; }
}
