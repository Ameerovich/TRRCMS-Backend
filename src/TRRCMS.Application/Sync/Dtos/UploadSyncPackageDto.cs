namespace TRRCMS.Application.Sync.DTOs;

/// <summary>
/// Upload payload excluding the raw file bytes (passed separately as stream).
/// </summary>
public sealed record UploadSyncPackageDto(
    Guid SyncSessionId,
    Guid PackageId,
    string DeviceId,
    DateTime CreatedUtc,
    string SchemaVersion,
    string AppVersion,
    string? VocabVersionsJson,
    string? FormSchemaVersion,
    string Sha256Checksum
);
