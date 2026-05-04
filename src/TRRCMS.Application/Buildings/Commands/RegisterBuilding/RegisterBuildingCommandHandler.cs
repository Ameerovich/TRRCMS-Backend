using MediatR;
using TRRCMS.Application.Buildings.Dtos;
using TRRCMS.Application.Common;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Services;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Buildings.Commands.RegisterBuilding;

/// <summary>
/// Handler for RegisterBuildingCommand (QGIS plugin).
/// Creates a building with minimal data: admin codes + polygon geometry.
/// Defaults: BuildingType=Residential, Status=Unknown, all counts=0.
/// Full details are provided later via field survey import (.uhc).
/// </summary>
public class RegisterBuildingCommandHandler : IRequestHandler<RegisterBuildingCommand, BuildingDto>
{
    private readonly IBuildingRepository _buildingRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IGeometryConverter _geometryConverter;
    private readonly IAdministrativeNameResolver _nameResolver;
    private readonly ICommunityRepository _communityRepository;

    public RegisterBuildingCommandHandler(
        IBuildingRepository buildingRepository,
        ICurrentUserService currentUserService,
        IGeometryConverter geometryConverter,
        IAdministrativeNameResolver nameResolver,
        ICommunityRepository communityRepository)
    {
        _buildingRepository = buildingRepository;
        _currentUserService = currentUserService;
        _geometryConverter = geometryConverter;
        _nameResolver = nameResolver;
        _communityRepository = communityRepository;
    }

    public async Task<BuildingDto> Handle(RegisterBuildingCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        // Normalize OCHA pCodes (when provided) to raw numeric admin codes.
        var (govCode, distCode, subDistCode) = OchaCommandNormalizer.ResolveAdmCodes(
            request.GovernorateCode, request.DistrictCode, request.SubDistrictCode,
            request.GovernoratePCode, request.DistrictPCode, request.SubDistrictPCode);
        var neighCode = OchaCommandNormalizer.ResolveNeighborhoodCode(
            request.NeighborhoodCode, request.NeighborhoodPCode);

        // Resolve community: pCode (e.g. "C1007") wins by ExternalPCode lookup; otherwise use raw 3-digit code.
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

        // Compose the 17-digit BuildingId
        var buildingIdCode = $"{govCode}{distCode}{subDistCode}" +
                             $"{commCode}{neighCode}{request.BuildingNumber}";

        // Check for existing building with same code
        var existing = await _buildingRepository.GetByBuildingIdAsync(buildingIdCode, cancellationToken);
        if (existing != null)
            throw new ConflictException($"Building with code '{buildingIdCode}' already exists.");

        // Resolve administrative hierarchy codes to Arabic names
        var names = await _nameResolver.ResolveAsync(
            govCode, distCode, subDistCode, commCode, neighCode, cancellationToken);

        // Create building with minimal QGIS data + defaults
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
            buildingType: BuildingType.Residential,
            status: BuildingStatus.Unknown,
            createdByUserId: userId);

        // Set polygon geometry from QGIS (auto-computes centroid lat/lng)
        var geometry = _geometryConverter.ParseWkt(request.BuildingGeometryWkt);
        building.SetGeometry(geometry, userId);

        // Set notes if provided
        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            building.UpdateDetails(
                notes: request.Notes,
                modifiedByUserId: userId);
        }

        await _buildingRepository.AddAsync(building, cancellationToken);
        await _buildingRepository.SaveChangesAsync(cancellationToken);

        // For the response, prefer the OCHA pCode the caller supplied (already validated above);
        // otherwise look up the community to find its stored ExternalPCode for accuracy.
        string? resolvedCommunityPCode = commPCode;
        if (resolvedCommunityPCode == null && !string.IsNullOrEmpty(commCode))
        {
            var matched = await _communityRepository.GetByCodeAsync(
                govCode, distCode, subDistCode, commCode, cancellationToken);
            resolvedCommunityPCode = matched?.ExternalPCode;
        }

        return MapToDto(building, resolvedCommunityPCode);
    }

    private static BuildingDto MapToDto(Building building, string? communityExternalPCode)
    {
        return new BuildingDto
        {
            Id = building.Id,
            BuildingId = building.BuildingId,

            // Administrative codes
            GovernorateCode = building.GovernorateCode,
            DistrictCode = building.DistrictCode,
            SubDistrictCode = building.SubDistrictCode,
            CommunityCode = building.CommunityCode,
            NeighborhoodCode = building.NeighborhoodCode,
            BuildingNumber = building.BuildingNumber,

            // Location names
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

            // Building attributes
            BuildingType = (int)building.BuildingType,
            Status = (int)building.Status,
            NumberOfPropertyUnits = building.NumberOfPropertyUnits,
            NumberOfApartments = building.NumberOfApartments,
            NumberOfShops = building.NumberOfShops,
            // Spatial
            Latitude = building.Latitude,
            Longitude = building.Longitude,
            BuildingGeometryWkt = building.BuildingGeometryWkt,

            // Additional
            Notes = building.Notes,

            // Audit
            CreatedAtUtc = building.CreatedAtUtc,
            LastModifiedAtUtc = building.LastModifiedAtUtc
        };
    }
}
