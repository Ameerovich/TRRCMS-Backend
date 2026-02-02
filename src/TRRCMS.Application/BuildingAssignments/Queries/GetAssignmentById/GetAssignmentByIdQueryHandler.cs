using MediatR;
using TRRCMS.Application.BuildingAssignments.Dtos;
using TRRCMS.Application.Common.Interfaces;

namespace TRRCMS.Application.BuildingAssignments.Queries.GetAssignmentById;

/// <summary>
/// Handler for GetAssignmentByIdQuery
/// </summary>
public class GetAssignmentByIdQueryHandler : IRequestHandler<GetAssignmentByIdQuery, BuildingAssignmentDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAssignmentByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
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

        return new BuildingAssignmentDto
        {
            Id = assignment.Id,
            
            // Building Info
            BuildingId = assignment.BuildingId,
            BuildingCode = building?.BuildingId ?? string.Empty,
            BuildingAddress = building?.Address,
            GovernorateCode = building?.GovernorateCode,
            DistrictCode = building?.DistrictCode,
            SubDistrictCode = building?.SubDistrictCode,
            CommunityCode = building?.CommunityCode,
            NeighborhoodCode = building?.NeighborhoodCode,
            
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
