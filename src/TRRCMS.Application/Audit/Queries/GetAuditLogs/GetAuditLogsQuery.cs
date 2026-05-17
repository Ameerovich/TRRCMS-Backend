using MediatR;
using TRRCMS.Application.Audit.Dtos;
using TRRCMS.Application.Common.Models;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Audit.Queries.GetAuditLogs;

public record GetAuditLogsQuery : IRequest<PagedResult<AuditLogDetailDto>>
{
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public Guid? UserId { get; init; }
    public string? UsernameContains { get; init; }
    public string? EntityType { get; init; }

    /// <summary>
    /// Optional list of action types (multi-select). When omitted, all action
    /// types match. Combined with <see cref="SecurityOnly"/> via AND.
    /// </summary>
    public IReadOnlyCollection<AuditActionType>? ActionTypes { get; init; }

    /// <summary>"Success" / "Failed" / "Partial". Omit to match all.</summary>
    public string? ActionResult { get; init; }

    /// <summary>Filter by the entity-stored security-sensitive flag.</summary>
    public bool? IsSecuritySensitive { get; init; }

    /// <summary>
    /// Convenience flag wired by <c>/audit/security</c>. Equivalent to
    /// <c>IsSecuritySensitive = true</c>.
    /// </summary>
    public bool? SecurityOnly { get; init; }

    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}
