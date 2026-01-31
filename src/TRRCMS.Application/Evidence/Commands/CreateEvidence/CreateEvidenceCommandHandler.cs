using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Evidences.Dtos;

namespace TRRCMS.Application.Evidences.Commands.CreateEvidence;

public class CreateEvidenceCommandHandler : IRequestHandler<CreateEvidenceCommand, EvidenceDto>
{
    private readonly IEvidenceRepository _evidenceRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public CreateEvidenceCommandHandler(
        IEvidenceRepository evidenceRepository,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _evidenceRepository = evidenceRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<EvidenceDto> Handle(CreateEvidenceCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        if (string.IsNullOrWhiteSpace(request.Description))
            throw new ArgumentException("Description is required", nameof(request.Description));

        if (string.IsNullOrWhiteSpace(request.OriginalFileName))
            throw new ArgumentException("OriginalFileName is required", nameof(request.OriginalFileName));

        if (string.IsNullOrWhiteSpace(request.FilePath))
            throw new ArgumentException("FilePath is required", nameof(request.FilePath));

        if (string.IsNullOrWhiteSpace(request.MimeType))
            throw new ArgumentException("MimeType is required", nameof(request.MimeType));

        // Create evidence using the EvidenceType enum
        var evidence = Domain.Entities.Evidence.Create(
            request.EvidenceType,
            request.Description,
            request.OriginalFileName,
            request.FilePath,
            request.FileSizeBytes,
            request.MimeType,
            request.FileHash,
            currentUserId);

        if (request.PersonId.HasValue)
            evidence.LinkToPerson(request.PersonId.Value, currentUserId);

        if (request.PersonPropertyRelationId.HasValue)
            evidence.LinkToRelation(request.PersonPropertyRelationId.Value, currentUserId);

        if (request.ClaimId.HasValue)
            evidence.LinkToClaim(request.ClaimId.Value, currentUserId);

        if (request.DocumentIssuedDate.HasValue ||
            request.DocumentExpiryDate.HasValue ||
            !string.IsNullOrWhiteSpace(request.IssuingAuthority) ||
            !string.IsNullOrWhiteSpace(request.DocumentReferenceNumber) ||
            !string.IsNullOrWhiteSpace(request.Notes))
        {
            evidence.UpdateMetadata(
                request.DocumentIssuedDate,
                request.DocumentExpiryDate,
                request.IssuingAuthority,
                request.DocumentReferenceNumber,
                request.Notes,
                currentUserId);
        }

        await _evidenceRepository.AddAsync(evidence, cancellationToken);
        await _evidenceRepository.SaveChangesAsync(cancellationToken);

        var result = _mapper.Map<EvidenceDto>(evidence);
        result.IsExpired = evidence.IsExpired();
        return result;
    }
}
