using AutoMapper;
using MediatR;
using TRRCMS.Application.AdministrativeDivisions.Dtos;
using TRRCMS.Application.Common.Interfaces;

namespace TRRCMS.Application.AdministrativeDivisions.Queries.GetSubDistricts;

/// <summary>
/// Handler for GetSubDistrictsQuery
/// </summary>
public class GetSubDistrictsQueryHandler : IRequestHandler<GetSubDistrictsQuery, List<SubDistrictDto>>
{
    private readonly ISubDistrictRepository _subDistrictRepository;
    private readonly IMapper _mapper;

    public GetSubDistrictsQueryHandler(
        ISubDistrictRepository subDistrictRepository,
        IMapper mapper)
    {
        _subDistrictRepository = subDistrictRepository;
        _mapper = mapper;
    }

    public async Task<List<SubDistrictDto>> Handle(
        GetSubDistrictsQuery request,
        CancellationToken cancellationToken)
    {
        var subDistricts = await _subDistrictRepository.GetAllAsync(
            request.GovernorateCode,
            request.DistrictCode,
            cancellationToken);

        return _mapper.Map<List<SubDistrictDto>>(subDistricts);
    }
}
