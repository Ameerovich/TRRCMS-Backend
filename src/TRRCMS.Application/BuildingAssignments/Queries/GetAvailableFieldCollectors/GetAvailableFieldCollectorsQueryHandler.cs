using MediatR;
using TRRCMS.Application.BuildingAssignments.Dtos;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.BuildingAssignments.Queries.GetAvailableFieldCollectors;

/// <summary>
/// Handler for GetAvailableFieldCollectorsQuery
/// UC-012: Select field collector for assignment
/// </summary>
public class GetAvailableFieldCollectorsQueryHandler 
    : IRequestHandler<GetAvailableFieldCollectorsQuery, List<AvailableFieldCollectorDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAvailableFieldCollectorsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<List<AvailableFieldCollectorDto>> Handle(
        GetAvailableFieldCollectorsQuery request, 
        CancellationToken cancellationToken)
    {
        // Get all field collectors
        var fieldCollectors = await _unitOfWork.Users.GetFieldCollectorsAsync(
            activeOnly: true, 
            cancellationToken: cancellationToken);

        // Apply filters
        var filtered = fieldCollectors.AsEnumerable();

        // Filter by availability
        if (request.IsAvailable.HasValue)
        {
            filtered = filtered.Where(u => u.IsAvailable == request.IsAvailable.Value);
        }

        // Filter by team
        if (!string.IsNullOrWhiteSpace(request.TeamName))
        {
            filtered = filtered.Where(u => 
                u.TeamName != null && 
                u.TeamName.Contains(request.TeamName, StringComparison.OrdinalIgnoreCase));
        }

        // Filter by search term (name or device ID)
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchLower = request.SearchTerm.ToLowerInvariant();
            filtered = filtered.Where(u =>
                u.FullNameArabic.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                (u.FullNameEnglish?.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (u.AssignedTabletId?.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                u.Username.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase));
        }

        // Filter by tablet assignment
        if (request.HasAssignedTablet.HasValue)
        {
            if (request.HasAssignedTablet.Value)
            {
                filtered = filtered.Where(u => !string.IsNullOrEmpty(u.AssignedTabletId));
            }
            else
            {
                filtered = filtered.Where(u => string.IsNullOrEmpty(u.AssignedTabletId));
            }
        }

        var fieldCollectorsList = filtered.ToList();

        // Get workload for each collector
        var result = new List<AvailableFieldCollectorDto>();

        foreach (var collector in fieldCollectorsList)
        {
            var stats = await _unitOfWork.BuildingAssignments.GetFieldCollectorStatsAsync(
                collector.Id, cancellationToken);

            result.Add(new AvailableFieldCollectorDto
            {
                Id = collector.Id,
                Username = collector.Username,
                FullNameArabic = collector.FullNameArabic,
                FullNameEnglish = collector.FullNameEnglish,
                AssignedTabletId = collector.AssignedTabletId,
                TeamName = collector.TeamName,
                IsAvailable = collector.IsAvailable,
                ActiveAssignments = stats.ActiveAssignments,
                PendingTransferCount = stats.PendingTransfer,
                TotalPropertyUnitsAssigned = stats.TotalPropertyUnits
            });
        }

        // Sort by workload
        if (request.SortByWorkloadAscending)
        {
            result = result.OrderBy(c => c.ActiveAssignments).ToList();
        }
        else
        {
            result = result.OrderByDescending(c => c.ActiveAssignments).ToList();
        }

        return result;
    }
}
