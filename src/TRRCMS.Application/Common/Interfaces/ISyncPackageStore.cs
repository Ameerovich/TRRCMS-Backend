namespace TRRCMS.Application.Common.Interfaces;

public interface ISyncPackageStore
{
    /// <summary>
    /// Stores package bytes to quarantine/ingest area.
    /// Returns a stable storage key/path for later import pipeline.
    /// Must be idempotent by packageId.
    /// </summary>
    Task<StoreSyncPackageResult> StoreAsync(
        Guid packageId,
        Stream content,
        string sha256Checksum,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid packageId, CancellationToken cancellationToken = default);
}

public sealed record StoreSyncPackageResult(
    bool Stored,
    bool AlreadyExists,
    string StorageKey
);
