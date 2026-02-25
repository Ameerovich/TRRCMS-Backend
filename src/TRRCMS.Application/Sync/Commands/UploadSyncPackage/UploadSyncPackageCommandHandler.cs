using System.Security.Cryptography;
using System.Text;
using MediatR;
using TRRCMS.Application.Common.Interfaces;
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
///   <item>Check idempotency — if the package already exists, return success.</item>
///   <item>Compute SHA-256 checksum while streaming and compare with manifest.</item>
///   <item>Store the verified package to the quarantine/ingest area.</item>
///   <item>Update session counters and persist.</item>
/// </list>
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

    public UploadSyncPackageCommandHandler(
        IUnitOfWork uow,
        ISyncPackageStore store,
        ICurrentUserService currentUser)
    {
        _uow = uow;
        _store = store;
        _currentUser = currentUser;
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
                false, request.Manifest.PackageId, false, "Sync session not found.");

        // Security guard: only the session owner may upload packages.
        if (session.FieldCollectorId != currentUserId)
            throw new UnauthorizedAccessException(
                "Session does not belong to the current user.");

        // Guard: reject uploads to sessions that are no longer active.
        if (session.SessionStatus != SyncSessionStatus.InProgress)
            return new UploadSyncPackageResultDto(
                false, request.Manifest.PackageId, false,
                "Session is no longer active.");

        // ── 3. Idempotency by packageId ────────────────────────────────────────────
        if (await _store.ExistsAsync(request.Manifest.PackageId, ct))
        {
            session.RecordUploadResult(success: true);
            await _uow.SyncSessions.UpdateAsync(session, ct);
            await _uow.SaveChangesAsync(ct);

            return new UploadSyncPackageResultDto(
                true, request.Manifest.PackageId, true, "Duplicate package (already received).");
        }

        // ── 4. Verify SHA-256 checksum while streaming ─────────────────────────────
        var expected = request.Manifest.Sha256Checksum.Trim().ToLowerInvariant();
        var actual = await ComputeSha256HexAsync(request.PackageStream, ct);

        if (!string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase))
        {
            session.RecordUploadResult(success: false);
            session.MarkFailed($"Checksum mismatch for package {request.Manifest.PackageId}.");
            await _uow.SyncSessions.UpdateAsync(session, ct);
            await _uow.SaveChangesAsync(ct);

            return new UploadSyncPackageResultDto(
                false, request.Manifest.PackageId, false,
                $"Checksum mismatch. Expected: {expected}, Actual: {actual}, StreamLength: {request.PackageStream.Length}");
        }

        // Reset stream position for storage (WebAPI provides a seekable buffered stream).
        if (request.PackageStream.CanSeek)
            request.PackageStream.Position = 0;
        else
            return new UploadSyncPackageResultDto(
                false, request.Manifest.PackageId, false,
                "Non-seekable stream is not supported for storage.");

        // ── 5. Store to quarantine/ingest area ─────────────────────────────────────
        var stored = await _store.StoreAsync(
            request.Manifest.PackageId, request.PackageStream, actual, ct);

        session.RecordUploadResult(success: stored.Stored || stored.AlreadyExists);
        await _uow.SyncSessions.UpdateAsync(session, ct);
        await _uow.SaveChangesAsync(ct);

        if (stored.Stored || stored.AlreadyExists)
            return new UploadSyncPackageResultDto(
                true, request.Manifest.PackageId, stored.AlreadyExists, "Package received.");

        return new UploadSyncPackageResultDto(
            false, request.Manifest.PackageId, false, "Failed to store package.");
    }

    /// <summary>
    /// Computes the SHA-256 hash of the entire stream and returns it as a lowercase hex string.
    /// </summary>
    private static async Task<string> ComputeSha256HexAsync(Stream stream, CancellationToken ct)
    {
        using var sha = SHA256.Create();
        var hash = await sha.ComputeHashAsync(stream, ct);
        var sb = new StringBuilder(hash.Length * 2);
        foreach (var b in hash)
            sb.Append(b.ToString("x2"));
        return sb.ToString();
    }
}
