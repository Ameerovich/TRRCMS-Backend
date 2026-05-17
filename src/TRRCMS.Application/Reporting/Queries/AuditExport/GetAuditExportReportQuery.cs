using MediatR;
using TRRCMS.Application.Reporting.Dtos;

namespace TRRCMS.Application.Reporting.Queries.AuditExport;

public sealed record GetAuditExportReportQuery(
    DateTime? From = null,
    DateTime? To = null,
    string? EntityType = null,
    bool SecurityOnly = false,
    int MaxRows = 10_000
) : IRequest<AuditExportReportDto>;
