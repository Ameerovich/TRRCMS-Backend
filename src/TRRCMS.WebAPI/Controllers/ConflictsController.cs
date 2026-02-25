using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.Conflicts.Commands.EscalateConflict;
using TRRCMS.Application.Conflicts.Commands.KeepSeparateConflict;
using TRRCMS.Application.Conflicts.Commands.MergeConflict;
using TRRCMS.Application.Conflicts.Commands.ResolveConflict;
using TRRCMS.Application.Conflicts.Dtos;
using TRRCMS.Application.Conflicts.Queries.GetConflictDetails;
using TRRCMS.Application.Conflicts.Queries.GetConflictQueue;
using TRRCMS.Application.Conflicts.Queries.GetConflictSummary;
using TRRCMS.Application.Conflicts.Queries.GetPersonDuplicates;
using TRRCMS.Application.Conflicts.Queries.GetPropertyDuplicates;

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

    // ====================================================================
    // QUEUE & DASHBOARD
    // ====================================================================

    /// <summary>
    /// List the conflict resolution queue with filtering, sorting, and pagination.
    /// Returns all conflict types (person, property, claim).
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

    // ====================================================================
    // UC-007: PROPERTY DUPLICATE REVIEW QUEUE
    // ====================================================================

    /// <summary>
    /// List the property duplicate review queue with filtering and pagination.
    /// UC-007 S01–S02: Returns building/unit records flagged as potential duplicates
    /// based on building_id or composite key (building_id + unit_code) matches.
    /// </summary>
    /// <response code="200">Paginated property duplicate queue.</response>
    [HttpGet("property-duplicates")]
    [ProducesResponseType(typeof(GetConflictQueueResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<GetConflictQueueResponse>> GetPropertyDuplicates(
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
        var query = new GetPropertyDuplicatesQuery
        {
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

    // ====================================================================
    // UC-008: PERSON DUPLICATE REVIEW QUEUE
    // ====================================================================

    /// <summary>
    /// List the person duplicate review queue with filtering and pagination.
    /// UC-008 S01–S02: Returns person records sharing the same national_id value,
    /// including within-batch duplicates.
    /// </summary>
    /// <response code="200">Paginated person duplicate queue.</response>
    [HttpGet("person-duplicates")]
    [ProducesResponseType(typeof(GetConflictQueueResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<GetConflictQueueResponse>> GetPersonDuplicates(
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
        var query = new GetPersonDuplicatesQuery
        {
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

    // ====================================================================
    // CONFLICT DETAILS (side-by-side review)
    // ====================================================================

    /// <summary>
    /// Get full conflict details for side-by-side review.
    /// UC-007 S03–S04: Property detail comparison with codes, location, geometry.
    /// UC-008 S03–S04: Person detail comparison with names, IDs, documents.
    /// </summary>
    /// <param name="id">ConflictResolution surrogate ID.</param>
    /// <response code="200">Conflict details returned.</response>
    /// <response code="404">Conflict not found.</response>
    [HttpGet("{id:guid}/details")]
    [ProducesResponseType(typeof(ConflictDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConflictDetailDto>> GetConflictDetails(Guid id)
    {
        var result = await _mediator.Send(new GetConflictDetailsQuery { Id = id });

        if (result is null)
            return NotFound($"Conflict with ID '{id}' was not found.");

        return Ok(result);
    }

    /// <summary>
    /// Get conflict details by ID (alternative route without /details suffix).
    /// </summary>
    /// <param name="id">ConflictResolution surrogate ID.</param>
    /// <response code="200">Conflict details returned.</response>
    /// <response code="404">Conflict not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ConflictDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConflictDetailDto>> GetConflictById(Guid id)
    {
        var result = await _mediator.Send(new GetConflictDetailsQuery { Id = id });

        if (result is null)
            return NotFound($"Conflict with ID '{id}' was not found.");

        return Ok(result);
    }

    // ====================================================================
    // MERGE (UC-007 S05a/S06, UC-008 S05a/S06)
    // ====================================================================

    /// <summary>
    /// Execute merge with selected master record.
    /// UC-007 S06: Apply Property Merge Rules — consolidate property records.
    /// UC-008 S06: Apply Person Merge Rules — consolidate person records.
    ///
    /// Invokes PersonMergeService or PropertyMergeService based on entity type.
    /// Propagates FK changes across related entities and builds audit trail.
    /// </summary>
    /// <param name="id">ConflictResolution surrogate ID.</param>
    /// <param name="command">Merge request with master entity selection and justification.</param>
    /// <response code="200">Merge completed successfully.</response>
    /// <response code="400">Invalid merge request or merge failed.</response>
    /// <response code="404">Conflict not found.</response>
    /// <response code="409">Conflict is not in PendingReview status.</response>
    [HttpPost("{id:guid}/merge")]
    [ProducesResponseType(typeof(ConflictDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ConflictDetailDto>> MergeConflict(
        Guid id, [FromBody] MergeConflictCommand command)
    {
        command.ConflictId = id;
        return Ok(await _mediator.Send(command));
    }

    // ====================================================================
    // KEEP SEPARATE (UC-007 S05b, UC-008 S05b)
    // ====================================================================

    /// <summary>
    /// Mark as reviewed, not duplicate — keep records separate.
    /// UC-007 S05(b)/S06: Records are distinct properties, not duplicates.
    /// UC-008 S05(b)/S06: Records are different individuals, not duplicates.
    ///
    /// Prevents the same group from being re-surfaced unless detection rules change.
    /// </summary>
    /// <param name="id">ConflictResolution surrogate ID.</param>
    /// <param name="command">Keep-separate request with justification.</param>
    /// <response code="200">Records marked as separate.</response>
    /// <response code="400">Invalid request.</response>
    /// <response code="404">Conflict not found.</response>
    /// <response code="409">Conflict is not in PendingReview status.</response>
    [HttpPost("{id:guid}/keep-separate")]
    [ProducesResponseType(typeof(ConflictDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ConflictDetailDto>> KeepSeparateConflict(
        Guid id, [FromBody] KeepSeparateConflictCommand command)
    {
        command.ConflictId = id;
        return Ok(await _mediator.Send(command));
    }

    // ====================================================================
    // GENERAL RESOLUTION (existing — kept for backward compatibility)
    // ====================================================================

    /// <summary>
    /// Resolve a conflict with any action (merge, keep-both, keep-first, keep-second, ignore).
    /// This is the general-purpose resolution endpoint.
    /// For UC-007/UC-008, prefer the dedicated /merge and /keep-separate endpoints.
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

    // ====================================================================
    // ESCALATION (UC-007 S05a, UC-008 S05a)
    // ====================================================================

    /// <summary>
    /// Escalate a conflict to senior/supervisor review.
    /// UC-007 S05a: Complex property case requiring investigation.
    /// UC-008 S05a: Complex person case requiring investigation (partial IDs, conflicting docs).
    ///
    /// Sets IsEscalated=true and Priority=High.
    /// Creates escalation record and adds to senior review queue.
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
