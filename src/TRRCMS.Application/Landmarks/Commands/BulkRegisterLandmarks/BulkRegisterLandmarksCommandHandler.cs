using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Models;
using TRRCMS.Application.Landmarks.Dtos;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;
using NetTopologySuite.Geometries;

namespace TRRCMS.Application.Landmarks.Commands.BulkRegisterLandmarks;

public class BulkRegisterLandmarksCommandHandler
    : IRequestHandler<BulkRegisterLandmarksCommand, BulkOperationResult<LandmarkDto>>
{
    private readonly ILandmarkRepository _landmarkRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IGeometryConverter _geometryConverter;
    private readonly IMapper _mapper;

    public BulkRegisterLandmarksCommandHandler(
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

    public async Task<BulkOperationResult<LandmarkDto>> Handle(
        BulkRegisterLandmarksCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var result = new BulkOperationResult<LandmarkDto>
        {
            TotalReceived = request.Landmarks.Count
        };

        for (int i = 0; i < request.Landmarks.Count; i++)
        {
            var item = request.Landmarks[i];
            try
            {
                // Skip duplicates by identifier
                var existing = await _landmarkRepository.GetByIdentifierAsync(item.Identifier, cancellationToken);
                if (existing != null)
                {
                    result.Skipped++;
                    result.Errors.Add(new BulkErrorItem
                    {
                        Index = i,
                        Identifier = item.Identifier.ToString(),
                        Error = $"Landmark with identifier '{item.Identifier}' already exists. Skipped."
                    });
                    continue;
                }

                // Parse WKT
                var geometry = _geometryConverter.ParseWkt(item.LocationWkt);
                if (geometry is not Point point)
                {
                    result.Failed++;
                    result.Errors.Add(new BulkErrorItem
                    {
                        Index = i,
                        Identifier = item.Identifier.ToString(),
                        Error = "Invalid WKT geometry. Must be a POINT."
                    });
                    continue;
                }

                var landmark = Landmark.Create(
                    identifier: item.Identifier,
                    name: item.Name,
                    type: (LandmarkType)item.Type,
                    location: point,
                    createdByUserId: userId);

                await _landmarkRepository.AddAsync(landmark, cancellationToken);
                result.CreatedItems.Add(_mapper.Map<LandmarkDto>(landmark));
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
            await _landmarkRepository.SaveChangesAsync(cancellationToken);

        return result;
    }
}
