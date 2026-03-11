using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Landmarks.Dtos;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;
using NetTopologySuite.Geometries;

namespace TRRCMS.Application.Landmarks.Commands.RegisterLandmark;

/// <summary>
/// Handler for RegisterLandmarkCommand (QGIS plugin).
/// Creates a landmark with point geometry.
/// </summary>
public class RegisterLandmarkCommandHandler : IRequestHandler<RegisterLandmarkCommand, LandmarkDto>
{
    private readonly ILandmarkRepository _landmarkRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IGeometryConverter _geometryConverter;
    private readonly IMapper _mapper;

    public RegisterLandmarkCommandHandler(
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

    public async Task<LandmarkDto> Handle(RegisterLandmarkCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        // Check for duplicate identifier
        var existing = await _landmarkRepository.GetByIdentifierAsync(request.Identifier, cancellationToken);
        if (existing != null)
            throw new ConflictException($"Landmark with identifier '{request.Identifier}' already exists.");

        // Parse WKT to Point geometry
        var geometry = _geometryConverter.ParseWkt(request.LocationWkt)
            ?? throw new ValidationException("Invalid WKT geometry provided.");

        if (geometry is not Point point)
            throw new ValidationException("Landmark geometry must be a POINT.");

        var landmarkType = (LandmarkType)request.Type;

        var landmark = Landmark.Create(
            identifier: request.Identifier,
            name: request.Name,
            type: landmarkType,
            location: point,
            createdByUserId: userId);

        await _landmarkRepository.AddAsync(landmark, cancellationToken);
        await _landmarkRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<LandmarkDto>(landmark);
    }
}
