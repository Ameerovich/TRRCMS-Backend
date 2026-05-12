using MediatR;
using TRRCMS.Application.Audit.Dtos;

namespace TRRCMS.Application.Audit.Queries.GetEntityAuditHistory;

public record GetEntityAuditHistoryQuery : IRequest<List<AuditLogDetailDto>>
{
    public string EntityType { get; init; } = string.Empty;
    public Guid EntityId { get; init; }
}
