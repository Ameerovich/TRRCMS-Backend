using MediatR;
using TRRCMS.Application.Audit.Dtos;

namespace TRRCMS.Application.Audit.Queries.GetAuditStats;

public record GetAuditStatsQuery : IRequest<AuditStatsDto>
{
    /// <summary>Window start (UTC). Defaults to <c>ToDate − 7 days</c> when omitted.</summary>
    public DateTime? FromDate { get; init; }

    /// <summary>Window end (UTC). Defaults to "now" when omitted.</summary>
    public DateTime? ToDate { get; init; }

    /// <summary>How many top users to include. Clamped to 1–50, default 5.</summary>
    public int TopUsersLimit { get; init; } = 5;
}
