using MediatR;
using TRRCMS.Application.Reporting.Dtos;

namespace TRRCMS.Application.Reporting.Queries.ImportPipeline;

public sealed record GetImportPipelineReportQuery(
    DateTime? From = null,
    DateTime? To = null
) : IRequest<ImportPipelineReportDto>;
