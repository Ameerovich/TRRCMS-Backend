using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Landmarks.Dtos;

namespace TRRCMS.Application.Landmarks.Queries.GetLandmarkById;

public class GetLandmarkByIdQueryHandler : IRequestHandler<GetLandmarkByIdQuery, LandmarkDto>
{
    private readonly ILandmarkRepository _landmarkRepository;
    private readonly IMapper _mapper;

    public GetLandmarkByIdQueryHandler(
        ILandmarkRepository landmarkRepository,
        IMapper mapper)
    {
        _landmarkRepository = landmarkRepository;
        _mapper = mapper;
    }

    public async Task<LandmarkDto> Handle(GetLandmarkByIdQuery request, CancellationToken cancellationToken)
    {
        var landmark = await _landmarkRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Landmark with ID {request.Id} not found.");

        return _mapper.Map<LandmarkDto>(landmark);
    }
}
