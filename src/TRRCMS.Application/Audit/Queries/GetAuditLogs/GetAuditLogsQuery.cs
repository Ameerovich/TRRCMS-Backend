using MediatR;
using TRRCMS.Application.Audit.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Audit.Queries.GetAuditLogs;

public record GetAuditLogsQuery : IRequest<List<AuditLogDetailDto>>
{
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public Guid? UserId { get; init; }
    public string? EntityType { get; init; }
    public AuditActionType? ActionType { get; init; }
    public bool? SecurityOnly { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}
