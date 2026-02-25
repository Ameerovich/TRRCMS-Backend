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
/// survey data with the TRRCMS server over a local Wi-Fi network (LAN) without internet connectivity.
/// </summary>
/// <remarks>
/// **Sync Protocol Overview:**
/// The LAN sync is a 4-step round-trip designed for offline-first mobile surveys:
/// <list type="number">
///   <item><b>Step 1 – Open Session</b>: POST /api/v1/sync/session — Initiate a sync session. Returns <c>SyncSessionId</c> for use in subsequent steps.</item>
///   <item><b>Step 2 – Upload Package</b>: POST /api/v1/sync/upload — Upload a <c>.uhc</c> survey package (SQLite database with collected survey data, buildings, persons, claims). Verifies checksum integrity.</item>
///   <item><b>Step 3 – Download Assignments</b>: GET /api/v1/sync/assignments — Retrieve new building assignments and complete vocabulary snapshots. Supports incremental sync via <c>modifiedSinceUtc</c>.</item>
///   <item><b>Step 4 – Acknowledge</b>: POST /api/v1/sync/assignments/ack — Confirm that assignments were successfully stored on the tablet. Updates session counters and assignment transfer status.</item>
/// </list>
///
/// <b>Authorization &amp; Permissions:</b>
/// All endpoints require the <c>CanSyncData</c> policy (System_Sync permission: 9010).
///
/// | Role | Can Sync? |
/// |------|-----------|
/// | Field Collector | ✓ Yes |
/// | Field Supervisor | ✓ Yes |
/// | Administrator | ✓ Yes |
/// | Data Manager | ✗ No |
/// | Office Clerk | ✗ No |
/// | Analyst | ✗ No |
///
/// <b>Session Lifecycle:</b>
/// <list type="bullet">
///   <item><c>InProgress (1)</c> — Active session, accepting uploads and downloads.</item>
///   <item><c>Completed (2)</c> — All packages processed successfully; session closed.</item>
///   <item><c>PartiallyCompleted (3)</c> — Some packages failed; session closed with warnings.</item>
///   <item><c>Failed (4)</c> — Critical error (e.g., checksum mismatch); session closed, no further uploads allowed.</item>
/// </list>
///
/// <b>Assignment Transfer Status:</b>
/// <list type="bullet">
///   <item><c>Pending (1)</c> — Assignment available for download, not yet transferred to tablet.</item>
///   <item><c>InProgress (2)</c> — Downloaded to tablet but not yet acknowledged.</item>
///   <item><c>Transferred (3)</c> — Tablet has acknowledged receipt; assignment is complete.</item>
///   <item><c>Failed (4)</c> — Transfer failed; tablet will attempt redownload on next sync.</item>
/// </list>
///
/// <b>Package Format:</b>
/// Packages are <c>.uhc</c> files (renamed SQLite3 databases) containing:
/// <list type="bullet">
///   <item>Manifest table: Metadata (schema version, app version, vocabulary versions)</item>
///   <item>8 data tables: surveys, buildings, property_units, persons, households, person_property_relations, claims, evidences</item>
///   <item>Attachments BLOB table: Documents and photos</item>
/// </list>
///
/// <b>FSD References:</b>
/// FR-D-1 through FR-D-6 (Tablet LAN Synchronisation Protocol)
///
/// <b>Use Cases:</b>
/// <list type="bullet">
///   <item>UC-003: Export Surveys (tablet exports to server)</item>
///   <item>UC-012: Assign Buildings to Field Collectors (server assigns to tablets)</item>
/// </list>
/// </remarks>
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
    /// Open a new synchronisation session for the authenticated field collector (Sync Protocol Step 1).
    /// Returns a <c>SyncSessionId</c> that must be passed to all subsequent sync endpoints.
    /// A session tracks the progress, assignments, and outcome of a single sync round-trip.
    /// </summary>
    /// <remarks>
    /// **Purpose:**
    /// Initiates a new sync session that will be used to track uploads, downloads, and acknowledgements
    /// for this specific tablet and collector. Each call creates a fresh session with status <c>InProgress</c>.
    ///
    /// **Required Permission:** System_Sync (9010) - CanSyncData policy
    ///
    /// **Example Request:**
    /// ```json
    /// {
    ///   "fieldCollectorId": "550e8400-e29b-41d4-a716-446655440000",
    ///   "deviceId": "TABLET-FIELD-001",
    ///   "serverIpAddress": "192.168.1.100"
    /// }
    /// ```
    ///
    /// **Example Response (201 Created):**
    /// ```json
    /// {
    ///   "id": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
    ///   "fieldCollectorId": "550e8400-e29b-41d4-a716-446655440000",
    ///   "deviceId": "TABLET-FIELD-001",
    ///   "serverIpAddress": "192.168.1.100",
    ///   "sessionStatus": 1,
    ///   "startedAtUtc": "2026-02-23T10:30:00Z",
    ///   "completedAtUtc": null,
    ///   "packagesUploaded": 0,
    ///   "packagesFailed": 0,
    ///   "assignmentsDownloaded": 0,
    ///   "assignmentsAcknowledged": 0,
    ///   "vocabularyVersionsSent": null,
    ///   "errorMessage": null
    /// }
    /// ```
    ///
    /// **Session Tracking:**
    /// The returned session is tracked on the server and used to:
    /// - Link uploads to this specific sync (Step 2)
    /// - Gate assignment downloads (Step 3)
    /// - Track acknowledgements (Step 4)
    /// - Audit and report sync outcomes
    ///
    /// **FSD:** FR-D-1 (Open Sync Session)
    /// </remarks>
    /// <param name="data">Sync session initialization data:
    ///   - <c>fieldCollectorId</c>: The user ID of the field collector (must match authenticated user).
    ///   - <c>deviceId</c>: Device identifier (e.g., tablet serial or name) for audit and assignment filtering.
    ///   - <c>serverIpAddress</c>: Server IP address visible to the tablet (for connectivity diagnostics).
    /// </param>
    /// <response code="201">Session successfully created. Body contains <see cref="SyncSessionDto"/> with the new <c>id</c> to use in Steps 2–4.</response>
    /// <response code="400">Validation error: missing or invalid field (e.g., empty deviceId).</response>
    /// <response code="401">No valid JWT Bearer token provided in the <c>Authorization</c> header.</response>
    /// <response code="403">Authenticated user lacks the <c>CanSyncData</c> permission (System_Sync). Verify user role is Field Collector, Supervisor, or Administrator.</response>
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
    /// Upload a <c>.uhc</c> survey package from the tablet (Sync Protocol Step 2).
    /// Verifies SHA-256 checksum integrity and stores the package in quarantine for import processing.
    /// The operation is idempotent — uploading the same package twice returns <c>accepted=true, isDuplicate=true</c>.
    /// </summary>
    /// <remarks>
    /// **Purpose:**
    /// Receives a <c>.uhc</c> file (SQLite database) collected by the tablet. The server validates:
    /// - File exists and has <c>.uhc</c> extension
    /// - Session is still <c>InProgress</c> and belongs to the authenticated user
    /// - SHA-256 checksum matches (integrity verification)
    /// - Package ID hasn't been received before (idempotency)
    ///
    /// On success, the package is stored to disk in the quarantine area and later ingested
    /// by the Import Pipeline for staging, validation, duplicate detection, and final commit.
    ///
    /// **Request Format:**
    /// <c>multipart/form-data</c> with the following parts:
    /// <list type="bullet">
    ///   <item><c>file</c> — binary <c>.uhc</c> package (renamed SQLite3 database, typically 50–500 MB)</item>
    ///   <item><c>SyncSessionId</c> — UUID from Step 1</item>
    ///   <item><c>PackageId</c> — UUID embedded in the .uhc manifest (for idempotency)</item>
    ///   <item><c>DeviceId</c> — tablet device identifier (e.g., "TABLET-FIELD-001")</item>
    ///   <item><c>CreatedUtc</c> — ISO-8601 UTC timestamp when the tablet created the package</item>
    ///   <item><c>SchemaVersion</c> — .uhc schema version (e.g., "1.0.0")</item>
    ///   <item><c>AppVersion</c> — tablet app version (e.g., "1.0.0")</item>
    ///   <item><c>Sha256Checksum</c> — lowercase hex SHA-256 of the entire .uhc file</item>
    ///   <item><c>VocabVersionsJson</c> — (optional) JSON mapping vocabulary names to versions</item>
    ///   <item><c>FormSchemaVersion</c> — (optional) survey form schema version</item>
    /// </list>
    ///
    /// **Example cURL:**
    /// ```bash
    /// curl -X POST http://server:5000/api/v1/sync/upload \
    ///   -H "Authorization: Bearer JWT_TOKEN" \
    ///   -F "file=@survey-2026-02-23.uhc;type=application/octet-stream" \
    ///   -F "SyncSessionId=f47ac10b-58cc-4372-a567-0e02b2c3d479" \
    ///   -F "PackageId=aaaa0001-0001-0001-0001-000000000001" \
    ///   -F "DeviceId=TABLET-FIELD-001" \
    ///   -F "CreatedUtc=2026-02-23T09:45:00Z" \
    ///   -F "SchemaVersion=1.0.0" \
    ///   -F "AppVersion=1.0.0" \
    ///   -F "Sha256Checksum=34d9f42c1305d4c85ceaa4b4f2a2dc057e72ae061cdeaa1417052aab334003f1"
    /// ```
    ///
    /// **Success Response (200 OK):**
    /// ```json
    /// {
    ///   "accepted": true,
    ///   "packageId": "aaaa0001-0001-0001-0001-000000000001",
    ///   "isDuplicate": false,
    ///   "message": "Package received."
    /// }
    /// ```
    ///
    /// **Duplicate Response (200 OK, idempotent):**
    /// ```json
    /// {
    ///   "accepted": true,
    ///   "packageId": "aaaa0001-0001-0001-0001-000000000001",
    ///   "isDuplicate": true,
    ///   "message": "Duplicate package (already received)."
    /// }
    /// ```
    ///
    /// **Checksum Mismatch (400 Bad Request):**
    /// ```json
    /// {
    ///   "accepted": false,
    ///   "packageId": "aaaa0001-0001-0001-0001-000000000001",
    ///   "isDuplicate": false,
    ///   "message": "Checksum mismatch. Expected: 34d9f42c..., Actual: 1a71bb2e..."
    /// }
    /// ```
    ///
    /// **Checksum Computation:**
    /// 1. Tablet: SHA-256 of entire .uhc file on device
    /// 2. Server: SHA-256 of received stream; must match the form field value exactly
    /// 3. If mismatch, session is marked Failed and no further uploads accepted
    ///
    /// **Required Permission:** System_Sync (9010) - CanSyncData policy
    ///
    /// **FSD:** FR-D-2 (Upload Package), FR-D-3 (Package Integrity)
    /// </remarks>
    /// <param name="file">The <c>.uhc</c> binary package file (multipart, renamed SQLite database). Must have <c>.uhc</c> extension and typically be 50–500 MB.</param>
    /// <param name="manifest">Package manifest metadata as form fields, including session ID, package ID, device ID, timestamps, and SHA-256 checksum.</param>
    /// <response code="200">Package accepted (may be new or duplicate). Body contains <see cref="UploadSyncPackageResultDto"/> with <c>accepted=true</c>.</response>
    /// <response code="400">Validation error: missing file, wrong file extension, invalid form fields, or checksum mismatch. Session is marked Failed on checksum error.</response>
    /// <response code="401">No valid JWT Bearer token provided.</response>
    /// <response code="403">Authenticated user lacks the <c>CanSyncData</c> permission.</response>
    /// <response code="409">Session is no longer active (already completed, failed, or belongs to a different user).</response>
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
    /// Download building assignments and complete vocabulary snapshot for offline tablet operation (Sync Protocol Step 3).
    /// Returns all active assignments with transfer status <c>Pending</c> or <c>Failed</c>,
    /// plus a full snapshot of all controlled vocabularies for the offline app.
    /// </summary>
    /// <remarks>
    /// **Purpose:**
    /// After uploading survey data (Step 2), the tablet downloads new building assignments and
    /// reference vocabularies. The tablet needs the full vocabulary set to:
    /// - Validate enum values entered by field staff (e.g., building types, claim types)
    /// - Populate dropdown lists for offline form entry
    /// - Support bilingual labels (Arabic + English)
    ///
    /// **Assignments Returned:**
    /// All <c>BuildingAssignment</c> records where:
    /// - <c>FieldCollectorId</c> matches the authenticated user
    /// - <c>TransferStatus</c> is <c>Pending (1)</c> or <c>Failed (4)</c>
    /// - Assignment was created/modified after <c>modifiedSinceUtc</c> (if provided)
    ///
    /// **Incremental Sync:**
    /// Omit <c>modifiedSinceUtc</c> for a full download (first sync or full refresh).
    /// On subsequent syncs, pass the <c>payloadAssembledAtUtc</c> from the previous response
    /// to fetch only newly assigned buildings, reducing bandwidth.
    ///
    /// **Vocabulary Content:**
    /// A single <c>vocabularies</c> array containing all semantic versions of all controlled vocabularies:
    /// - Building types, status, damage levels
    /// - Property unit types, occupancy types
    /// - Claim types, ownership types
    /// - Evidence types, document types
    /// - And others as defined in <c>Permission</c> and <c>VocabularySeedData</c>
    ///
    /// Each vocabulary entry includes bilingual labels:
    /// ```json
    /// {
    ///   "name": "building_type",
    ///   "version": "1.0.0",
    ///   "values": [
    ///     { "code": 1, "labelArabic": "سكني", "labelEnglish": "Residential" },
    ///     { "code": 2, "labelArabic": "تجاري", "labelEnglish": "Commercial" }
    ///   ]
    /// }
    /// ```
    ///
    /// **Example Request:**
    /// ```
    /// GET /api/v1/sync/assignments?sessionId=f47ac10b-58cc-4372-a567-0e02b2c3d479&amp;modifiedSinceUtc=2026-02-22T10:00:00Z
    /// ```
    ///
    /// **Example Response (200 OK):**
    /// ```json
    /// {
    ///   "syncSessionId": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
    ///   "fieldCollectorId": "550e8400-e29b-41d4-a716-446655440000",
    ///   "payloadAssembledAtUtc": "2026-02-23T10:30:45Z",
    ///   "assignments": [
    ///     {
    ///       "assignmentId": "asgn-0001",
    ///       "buildingId": "bbbb0001-0001-0001-0001-000000000001",
    ///       "buildingCode": "14-14-01-010-011-00001",
    ///       "transferStatus": 1,
    ///       "assignedAtUtc": "2026-02-23T09:00:00Z"
    ///     }
    ///   ],
    ///   "assignmentCount": 1,
    ///   "vocabularies": [ ... ]
    /// }
    /// ```
    ///
    /// **Required Permission:** System_Sync (9010) - CanSyncData policy
    ///
    /// **FSD:** FR-D-5 (Download Assignments), FR-D-4 (Vocabulary Distribution)
    /// </remarks>
    /// <param name="sessionId">The active sync session ID returned by Step 1. Required.</param>
    /// <param name="modifiedSinceUtc">
    /// Optional ISO-8601 UTC timestamp for incremental sync.
    /// When provided, only assignments created or modified after this time are returned.
    /// Omit (or pass <c>null</c>) for a full download of all pending assignments.
    /// Reduce bandwidth on subsequent syncs by passing the <c>payloadAssembledAtUtc</c> from the previous response.
    /// Example: <c>2026-02-22T10:30:00Z</c>
    /// </param>
    /// <response code="200">Payload successfully assembled. Body contains <see cref="SyncAssignmentPayloadDto"/> with assignments and vocabularies.</response>
    /// <response code="400">Invalid <paramref name="sessionId"/> (empty or not a valid UUID).</response>
    /// <response code="401">No valid JWT Bearer token provided.</response>
    /// <response code="403">Authenticated user lacks the <c>CanSyncData</c> permission.</response>
    /// <response code="404">Sync session not found or does not belong to the authenticated user.</response>
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
    /// Acknowledge that the tablet has successfully stored the downloaded assignments (Sync Protocol Step 4).
    /// Transitions each acknowledged assignment from <c>Pending/Failed</c> to <c>Transferred</c> status.
    /// The operation is idempotent — acknowledging an already-transferred assignment is safe and does not error.
    /// </summary>
    /// <remarks>
    /// **Purpose:**
    /// After the tablet downloads assignments in Step 3, it stores them locally in its database.
    /// This endpoint confirms that storage succeeded and updates the server-side transfer status
    /// to <c>Transferred</c>, completing the assignment cycle.
    ///
    /// **Idempotent Design:**
    /// If the tablet calls this endpoint twice with the same assignment IDs, the second call
    /// is a no-op — assignments already marked as <c>Transferred</c> are not modified again.
    /// This allows tablets to safely retry the acknowledgement without causing duplicates.
    ///
    /// **Batch Acknowledgement:**
    /// The tablet sends a list of assignment IDs in a single request. The server processes each:
    /// - If found and belongs to this collector: transitions to <c>Transferred</c>
    /// - If not found or belongs to a different collector: marked as failed
    /// - If already <c>Transferred</c>: skipped (idempotency)
    ///
    /// **Example Request:**
    /// ```json
    /// {
    ///   "syncSessionId": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
    ///   "assignmentIds": [
    ///     "asgn-0001",
    ///     "asgn-0002",
    ///     "asgn-0003"
    ///   ]
    /// }
    /// ```
    ///
    /// **Success Response (200 OK):**
    /// ```json
    /// {
    ///   "acknowledgedCount": 3,
    ///   "failedCount": 0,
    ///   "failedAssignmentIds": [],
    ///   "message": "3 assignments acknowledged. Sync session completed successfully."
    /// }
    /// ```
    ///
    /// **Partial Success Response (200 OK):**
    /// ```json
    /// {
    ///   "acknowledgedCount": 2,
    ///   "failedCount": 1,
    ///   "failedAssignmentIds": [ "asgn-0003" ],
    ///   "message": "2 assignments acknowledged, 1 failed."
    /// }
    /// ```
    ///
    /// **Session Update:**
    /// Upon successful acknowledgement, the session status may transition:
    /// - If all packages uploaded and acknowledged: <c>Completed</c>
    /// - If any package failed during upload: <c>PartiallyCompleted</c>
    ///
    /// **Required Permission:** System_Sync (9010) - CanSyncData policy
    ///
    /// **FSD:** FR-D-6 (Transfer Acknowledgement)
    /// </remarks>
    /// <param name="request">
    /// Command containing:
    ///   - <c>SyncSessionId</c>: UUID from Step 1
    ///   - <c>AssignmentIds</c>: List of assignment IDs to acknowledge (received in Step 3 response as <c>assignmentId</c> field).
    ///     Must contain at least one ID. Empty list will be rejected with HTTP 400.
    /// </param>
    /// <response code="200">Acknowledgements processed (partially or fully successful). Body contains <see cref="SyncAckResultDto"/> with counts and any failed IDs.</response>
    /// <response code="400">Validation error: missing session ID, empty assignment list, or invalid UUID format.</response>
    /// <response code="401">No valid JWT Bearer token provided.</response>
    /// <response code="403">Authenticated user lacks the <c>CanSyncData</c> permission or session does not belong to the user.</response>
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
