namespace TRRCMS.Application.Sync.DTOs;

/// <summary>
/// Result of a sync package upload (Sync Protocol Step 2).
/// </summary>
/// <param name="Accepted">Whether the package was accepted by the server.</param>
/// <param name="PackageId">The package GUID from the tablet manifest.</param>
/// <param name="IsDuplicate">True if the package was already received (idempotent).</param>
/// <param name="Message">Human-readable status message.</param>
/// <param name="ImportPackageId">
/// The <c>ImportPackage.Id</c> created by the import pipeline, if the package
/// was successfully fed into the pipeline. Null if the import step was skipped
/// or failed (the file is still safely stored in quarantine for manual re-import).
/// </param>
public sealed record UploadSyncPackageResultDto(
    bool Accepted,
    Guid PackageId,
    bool IsDuplicate,
    string Message,
    Guid? ImportPackageId = null
);
