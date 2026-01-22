using MediatR;

namespace TRRCMS.Application.Surveys.Queries.DownloadEvidence;

/// <summary>
/// Query to download evidence file
/// Returns file stream and metadata for download
/// </summary>
public class DownloadEvidenceQuery : IRequest<DownloadEvidenceResult>
{
    /// <summary>
    /// Evidence ID to download
    /// </summary>
    public Guid EvidenceId { get; set; }
}

/// <summary>
/// Result containing file stream and metadata
/// </summary>
public class DownloadEvidenceResult
{
    public Stream FileStream { get; set; } = null!;
    public string FileName { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
}