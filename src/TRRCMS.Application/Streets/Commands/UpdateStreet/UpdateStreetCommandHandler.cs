using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Streets.Dtos;
using NetTopologySuite.Geometries;

namespace TRRCMS.Application.Streets.Commands.UpdateStreet;

public class UpdateStreetCommandHandler : IRequestHandler<UpdateStreetCommand, StreetDto>
{
    private readonly IStreetRepository _streetRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IGeometryConverter _geometryConverter;
    private readonly IMapper _mapper;

    public UpdateStreetCommandHandler(
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

    public async Task<StreetDto> Handle(UpdateStreetCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var street = await _streetRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Street with ID {request.Id} not found.");

        street.UpdateName(request.Name, userId);

        if (!string.IsNullOrWhiteSpace(request.GeometryWkt))
        {
            var geometry = _geometryConverter.ParseWkt(request.GeometryWkt)
                ?? throw new ValidationException("Invalid WKT geometry provided.");

            if (geometry is not LineString lineString)
                throw new ValidationException("Street geometry must be a LINESTRING.");

            street.UpdateGeometry(lineString, userId);
        }

        await _streetRepository.UpdateAsync(street, cancellationToken);
        await _streetRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<StreetDto>(street);
    }
}
