using MediatR;

namespace TRRCMS.Application.Buildings.Queries.DownloadBuildingDocument;

public record DownloadBuildingDocumentQuery(Guid DocumentId)
    : IRequest<DownloadBuildingDocumentResult>;

/// <summary>
/// Result of downloading a building document: the file stream plus the
/// metadata needed to set correct response headers (Content-Type, filename).
/// </summary>
public class DownloadBuildingDocumentResult
{
    public Stream FileStream { get; set; } = null!;
    public string FileName { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
}
