using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Landmarks.Dtos;

namespace TRRCMS.Application.Landmarks.Queries.GetLandmarkTypes;

public class GetLandmarkTypesQueryHandler : IRequestHandler<GetLandmarkTypesQuery, List<LandmarkTypeIconDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetLandmarkTypesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<LandmarkTypeIconDto>> Handle(GetLandmarkTypesQuery request, CancellationToken cancellationToken)
    {
        var icons = await _unitOfWork.LandmarkTypeIcons.GetAllAsync(cancellationToken);
        return _mapper.Map<List<LandmarkTypeIconDto>>(icons);
    }
}
