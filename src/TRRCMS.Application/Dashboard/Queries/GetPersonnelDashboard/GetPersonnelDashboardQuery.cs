using MediatR;
using TRRCMS.Application.Dashboard.Dtos;

namespace TRRCMS.Application.Dashboard.Queries.GetPersonnelDashboard;

/// <summary>
/// Query to retrieve personnel workload dashboard data.
/// </summary>
public sealed record GetPersonnelDashboardQuery(
    DateTime? From = null,
    DateTime? To = null
) : IRequest<PersonnelDashboardDto>;
