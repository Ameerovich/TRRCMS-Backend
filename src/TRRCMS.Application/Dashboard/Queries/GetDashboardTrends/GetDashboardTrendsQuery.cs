using MediatR;
using TRRCMS.Application.Dashboard.Dtos;

namespace TRRCMS.Application.Dashboard.Queries.GetDashboardTrends;

/// <summary>
/// Query to retrieve monthly time-series trends for dashboard.
/// </summary>
public sealed record GetDashboardTrendsQuery(
    DateTime? From = null,
    DateTime? To = null
) : IRequest<DashboardTrendsDto>;
