using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Evidences.Dtos;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Application.Evidences.Commands.CreateEvidence;

/// <summary>
/// Handler for CreateEvidenceCommand
/// </summary>
public class CreateEvidenceCommandHandler : IRequestHandler<CreateEvidenceCommand, EvidenceDto>
{
    private readonly IEvidenceRepository _evidenceRepository;
    private readonly IMapper _mapper;

    public CreateEvidenceCommandHandler(
        IEvidenceRepository evidenceRepository,
        IMapper mapper)
    {
        _evidenceRepository = evidenceRepository;
        _mapper = mapper;
    }

    public async Task<EvidenceDto> Handle(CreateEvidenceCommand request, CancellationToken cancellationToken)
    {
        // Validate required fields
        if (string.IsNullOrWhiteSpace(request.EvidenceType))
            throw new ArgumentException("EvidenceType is required", nameof(request.EvidenceType));

        if (string.IsNullOrWhiteSpace(request.Description))
            throw new ArgumentException("Description is required", nameof(request.Description));

        if (string.IsNullOrWhiteSpace(request.OriginalFileName))
            throw new ArgumentException("OriginalFileName is required", nameof(request.OriginalFileName));

        if (string.IsNullOrWhiteSpace(request.FilePath))
            throw new ArgumentException("FilePath is required", nameof(request.FilePath));

        if (request.FileSizeBytes <= 0)
            throw new ArgumentException("FileSizeBytes must be greater than 0", nameof(request.FileSizeBytes));

        if (string.IsNullOrWhiteSpace(request.MimeType))
            throw new ArgumentException("MimeType is required", nameof(request.MimeType));

        if (request.CreatedByUserId == Guid.Empty)
            throw new ArgumentException("CreatedByUserId is required", nameof(request.CreatedByUserId));

        // Create evidence using factory method
        var evidence = Evidence.Create(
            request.EvidenceType,
            request.Description,
            request.OriginalFileName,
            request.FilePath,
            request.FileSizeBytes,
            request.MimeType,
            request.FileHash,
            request.CreatedByUserId);

        // Convert DateTime values to UTC if they have a value
        var issuedDateUtc = request.DocumentIssuedDate.HasValue
            ? DateTime.SpecifyKind(request.DocumentIssuedDate.Value, DateTimeKind.Utc)
            : (DateTime?)null;

        var expiryDateUtc = request.DocumentExpiryDate.HasValue
            ? DateTime.SpecifyKind(request.DocumentExpiryDate.Value, DateTimeKind.Utc)
            : (DateTime?)null;

        // Update metadata if provided
        if (issuedDateUtc.HasValue ||
            expiryDateUtc.HasValue ||
            !string.IsNullOrWhiteSpace(request.IssuingAuthority) ||
            !string.IsNullOrWhiteSpace(request.DocumentReferenceNumber) ||
            !string.IsNullOrWhiteSpace(request.Notes))
        {
            evidence.UpdateMetadata(
                issuedDateUtc,
                expiryDateUtc,
                request.IssuingAuthority,
                request.DocumentReferenceNumber,
                request.Notes,
                request.CreatedByUserId);
        }

        // Link to entities if provided
        if (request.PersonId.HasValue)
        {
            evidence.LinkToPerson(request.PersonId.Value, request.CreatedByUserId);
        }

        if (request.PersonPropertyRelationId.HasValue)
        {
            evidence.LinkToRelation(request.PersonPropertyRelationId.Value, request.CreatedByUserId);
        }

        if (request.ClaimId.HasValue)
        {
            evidence.LinkToClaim(request.ClaimId.Value, request.CreatedByUserId);
        }

        // Add to repository
        await _evidenceRepository.AddAsync(evidence, cancellationToken);
        await _evidenceRepository.SaveChangesAsync(cancellationToken);

        // Map to DTO
        return _mapper.Map<EvidenceDto>(evidence);
    }
}
