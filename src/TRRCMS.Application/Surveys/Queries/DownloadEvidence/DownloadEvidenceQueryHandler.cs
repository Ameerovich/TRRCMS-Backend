using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;

namespace TRRCMS.Application.Surveys.Queries.DownloadEvidence;

/// <summary>
/// Handler for downloading evidence files
/// </summary>
public class DownloadEvidenceQueryHandler : IRequestHandler<DownloadEvidenceQuery, DownloadEvidenceResult>
{
    private readonly IEvidenceRepository _evidenceRepository;
    private readonly IFileStorageService _fileStorageService;

    public DownloadEvidenceQueryHandler(
        IEvidenceRepository evidenceRepository,
        IFileStorageService fileStorageService)
    {
        _evidenceRepository = evidenceRepository ?? throw new ArgumentNullException(nameof(evidenceRepository));
        _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
    }

    public async Task<DownloadEvidenceResult> Handle(DownloadEvidenceQuery request, CancellationToken cancellationToken)
    {
        // Get evidence details
        var evidence = await _evidenceRepository.GetByIdAsync(request.EvidenceId, cancellationToken);
        if (evidence == null)
        {
            throw new NotFoundException($"Evidence with ID {request.EvidenceId} not found");
        }

        // Check if file exists
        var fileExists = await _fileStorageService.FileExistsAsync(evidence.FilePath);
        if (!fileExists)
        {
            throw new NotFoundException($"File not found: {evidence.OriginalFileName}");
        }

        // Get file stream
        var fileStream = await _fileStorageService.GetFileAsync(evidence.FilePath, cancellationToken);

        // Return result
        return new DownloadEvidenceResult
        {
            FileStream = fileStream,
            FileName = evidence.OriginalFileName,
            MimeType = evidence.MimeType,
            FileSizeBytes = evidence.FileSizeBytes
        };
    }
}