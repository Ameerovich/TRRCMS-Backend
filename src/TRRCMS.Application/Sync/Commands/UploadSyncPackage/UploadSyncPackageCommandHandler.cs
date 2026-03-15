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
///   <item>Store the package to a temporary path (needed for SQLite access).</item>
///   <item>Read manifest from the .uhc SQLite to extract PackageId, DeviceId, checksum, etc.</item>
///   <item>Verify content checksum: server-computed vs manifest checksum (and optional client checksum).</item>
///   <item>Move to quarantine/ingest area (idempotent by packageId from manifest).</item>
///   <item>Feed the verified package into the Import Pipeline via <see cref="UploadPackageCommand"/>
///         for vocabulary checking and <c>ImportPackage</c> creation.</item>
///   <item>Update session counters and persist.</item>
/// </list>
///
/// The API requires only <c>file + SyncSessionId</c>. All metadata (PackageId, DeviceId,
/// SchemaVersion, AppVersion, checksum, etc.) is extracted from the .uhc manifest automatically,
/// matching the simplicity of the Import upload endpoint.
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
                false, Guid.Empty, false,
                "Sync session not found.");

        // Security guard: only the session owner may upload packages.
        if (session.FieldCollectorId != currentUserId)
            throw new UnauthorizedAccessException(
                "Session does not belong to the current user.");

        // Guard: reject uploads to sessions that are no longer active.
        if (session.SessionStatus != SyncSessionStatus.InProgress)
            return new UploadSyncPackageResultDto(
                false, Guid.Empty, false,
                "Session is no longer active.");

        // ── 3. Save to a temporary file so we can open it as SQLite ─────────────────
        var tempPath = Path.Combine(Path.GetTempPath(), $"sync_{Guid.NewGuid()}.uhc");
        try
        {
            await using (var tempFile = File.Create(tempPath))
            {
                await request.PackageStream.CopyToAsync(tempFile, ct);
            }

            // ── 4. Read manifest from the .uhc SQLite ───────────────────────────────
            var manifest = await _importService.ParseManifestAsync(tempPath, ct);

            _logger.LogInformation(
                "Parsed manifest from sync package: PackageId={PackageId}, DeviceId={DeviceId}, " +
                "SchemaVersion={SchemaVersion}, Records={RecordCount}",
                manifest.PackageId, manifest.DeviceId,
                manifest.SchemaVersion, manifest.TotalRecordCount);

            // ── 5. Verify content checksum ───────────────────────────────────────────
            var contentChecksum = await _importService.ComputeContentChecksumAsync(tempPath, ct);

            // 5a. Compare server-computed checksum against manifest checksum
            if (!string.IsNullOrEmpty(manifest.Checksum))
            {
                var manifestChecksum = manifest.Checksum.Trim().ToLowerInvariant();
                if (!string.Equals(manifestChecksum, contentChecksum, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning(
                        "Content checksum mismatch for sync package {PackageId}. " +
                        "Manifest={ManifestChecksum}, Computed={ComputedChecksum}",
                        manifest.PackageId, manifestChecksum, contentChecksum);

                    session.RecordUploadResult(success: false);
                    session.MarkFailed(
                        $"Content checksum mismatch for package {manifest.PackageId}.");
                    await _uow.SyncSessions.UpdateAsync(session, ct);
                    await _uow.SaveChangesAsync(ct);

                    return new UploadSyncPackageResultDto(
                        false, manifest.PackageId, false,
                        "Content checksum mismatch. Package integrity verification failed.");
                }
            }

            // 5b. If client also provided a checksum, cross-verify as defense-in-depth
            if (!string.IsNullOrEmpty(request.Manifest.Sha256Checksum))
            {
                var clientChecksum = request.Manifest.Sha256Checksum.Trim().ToLowerInvariant();
                if (!string.Equals(clientChecksum, contentChecksum, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning(
                        "Client-provided checksum mismatch for sync package {PackageId}. " +
                        "Client={ClientChecksum}, Computed={ComputedChecksum}",
                        manifest.PackageId, clientChecksum, contentChecksum);

                    session.RecordUploadResult(success: false);
                    session.MarkFailed(
                        $"Client checksum mismatch for package {manifest.PackageId}.");
                    await _uow.SyncSessions.UpdateAsync(session, ct);
                    await _uow.SaveChangesAsync(ct);

                    return new UploadSyncPackageResultDto(
                        false, manifest.PackageId, false,
                        "Client-provided checksum mismatch. Package integrity verification failed.");
                }
            }

            _logger.LogInformation(
                "Content checksum verified for sync package {PackageId}: {Checksum}",
                manifest.PackageId, contentChecksum);

            // ── 6. Move to quarantine/ingest area (idempotent by PackageId) ──────────
            await using var storeStream = File.OpenRead(tempPath);
            var stored = await _store.StoreAsync(
                manifest.PackageId, storeStream,
                contentChecksum, ct);

            var packagePath = stored.StorageKey;

            _logger.LogInformation(
                "Sync package {PackageId} {Status} at {Path}",
                manifest.PackageId,
                stored.AlreadyExists ? "already exists" : "stored",
                packagePath);

            // ── 7. Feed into Import Pipeline ─────────────────────────────────────────
            Guid? importPackageId = null;
            string? importError = null;
            try
            {
                await using var fileStream = File.OpenRead(packagePath);
                var fileInfo = new FileInfo(packagePath);

                var importResult = await _sender.Send(new UploadPackageCommand
                {
                    FileStream = fileStream,
                    FileName = $"{manifest.PackageId}.uhc",
                    FileSizeBytes = fileInfo.Length,
                    ImportMethod = "Sync"
                }, ct);

                importPackageId = importResult.Package?.Id;

                if (importResult.IsQuarantined)
                {
                    _logger.LogWarning(
                        "Sync package {PackageId} was quarantined by import pipeline: {Message}",
                        manifest.PackageId, importResult.Message);
                }
                else if (importResult.IsDuplicatePackage)
                {
                    _logger.LogInformation(
                        "Sync package {PackageId} already exists in import pipeline (idempotent).",
                        manifest.PackageId);
                }
                else
                {
                    _logger.LogInformation(
                        "Sync package {PackageId} queued in import pipeline as ImportPackage {ImportId}",
                        manifest.PackageId, importPackageId);
                }
            }
            catch (Exception ex)
            {
                importError = ex.Message;
                _logger.LogError(ex,
                    "Failed to feed sync package {PackageId} into import pipeline. " +
                    "File is stored at {Path} for manual re-import.",
                    manifest.PackageId, packagePath);
            }

            // ── 8. Update session counters ───────────────────────────────────────────
            session.RecordUploadResult(success: true);
            await _uow.SyncSessions.UpdateAsync(session, ct);
            await _uow.SaveChangesAsync(ct);

            return new UploadSyncPackageResultDto(
                true,
                manifest.PackageId,
                stored.AlreadyExists,
                importError != null
                    ? $"Package received but import pipeline failed: {importError}. File stored for manual re-import."
                    : stored.AlreadyExists
                        ? "Duplicate package (already received)."
                        : "Package received and queued for import.",
                importPackageId,
                importError);
        }
        finally
        {
            // Clean up temp file
            if (File.Exists(tempPath))
            {
                try { File.Delete(tempPath); }
                catch { /* best effort */ }
            }
        }
    }
}
