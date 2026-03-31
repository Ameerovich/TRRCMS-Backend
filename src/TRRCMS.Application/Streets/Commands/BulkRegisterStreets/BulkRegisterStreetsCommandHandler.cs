using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Models;
using TRRCMS.Application.Streets.Dtos;
using TRRCMS.Domain.Entities;
using NetTopologySuite.Geometries;

namespace TRRCMS.Application.Streets.Commands.BulkRegisterStreets;

public class BulkRegisterStreetsCommandHandler
    : IRequestHandler<BulkRegisterStreetsCommand, BulkOperationResult<StreetDto>>
{
    private readonly IStreetRepository _streetRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IGeometryConverter _geometryConverter;
    private readonly IMapper _mapper;

    public BulkRegisterStreetsCommandHandler(
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

    public async Task<BulkOperationResult<StreetDto>> Handle(
        BulkRegisterStreetsCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var result = new BulkOperationResult<StreetDto>
        {
            TotalReceived = request.Streets.Count
        };

        for (int i = 0; i < request.Streets.Count; i++)
        {
            var item = request.Streets[i];
            try
            {
                // Skip duplicates by identifier
                var existing = await _streetRepository.GetByIdentifierAsync(item.Identifier, cancellationToken);
                if (existing != null)
                {
                    result.Skipped++;
                    result.Errors.Add(new BulkErrorItem
                    {
                        Index = i,
                        Identifier = item.Identifier.ToString(),
                        Error = $"Street with identifier '{item.Identifier}' already exists. Skipped."
                    });
                    continue;
                }

                // Parse WKT
                var geometry = _geometryConverter.ParseWkt(item.GeometryWkt);
                if (geometry is not LineString lineString)
                {
                    result.Failed++;
                    result.Errors.Add(new BulkErrorItem
                    {
                        Index = i,
                        Identifier = item.Identifier.ToString(),
                        Error = "Invalid WKT geometry. Must be a LINESTRING."
                    });
                    continue;
                }

                var street = Street.Create(
                    identifier: item.Identifier,
                    name: item.Name,
                    geometry: lineString,
                    createdByUserId: userId);

                await _streetRepository.AddAsync(street, cancellationToken);
                result.CreatedItems.Add(_mapper.Map<StreetDto>(street));
                result.Succeeded++;
            }
            catch (Exception ex)
            {
                result.Failed++;
                result.Errors.Add(new BulkErrorItem
                {
                    Index = i,
                    Identifier = item.Identifier.ToString(),
                    Error = ex.Message
                });
            }
        }

        // Save all at once
        if (result.Succeeded > 0)
            await _streetRepository.SaveChangesAsync(cancellationToken);

        return result;
    }
}
