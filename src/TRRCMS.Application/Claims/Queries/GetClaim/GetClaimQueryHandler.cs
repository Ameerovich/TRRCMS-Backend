using AutoMapper;
using MediatR;
using TRRCMS.Application.Claims.Dtos;
using TRRCMS.Application.Common.Interfaces;

namespace TRRCMS.Application.Claims.Queries.GetClaim;

/// <summary>
/// Handler for GetClaimQuery
/// Retrieves a single claim by ID with all navigation properties
/// </summary>
public class GetClaimQueryHandler : IRequestHandler<GetClaimQuery, ClaimDto?>
{
    private readonly IClaimRepository _claimRepository;
    private readonly IMapper _mapper;
    
    public GetClaimQueryHandler(
        IClaimRepository claimRepository,
        IMapper mapper)
    {
        _claimRepository = claimRepository ?? throw new ArgumentNullException(nameof(claimRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }
    
    public async Task<ClaimDto?> Handle(GetClaimQuery request, CancellationToken cancellationToken)
    {
        var claim = await _claimRepository.GetByIdAsync(request.Id, cancellationToken);
        
        if (claim == null)
        {
            return null;
        }
        
        return _mapper.Map<ClaimDto>(claim);
    }
}
