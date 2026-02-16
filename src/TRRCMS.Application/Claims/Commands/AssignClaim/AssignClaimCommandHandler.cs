using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;

namespace TRRCMS.Application.Claims.Commands.AssignClaim;

/// <summary>
/// Handler for AssignClaimCommand
/// Assigns claim to a case officer with optional target completion date
/// </summary>
public class AssignClaimCommandHandler : IRequestHandler<AssignClaimCommand, Unit>
{
    private readonly IClaimRepository _claimRepository;
    
    public AssignClaimCommandHandler(IClaimRepository claimRepository)
    {
        _claimRepository = claimRepository ?? throw new ArgumentNullException(nameof(claimRepository));
    }
    
    public async Task<Unit> Handle(AssignClaimCommand request, CancellationToken cancellationToken)
    {
        // Get claim
        var claim = await _claimRepository.GetByIdAsync(request.ClaimId, cancellationToken);
        
        if (claim == null)
        {
            throw new NotFoundException($"Claim with ID {request.ClaimId} not found.");
        }
        
        // Assign claim using domain method
        claim.AssignTo(
            userId: request.AssignToUserId,
            targetCompletionDate: request.TargetCompletionDate,
            modifiedByUserId: request.ModifiedByUserId
        );
        
        // Save changes
        await _claimRepository.UpdateAsync(claim, cancellationToken);
        await _claimRepository.SaveChangesAsync(cancellationToken);
        
        return Unit.Value;
    }
}
