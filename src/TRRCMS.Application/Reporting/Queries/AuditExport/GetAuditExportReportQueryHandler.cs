using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Reporting.Dtos;

namespace TRRCMS.Application.Reporting.Queries.AuditExport;

/// <summary>
/// Builds the audit-export report. Caps at MaxRows (default 10k) to honor the
/// sync-only / small-report scope. Larger ranges should be split client-side.
/// </summary>
public sealed class GetAuditExportReportQueryHandler
    : IRequestHandler<GetAuditExportReportQuery, AuditExportReportDto>
{
    private readonly IAuditService _auditService;

    public GetAuditExportReportQueryHandler(IAuditService auditService)
    {
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
    }

    public async Task<AuditExportReportDto> Handle(
        GetAuditExportReportQuery request,
        CancellationToken cancellationToken)
    {
        var pageSize = Math.Clamp(request.MaxRows, 1, 10_000);

        var logs = await _auditService.GetAuditLogsAsync(
            fromDate: request.From,
            toDate: request.To,
            entityType: request.EntityType,
            pageNumber: 1,
            pageSize: pageSize,
            cancellationToken: cancellationToken);

        if (request.SecurityOnly)
            logs = logs.Where(l => l.IsSecuritySensitive).ToList();

        var rows = logs.Select(l => new AuditExportRow
        {
            AuditLogNumber = l.AuditLogNumber,
            Timestamp = l.Timestamp,
            ActionType = l.ActionType.ToString(),
            ActionDescription = l.ActionDescription,
            ActionResult = l.ActionResult,
            Username = l.Username,
            UserFullName = l.UserFullName,
            UserRole = l.UserRole,
            EntityType = l.EntityType,
            EntityIdentifier = l.EntityIdentifier,
            IpAddress = l.IpAddress,
            IsSecuritySensitive = l.IsSecuritySensitive
        }).ToList();

        return new AuditExportReportDto
        {
            FromUtc = request.From,
            ToUtc = request.To,
            EntityTypeFilter = request.EntityType,
            SecurityOnly = request.SecurityOnly,
            GeneratedAtUtc = DateTime.UtcNow,
            Entries = rows
        };
    }
}
