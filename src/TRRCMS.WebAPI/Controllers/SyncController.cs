using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.Sync.Commands.AcknowledgeSyncAssignments;
using TRRCMS.Application.Sync.Commands.CreateSyncSession;
using TRRCMS.Application.Sync.Commands.UploadSyncPackage;
using TRRCMS.Application.Sync.DTOs;
using TRRCMS.Application.Sync.Queries.GetSyncAssignments;

namespace TRRCMS.WebAPI.Controllers;

/// <summary>
/// Tablet LAN Synchronisation API controller.
/// Implements the 4-step sync protocol used by Android field tablets to exchange
/// data with the TRRCMS server over a local Wi-Fi network (no internet required).
///
/// Sync Protocol (per TRRCMS_Vocabulary_Sync_Plan_v2):
/// <list type="number">
///   <item><b>Step 1 – Open Session</b>: POST /api/v1/sync/session</item>
///   <item><b>Step 2 – Upload Package</b>: POST /api/v1/sync/upload</item>
///   <item><b>Step 3 – Download Assignments</b>: GET /api/v1/sync/assignments</item>
///   <item><b>Step 4 – Acknowledge</b>: POST /api/v1/sync/assignments/ack</item>
/// </list>
///
/// All endpoints require the <c>CanSyncData</c> policy (System_Sync permission).
/// Field collectors, field supervisors, and administrators hold this permission
/// by default (PermissionSeeder).
///
/// FSD: FR-D-1 through FR-D-6 (Sync Protocol).
/// UC-003: Export Surveys; UC-012: Assign Buildings to Field Collectors.
/// </summary>
[ApiController]
[Route("api/v1/sync")]
[Authorize(Policy = "CanSyncData")]
public class SyncController : ControllerBase
{
    private readonly IMediator _mediator;

    public SyncController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    // ==================== STEP 1 — OPEN SESSION ====================

    /// <summary>
    /// Open a new synchronisation session for the authenticated field collector.
    /// The returned session ID is required by all subsequent sync endpoints.
    /// A session tracks the progress and outcome of a single sync round-trip.
    /// </summary>
    /// <param name="data">Session initialisation data (field collector ID, device ID, server IP).</param>
    /// <response code="201">Session created; body contains the <see cref="SyncSessionDto"/>.</response>
    /// <response code="400">Validation errors in the request body.</response>
    /// <response code="401">No valid JWT token provided.</response>
    /// <response code="403">Authenticated user lacks the <c>CanSyncData</c> permission.</response>
    [HttpPost("session")]
    [ProducesResponseType(typeof(SyncSessionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SyncSessionDto>> CreateSession(
        [FromBody] CreateSyncSessionDto data)
    {
        var result = await _mediator.Send(new CreateSyncSessionCommand(data));
        return CreatedAtAction(nameof(CreateSession), new { id = result.Id }, result);
    }

    // ==================== STEP 2 — UPLOAD SURVEY PACKAGE ====================

    /// <summary>
    /// Upload a <c>.uhc</c> survey package from the tablet.
    /// The file is verified (SHA-256 checksum) and stored in the quarantine area
    /// for subsequent import pipeline processing.  The operation is idempotent by
    /// package ID — uploading the same package twice returns <c>IsDuplicate = true</c>.
    /// </summary>
    /// <remarks>
    /// Request format: <c>multipart/form-data</c> with fields:
    /// <list type="bullet">
    ///   <item><c>file</c> — the <c>.uhc</c> binary.</item>
    ///   <item>All <see cref="UploadSyncPackageDto"/> fields as individual form fields.</item>
    /// </list>
    /// </remarks>
    /// <param name="file">The <c>.uhc</c> package file.</param>
    /// <param name="manifest">Package manifest metadata (session ID, checksums, versions).</param>
    /// <response code="200">Package accepted; body contains <see cref="UploadSyncPackageResultDto"/>.</response>
    /// <response code="400">Missing file, wrong extension, or validation errors.</response>
    /// <response code="409">Package already received (idempotency — duplicate upload).</response>
    [HttpPost("upload")]
    [ProducesResponseType(typeof(UploadSyncPackageResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [RequestSizeLimit(524_288_000)] // 500 MB — matches Kestrel / FormOptions limits
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<UploadSyncPackageResultDto>> UploadPackage(
        IFormFile file,
        [FromForm] UploadSyncPackageDto manifest)
    {
        if (file is null || file.Length == 0)
            return BadRequest("A .uhc package file is required.");

        if (!file.FileName.EndsWith(".uhc", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Only .uhc package files are accepted.");

        await using var stream = file.OpenReadStream();

        var result = await _mediator.Send(new UploadSyncPackageCommand(manifest, stream));

        if (!result.Accepted && result.IsDuplicate)
            return Conflict(result);

        return Ok(result);
    }

    // ==================== STEP 3 — DOWNLOAD ASSIGNMENTS ====================

    /// <summary>
    /// Download building assignments and vocabulary lists for the authenticated
    /// field collector.
    /// Returns all active assignments whose <c>TransferStatus</c> is
    /// <c>Pending</c> or <c>Failed</c>, together with a full snapshot of every
    /// controlled vocabulary so the tablet can operate offline.
    /// </summary>
    /// <param name="sessionId">Active sync session ID (from Step 1).</param>
    /// <param name="modifiedSinceUtc">
    /// Optional incremental-sync filter (ISO-8601 UTC).
    /// When provided, only assignments touched after this timestamp are returned,
    /// reducing bandwidth on subsequent syncs.  Omit for a full download.
    /// </param>
    /// <response code="200">Payload assembled; body contains <see cref="SyncAssignmentPayloadDto"/>.</response>
    /// <response code="400">Missing or invalid <paramref name="sessionId"/>.</response>
    /// <response code="403">Session does not belong to the current user.</response>
    /// <response code="404">Sync session not found.</response>
    [HttpGet("assignments")]
    [ProducesResponseType(typeof(SyncAssignmentPayloadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SyncAssignmentPayloadDto>> GetAssignments(
        [FromQuery] Guid sessionId,
        [FromQuery] DateTime? modifiedSinceUtc = null)
    {
        if (sessionId == Guid.Empty)
            return BadRequest("A valid sessionId is required.");

        var result = await _mediator.Send(
            new GetSyncAssignmentsQuery(sessionId, modifiedSinceUtc));

        return Ok(result);
    }

    // ==================== STEP 4 — ACKNOWLEDGE ASSIGNMENTS ====================

    /// <summary>
    /// Acknowledge that the tablet has successfully stored the downloaded assignments.
    /// The server transitions each acknowledged assignment's <c>TransferStatus</c>
    /// from <c>Pending/Failed</c> to <c>Transferred</c> and updates the session counters.
    /// The operation is idempotent — acknowledging an already-transferred assignment
    /// is safe and does not raise an error.
    /// </summary>
    /// <param name="request">
    /// Contains the session ID and the list of assignment IDs to acknowledge.
    /// </param>
    /// <response code="200">Acknowledgement processed; body contains <see cref="SyncAckResultDto"/>.</response>
    /// <response code="400">Validation errors (missing session ID or empty assignment list).</response>
    /// <response code="403">Session does not belong to the current user.</response>
    /// <response code="404">Sync session not found.</response>
    [HttpPost("assignments/ack")]
    [ProducesResponseType(typeof(SyncAckResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SyncAckResultDto>> AcknowledgeAssignments(
        [FromBody] AcknowledgeSyncAssignmentsCommand request)
    {
        var result = await _mediator.Send(request);
        return Ok(result);
    }
}
