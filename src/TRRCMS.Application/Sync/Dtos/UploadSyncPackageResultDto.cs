namespace TRRCMS.Application.Sync.DTOs;

public sealed record UploadSyncPackageResultDto(
    bool Accepted,
    Guid PackageId,
    bool IsDuplicate,
    string Message
);
