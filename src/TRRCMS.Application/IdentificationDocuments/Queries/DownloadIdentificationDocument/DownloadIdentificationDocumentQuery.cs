using MediatR;

namespace TRRCMS.Application.IdentificationDocuments.Queries.DownloadIdentificationDocument;

public record DownloadIdentificationDocumentQuery(Guid PersonId, Guid DocumentId) : IRequest<Stream>;
