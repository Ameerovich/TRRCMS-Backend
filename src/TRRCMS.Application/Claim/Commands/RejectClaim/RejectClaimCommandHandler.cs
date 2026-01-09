using MediatR;
using TRRCMS.Application.Common.Interfaces;

namespace TRRCMS.Application.Claims.Commands.RejectClaim;

/// <summary>
/// Handler for RejectClaimCommand
/// Rejects claim with required reason
/// </summary>
public class RejectClaimCommandHandler : IRequestHandler<RejectClaimCommand, Unit>
{
    private readonly IClaimRepository _claimRepository;
    
    public RejectClaimCommandHandler(IClaimRepository claimRepository)
    {
        _claimRepository = claimRepository ?? throw new ArgumentNullException(nameof(claimRepository));
    }
    
    public async Task<Unit> Handle(RejectClaimCommand request, CancellationToken cancellationToken)
    {
        // Get claim
        var claim = await _claimRepository.GetByIdAsync(request.ClaimId, cancellationToken);
        
        if (claim == null)
        {
            throw new InvalidOperationException($"Claim with ID {request.ClaimId} not found.");
        }
        
        // Reject claim using domain method
        claim.Reject(
            decisionReason: request.DecisionReason,
            decisionNotes: request.DecisionNotes,
            decisionByUserId: request.DecisionByUserId,
            modifiedByUserId: request.DecisionByUserId
        );
        
        // Save changes
        await _claimRepository.UpdateAsync(claim, cancellationToken);
        await _claimRepository.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}
