using MediatR;
using TRRCMS.Application.Common.Interfaces;

namespace TRRCMS.Application.Claims.Commands.SubmitClaim;

/// <summary>
/// Handler for SubmitClaimCommand
/// Submits claim for processing and moves to Submitted lifecycle stage
/// </summary>
public class SubmitClaimCommandHandler : IRequestHandler<SubmitClaimCommand, Unit>
{
    private readonly IClaimRepository _claimRepository;
    
    public SubmitClaimCommandHandler(IClaimRepository claimRepository)
    {
        _claimRepository = claimRepository ?? throw new ArgumentNullException(nameof(claimRepository));
    }
    
    public async Task<Unit> Handle(SubmitClaimCommand request, CancellationToken cancellationToken)
    {
        // Get claim
        var claim = await _claimRepository.GetByIdAsync(request.ClaimId, cancellationToken);
        
        if (claim == null)
        {
            throw new InvalidOperationException($"Claim with ID {request.ClaimId} not found.");
        }
        
        // Submit claim using domain method
        claim.Submit(
            submittedByUserId: request.SubmittedByUserId,
            modifiedByUserId: request.SubmittedByUserId
        );
        
        // Save changes
        await _claimRepository.UpdateAsync(claim, cancellationToken);
        await _claimRepository.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}
