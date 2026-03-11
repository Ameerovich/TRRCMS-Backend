using MediatR;
using TRRCMS.Application.Dashboard.Dtos;

namespace TRRCMS.Application.Dashboard.Queries.GetGeographicDashboard;

/// <summary>
/// Query to retrieve geographic coverage dashboard data.
/// </summary>
public sealed record GetGeographicDashboardQuery : IRequest<GeographicDashboardDto>;
