using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Application.IdentificationDocuments.Queries.DownloadIdentificationDocument;

public class DownloadIdentificationDocumentQueryHandler : IRequestHandler<DownloadIdentificationDocumentQuery, Stream>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;

    public DownloadIdentificationDocumentQueryHandler(
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorageService)
    {
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
    }

    public async Task<Stream> Handle(
        DownloadIdentificationDocumentQuery request,
        CancellationToken cancellationToken)
    {
        var document = await _unitOfWork.IdentificationDocuments
            .GetByIdAsync(request.DocumentId, cancellationToken);

        if (document == null)
            throw new NotFoundException($"Identification document {request.DocumentId} not found");

        if (document.PersonId != request.PersonId)
            throw new NotFoundException("Document does not belong to the specified person");

        if (string.IsNullOrEmpty(document.FilePath))
            throw new NotFoundException("Document has no associated file");

        var fileStream = await _fileStorageService.GetFileAsync(document.FilePath, cancellationToken);
        return fileStream;
    }
}
