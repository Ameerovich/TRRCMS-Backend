using MediatR;
using TRRCMS.Application.Buildings.Dtos;
using TRRCMS.Application.Common;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Services;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Buildings.Commands.CreateBuilding;

/// <summary>
/// Handler for CreateBuildingCommand
/// Creates a new building and returns full BuildingDto
/// </summary>
public class CreateBuildingCommandHandler : IRequestHandler<CreateBuildingCommand, BuildingDto>
{
    private readonly IBuildingRepository _buildingRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IGeometryConverter _geometryConverter;
    private readonly IAdministrativeNameResolver _nameResolver;
    private readonly ICommunityRepository _communityRepository;
    private readonly IAuditService _auditService;

    public CreateBuildingCommandHandler(
        IBuildingRepository buildingRepository,
        ICurrentUserService currentUserService,
        IGeometryConverter geometryConverter,
        IAdministrativeNameResolver nameResolver,
        ICommunityRepository communityRepository,
        IAuditService auditService)
    {
        _buildingRepository = buildingRepository;
        _currentUserService = currentUserService;
        _geometryConverter = geometryConverter;
        _nameResolver = nameResolver;
        _communityRepository = communityRepository;
        _auditService = auditService;
    }

    public async Task<BuildingDto> Handle(CreateBuildingCommand request, CancellationToken cancellationToken)
    {
        // Get current user ID
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated");

        // Normalize OCHA pCodes (when provided) to raw numeric admin codes.
        var (govCode, distCode, subDistCode) = OchaCommandNormalizer.ResolveAdmCodes(
            request.GovernorateCode, request.DistrictCode, request.SubDistrictCode,
            request.GovernoratePCode, request.DistrictPCode, request.SubDistrictPCode);
        var neighCode = OchaCommandNormalizer.ResolveNeighborhoodCode(
            request.NeighborhoodCode, request.NeighborhoodPCode);

        var commCode = request.CommunityCode ?? string.Empty;
        var commPCode = OchaCommandNormalizer.NormalizeCommunityPCode(request.CommunityPCode);
        if (commPCode != null)
        {
            var matched = await _communityRepository.GetByExternalPCodeAsync(
                commPCode, govCode, distCode, subDistCode, cancellationToken);
            if (matched == null)
                throw new ValidationException(
                    $"No community matches OCHA P-Code '{commPCode}' under {govCode}/{distCode}/{subDistCode}.");
            commCode = matched.Code;
        }

        // Generate Building ID for duplicate check (stored without dashes)
        var buildingIdCode = $"{govCode}{distCode}{subDistCode}" +
                             $"{commCode}{neighCode}{request.BuildingNumber}";

        // Check if building ID already exists
        var existingBuilding = await _buildingRepository.GetByBuildingIdAsync(buildingIdCode, cancellationToken);
        if (existingBuilding != null)
        {
            throw new ConflictException($"Building with code {buildingIdCode} already exists.");
        }

        // Resolve administrative hierarchy codes to Arabic names
        var names = await _nameResolver.ResolveAsync(
            govCode, distCode, subDistCode, commCode, neighCode, cancellationToken);

        // Create building entity
        var building = Building.Create(
            governorateCode: govCode,
            districtCode: distCode,
            subDistrictCode: subDistCode,
            communityCode: commCode,
            neighborhoodCode: neighCode,
            buildingNumber: request.BuildingNumber,
            governorateName: names.GovernorateName,
            districtName: names.DistrictName,
            subDistrictName: names.SubDistrictName,
            communityName: names.CommunityName,
            neighborhoodName: names.NeighborhoodName,
            buildingType: request.BuildingType,
            status: request.BuildingStatus,
            createdByUserId: userId
        );

        // Set unit counts
        building.UpdateUnitCounts(
            propertyUnits: request.NumberOfPropertyUnits,
            apartments: request.NumberOfApartments,
            shops: request.NumberOfShops,
            modifiedByUserId: userId
        );

        // Set coordinates if provided
        if (request.Latitude.HasValue && request.Longitude.HasValue)
        {
            var fallbackPoint = _geometryConverter.CreatePoint(
                (double)request.Longitude.Value, (double)request.Latitude.Value);
            building.SetCoordinates(request.Latitude.Value, request.Longitude.Value, userId, fallbackPoint);
        }

        // Set geometry if provided
        if (!string.IsNullOrWhiteSpace(request.BuildingGeometryWkt))
        {
            var geometry = _geometryConverter.ParseWkt(request.BuildingGeometryWkt);
            building.SetGeometry(geometry, userId);
        }

        // Set notes
        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            building.UpdateDetails(
                notes: request.Notes,
                modifiedByUserId: userId
            );
        }

        // Save to database
        await _buildingRepository.AddAsync(building, cancellationToken);
        await _buildingRepository.SaveChangesAsync(cancellationToken);

        // Resolve community ExternalPCode for response accuracy.
        string? resolvedCommunityPCode = commPCode;
        if (resolvedCommunityPCode == null && !string.IsNullOrEmpty(commCode))
        {
            var matched = await _communityRepository.GetByCodeAsync(
                govCode, distCode, subDistCode, commCode, cancellationToken);
            resolvedCommunityPCode = matched?.ExternalPCode;
        }

        await _auditService.LogActionAsync(
            actionType: AuditActionType.Create,
            actionDescription: $"Building {building.BuildingId} created",
            entityType: "Building",
            entityId: building.Id,
            entityIdentifier: building.BuildingId,
            cancellationToken: cancellationToken);

        // Return full DTO
        return MapToDto(building, resolvedCommunityPCode);
    }

    private static BuildingDto MapToDto(Building building, string? communityExternalPCode)
    {
        return new BuildingDto
        {
            Id = building.Id,
            BuildingId = building.BuildingId,

            // Administrative Codes
            GovernorateCode = building.GovernorateCode,
            DistrictCode = building.DistrictCode,
            SubDistrictCode = building.SubDistrictCode,
            CommunityCode = building.CommunityCode,
            NeighborhoodCode = building.NeighborhoodCode,
            BuildingNumber = building.BuildingNumber,

            // Location Names
            GovernorateName = building.GovernorateName,
            DistrictName = building.DistrictName,
            SubDistrictName = building.SubDistrictName,
            CommunityName = building.CommunityName,
            NeighborhoodName = building.NeighborhoodName,

            // OCHA P-Codes
            GovernoratePCode = OchaPCodeConverter.ToGovPCode(building.GovernorateCode),
            DistrictPCode = OchaPCodeConverter.ToDistrictPCode(building.GovernorateCode, building.DistrictCode),
            SubDistrictPCode = OchaPCodeConverter.ToSubDistrictPCode(building.GovernorateCode, building.DistrictCode, building.SubDistrictCode),
            CommunityPCode = OchaPCodeConverter.ToCommunityPCode(communityExternalPCode, building.CommunityCode),
            NeighborhoodPCode = OchaPCodeConverter.ToNeighborhoodPCode(building.NeighborhoodCode),

            // Attributes
            BuildingType = (int)building.BuildingType,
            Status = (int)building.Status,
            NumberOfPropertyUnits = building.NumberOfPropertyUnits,
            NumberOfApartments = building.NumberOfApartments,
            NumberOfShops = building.NumberOfShops,
            // Location
            Latitude = building.Latitude,
            Longitude = building.Longitude,
            BuildingGeometryWkt = building.BuildingGeometryWkt,

            // Additional Information
            Notes = building.Notes,

            // Audit
            CreatedAtUtc = building.CreatedAtUtc,
            LastModifiedAtUtc = building.LastModifiedAtUtc
        };
    }
}