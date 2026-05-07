using MediatR;
using TRRCMS.Application.BuildingAssignments.Dtos;
using TRRCMS.Application.Common;
using TRRCMS.Application.Common.Interfaces;

namespace TRRCMS.Application.BuildingAssignments.Queries.GetAssignmentById;

/// <summary>
/// Handler for GetAssignmentByIdQuery
/// </summary>
public class GetAssignmentByIdQueryHandler : IRequestHandler<GetAssignmentByIdQuery, BuildingAssignmentDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICommunityRepository _communityRepository;

    public GetAssignmentByIdQueryHandler(
        IUnitOfWork unitOfWork,
        ICommunityRepository communityRepository)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _communityRepository = communityRepository ?? throw new ArgumentNullException(nameof(communityRepository));
    }

    public async Task<BuildingAssignmentDto?> Handle(GetAssignmentByIdQuery request, CancellationToken cancellationToken)
    {
        var assignment = await _unitOfWork.BuildingAssignments.GetByIdWithDetailsAsync(
            request.AssignmentId, cancellationToken);

        if (assignment == null)
            return null;

        // Get additional details
        var building = await _unitOfWork.Buildings.GetByIdAsync(assignment.BuildingId, cancellationToken);
        var fieldCollector = await _unitOfWork.Users.GetByIdAsync(assignment.FieldCollectorId, cancellationToken);
        var assignedByUser = assignment.AssignedByUserId.HasValue
            ? await _unitOfWork.Users.GetByIdAsync(assignment.AssignedByUserId.Value, cancellationToken)
            : null;

        // Resolve community ExternalPCode for OCHA-accurate output.
        string? communityExternalPCode = null;
        if (building != null && !string.IsNullOrEmpty(building.CommunityCode))
        {
            var commRow = await _communityRepository.GetByCodeAsync(
                building.GovernorateCode, building.DistrictCode, building.SubDistrictCode,
                building.CommunityCode, cancellationToken);
            communityExternalPCode = commRow?.ExternalPCode;
        }

        return new BuildingAssignmentDto
        {
            Id = assignment.Id,

            // Building Info
            BuildingId = assignment.BuildingId,
            BuildingCode = building?.BuildingId ?? string.Empty,
            GovernorateCode = building?.GovernorateCode,
            DistrictCode = building?.DistrictCode,
            SubDistrictCode = building?.SubDistrictCode,
            CommunityCode = building?.CommunityCode,
            NeighborhoodCode = building?.NeighborhoodCode,
            BuildingGeometryWkt = building?.BuildingGeometryWkt,
            Latitude = building?.Latitude,
            Longitude = building?.Longitude,

            // OCHA P-Codes
            GovernoratePCode = OchaPCodeConverter.ToGovPCode(building?.GovernorateCode),
            DistrictPCode = OchaPCodeConverter.ToDistrictPCode(building?.GovernorateCode, building?.DistrictCode),
            SubDistrictPCode = OchaPCodeConverter.ToSubDistrictPCode(building?.GovernorateCode, building?.DistrictCode, building?.SubDistrictCode),
            CommunityPCode = OchaPCodeConverter.ToCommunityPCode(communityExternalPCode, building?.CommunityCode),
            NeighborhoodPCode = OchaPCodeConverter.ToNeighborhoodPCode(building?.NeighborhoodCode),
            
            // Field Collector Info
            FieldCollectorId = assignment.FieldCollectorId,
            FieldCollectorName = fieldCollector?.FullNameArabic ?? "Unknown",
            FieldCollectorDeviceId = fieldCollector?.AssignedTabletId,
            
            // Assignment Info
            AssignedByUserId = assignment.AssignedByUserId,
            AssignedByUserName = assignedByUser?.FullNameArabic,
            AssignedDate = assignment.AssignedDate,
            TargetCompletionDate = assignment.TargetCompletionDate,
            ActualCompletionDate = assignment.ActualCompletionDate,
            
            // Transfer Status
            TransferStatus = assignment.TransferStatus,
            TransferStatusName = assignment.TransferStatus.ToString(),
            TransferredToTabletDate = assignment.TransferredToTabletDate,
            SynchronizedFromTabletDate = assignment.SynchronizedFromTabletDate,
            TransferErrorMessage = assignment.TransferErrorMessage,
            TransferRetryCount = assignment.TransferRetryCount,
            
            // Progress
            TotalPropertyUnits = assignment.TotalPropertyUnits,
            CompletedPropertyUnits = assignment.CompletedPropertyUnits,
            CompletionPercentage = assignment.GetCompletionPercentage(),
            
            // Revisit Info
            IsRevisit = assignment.IsRevisit,
            OriginalAssignmentId = assignment.OriginalAssignmentId,
            UnitsForRevisit = assignment.UnitsForRevisit,
            RevisitReason = assignment.RevisitReason,
            
            // Status
            Priority = assignment.Priority,
            AssignmentNotes = assignment.AssignmentNotes,
            IsActive = assignment.IsActive,
            IsOverdue = assignment.IsOverdue(),
            
            // Audit
            CreatedAt = assignment.CreatedAtUtc,
            ModifiedAt = assignment.LastModifiedAtUtc
        };
    }
}
