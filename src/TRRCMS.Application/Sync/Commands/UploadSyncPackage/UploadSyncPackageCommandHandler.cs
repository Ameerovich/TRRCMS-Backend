using MediatR;
using Microsoft.Extensions.Logging;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Import.Commands.UploadPackage;
using TRRCMS.Application.Sync.DTOs;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Sync.Commands.UploadSyncPackage;

/// <summary>
/// Handles <see cref="UploadSyncPackageCommand"/> — Sync Protocol Step 2.
///
/// Execution flow:
/// <list type="number">
///   <item>Authenticate the requesting user and validate session ownership.</item>
///   <item>Verify the session is still <c>InProgress</c>.</item>
///   <item>Store the package to the quarantine/ingest area (idempotent by packageId).</item>
///   <item>Compute SHA-256 content checksum (data tables only, excluding manifest)
///         and compare with the tablet-provided <c>Sha256Checksum</c>.</item>
///   <item>Feed the verified package into the Import Pipeline via <see cref="UploadPackageCommand"/>
///         for manifest parsing, vocabulary checking, and <c>ImportPackage</c> creation.</item>
///   <item>Update session counters and persist.</item>
/// </list>
///
/// The content checksum algorithm matches the one used in
/// <c>UploadPackageCommandHandler</c> — both call
/// <see cref="IImportService.ComputeContentChecksumAsync"/> so the mobile team
/// only needs to implement a single checksum scheme.
///
/// Security: only the field collector who owns the session may upload packages.
/// FSD: FR-D-3 (Package Integrity), FR-D-4 (Package Storage).
/// UC-003 Stage 2: S12 (Verify Package Integrity and Compatibility).
/// </summary>
public sealed class UploadSyncPackageCommandHandler
    : IRequestHandler<UploadSyncPackageCommand, UploadSyncPackageResultDto>
{
    private readonly IUnitOfWork _uow;
    private readonly ISyncPackageStore _store;
    private readonly ICurrentUserService _currentUser;
    private readonly IImportService _importService;
    private readonly ISender _sender;
    private readonly ILogger<UploadSyncPackageCommandHandler> _logger;

    public UploadSyncPackageCommandHandler(
        IUnitOfWork uow,
        ISyncPackageStore store,
        ICurrentUserService currentUser,
        IImportService importService,
        ISender sender,
        ILogger<UploadSyncPackageCommandHandler> logger)
    {
        _uow = uow;
        _store = store;
        _currentUser = currentUser;
        _importService = importService;
        _sender = sender;
        _logger = logger;
    }

    public async Task<UploadSyncPackageResultDto> Handle(
        UploadSyncPackageCommand request, CancellationToken ct)
    {
        // ── 1. Authenticate the requesting user ────────────────────────────────────
        var currentUserId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated.");

        // ── 2. Resolve and validate the sync session ───────────────────────────────
        var session = await _uow.SyncSessions.GetByIdAsync(request.Manifest.SyncSessionId, ct);
        if (session is null)
            return new UploadSyncPackageResultDto(
                false, request.Manifest.PackageId, false,
                "Sync session not found.");

        // Security guard: only the session owner may upload packages.
        if (session.FieldCollectorId != currentUserId)
            throw new UnauthorizedAccessException(
                "Session does not belong to the current user.");

        // Guard: reject uploads to sessions that are no longer active.
        if (session.SessionStatus != SyncSessionStatus.InProgress)
            return new UploadSyncPackageResultDto(
                false, request.Manifest.PackageId, false,
                "Session is no longer active.");

        // ── 3. Store to quarantine/ingest area (idempotent) ────────────────────────
        // Store first, then verify — we need the file on disk to compute
        // content checksum (SQLite must be opened from a file path).
        var stored = await _store.StoreAsync(
            request.Manifest.PackageId, request.PackageStream,
            request.Manifest.Sha256Checksum, ct);

        var packagePath = stored.StorageKey;

        _logger.LogInformation(
            "Sync package {PackageId} {Status} at {Path}",
            request.Manifest.PackageId,
            stored.AlreadyExists ? "already exists" : "stored",
            packagePath);

        // ── 4. Verify content checksum ─────────────────────────────────────────────
        // Uses the same algorithm as the Import Pipeline (IImportService.ComputeContentChecksumAsync):
        // SHA-256 of all data table contents, excluding manifest and attachments tables.
        // Mobile team sends this content checksum in the Sha256Checksum form field.
        if (!string.IsNullOrEmpty(request.Manifest.Sha256Checksum))
        {
            var expected = request.Manifest.Sha256Checksum.Trim().ToLowerInvariant();
            var contentChecksum = await _importService.ComputeContentChecksumAsync(packagePath, ct);

            if (!string.Equals(expected, contentChecksum, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning(
                    "Content checksum mismatch for sync package {PackageId}. " +
                    "Expected={Expected}, Actual={Actual}",
                    request.Manifest.PackageId, expected, contentChecksum);

                session.RecordUploadResult(success: false);
                session.MarkFailed(
                    $"Content checksum mismatch for package {request.Manifest.PackageId}.");
                await _uow.SyncSessions.UpdateAsync(session, ct);
                await _uow.SaveChangesAsync(ct);

                return new UploadSyncPackageResultDto(
                    false, request.Manifest.PackageId, false,
                    "Content checksum mismatch. Package integrity verification failed.");
            }

            _logger.LogInformation(
                "Content checksum verified for sync package {PackageId}: {Checksum}",
                request.Manifest.PackageId, contentChecksum);
        }

        // ── 5. Feed into Import Pipeline ───────────────────────────────────────────
        // The import handler performs: manifest parsing → content checksum verification →
        // vocabulary compatibility check → ImportPackage entity creation.
        // Its own idempotency (by manifest PackageId) handles duplicate submissions safely.
        Guid? importPackageId = null;
        try
        {
            await using var fileStream = File.OpenRead(packagePath);
            var fileInfo = new FileInfo(packagePath);

            var importResult = await _sender.Send(new UploadPackageCommand
            {
                FileStream = fileStream,
                FileName = $"{request.Manifest.PackageId}.uhc",
                FileSizeBytes = fileInfo.Length,
                ImportMethod = "Sync"
            }, ct);

            importPackageId = importResult.Package?.Id;

            if (importResult.IsQuarantined)
            {
                _logger.LogWarning(
                    "Sync package {PackageId} was quarantined by import pipeline: {Message}",
                    request.Manifest.PackageId, importResult.Message);
            }
            else if (importResult.IsDuplicatePackage)
            {
                _logger.LogInformation(
                    "Sync package {PackageId} already exists in import pipeline (idempotent).",
                    request.Manifest.PackageId);
            }
            else
            {
                _logger.LogInformation(
                    "Sync package {PackageId} queued in import pipeline as ImportPackage {ImportId}",
                    request.Manifest.PackageId, importPackageId);
            }
        }
        catch (Exception ex)
        {
            // Import pipeline failure is non-fatal for sync — the file is safely
            // stored in quarantine and can be re-processed via manual import.
            _logger.LogError(ex,
                "Failed to feed sync package {PackageId} into import pipeline. " +
                "File is stored at {Path} for manual re-import.",
                request.Manifest.PackageId, packagePath);
        }

        // ── 6. Update session counters ─────────────────────────────────────────────
        session.RecordUploadResult(success: true);
        await _uow.SyncSessions.UpdateAsync(session, ct);
        await _uow.SaveChangesAsync(ct);

        return new UploadSyncPackageResultDto(
            true,
            request.Manifest.PackageId,
            stored.AlreadyExists,
            stored.AlreadyExists
                ? "Duplicate package (already received)."
                : "Package received and queued for import.",
            importPackageId);
    }
}
