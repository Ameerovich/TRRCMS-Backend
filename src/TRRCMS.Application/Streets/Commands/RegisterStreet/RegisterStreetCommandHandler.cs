using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Streets.Dtos;
using TRRCMS.Domain.Entities;
using NetTopologySuite.Geometries;

namespace TRRCMS.Application.Streets.Commands.RegisterStreet;

/// <summary>
/// Handler for RegisterStreetCommand (QGIS plugin).
/// Creates a street with linestring geometry.
/// </summary>
public class RegisterStreetCommandHandler : IRequestHandler<RegisterStreetCommand, StreetDto>
{
    private readonly IStreetRepository _streetRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IGeometryConverter _geometryConverter;
    private readonly IMapper _mapper;

    public RegisterStreetCommandHandler(
        IStreetRepository streetRepository,
        ICurrentUserService currentUserService,
        IGeometryConverter geometryConverter,
        IMapper mapper)
    {
        _streetRepository = streetRepository;
        _currentUserService = currentUserService;
        _geometryConverter = geometryConverter;
        _mapper = mapper;
    }

    public async Task<StreetDto> Handle(RegisterStreetCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        // Check for duplicate identifier
        var existing = await _streetRepository.GetByIdentifierAsync(request.Identifier, cancellationToken);
        if (existing != null)
            throw new ConflictException($"Street with identifier '{request.Identifier}' already exists.");

        // Parse WKT to LineString geometry
        var geometry = _geometryConverter.ParseWkt(request.GeometryWkt)
            ?? throw new ValidationException("Invalid WKT geometry provided.");

        if (geometry is not LineString lineString)
            throw new ValidationException("Street geometry must be a LINESTRING.");

        var street = Street.Create(
            identifier: request.Identifier,
            name: request.Name,
            geometry: lineString,
            createdByUserId: userId);

        await _streetRepository.AddAsync(street, cancellationToken);
        await _streetRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<StreetDto>(street);
    }
}
