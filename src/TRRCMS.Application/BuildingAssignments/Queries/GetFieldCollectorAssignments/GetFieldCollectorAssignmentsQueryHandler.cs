using MediatR;
using TRRCMS.Application.BuildingAssignments.Dtos;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.BuildingAssignments.Queries.GetFieldCollectorAssignments;

/// <summary>
/// Handler for GetFieldCollectorAssignmentsQuery
/// UC-012: View collector's current tasks
/// </summary>
public class GetFieldCollectorAssignmentsQueryHandler 
    : IRequestHandler<GetFieldCollectorAssignmentsQuery, FieldCollectorTasksDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetFieldCollectorAssignmentsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<FieldCollectorTasksDto> Handle(
        GetFieldCollectorAssignmentsQuery request, 
        CancellationToken cancellationToken)
    {
        // Validate field collector exists
        var fieldCollector = await _unitOfWork.Users.GetByIdAsync(request.FieldCollectorId, cancellationToken);
        if (fieldCollector == null)
        {
            throw new NotFoundException($"Field collector with ID {request.FieldCollectorId} not found");
        }

        // Get assignments based on filters
        var (assignments, _) = await _unitOfWork.BuildingAssignments.SearchAssignmentsAsync(
            fieldCollectorId: request.FieldCollectorId,
            transferStatus: request.TransferStatus,
            isActive: request.IsActive,
            isRevisit: request.IsRevisit,
            page: 1,
            pageSize: 1000, // Get all assignments
            sortBy: "AssignedDate",
            sortDescending: true,
            cancellationToken: cancellationToken
        );

        // Calculate summary counts
        var activeAssignments = assignments.Where(a => a.IsActive).ToList();
        var pendingTransfer = activeAssignments.Count(a => a.TransferStatus == TransferStatus.Pending);
        var transferred = activeAssignments.Count(a => a.TransferStatus == TransferStatus.Transferred);
        var inProgress = activeAssignments.Count(a => a.TransferStatus == TransferStatus.InProgress);
        var synchronized = activeAssignments.Count(a => a.TransferStatus == TransferStatus.Synchronized);

        // Map assignments to DTOs
        var assignmentDtos = new List<BuildingAssignmentSummaryDto>();

        foreach (var assignment in assignments)
        {
            var building = await _unitOfWork.Buildings.GetByIdAsync(assignment.BuildingId, cancellationToken);

            assignmentDtos.Add(new BuildingAssignmentSummaryDto
            {
                Id = assignment.Id,
                BuildingId = assignment.BuildingId,
                BuildingCode = building?.BuildingId ?? string.Empty,
                BuildingAddress = building?.Address,
                FieldCollectorId = assignment.FieldCollectorId,
                FieldCollectorName = fieldCollector.FullNameArabic,
                AssignedDate = assignment.AssignedDate,
                TransferStatus = assignment.TransferStatus,
                TransferStatusName = assignment.TransferStatus.ToString(),
                TotalPropertyUnits = assignment.TotalPropertyUnits,
                CompletedPropertyUnits = assignment.CompletedPropertyUnits,
                CompletionPercentage = assignment.GetCompletionPercentage(),
                IsActive = assignment.IsActive,
                IsOverdue = assignment.IsOverdue(),
                IsRevisit = assignment.IsRevisit,
                Priority = assignment.Priority
            });
        }

        return new FieldCollectorTasksDto
        {
            FieldCollectorId = fieldCollector.Id,
            FieldCollectorName = fieldCollector.FullNameArabic,
            TotalAssignments = assignments.Count,
            PendingTransfer = pendingTransfer,
            ReadyForSurvey = transferred,
            InProgress = inProgress,
            Completed = synchronized,
            Assignments = assignmentDtos
        };
    }
}
