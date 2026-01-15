using MediatR;
using TRRCMS.Application.Common.Interfaces;

namespace TRRCMS.Application.Claims.Commands.VerifyClaim;

/// <summary>
/// Handler for VerifyClaimCommand
/// Marks claim as verified and records verification details
/// </summary>
public class VerifyClaimCommandHandler : IRequestHandler<VerifyClaimCommand, Unit>
{
    private readonly IClaimRepository _claimRepository;
    
    public VerifyClaimCommandHandler(IClaimRepository claimRepository)
    {
        _claimRepository = claimRepository ?? throw new ArgumentNullException(nameof(claimRepository));
    }
    
    public async Task<Unit> Handle(VerifyClaimCommand request, CancellationToken cancellationToken)
    {
        // Get claim
        var claim = await _claimRepository.GetByIdAsync(request.ClaimId, cancellationToken);
        
        if (claim == null)
        {
            throw new InvalidOperationException($"Claim with ID {request.ClaimId} not found.");
        }
        
        // Verify claim using domain method
        claim.Verify(
            verifiedByUserId: request.VerifiedByUserId,
            verificationNotes: request.VerificationNotes,
            modifiedByUserId: request.VerifiedByUserId
        );
        
        // Save changes
        await _claimRepository.UpdateAsync(claim, cancellationToken);
        await _claimRepository.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}
