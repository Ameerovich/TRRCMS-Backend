using MediatR;
using System.Text.Json;
using TRRCMS.Application.Buildings.Dtos;
using TRRCMS.Application.Common;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Buildings.Commands.UpdateBuilding;

/// <summary>
/// Handler for UpdateBuildingCommand
/// Updates building attributes matching frontend form fields
/// </summary>
public class UpdateBuildingCommandHandler : IRequestHandler<UpdateBuildingCommand, BuildingDto>
{
    private readonly IBuildingRepository _buildingRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly IGeometryConverter _geometryConverter;
    private readonly ICommunityRepository _communityRepository;

    public UpdateBuildingCommandHandler(
        IBuildingRepository buildingRepository,
        ICurrentUserService currentUserService,
        IAuditService auditService,
        IGeometryConverter geometryConverter,
        ICommunityRepository communityRepository)
    {
        _buildingRepository = buildingRepository;
        _currentUserService = currentUserService;
        _auditService = auditService;
        _geometryConverter = geometryConverter;
        _communityRepository = communityRepository;
    }

    public async Task<BuildingDto> Handle(UpdateBuildingCommand request, CancellationToken cancellationToken)
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

        // Normalize OCHA pCodes (when provided) so the rest of the logic can work with raw codes.
        var (govCodeEff, distCodeEff, subDistCodeEff) = OchaCommandNormalizer.ResolveAdmCodes(
            request.GovernorateCode, request.DistrictCode, request.SubDistrictCode,
            request.GovernoratePCode, request.DistrictPCode, request.SubDistrictPCode);
        var neighCodeEff = OchaCommandNormalizer.ResolveNeighborhoodCode(
            request.NeighborhoodCode, request.NeighborhoodPCode);
        var commCodeEff = request.CommunityCode ?? string.Empty;
        var commPCodeNorm = OchaCommandNormalizer.NormalizeCommunityPCode(request.CommunityPCode);
        if (commPCodeNorm != null)
        {
            var matched = await _communityRepository.GetByExternalPCodeAsync(
                commPCodeNorm, govCodeEff, distCodeEff, subDistCodeEff, cancellationToken);
            if (matched == null)
                throw new ValidationException(
                    $"No community matches OCHA P-Code '{commPCodeNorm}' under {govCodeEff}/{distCodeEff}/{subDistCodeEff}.");
            commCodeEff = matched.Code;
        }

        // Update administrative codes (building code - 17 digits) if all 6 codes are provided
        // Names are optional - only codes are required to trigger the update
        if (!string.IsNullOrEmpty(govCodeEff) &&
            !string.IsNullOrEmpty(distCodeEff) &&
            !string.IsNullOrEmpty(subDistCodeEff) &&
            !string.IsNullOrEmpty(commCodeEff) &&
            !string.IsNullOrEmpty(neighCodeEff) &&
            !string.IsNullOrEmpty(request.BuildingNumber))
        {
            oldValues["BuildingId"] = building.BuildingId;
            newValues["BuildingId"] = $"{govCodeEff}{distCodeEff}{subDistCodeEff}" +
                                      $"{commCodeEff}{neighCodeEff}{request.BuildingNumber}";
            changedFields.Add("BuildingCode");

            building.UpdateAdministrativeCodes(
                govCodeEff,
                distCodeEff,
                subDistCodeEff,
                commCodeEff,
                neighCodeEff,
                request.BuildingNumber,
                request.GovernorateName,
                request.DistrictName,
                request.SubDistrictName,
                request.CommunityName,
                request.NeighborhoodName,
                currentUserId);
        }

        // Update building type
        if (request.BuildingType.HasValue && request.BuildingType.Value != building.BuildingType)
        {
            oldValues["BuildingType"] = building.BuildingType.ToString();
            newValues["BuildingType"] = request.BuildingType.Value.ToString();
            changedFields.Add("BuildingType");

            building.UpdateBuildingType(request.BuildingType.Value, currentUserId);
        }

        // Update building status
        if (request.BuildingStatus.HasValue && request.BuildingStatus.Value != building.Status)
        {
            oldValues["Status"] = building.Status.ToString();
            newValues["Status"] = request.BuildingStatus.Value.ToString();
            changedFields.Add("Status");

            building.UpdateStatus(request.BuildingStatus.Value, currentUserId);
        }

        // Update unit counts
        if (request.NumberOfPropertyUnits.HasValue ||
            request.NumberOfApartments.HasValue ||
            request.NumberOfShops.HasValue)
        {
            var newPropertyUnits = request.NumberOfPropertyUnits ?? building.NumberOfPropertyUnits;
            var newApartments = request.NumberOfApartments ?? building.NumberOfApartments;
            var newShops = request.NumberOfShops ?? building.NumberOfShops;

            if (newPropertyUnits != building.NumberOfPropertyUnits ||
                newApartments != building.NumberOfApartments ||
                newShops != building.NumberOfShops)
            {
                oldValues["NumberOfPropertyUnits"] = building.NumberOfPropertyUnits;
                oldValues["NumberOfApartments"] = building.NumberOfApartments;
                oldValues["NumberOfShops"] = building.NumberOfShops;
                newValues["NumberOfPropertyUnits"] = newPropertyUnits;
                newValues["NumberOfApartments"] = newApartments;
                newValues["NumberOfShops"] = newShops;
                changedFields.Add("UnitCounts");

                building.UpdateUnitCounts(newPropertyUnits, newApartments, newShops, currentUserId);
            }
        }

        // Update coordinates
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

        // Update notes
        var newNotes = request.Notes ?? building.Notes;

        if (newNotes != building.Notes)
        {
            oldValues["Notes"] = building.Notes;
            newValues["Notes"] = newNotes;
            changedFields.Add("Notes");
            building.UpdateDetails(newNotes, currentUserId);
        }

        // Save changes
        await _buildingRepository.UpdateAsync(building, cancellationToken);
        await _buildingRepository.SaveChangesAsync(cancellationToken);

        // Log audit entry
        if (changedFields.Any())
        {
            await _auditService.LogActionAsync(
                actionType: AuditActionType.Update,
                actionDescription: $"Updated building {building.BuildingId}",
                entityType: "Building",
                entityId: building.Id,
                entityIdentifier: building.BuildingId,
                oldValues: JsonSerializer.Serialize(oldValues),
                newValues: JsonSerializer.Serialize(newValues),
                changedFields: string.Join(", ", changedFields),
                cancellationToken: cancellationToken);
        }

        // Resolve community ExternalPCode for response accuracy.
        string? resolvedCommunityPCode = commPCodeNorm;
        if (resolvedCommunityPCode == null && !string.IsNullOrEmpty(building.CommunityCode))
        {
            var matched = await _communityRepository.GetByCodeAsync(
                building.GovernorateCode, building.DistrictCode, building.SubDistrictCode,
                building.CommunityCode, cancellationToken);
            resolvedCommunityPCode = matched?.ExternalPCode;
        }

        // Return updated building DTO
        return MapToDto(building, resolvedCommunityPCode);
    }

    private static BuildingDto MapToDto(Domain.Entities.Building building, string? communityExternalPCode)
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