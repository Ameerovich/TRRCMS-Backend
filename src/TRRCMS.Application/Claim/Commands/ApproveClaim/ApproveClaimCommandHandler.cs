using MediatR;
using TRRCMS.Application.Common.Interfaces;

namespace TRRCMS.Application.Claims.Commands.ApproveClaim;

/// <summary>
/// Handler for ApproveClaimCommand
/// Approves claim and sets certificate status to pending generation
/// </summary>
public class ApproveClaimCommandHandler : IRequestHandler<ApproveClaimCommand, Unit>
{
    private readonly IClaimRepository _claimRepository;
    
    public ApproveClaimCommandHandler(IClaimRepository claimRepository)
    {
        _claimRepository = claimRepository ?? throw new ArgumentNullException(nameof(claimRepository));
    }
    
    public async Task<Unit> Handle(ApproveClaimCommand request, CancellationToken cancellationToken)
    {
        // Get claim
        var claim = await _claimRepository.GetByIdAsync(request.ClaimId, cancellationToken);
        
        if (claim == null)
        {
            throw new InvalidOperationException($"Claim with ID {request.ClaimId} not found.");
        }
        
        // Approve claim using domain method
        claim.Approve(
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
