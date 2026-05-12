using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.Audit.Dtos;
using TRRCMS.Application.Audit.Queries.GetAuditLogs;
using TRRCMS.Application.Audit.Queries.GetEntityAuditHistory;
using TRRCMS.Domain.Enums;

namespace TRRCMS.WebAPI.Controllers;

/// <summary>
/// Audit log API — exposes the system audit trail for dashboard reporting and compliance.
///
/// Endpoints:
///   GET /api/v1/audit              — global feed with date/user/entity/action filters + pagination
///   GET /api/v1/audit/security     — security-sensitive events only (logins, password changes, etc.)
///   GET /api/v1/audit/entity/{type}/{id} — full history for a specific entity
///
/// All endpoints require Administrator or DataManager role.
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
    /// <param name="toDate">Latest timestamp (UTC). Optional.</param>
    /// <param name="userId">Filter by the user who performed the action. Optional.</param>
    /// <param name="entityType">Filter by entity type (e.g. "Building", "ImportPackage"). Optional.</param>
    /// <param name="actionType">Filter by action type enum value. Optional.</param>
    /// <param name="pageNumber">Page number (1-based, default 1).</param>
    /// <param name="pageSize">Records per page (1–200, default 50).</param>
    [HttpGet]
    [ProducesResponseType(typeof(List<AuditLogDetailDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] Guid? userId,
        [FromQuery] string? entityType,
        [FromQuery] AuditActionType? actionType,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetAuditLogsQuery
        {
            FromDate = fromDate,
            ToDate = toDate,
            UserId = userId,
            EntityType = entityType,
            ActionType = actionType,
            PageNumber = pageNumber,
            PageSize = pageSize
        }, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Get security-sensitive audit events only (logins, failed logins, password changes,
    /// permission grants/revocations, etc.).
    /// </summary>
    /// <param name="fromDate">Earliest timestamp (UTC). Optional.</param>
    /// <param name="toDate">Latest timestamp (UTC). Optional.</param>
    /// <param name="userId">Filter by user. Optional.</param>
    /// <param name="pageNumber">Page number (default 1).</param>
    /// <param name="pageSize">Records per page (1–200, default 50).</param>
    [HttpGet("security")]
    [ProducesResponseType(typeof(List<AuditLogDetailDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSecurityAuditLogs(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] Guid? userId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetAuditLogsQuery
        {
            FromDate = fromDate,
            ToDate = toDate,
            UserId = userId,
            SecurityOnly = true,
            PageNumber = pageNumber,
            PageSize = pageSize
        }, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Get the complete audit history for a specific entity.
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
}
