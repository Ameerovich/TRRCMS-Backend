using MediatR;
using TRRCMS.Application.Reporting.Dtos;

namespace TRRCMS.Application.Reporting.Queries.SurveyActivity;

public sealed record GetSurveyActivityReportQuery(
    DateTime? From = null,
    DateTime? To = null
) : IRequest<SurveyActivityReportDto>;
