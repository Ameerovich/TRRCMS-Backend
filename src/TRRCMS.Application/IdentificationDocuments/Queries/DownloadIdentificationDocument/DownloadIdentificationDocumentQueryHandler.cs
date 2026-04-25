using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;

namespace TRRCMS.Application.IdentificationDocuments.Queries.DownloadIdentificationDocument;

public class DownloadIdentificationDocumentQueryHandler : IRequestHandler<DownloadIdentificationDocumentQuery, FileStream>
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

    public async Task<FileStream> Handle(
        DownloadIdentificationDocumentQuery request,
        CancellationToken cancellationToken)
    {
        var document = await _unitOfWork.IdentificationDocuments
            .GetByIdAsync(request.DocumentId, cancellationToken);

        if (document == null)
            throw new NotFoundException(nameof(IdentificationDocument), request.DocumentId);

        if (document.PersonId != request.PersonId)
            throw new NotFoundException("Document does not belong to the specified person");

        if (string.IsNullOrEmpty(document.FilePath))
            throw new NotFoundException("Document has no associated file");

        var fileStream = (FileStream)await _fileStorageService.GetFileAsync(document.FilePath, cancellationToken);
        return fileStream;
    }
}
