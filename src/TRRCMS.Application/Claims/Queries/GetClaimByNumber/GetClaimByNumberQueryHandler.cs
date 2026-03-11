using AutoMapper;
using MediatR;
using TRRCMS.Application.Claims.Dtos;
using TRRCMS.Application.Common.Interfaces;

namespace TRRCMS.Application.Claims.Queries.GetClaimByNumber;

public class GetClaimByNumberQueryHandler : IRequestHandler<GetClaimByNumberQuery, ClaimDto?>
{
    private readonly IClaimRepository _claimRepository;
    private readonly IMapper _mapper;

    public GetClaimByNumberQueryHandler(IClaimRepository claimRepository, IMapper mapper)
    {
        _claimRepository = claimRepository;
        _mapper = mapper;
    }

    public async Task<ClaimDto?> Handle(GetClaimByNumberQuery request, CancellationToken cancellationToken)
    {
        var claim = await _claimRepository.GetByClaimNumberAsync(request.ClaimNumber, cancellationToken);
        return claim != null ? _mapper.Map<ClaimDto>(claim) : null;
    }
}
