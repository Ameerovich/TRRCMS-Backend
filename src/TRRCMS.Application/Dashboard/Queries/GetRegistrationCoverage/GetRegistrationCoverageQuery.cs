using MediatR;
using TRRCMS.Application.Dashboard.Dtos;

namespace TRRCMS.Application.Dashboard.Queries.GetRegistrationCoverage;

/// <summary>
/// Query to retrieve registration coverage dashboard data.
/// </summary>
public sealed record GetRegistrationCoverageQuery : IRequest<RegistrationCoverageDashboardDto>;
