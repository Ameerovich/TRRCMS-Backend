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
using TRRCMS.Application.Conflicts.Queries.GetConflictDocumentComparison;
using TRRCMS.Application.Conflicts.Queries.GetEscalatedConflicts;
using TRRCMS.Application.Conflicts.Queries.GetPersonDuplicates;
using TRRCMS.Application.Conflicts.Queries.GetPropertyDuplicates;

namespace TRRCMS.WebAPI.Controllers;

/// <summary>
/// Conflict resolution API controller.
/// Provides endpoints for viewing, resolving, and escalating duplicate conflicts
/// detected during the import pipeline.
///
/// All endpoints require the CanImportData policy (System_Import permission).
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
    // PROPERTY DUPLICATE REVIEW QUEUE
    // ====================================================================

    /// <summary>
    /// List the property duplicate review queue with filtering and pagination.
    /// Returns building/unit records flagged as potential duplicates
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
    // PERSON DUPLICATE REVIEW QUEUE
    // ====================================================================

    /// <summary>
    /// List the person duplicate review queue with filtering and pagination.
    /// Returns person records sharing the same national_id value,
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
    // ESCALATED — SENIOR REVIEW QUEUE
    // ====================================================================

    /// <summary>
    /// List the senior review queue — only escalated conflicts still pending resolution.
    /// Complex cases escalated to supervisor for investigation.
    /// Default sort: EscalatedDate descending (most recent escalations first).
    /// </summary>
    /// <response code="200">Paginated escalated conflict queue.</response>
    [HttpGet("escalated")]
    [ProducesResponseType(typeof(GetConflictQueueResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<GetConflictQueueResponse>> GetEscalatedConflicts(
        [FromQuery] string? conflictType = null,
        [FromQuery] string? priority = null,
        [FromQuery] Guid? importPackageId = null,
        [FromQuery] bool? isOverdue = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = true)
    {
        var query = new GetEscalatedConflictsQuery
        {
            ConflictType = conflictType,
            Priority = priority,
            ImportPackageId = importPackageId,
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
    /// Property detail comparison with codes, location, geometry.
    /// Person detail comparison with names, IDs, documents.
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
    // DOCUMENT COMPARISON
    // ====================================================================

    /// <summary>
    /// Get side-by-side document/evidence comparison for a conflict pair.
    /// Loads all evidence files and document records for both entities
    /// to enable the document viewer in the duplicate review screen.
    /// Also supports property unit document comparison.
    /// </summary>
    /// <param name="id">ConflictResolution surrogate ID.</param>
    /// <response code="200">Document comparison data returned.</response>
    /// <response code="404">Conflict not found.</response>
    [HttpGet("{id:guid}/document-comparison")]
    [ProducesResponseType(typeof(DocumentComparisonDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DocumentComparisonDto>> GetConflictDocumentComparison(Guid id)
    {
        var result = await _mediator.Send(new GetConflictDocumentComparisonQuery(id));

        if (result is null)
            return NotFound($"Conflict with ID '{id}' was not found.");

        return Ok(result);
    }

    // ====================================================================
    // MERGE
    // ====================================================================

    /// <summary>
    /// Execute merge with selected master record.
    /// Applies property merge rules to consolidate property records,
    /// or person merge rules to consolidate person records.
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
    // KEEP SEPARATE
    // ====================================================================

    /// <summary>
    /// Mark as reviewed, not duplicate — keep records separate.
    /// Records are distinct properties or different individuals, not duplicates.
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
    // GENERAL RESOLUTION (existing � kept for backward compatibility)
    // ====================================================================

    /// <summary>
    /// Resolve a conflict with any action (merge, keep-both, keep-first, keep-second, ignore).
    /// This is the general-purpose resolution endpoint.
    /// Prefer the dedicated /merge and /keep-separate endpoints for duplicate resolution.
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
    // ESCALATION
    // ====================================================================

    /// <summary>
    /// Escalate a conflict to senior/supervisor review.
    /// Complex property or person case requiring investigation (partial IDs, conflicting docs).
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
