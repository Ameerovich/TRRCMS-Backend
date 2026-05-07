using AutoMapper;
using MediatR;
using TRRCMS.Application.AdministrativeDivisions.Dtos;
using TRRCMS.Application.Common;
using TRRCMS.Application.Common.Interfaces;

namespace TRRCMS.Application.AdministrativeDivisions.Queries.GetDistricts;

/// <summary>
/// Handler for GetDistrictsQuery
/// </summary>
public class GetDistrictsQueryHandler : IRequestHandler<GetDistrictsQuery, List<DistrictDto>>
{
    private readonly IDistrictRepository _districtRepository;
    private readonly IMapper _mapper;

    public GetDistrictsQueryHandler(
        IDistrictRepository districtRepository,
        IMapper mapper)
    {
        _districtRepository = districtRepository;
        _mapper = mapper;
    }

    public async Task<List<DistrictDto>> Handle(
        GetDistrictsQuery request,
        CancellationToken cancellationToken)
    {
        var (govCode, _, _) = OchaCommandNormalizer.ResolveAdmCodes(
            request.GovernorateCode, null, null,
            request.GovernoratePCode, null, null);

        var districts = await _districtRepository.GetAllAsync(
            string.IsNullOrWhiteSpace(govCode) ? null : govCode,
            cancellationToken);

        return _mapper.Map<List<DistrictDto>>(districts);
    }
}
