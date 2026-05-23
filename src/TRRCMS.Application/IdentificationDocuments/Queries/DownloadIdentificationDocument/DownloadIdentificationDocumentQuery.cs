using MediatR;

namespace TRRCMS.Application.IdentificationDocuments.Queries.DownloadIdentificationDocument;

public record DownloadIdentificationDocumentQuery(Guid PersonId, Guid DocumentId)
    : IRequest<DownloadIdentificationDocumentResult>;

/// <summary>
/// Result of downloading an identification document: the file stream plus the
/// metadata needed to set correct response headers (Content-Type, filename).
/// </summary>
public class DownloadIdentificationDocumentResult
{
    public Stream FileStream { get; set; } = null!;
    public string FileName { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
}
