namespace TRRCMS.Application.Sync.DTOs;

/// <summary>
/// Upload payload excluding the raw file bytes (passed separately as stream).
/// Only the sync session ID is required — all other metadata (PackageId, DeviceId,
/// SchemaVersion, etc.) is extracted from the .uhc manifest table automatically.
/// </summary>
/// <param name="SyncSessionId">The sync session ID from Step 1.</param>
/// <param name="Sha256Checksum">
/// Optional client-side SHA-256 content checksum for extra integrity verification.
/// If provided, compared against the server-computed checksum as a defense-in-depth measure.
/// If omitted, the server still verifies using the checksum from the .uhc manifest.
/// </param>
public sealed record UploadSyncPackageDto(
    Guid SyncSessionId,
    string? Sha256Checksum = null
);
