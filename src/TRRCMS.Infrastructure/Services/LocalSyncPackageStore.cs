using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TRRCMS.Application.Common.Interfaces;

namespace TRRCMS.Infrastructure.Services;

/// <summary>
/// Local file-system implementation of <see cref="ISyncPackageStore"/>.
///
/// Incoming <c>.uhc</c> packages uploaded by the tablet are written to a
/// configurable quarantine directory on the server's local disk.  The import
/// pipeline can then pick them up asynchronously for staging and validation.
///
/// Storage layout:
/// <code>
/// {SyncPackageStore:QuarantinePath}/
///   {packageId}.uhc          ← package bytes
///   {packageId}.sha256       ← hex checksum (one line, no newline)
/// </code>
///
/// Configuration key: <c>SyncPackageStore:QuarantinePath</c>.
/// Default path: <c>wwwroot/sync-quarantine</c>.
///
/// This implementation is intentionally simple and suitable for single-server
/// deployments.  Replace with a distributed store (Azure Blob, S3) for
/// multi-node environments.
///
/// FSD: FR-D-4 (Package Storage); FR-D-5 (Import Pipeline Integration).
/// </summary>
public sealed class LocalSyncPackageStore : ISyncPackageStore
{
    private readonly string _quarantinePath;
    private readonly ILogger<LocalSyncPackageStore> _logger;

    /// <summary>
    /// File extension used for incoming sync packages.
    /// </summary>
    private const string PackageExtension = ".uhc";

    /// <summary>
    /// File extension used for the companion checksum file.
    /// </summary>
    private const string ChecksumExtension = ".sha256";

    public LocalSyncPackageStore(
        IConfiguration configuration,
        ILogger<LocalSyncPackageStore> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Read quarantine directory from configuration; fall back to a safe default.
        _quarantinePath = configuration["SyncPackageStore:QuarantinePath"]
                          ?? "wwwroot/sync-quarantine";

        // Ensure the quarantine directory exists at startup so the first write
        // does not fail with a DirectoryNotFoundException.
        if (!Directory.Exists(_quarantinePath))
        {
            Directory.CreateDirectory(_quarantinePath);
            _logger.LogInformation(
                "Created sync package quarantine directory at '{Path}'.", _quarantinePath);
        }
    }

    // ==================== PUBLIC API ====================

    /// <inheritdoc />
    /// <remarks>
    /// The operation is idempotent: if a file with the same <paramref name="packageId"/>
    /// already exists the method returns <c>AlreadyExists = true</c> and skips the write,
    /// protecting against double-upload from the same tablet session.
    /// </remarks>
    public async Task<StoreSyncPackageResult> StoreAsync(
        Guid packageId,
        Stream content,
        string sha256Checksum,
        CancellationToken cancellationToken = default)
    {
        var packagePath  = BuildPackagePath(packageId);
        var checksumPath = BuildChecksumPath(packageId);

        // Idempotency guard — if the file already exists, skip writing.
        if (File.Exists(packagePath))
        {
            _logger.LogInformation(
                "Sync package '{PackageId}' already exists in quarantine — skipping write.",
                packageId);

            return new StoreSyncPackageResult(
                Stored: false,
                AlreadyExists: true,
                StorageKey: packagePath);
        }

        try
        {
            // Write the package bytes to a temporary file first, then rename atomically.
            // This prevents a partially written file from being picked up by the import
            // pipeline while the stream is still being received.
            var tempPath = packagePath + ".tmp";

            await using (var fileStream = new FileStream(
                tempPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,        // 80 KiB buffer — optimised for LAN speeds
                useAsync: true))
            {
                await content.CopyToAsync(fileStream, cancellationToken);
                await fileStream.FlushAsync(cancellationToken);
            }

            // Atomic rename: either the file is fully written or it does not exist.
            File.Move(tempPath, packagePath, overwrite: false);

            // Write companion checksum file for downstream verification.
            await File.WriteAllTextAsync(checksumPath, sha256Checksum, cancellationToken);

            _logger.LogInformation(
                "Sync package '{PackageId}' stored to quarantine at '{Path}' (SHA-256: {Checksum}).",
                packageId, packagePath, sha256Checksum);

            return new StoreSyncPackageResult(
                Stored: true,
                AlreadyExists: false,
                StorageKey: packagePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to store sync package '{PackageId}' to quarantine path '{Path}'.",
                packageId, packagePath);

            // Clean up any partial write before propagating.
            TryDeleteFile(packagePath + ".tmp");

            throw;
        }
    }

    /// <inheritdoc />
    public Task<bool> ExistsAsync(Guid packageId, CancellationToken cancellationToken = default)
    {
        var exists = File.Exists(BuildPackagePath(packageId));
        return Task.FromResult(exists);
    }

    // ==================== PRIVATE HELPERS ====================

    /// <summary>
    /// Builds the full file-system path for a package's <c>.uhc</c> blob.
    /// </summary>
    private string BuildPackagePath(Guid packageId) =>
        Path.Combine(_quarantinePath, packageId.ToString("N") + PackageExtension);

    /// <summary>
    /// Builds the full file-system path for a package's companion checksum file.
    /// </summary>
    private string BuildChecksumPath(Guid packageId) =>
        Path.Combine(_quarantinePath, packageId.ToString("N") + ChecksumExtension);

    /// <summary>
    /// Silently deletes a file if it exists.
    /// Used for cleanup after a partial write; exceptions are swallowed to avoid
    /// masking the original failure.
    /// </summary>
    private void TryDeleteFile(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not delete temporary file '{Path}' during cleanup.", path);
        }
    }
}
