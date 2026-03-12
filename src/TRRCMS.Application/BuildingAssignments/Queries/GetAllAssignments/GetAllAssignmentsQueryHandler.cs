using MediatR;
using TRRCMS.Application.BuildingAssignments.Dtos;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Models;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.BuildingAssignments.Queries.GetAllAssignments;

public class GetAllAssignmentsQueryHandler
    : IRequestHandler<GetAllAssignmentsQuery, PagedResult<BuildingAssignmentSummaryDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllAssignmentsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<PagedResult<BuildingAssignmentSummaryDto>> Handle(
        GetAllAssignmentsQuery request,
        CancellationToken cancellationToken)
    {
        // Parse transfer status
        TransferStatus? transferStatus = request.TransferStatus.HasValue
            ? (TransferStatus)request.TransferStatus.Value
            : null;

        var sortDescending = request.SortDirection == SortDirection.Descending;

        var (assignments, totalCount) = await _unitOfWork.BuildingAssignments.SearchAssignmentsAsync(
            fieldCollectorId: request.FieldCollectorId,
            buildingId: request.BuildingId,
            transferStatus: transferStatus,
            isActive: request.IsActive,
            isRevisit: request.IsRevisit,
            assignedFromDate: request.AssignedFromDate,
            assignedToDate: request.AssignedToDate,
            page: request.PageNumber,
            pageSize: request.PageSize,
            sortBy: request.SortBy,
            sortDescending: sortDescending,
            cancellationToken: cancellationToken);

        // Batch-load unique buildings and field collectors to avoid N+1
        var buildingIds = assignments.Select(a => a.BuildingId).Distinct().ToList();
        var collectorIds = assignments.Select(a => a.FieldCollectorId).Distinct().ToList();

        var buildings = new Dictionary<Guid, Domain.Entities.Building>();
        foreach (var buildingId in buildingIds)
        {
            var building = await _unitOfWork.Buildings.GetByIdAsync(buildingId, cancellationToken);
            if (building != null)
                buildings[buildingId] = building;
        }

        var collectors = new Dictionary<Guid, Domain.Entities.User>();
        foreach (var collectorId in collectorIds)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(collectorId, cancellationToken);
            if (user != null)
                collectors[collectorId] = user;
        }

        // Map to DTOs
        var dtos = assignments.Select(assignment =>
        {
            buildings.TryGetValue(assignment.BuildingId, out var building);
            collectors.TryGetValue(assignment.FieldCollectorId, out var collector);

            return new BuildingAssignmentSummaryDto
            {
                Id = assignment.Id,
                BuildingId = assignment.BuildingId,
                BuildingCode = building?.BuildingId ?? string.Empty,
                BuildingGeometryWkt = building?.BuildingGeometryWkt,
                Latitude = building?.Latitude,
                Longitude = building?.Longitude,
                FieldCollectorId = assignment.FieldCollectorId,
                FieldCollectorName = collector?.FullNameArabic ?? string.Empty,
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
            };
        }).ToList();

        return new PagedResult<BuildingAssignmentSummaryDto>(
            dtos, totalCount, request.PageNumber, request.PageSize);
    }
}
