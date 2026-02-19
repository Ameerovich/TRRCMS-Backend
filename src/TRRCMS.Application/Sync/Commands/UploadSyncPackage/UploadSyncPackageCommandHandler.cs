using System.Security.Cryptography;
using System.Text;
using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Sync.DTOs;

namespace TRRCMS.Application.Sync.Commands.UploadSyncPackage;

public sealed class UploadSyncPackageCommandHandler : IRequestHandler<UploadSyncPackageCommand, UploadSyncPackageResultDto>
{
    private readonly IUnitOfWork _uow;
    private readonly ISyncPackageStore _store;

    public UploadSyncPackageCommandHandler(IUnitOfWork uow, ISyncPackageStore store)
    {
        _uow = uow;
        _store = store;
    }

    public async Task<UploadSyncPackageResultDto> Handle(UploadSyncPackageCommand request, CancellationToken ct)
    {
        // 1) Ensure session exists
        var session = await _uow.SyncSessions.GetByIdAsync(request.Manifest.SyncSessionId, ct);
        if (session is null)
            return new UploadSyncPackageResultDto(false, request.Manifest.PackageId, false, "Sync session not found.");

        // 2) Idempotency by packageId
        if (await _store.ExistsAsync(request.Manifest.PackageId, ct))
        {
            session.RecordUploadResult(success: true);
            await _uow.SyncSessions.UpdateAsync(session, ct);
            await _uow.SaveChangesAsync(ct);

            return new UploadSyncPackageResultDto(true, request.Manifest.PackageId, true, "Duplicate package (already received).");
        }

        // 3) Verify checksum while streaming
        var expected = request.Manifest.Sha256Checksum.Trim().ToLowerInvariant();
        var actual = await ComputeSha256HexAsync(request.PackageStream, ct);

        if (!string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase))
        {
            session.RecordUploadResult(success: false);
            session.MarkFailed($"Checksum mismatch for package {request.Manifest.PackageId}.");
            await _uow.SyncSessions.UpdateAsync(session, ct);
            await _uow.SaveChangesAsync(ct);

            return new UploadSyncPackageResultDto(false, request.Manifest.PackageId, false, "Checksum mismatch.");
        }

        // reset stream position if possible for storing (WebAPI should provide seekable stream; if not, buffer there)
        if (request.PackageStream.CanSeek)
            request.PackageStream.Position = 0;
        else
            return new UploadSyncPackageResultDto(false, request.Manifest.PackageId, false, "Non-seekable stream is not supported for storage.");

        // 4) Store to quarantine/ingest area
        var stored = await _store.StoreAsync(request.Manifest.PackageId, request.PackageStream, actual, ct);

        session.RecordUploadResult(success: stored.Stored || stored.AlreadyExists);
        await _uow.SyncSessions.UpdateAsync(session, ct);
        await _uow.SaveChangesAsync(ct);

        if (stored.Stored || stored.AlreadyExists)
            return new UploadSyncPackageResultDto(true, request.Manifest.PackageId, stored.AlreadyExists, "Package received.");

        return new UploadSyncPackageResultDto(false, request.Manifest.PackageId, false, "Failed to store package.");
    }

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
