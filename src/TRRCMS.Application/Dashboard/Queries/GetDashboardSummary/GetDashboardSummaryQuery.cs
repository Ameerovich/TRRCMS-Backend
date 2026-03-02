using MediatR;
using TRRCMS.Application.Dashboard.Dtos;

namespace TRRCMS.Application.Dashboard.Queries.GetDashboardSummary;

/// <summary>
/// Query to retrieve aggregated dashboard statistics.
/// FR-D-12: Desktop dashboard summary data.
/// </summary>
public sealed record GetDashboardSummaryQuery : IRequest<DashboardSummaryDto>;
