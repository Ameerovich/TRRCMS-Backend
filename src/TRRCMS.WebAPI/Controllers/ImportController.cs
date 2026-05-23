using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.Import.Commands.ApproveForCommit;
using TRRCMS.Application.Import.Commands.CancelPackage;
using TRRCMS.Application.Import.Commands.CommitPackage;
using TRRCMS.Application.Import.Commands.DetectDuplicates;
using TRRCMS.Application.Import.Commands.QuarantinePackage;
using TRRCMS.Application.Import.Commands.ResetCommit;
using TRRCMS.Application.Import.Commands.UncancelPackage;
using TRRCMS.Application.Import.Queries.GetQuarantineReport;
using TRRCMS.Application.Import.Commands.StagePackage;
using TRRCMS.Application.Import.Commands.UploadPackage;
using TRRCMS.Application.Import.Dtos;
using TRRCMS.Application.Import.Queries.GetCommitReport;
using TRRCMS.Application.Import.Queries.GetImportPackage;
using TRRCMS.Application.Import.Queries.GetImportPackages;
using TRRCMS.Application.Import.Queries.GetReconciliationQueue;
using TRRCMS.Application.Import.Queries.GetStagingSummary;
using TRRCMS.Application.Import.Queries.GetStagedEntities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.WebAPI.Controllers;

/// <summary>
/// Import pipeline API controller.
/// Exposes the full .uhc package import workflow: upload → stage → validate →
/// detect duplicates → approve → commit → archive.
///
/// All endpoints require the CanImportData policy (System_Import permission).
/// </summary>
[ApiController]
[Route("api/v1/import")]
[Authorize(Policy = "CanImportData")]
public class ImportController : ControllerBase
{
    private readonly IMediator _mediator;

    public ImportController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    // ==================== UPLOAD ====================

    /// <summary>
    /// Upload a .uhc package for import.
    /// Performs integrity checks: magic bytes (must be valid SQLite), checksum, signature, vocabulary compatibility.
    /// </summary>
    /// <param name="file">The .uhc file (multipart form-data). Must be a valid SQLite database with .uhc extension. Max 500 MB.</param>
    /// <response code="201">Package uploaded and validated.</response>
    /// <response code="400">Invalid file (wrong extension, invalid SQLite format, or validation errors).</response>
    /// <response code="409">Package already imported (idempotency).</response>
    [HttpPost("upload")]
    [ProducesResponseType(typeof(UploadPackageResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [RequestSizeLimit(524_288_000)] // 500 MB
    public async Task<ActionResult<UploadPackageResultDto>> UploadPackage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("File is required.");

        if (!file.FileName.EndsWith(".uhc", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Only .uhc package files are accepted.");

        await using var stream = file.OpenReadStream();

        // Verify .uhc is actually a SQLite file (magic bytes: "SQLite format 3\000")
        var header = new byte[6];
        var bytesRead = await stream.ReadAsync(header.AsMemory(0, 6));
        stream.Position = 0;
        if (bytesRead < 6 || !header.AsSpan().SequenceEqual("SQLite"u8))
            return BadRequest("Invalid .uhc package — file is not a valid SQLite database.");

        var command = new UploadPackageCommand
        {
            FileStream = stream,
            FileName = file.FileName,
            FileSizeBytes = file.Length,
            ImportMethod = "Manual"
        };

        var result = await _mediator.Send(command);

        if (result.IsDuplicatePackage)
            return Conflict(result);

        return CreatedAtAction(nameof(GetPackage), new { id = result.Package.Id }, result);
    }

    // ==================== PACKAGE QUERIES ====================

    /// <summary>
    /// List all import packages with optional filtering, sorting, and pagination.
    /// </summary>
    /// <response code="200">Paginated list of import packages.</response>
    [HttpGet("packages")]
    [ProducesResponseType(typeof(GetImportPackagesResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<GetImportPackagesResponse>> GetPackages(
        [FromQuery] ImportStatus? status = null,
        [FromQuery] Guid? exportedByUserId = null,
        [FromQuery] Guid? importedByUserId = null,
        [FromQuery] DateTime? importedAfter = null,
        [FromQuery] DateTime? importedBefore = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = true)
    {
        var query = new GetImportPackagesQuery
        {
            Status = status,
            ExportedByUserId = exportedByUserId,
            ImportedByUserId = importedByUserId,
            ImportedAfter = importedAfter,
            ImportedBefore = importedBefore,
            SearchTerm = searchTerm,
            Page = page,
            PageSize = pageSize,
            SortBy = sortBy,
            SortDescending = sortDescending
        };

        return Ok(await _mediator.Send(query));
    }

    /// <summary>
    /// Get import package details by ID.
    /// </summary>
    /// <param name="id">ImportPackage surrogate ID.</param>
    /// <response code="200">Package found.</response>
    /// <response code="404">Package not found.</response>
    [HttpGet("packages/{id:guid}")]
    [ProducesResponseType(typeof(ImportPackageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ImportPackageDto>> GetPackage(Guid id)
    {
        var result = await _mediator.Send(new GetImportPackageQuery { Id = id });

        if (result is null)
            return NotFound($"Import package with ID '{id}' was not found.");

        return Ok(result);
    }

    // ==================== STAGING & VALIDATION ====================

    /// <summary>
    /// Trigger staging (unpack .uhc → staging tables) and row-level validation.
    /// </summary>
    /// <param name="id">ImportPackage surrogate ID.</param>
    /// <response code="200">Staging and validation completed.</response>
    /// <response code="404">Package not found.</response>
    [HttpPost("packages/{id:guid}/stage")]
    [ProducesResponseType(typeof(StagingSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StagingSummaryDto>> StagePackage(Guid id)
    {
        return Ok(await _mediator.Send(new StagePackageCommand { ImportPackageId = id }));
    }

    /// <summary>
    /// Get the current validation report for a package (read-only).
    /// </summary>
    /// <param name="id">ImportPackage surrogate ID.</param>
    /// <response code="200">Validation report returned.</response>
    /// <response code="404">Package not found.</response>
    [HttpGet("packages/{id:guid}/validation-report")]
    [ProducesResponseType(typeof(StagingSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StagingSummaryDto>> GetValidationReport(Guid id)
    {
        return Ok(await _mediator.Send(new GetStagingSummaryQuery { ImportPackageId = id }));
    }

    // ==================== STAGED ENTITIES ====================

    /// <summary>
    /// Get all staged entities for a package with their IDs, identifiers, and status.
    /// Used by the conflict resolution UI to display staging vs production entity details.
    /// </summary>
    /// <param name="id">ImportPackage surrogate ID.</param>
    /// <param name="entityType">Optional filter: Building, PropertyUnit, Person, Household, etc.</param>
    /// <response code="200">Staged entities grouped by type.</response>
    /// <response code="404">Package not found.</response>
    [HttpGet("packages/{id:guid}/staged-entities")]
    [ProducesResponseType(typeof(GetStagedEntitiesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetStagedEntitiesResponse>> GetStagedEntities(
        Guid id, [FromQuery] string? entityType = null)
    {
        return Ok(await _mediator.Send(new GetStagedEntitiesQuery
        {
            ImportPackageId = id,
            EntityTypeFilter = entityType
        }));
    }

    /// <summary>
    /// Get the post-commit reconciliation queue: records the import pipeline auto-adjusted to honour a
    /// Keep-Separate decision and which await manual review — persons whose National ID was cleared, and
    /// property units whose identifier was suffix-disambiguated.
    /// </summary>
    /// <param name="entityType">Optional filter: "person" or "propertyunit". Omit for both.</param>
    /// <param name="page">1-based page number (default 1).</param>
    /// <param name="pageSize">Page size per entity type (default 20).</param>
    /// <response code="200">Reconciliation queue (may be empty).</response>
    [HttpGet("reconciliation-queue")]
    [ProducesResponseType(typeof(ReconciliationQueueDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ReconciliationQueueDto>> GetReconciliationQueue(
        [FromQuery] string? entityType = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        return Ok(await _mediator.Send(new GetReconciliationQueueQuery
        {
            EntityTypeFilter = entityType,
            Page = page,
            PageSize = pageSize
        }));
    }

    // ==================== DUPLICATE DETECTION ====================

    /// <summary>
    /// Trigger duplicate detection (person + property matching) for a staged package.
    /// </summary>
    /// <param name="id">ImportPackage surrogate ID.</param>
    /// <response code="200">Detection completed.</response>
    /// <response code="404">Package not found.</response>
    [HttpPost("packages/{id:guid}/detect-duplicates")]
    [ProducesResponseType(typeof(DuplicateDetectionResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DuplicateDetectionResultDto>> DetectDuplicates(Guid id)
    {
        return Ok(await _mediator.Send(new DetectDuplicatesCommand { ImportPackageId = id }));
    }

    // ==================== APPROVE & COMMIT ====================

    /// <summary>
    /// Approve staging records for commit. Requires all conflicts resolved.
    /// </summary>
    /// <param name="id">ImportPackage surrogate ID.</param>
    /// <param name="command">Approval options (all valid or selective).</param>
    /// <response code="200">Records approved, package ready to commit.</response>
    /// <response code="404">Package not found.</response>
    [HttpPost("packages/{id:guid}/approve")]
    [ProducesResponseType(typeof(ImportPackageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ImportPackageDto>> ApproveForCommit(
        Guid id, [FromBody] ApproveForCommitCommand command)
    {
        command.ImportPackageId = id;
        return Ok(await _mediator.Send(command));
    }

    /// <summary>
    /// Commit approved staging records to production tables.
    /// Generates Record IDs and deduplicates attachments.
    /// </summary>
    /// <param name="id">ImportPackage surrogate ID.</param>
    /// <param name="command">Commit options.</param>
    /// <response code="200">Commit completed.</response>
    /// <response code="404">Package not found.</response>
    [HttpPost("packages/{id:guid}/commit")]
    [ProducesResponseType(typeof(CommitReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CommitReportDto>> CommitPackage(
        Guid id, [FromBody] CommitPackageCommand command)
    {
        command.ImportPackageId = id;
        return Ok(await _mediator.Send(command));
    }

    /// <summary>
    /// Get the commit report for a completed/failed import package.
    /// </summary>
    /// <param name="id">ImportPackage surrogate ID.</param>
    /// <response code="200">Commit report returned.</response>
    /// <response code="404">Package not found or commit not yet attempted.</response>
    [HttpGet("packages/{id:guid}/commit-report")]
    [ProducesResponseType(typeof(CommitReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CommitReportDto>> GetCommitReport(Guid id)
    {
        return Ok(await _mediator.Send(new GetCommitReportQuery { ImportPackageId = id }));
    }

    // ==================== RESET STUCK COMMIT ====================

    /// <summary>
    /// Reset a package stuck in Committing or Failed status back to ReadyToCommit.
    /// Use when a commit crashed mid-way and the package is stuck.
    /// Preserves all staging data, conflict resolutions, and approvals.
    /// </summary>
    /// <param name="id">ImportPackage surrogate ID.</param>
    /// <param name="command">Reset details with mandatory reason.</param>
    /// <response code="200">Package reset to ReadyToCommit.</response>
    /// <response code="404">Package not found.</response>
    /// <response code="409">Package is not in Committing or Failed status.</response>
    [HttpPost("packages/{id:guid}/reset-commit")]
    [ProducesResponseType(typeof(ImportPackageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ImportPackageDto>> ResetCommit(
        Guid id, [FromBody] ResetCommitCommand command)
    {
        command.ImportPackageId = id;
        return Ok(await _mediator.Send(command));
    }

    // ==================== CANCEL & QUARANTINE ====================

    /// <summary>
    /// Cancel an active import package.
    /// </summary>
    /// <param name="id">ImportPackage surrogate ID.</param>
    /// <param name="command">Cancellation details with mandatory reason.</param>
    /// <response code="200">Package cancelled.</response>
    /// <response code="404">Package not found.</response>
    [HttpPost("packages/{id:guid}/cancel")]
    [ProducesResponseType(typeof(ImportPackageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ImportPackageDto>> CancelPackage(
        Guid id, [FromBody] CancelPackageCommand command)
    {
        command.ImportPackageId = id;
        return Ok(await _mediator.Send(command));
    }

    /// <summary>
    /// Quarantine a suspicious import package.
    /// </summary>
    /// <param name="id">ImportPackage surrogate ID.</param>
    /// <param name="command">Quarantine details with mandatory reason.</param>
    /// <response code="200">Package quarantined.</response>
    /// <response code="404">Package not found.</response>
    [HttpPost("packages/{id:guid}/quarantine")]
    [ProducesResponseType(typeof(ImportPackageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ImportPackageDto>> QuarantinePackage(
        Guid id, [FromBody] QuarantinePackageCommand command)
    {
        command.ImportPackageId = id;
        return Ok(await _mediator.Send(command));
    }

    /// <summary>
    /// Restore a Cancelled import package to its pre-cancellation status.
    /// The package is returned to the status it held immediately before it was cancelled,
    /// so the import pipeline can resume. If no previous status was recorded (packages
    /// cancelled before this feature), the package is restored to Pending.
    /// </summary>
    /// <param name="id">ImportPackage surrogate ID.</param>
    /// <param name="command">Optional reason for the audit trail.</param>
    /// <response code="200">Package restored to previous status.</response>
    /// <response code="404">Package not found.</response>
    /// <response code="409">Package is not in Cancelled status.</response>
    [HttpPost("packages/{id:guid}/uncancel")]
    [ProducesResponseType(typeof(ImportPackageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ImportPackageDto>> UncancelPackage(
        Guid id, [FromBody] UncancelPackageCommand command)
    {
        command.ImportPackageId = id;
        return Ok(await _mediator.Send(command));
    }

    /// <summary>
    /// Get a structured report explaining why a package is quarantined.
    /// Returns the quarantine reason, category (ChecksumFailure / SignatureFailure /
    /// VocabularyVersionMismatch / SchemaInvalid / ManualQuarantine), and all
    /// integrity flags so the frontend can display a clear diagnosis.
    /// </summary>
    /// <param name="id">ImportPackage surrogate ID.</param>
    /// <response code="200">Quarantine report returned.</response>
    /// <response code="404">Package not found.</response>
    /// <response code="409">Package is not in Quarantined status.</response>
    [HttpGet("packages/{id:guid}/quarantine-report")]
    [ProducesResponseType(typeof(QuarantineReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<QuarantineReportDto>> GetQuarantineReport(Guid id)
    {
        return Ok(await _mediator.Send(new GetQuarantineReportQuery { ImportPackageId = id }));
    }
}
