using MediatR;
using TRRCMS.Application.Dashboard.Dtos;

namespace TRRCMS.Application.Dashboard.Queries.GetDashboardSummary;

/// <summary>
/// Query to retrieve aggregated dashboard statistics.
/// </summary>
public sealed record GetDashboardSummaryQuery : IRequest<DashboardSummaryDto>;
