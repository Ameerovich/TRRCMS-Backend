using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.Audit.Dtos;
using TRRCMS.Application.Audit.Queries.GetAuditLogs;
using TRRCMS.Application.Audit.Queries.GetAuditStats;
using TRRCMS.Application.Audit.Queries.GetEntityAuditHistory;
using TRRCMS.Application.Common.Models;
using TRRCMS.Domain.Enums;

namespace TRRCMS.WebAPI.Controllers;

/// <summary>
/// Audit log API — exposes the system audit trail for dashboard reporting and compliance.
///
/// Endpoints:
///   GET /api/v1/audit              — global feed with date/user/entity/action filters + pagination
///   GET /api/v1/audit/security     — security-sensitive events only (logins, password changes, etc.)
///   GET /api/v1/audit/stats        — aggregated counts over a window (overview cards / charts)
///   GET /api/v1/audit/entity/{type}/{id} — full history for a specific entity
///
/// All endpoints require Administrator or DataManager role.
///
/// Wire contract notes:
///   - List endpoints return <see cref="PagedResult{T}"/> with items + totalCount + paging metadata.
///   - <c>actionType</c> integer values are part of the public contract (declared in
///     <see cref="AuditActionType"/>) and will not change. <c>actionTypeName</c> is the C# enum
///     name; frontend maps both to user-facing labels.
///   - Default sort: <c>timestamp DESC, auditLogNumber DESC</c> (deterministic tiebreaker).
///   - <c>changedFields</c> is a comma-separated legacy hint. Prefer the structured
///     <c>changes[]</c> array on each entry.
/// </summary>
[ApiController]
[Route("api/v1/audit")]
[Authorize(Roles = "Administrator,DataManager")]
public class AuditController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuditController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>
    /// Get the global audit log feed with optional filters.
    /// </summary>
    /// <param name="fromDate">Earliest timestamp (UTC). Optional.</param>
    /// <param name="toDate">Latest timestamp (UTC). Optional. Must be ≥ fromDate.</param>
    /// <param name="userId">Filter by the user who performed the action. Optional.</param>
    /// <param name="usernameContains">Case-insensitive substring match against username or full name. Optional.</param>
    /// <param name="entityType">Filter by entity type (e.g. "Building", "ImportPackage"). Optional.</param>
    /// <param name="actionType">One or more <see cref="AuditActionType"/> values. Comma-separated or repeated. Optional.</param>
    /// <param name="actionResult">"Success" / "Failed" / "Partial". Optional.</param>
    /// <param name="isSecuritySensitive">Filter by the security-sensitive flag. Optional.</param>
    /// <param name="pageNumber">Page number (1-based, default 1).</param>
    /// <param name="pageSize">Records per page (1–200, default 50).</param>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AuditLogDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] Guid? userId,
        [FromQuery] string? usernameContains,
        [FromQuery] string? entityType,
        [FromQuery(Name = "actionType")] string[]? actionType,
        [FromQuery] string? actionResult,
        [FromQuery] bool? isSecuritySensitive,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetAuditLogsQuery
        {
            FromDate = fromDate,
            ToDate = toDate,
            UserId = userId,
            UsernameContains = usernameContains,
            EntityType = entityType,
            ActionTypes = ParseActionTypes(actionType),
            ActionResult = actionResult,
            IsSecuritySensitive = isSecuritySensitive,
            PageNumber = pageNumber,
            PageSize = pageSize
        }, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Get security-sensitive audit events only (logins, failed logins, password changes,
    /// permission grants/revocations, etc.). Same filters as <c>/audit</c> minus the
    /// security-flag toggle (forced on).
    /// </summary>
    [HttpGet("security")]
    [ProducesResponseType(typeof(PagedResult<AuditLogDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSecurityAuditLogs(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] Guid? userId,
        [FromQuery] string? usernameContains,
        [FromQuery(Name = "actionType")] string[]? actionType,
        [FromQuery] string? actionResult,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetAuditLogsQuery
        {
            FromDate = fromDate,
            ToDate = toDate,
            UserId = userId,
            UsernameContains = usernameContains,
            ActionTypes = ParseActionTypes(actionType),
            ActionResult = actionResult,
            SecurityOnly = true,
            PageNumber = pageNumber,
            PageSize = pageSize
        }, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Aggregated counts over a window for the dashboard overview cards.
    /// Defaults to the last 7 days when <paramref name="fromDate"/>/<paramref name="toDate"/> are omitted.
    /// </summary>
    /// <param name="fromDate">Window start (UTC). Optional.</param>
    /// <param name="toDate">Window end (UTC). Optional.</param>
    /// <param name="topUsersLimit">How many top users to include (1–50, default 5).</param>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(AuditStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAuditStats(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int topUsersLimit = 5,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetAuditStatsQuery
        {
            FromDate = fromDate,
            ToDate = toDate,
            TopUsersLimit = topUsersLimit
        }, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Get the complete audit history for a specific entity, ordered by
    /// <c>timestamp DESC, auditLogNumber DESC</c>.
    /// </summary>
    /// <param name="type">Entity type name (e.g. "Building", "Claim", "ImportPackage").</param>
    /// <param name="id">Entity GUID.</param>
    [HttpGet("entity/{type}/{id:guid}")]
    [ProducesResponseType(typeof(List<AuditLogDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetEntityAuditHistory(
        string type,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetEntityAuditHistoryQuery
        {
            EntityType = type,
            EntityId = id
        }, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Accepts either repeated <c>?actionType=1&amp;actionType=2</c> or a single
    /// comma-separated <c>?actionType=1,2,3</c>. Unknown / unparseable values are skipped.
    /// </summary>
    private static IReadOnlyCollection<AuditActionType>? ParseActionTypes(string[]? raw)
    {
        if (raw == null || raw.Length == 0) return null;

        var parsed = new List<AuditActionType>();
        foreach (var token in raw.SelectMany(s => s?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? Array.Empty<string>()))
        {
            if (Enum.TryParse<AuditActionType>(token, ignoreCase: true, out var typed))
                parsed.Add(typed);
        }

        return parsed.Count == 0 ? null : parsed.Distinct().ToList();
    }
}
