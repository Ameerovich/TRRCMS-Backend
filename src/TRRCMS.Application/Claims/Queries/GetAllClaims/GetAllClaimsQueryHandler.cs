using AutoMapper;
using MediatR;
using TRRCMS.Application.Claims.Dtos;
using TRRCMS.Application.Common.Interfaces;

namespace TRRCMS.Application.Claims.Queries.GetAllClaims;

/// <summary>
/// Handler for GetAllClaimsQuery
/// Retrieves claims with optional filtering
/// </summary>
public class GetAllClaimsQueryHandler : IRequestHandler<GetAllClaimsQuery, IEnumerable<ClaimDto>>
{
    private readonly IClaimRepository _claimRepository;
    private readonly IMapper _mapper;
    
    public GetAllClaimsQueryHandler(
        IClaimRepository claimRepository,
        IMapper mapper)
    {
        _claimRepository = claimRepository ?? throw new ArgumentNullException(nameof(claimRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }
    
    public async Task<IEnumerable<ClaimDto>> Handle(GetAllClaimsQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<Domain.Entities.Claim> claims;
        
        // Apply filtering based on provided parameters
        // Priority: specific filters > general listing
        
        if (request.IsOverdue == true)
        {
            // Get overdue claims
            claims = await _claimRepository.GetOverdueClaimsAsync(cancellationToken);
        }
        else if (request.HasConflicts == true)
        {
            // Get conflicting claims
            claims = await _claimRepository.GetConflictingClaimsAsync(cancellationToken);
        }
        else if (request.AwaitingDocuments == true)
        {
            // Get claims awaiting documents
            claims = await _claimRepository.GetClaimsAwaitingDocumentsAsync(cancellationToken);
        }
        else if (request.PropertyUnitId.HasValue)
        {
            // Get claim by property unit (single claim expected)
            var claim = await _claimRepository.GetByPropertyUnitIdAsync(
                request.PropertyUnitId.Value, 
                cancellationToken
            );
            claims = claim != null ? new[] { claim } : Array.Empty<Domain.Entities.Claim>();
        }
        else if (request.PrimaryClaimantId.HasValue)
        {
            // Get claims by primary claimant
            claims = await _claimRepository.GetByPrimaryClaimantIdAsync(
                request.PrimaryClaimantId.Value, 
                cancellationToken
            );
        }
        else if (request.AssignedToUserId.HasValue)
        {
            // Get claims by assigned user
            claims = await _claimRepository.GetByAssignedUserIdAsync(
                request.AssignedToUserId.Value, 
                cancellationToken
            );
        }
        else if (request.LifecycleStage.HasValue)
        {
            // Get claims by lifecycle stage
            claims = await _claimRepository.GetByLifecycleStageAsync(
                request.LifecycleStage.Value, 
                cancellationToken
            );
        }
        else if (request.Status.HasValue)
        {
            // Get claims by status
            claims = await _claimRepository.GetByStatusAsync(
                request.Status.Value, 
                cancellationToken
            );
        }
        else if (request.Priority.HasValue)
        {
            // Get claims by priority
            claims = await _claimRepository.GetByPriorityAsync(
                request.Priority.Value, 
                cancellationToken
            );
        }
        else if (request.VerificationStatus.HasValue)
        {
            // Get claims by verification status
            claims = await _claimRepository.GetByVerificationStatusAsync(
                request.VerificationStatus.Value, 
                cancellationToken
            );
        }
        else
        {
            // No filters - get all claims
            claims = await _claimRepository.GetAllAsync(cancellationToken);
        }
        
        // Map to DTOs
        return _mapper.Map<IEnumerable<ClaimDto>>(claims);
    }
}
