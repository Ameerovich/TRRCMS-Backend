using AutoMapper;
using MediatR;
using TRRCMS.Application.AdministrativeDivisions.Dtos;
using TRRCMS.Application.Common;
using TRRCMS.Application.Common.Interfaces;

namespace TRRCMS.Application.AdministrativeDivisions.Queries.GetCommunities;

/// <summary>
/// Handler for GetCommunitiesQuery
/// </summary>
public class GetCommunitiesQueryHandler : IRequestHandler<GetCommunitiesQuery, List<CommunityDto>>
{
    private readonly ICommunityRepository _communityRepository;
    private readonly IMapper _mapper;

    public GetCommunitiesQueryHandler(
        ICommunityRepository communityRepository,
        IMapper mapper)
    {
        _communityRepository = communityRepository;
        _mapper = mapper;
    }

    public async Task<List<CommunityDto>> Handle(
        GetCommunitiesQuery request,
        CancellationToken cancellationToken)
    {
        var (govCode, distCode, subDistCode) = OchaCommandNormalizer.ResolveAdmCodes(
            request.GovernorateCode, request.DistrictCode, request.SubDistrictCode,
            request.GovernoratePCode, request.DistrictPCode, request.SubDistrictPCode);

        var communities = await _communityRepository.GetAllAsync(
            string.IsNullOrWhiteSpace(govCode) ? null : govCode,
            string.IsNullOrWhiteSpace(distCode) ? null : distCode,
            string.IsNullOrWhiteSpace(subDistCode) ? null : subDistCode,
            cancellationToken);

        return _mapper.Map<List<CommunityDto>>(communities);
    }
}
