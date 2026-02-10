using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.Conflicts.Commands.EscalateConflict;
using TRRCMS.Application.Conflicts.Commands.ResolveConflict;
using TRRCMS.Application.Conflicts.Dtos;
using TRRCMS.Application.Conflicts.Queries.GetConflictDetails;
using TRRCMS.Application.Conflicts.Queries.GetConflictQueue;
using TRRCMS.Application.Conflicts.Queries.GetConflictSummary;

namespace TRRCMS.WebAPI.Controllers;

/// <summary>
/// Conflict resolution API controller.
/// Provides endpoints for viewing, resolving, and escalating duplicate conflicts
/// detected during the import pipeline.
///
/// All endpoints require the CanImportData policy (System_Import permission).
///
/// UC-007 (Resolve Duplicate Properties), UC-008 (Resolve Duplicate Persons).
/// FSD: FR-D-5, FR-D-6, FR-D-7.
/// </summary>
[ApiController]
[Route("api/v1/conflicts")]
[Authorize(Policy = "CanImportData")]
public class ConflictsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ConflictsController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    // ==================== QUEUE & DASHBOARD ====================

    /// <summary>
    /// List the conflict resolution queue with filtering, sorting, and pagination.
    /// </summary>
    /// <response code="200">Paginated conflict queue.</response>
    [HttpGet]
    [ProducesResponseType(typeof(GetConflictQueueResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<GetConflictQueueResponse>> GetConflictQueue(
        [FromQuery] string? conflictType = null,
        [FromQuery] string? status = null,
        [FromQuery] string? priority = null,
        [FromQuery] Guid? importPackageId = null,
        [FromQuery] Guid? assignedToUserId = null,
        [FromQuery] bool? isEscalated = null,
        [FromQuery] bool? isOverdue = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = true)
    {
        var query = new GetConflictQueueQuery
        {
            ConflictType = conflictType,
            Status = status,
            Priority = priority,
            ImportPackageId = importPackageId,
            AssignedToUserId = assignedToUserId,
            IsEscalated = isEscalated,
            IsOverdue = isOverdue,
            Page = page,
            PageSize = pageSize,
            SortBy = sortBy,
            SortDescending = sortDescending
        };

        return Ok(await _mediator.Send(query));
    }

    /// <summary>
    /// Get aggregate conflict counts for the dashboard.
    /// </summary>
    /// <response code="200">Conflict summary counts.</response>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(ConflictSummaryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ConflictSummaryDto>> GetConflictSummary()
    {
        return Ok(await _mediator.Send(new GetConflictSummaryQuery()));
    }

    // ==================== CONFLICT DETAILS ====================

    /// <summary>
    /// Get full conflict details for side-by-side review.
    /// </summary>
    /// <param name="id">ConflictResolution surrogate ID.</param>
    /// <response code="200">Conflict details returned.</response>
    /// <response code="404">Conflict not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ConflictDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConflictDetailDto>> GetConflictDetails(Guid id)
    {
        var result = await _mediator.Send(new GetConflictDetailsQuery { Id = id });

        if (result is null)
            return NotFound($"Conflict with ID '{id}' was not found.");

        return Ok(result);
    }

    // ==================== RESOLUTION ====================

    /// <summary>
    /// Resolve a conflict (merge, keep-both, keep-first, keep-second, ignore).
    /// </summary>
    /// <param name="id">ConflictResolution surrogate ID.</param>
    /// <param name="command">Resolution action and mandatory reason.</param>
    /// <response code="200">Conflict resolved.</response>
    /// <response code="400">Invalid resolution action.</response>
    /// <response code="404">Conflict not found.</response>
    [HttpPost("{id:guid}/resolve")]
    [ProducesResponseType(typeof(ConflictDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConflictDetailDto>> ResolveConflict(
        Guid id, [FromBody] ResolveConflictCommand command)
    {
        command.ConflictId = id;
        return Ok(await _mediator.Send(command));
    }

    /// <summary>
    /// Escalate a conflict to senior/supervisor review.
    /// </summary>
    /// <param name="id">ConflictResolution surrogate ID.</param>
    /// <param name="command">Escalation reason (mandatory).</param>
    /// <response code="200">Conflict escalated.</response>
    /// <response code="400">Missing escalation reason.</response>
    /// <response code="404">Conflict not found.</response>
    [HttpPost("{id:guid}/escalate")]
    [ProducesResponseType(typeof(ConflictDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConflictDetailDto>> EscalateConflict(
        Guid id, [FromBody] EscalateConflictCommand command)
    {
        command.ConflictId = id;
        return Ok(await _mediator.Send(command));
    }
}
