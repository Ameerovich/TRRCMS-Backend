using AutoMapper;
using MediatR;
using System.Text.Json;
using TRRCMS.Application.Buildings.Dtos;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Buildings.Commands.UpdateBuildingGeometry;

public class UpdateBuildingGeometryCommandHandler : IRequestHandler<UpdateBuildingGeometryCommand, BuildingDto>
{
    private readonly IBuildingRepository _buildingRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;
    private readonly IGeometryConverter _geometryConverter;

    public UpdateBuildingGeometryCommandHandler(
        IBuildingRepository buildingRepository,
        ICurrentUserService currentUserService,
        IAuditService auditService,
        IMapper mapper,
        IGeometryConverter geometryConverter)
    {
        _buildingRepository = buildingRepository;
        _currentUserService = currentUserService;
        _auditService = auditService;
        _mapper = mapper;
        _geometryConverter = geometryConverter;
    }

    public async Task<BuildingDto> Handle(UpdateBuildingGeometryCommand request, CancellationToken cancellationToken)
    {
        // Get current user
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated");

        // Get building
        var building = await _buildingRepository.GetByIdAsync(request.BuildingId, cancellationToken)
            ?? throw new NotFoundException($"Building with ID {request.BuildingId} not found");

        // Track changes for audit
        var changedFields = new List<string>();
        var oldValues = new Dictionary<string, object?>();
        var newValues = new Dictionary<string, object?>();

        // Update geometry WKT if provided
        if (!string.IsNullOrWhiteSpace(request.GeometryWkt))
        {
            if (request.GeometryWkt != building.BuildingGeometryWkt)
            {
                oldValues["BuildingGeometryWkt"] = building.BuildingGeometryWkt;
                newValues["BuildingGeometryWkt"] = request.GeometryWkt;
                changedFields.Add("BuildingGeometryWkt");

                var geometry = _geometryConverter.ParseWkt(request.GeometryWkt);
                building.SetGeometry(geometry, currentUserId);
            }
        }

        // Update coordinates if both provided
        if (request.Latitude.HasValue && request.Longitude.HasValue)
        {
            if (request.Latitude.Value != building.Latitude || request.Longitude.Value != building.Longitude)
            {
                oldValues["Latitude"] = building.Latitude;
                oldValues["Longitude"] = building.Longitude;
                newValues["Latitude"] = request.Latitude.Value;
                newValues["Longitude"] = request.Longitude.Value;
                changedFields.Add("Coordinates");

                var fallbackPoint = _geometryConverter.CreatePoint(
                    (double)request.Longitude.Value, (double)request.Latitude.Value);
                building.SetCoordinates(request.Latitude.Value, request.Longitude.Value, currentUserId, fallbackPoint);
            }
        }

        // Save changes if any updates were made
        if (changedFields.Any())
        {
            await _buildingRepository.UpdateAsync(building, cancellationToken);
            await _buildingRepository.SaveChangesAsync(cancellationToken);

            // Log audit entry
            await _auditService.LogActionAsync(
                actionType: AuditActionType.Update,
                actionDescription: $"Updated geometry/coordinates for building {building.BuildingId}",
                entityType: "Building",
                entityId: building.Id,
                entityIdentifier: building.BuildingId,
                oldValues: JsonSerializer.Serialize(oldValues),
                newValues: JsonSerializer.Serialize(newValues),
                changedFields: string.Join(", ", changedFields),
                cancellationToken: cancellationToken);
        }

        // Return updated building DTO
        return _mapper.Map<BuildingDto>(building);
    }
}