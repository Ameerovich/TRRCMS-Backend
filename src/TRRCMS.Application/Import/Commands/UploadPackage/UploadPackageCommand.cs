using MediatR;
using TRRCMS.Application.Import.Dtos;

namespace TRRCMS.Application.Import.Commands.UploadPackage;

/// <summary>
/// Command to upload a .uhc package for import.
/// Triggers the full intake pipeline: save → parse manifest → integrity check →
/// vocabulary compatibility → create ImportPackage entity.
/// 
/// UC-003 Stage 2 — S12 (Verify Package Integrity).
/// FSD: FR-D-2 (Import Management), FR-D-3 (Validation & Verification).
/// </summary>
public record UploadPackageCommand : IRequest<UploadPackageResultDto>
{
    /// <summary>
    /// The .uhc file stream (from multipart form upload).
    /// </summary>
    public Stream FileStream { get; init; } = null!;

    /// <summary>
    /// Original filename of the uploaded .uhc file.
    /// </summary>
    public string FileName { get; init; } = string.Empty;

    /// <summary>
    /// File size in bytes (from Content-Length or stream).
    /// </summary>
    public long FileSizeBytes { get; init; }

    /// <summary>
    /// Import method: "Manual", "NetworkSync", or "WatchedFolder".
    /// Default is "Manual" for desktop uploads.
    /// </summary>
    public string ImportMethod { get; init; } = "Manual";
}
