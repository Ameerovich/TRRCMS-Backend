using AutoMapper;
using MediatR;
using TRRCMS.Application.Claims.Dtos;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Models;

namespace TRRCMS.Application.Claims.Queries.GetAllClaims;

/// <summary>
/// Handler for GetAllClaimsQuery
/// Retrieves claims with optional filtering and pagination
/// </summary>
public class GetAllClaimsQueryHandler : IRequestHandler<GetAllClaimsQuery, PagedResult<ClaimDto>>
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

    public async Task<PagedResult<ClaimDto>> Handle(GetAllClaimsQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<Domain.Entities.Claim> claims;

        // Apply filtering based on provided parameters
        if (request.IsOverdue == true)
        {
            claims = await _claimRepository.GetOverdueClaimsAsync(cancellationToken);
        }
        else if (request.HasConflicts == true)
        {
            claims = await _claimRepository.GetConflictingClaimsAsync(cancellationToken);
        }
        else if (request.AwaitingDocuments == true)
        {
            claims = await _claimRepository.GetClaimsAwaitingDocumentsAsync(cancellationToken);
        }
        else if (request.PropertyUnitId.HasValue)
        {
            var claim = await _claimRepository.GetByPropertyUnitIdAsync(
                request.PropertyUnitId.Value,
                cancellationToken
            );
            claims = claim != null ? new[] { claim } : Array.Empty<Domain.Entities.Claim>();
        }
        else if (request.PrimaryClaimantId.HasValue)
        {
            claims = await _claimRepository.GetByPrimaryClaimantIdAsync(
                request.PrimaryClaimantId.Value,
                cancellationToken
            );
        }
        else if (request.AssignedToUserId.HasValue)
        {
            claims = await _claimRepository.GetByAssignedUserIdAsync(
                request.AssignedToUserId.Value,
                cancellationToken
            );
        }
        else if (request.LifecycleStage.HasValue)
        {
            claims = await _claimRepository.GetByLifecycleStageAsync(
                request.LifecycleStage.Value,
                cancellationToken
            );
        }
        else if (request.Status.HasValue)
        {
            claims = await _claimRepository.GetByStatusAsync(
                request.Status.Value,
                cancellationToken
            );
        }
        else if (request.Priority.HasValue)
        {
            claims = await _claimRepository.GetByPriorityAsync(
                request.Priority.Value,
                cancellationToken
            );
        }
        else if (request.VerificationStatus.HasValue)
        {
            claims = await _claimRepository.GetByVerificationStatusAsync(
                request.VerificationStatus.Value,
                cancellationToken
            );
        }
        else
        {
            claims = await _claimRepository.GetAllAsync(cancellationToken);
        }

        // Map to DTOs and paginate
        var dtos = _mapper.Map<List<ClaimDto>>(claims);
        return PaginatedList.FromEnumerable(dtos, request.PageNumber, request.PageSize);
    }
}
