using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;

namespace TRRCMS.Application.Buildings.Queries.DownloadBuildingDocument;

public class DownloadBuildingDocumentQueryHandler
    : IRequestHandler<DownloadBuildingDocumentQuery, DownloadBuildingDocumentResult>
{
    private readonly IBuildingDocumentRepository _repository;
    private readonly IFileStorageService _fileStorageService;

    public DownloadBuildingDocumentQueryHandler(
        IBuildingDocumentRepository repository,
        IFileStorageService fileStorageService)
    {
        _repository = repository;
        _fileStorageService = fileStorageService;
    }

    public async Task<DownloadBuildingDocumentResult> Handle(
        DownloadBuildingDocumentQuery request,
        CancellationToken cancellationToken)
    {
        var document = await _repository.GetByIdAsync(request.DocumentId, cancellationToken);

        if (document == null)
            throw new NotFoundException($"Building document {request.DocumentId} not found");

        if (string.IsNullOrEmpty(document.FilePath))
            throw new NotFoundException("Document has no associated file");

        if (!await _fileStorageService.FileExistsAsync(document.FilePath))
            throw new NotFoundException("Document file is missing on the server");

        var fileStream = await _fileStorageService.GetFileAsync(document.FilePath, cancellationToken);

        return new DownloadBuildingDocumentResult
        {
            FileStream = fileStream,
            FileName = string.IsNullOrWhiteSpace(document.OriginalFileName)
                ? $"{document.Id}"
                : document.OriginalFileName,
            MimeType = string.IsNullOrWhiteSpace(document.MimeType)
                ? "application/octet-stream"
                : document.MimeType
        };
    }
}
