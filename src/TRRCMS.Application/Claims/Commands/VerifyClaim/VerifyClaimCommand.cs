using MediatR;

namespace TRRCMS.Application.Claims.Commands.VerifyClaim;

/// <summary>
/// Command to verify a claim
/// Marks claim as verified with optional notes
/// </summary>
public class VerifyClaimCommand : IRequest<Unit>
{
    /// <summary>
    /// Claim ID to verify
    /// </summary>
    public Guid ClaimId { get; set; }
    
    /// <summary>
    /// Verification notes (optional)
    /// </summary>
    public string? VerificationNotes { get; set; }
    
    /// <summary>
    /// User ID performing verification
    /// </summary>
    public Guid VerifiedByUserId { get; set; }
}
