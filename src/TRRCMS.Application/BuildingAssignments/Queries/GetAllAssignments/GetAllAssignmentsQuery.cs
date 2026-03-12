using MediatR;
using TRRCMS.Application.BuildingAssignments.Dtos;
using TRRCMS.Application.Common.Models;

namespace TRRCMS.Application.BuildingAssignments.Queries.GetAllAssignments;

/// <summary>
/// Get all building assignments with optional filtering and pagination.
/// عرض جميع تعيينات المباني مع التصفية والتقسيم إلى صفحات
/// </summary>
public class GetAllAssignmentsQuery : PagedQuery, IRequest<PagedResult<BuildingAssignmentSummaryDto>>
{
    /// <summary>Filter by field collector ID</summary>
    public Guid? FieldCollectorId { get; init; }

    /// <summary>Filter by building ID</summary>
    public Guid? BuildingId { get; init; }

    /// <summary>
    /// Filter by transfer status (int).
    /// Pending=1, InProgress=2, Transferred=3, Failed=4, Cancelled=5, PartialTransfer=6, Synchronized=7
    /// </summary>
    public int? TransferStatus { get; init; }

    /// <summary>Filter by active status</summary>
    public bool? IsActive { get; init; }

    /// <summary>Filter by revisit flag</summary>
    public bool? IsRevisit { get; init; }

    /// <summary>Filter assignments created on or after this date</summary>
    public DateTime? AssignedFromDate { get; init; }

    /// <summary>Filter assignments created on or before this date</summary>
    public DateTime? AssignedToDate { get; init; }
}
