using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Landmarks.Dtos;
using TRRCMS.Domain.Enums;
using NetTopologySuite.Geometries;

namespace TRRCMS.Application.Landmarks.Commands.UpdateLandmark;

public class UpdateLandmarkCommandHandler : IRequestHandler<UpdateLandmarkCommand, LandmarkDto>
{
    private readonly ILandmarkRepository _landmarkRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IGeometryConverter _geometryConverter;
    private readonly IMapper _mapper;

    public UpdateLandmarkCommandHandler(
        ILandmarkRepository landmarkRepository,
        ICurrentUserService currentUserService,
        IGeometryConverter geometryConverter,
        IMapper mapper)
    {
        _landmarkRepository = landmarkRepository;
        _currentUserService = currentUserService;
        _geometryConverter = geometryConverter;
        _mapper = mapper;
    }

    public async Task<LandmarkDto> Handle(UpdateLandmarkCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var landmark = await _landmarkRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Landmark with ID {request.Id} not found.");

        landmark.Update(request.Name, (LandmarkType)request.Type, userId);

        if (!string.IsNullOrWhiteSpace(request.LocationWkt))
        {
            var geometry = _geometryConverter.ParseWkt(request.LocationWkt)
                ?? throw new ValidationException("Invalid WKT geometry provided.");

            if (geometry is not Point point)
                throw new ValidationException("Landmark geometry must be a POINT.");

            landmark.UpdateLocation(point, userId);
        }

        await _landmarkRepository.UpdateAsync(landmark, cancellationToken);
        await _landmarkRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<LandmarkDto>(landmark);
    }
}
