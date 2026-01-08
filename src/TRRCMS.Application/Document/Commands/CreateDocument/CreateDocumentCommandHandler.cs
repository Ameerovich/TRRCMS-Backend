using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Documents.Dtos;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Application.Documents.Commands.CreateDocument;

/// <summary>
/// Handler for CreateDocumentCommand
/// </summary>
public class CreateDocumentCommandHandler : IRequestHandler<CreateDocumentCommand, DocumentDto>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IMapper _mapper;

    public CreateDocumentCommandHandler(
        IDocumentRepository documentRepository,
        IMapper mapper)
    {
        _documentRepository = documentRepository;
        _mapper = mapper;
    }

    public async Task<DocumentDto> Handle(CreateDocumentCommand request, CancellationToken cancellationToken)
    {
        // Validate required fields
        if (request.CreatedByUserId == Guid.Empty)
            throw new ArgumentException("CreatedByUserId is required", nameof(request.CreatedByUserId));

        // Convert DateTime values to UTC if they have a value
        var issueDateUtc = request.IssueDate.HasValue
            ? DateTime.SpecifyKind(request.IssueDate.Value, DateTimeKind.Utc)
            : (DateTime?)null;

        var expiryDateUtc = request.ExpiryDate.HasValue
            ? DateTime.SpecifyKind(request.ExpiryDate.Value, DateTimeKind.Utc)
            : (DateTime?)null;

        // Create document using factory method
        var document = Document.Create(
            request.DocumentType,
            request.DocumentNumber,
            request.DocumentTitle,
            issueDateUtc,
            request.IssuingAuthority,
            request.CreatedByUserId);

        // Update document details if additional info provided
        if (expiryDateUtc.HasValue ||
            !string.IsNullOrWhiteSpace(request.IssuingPlace) ||
            !string.IsNullOrWhiteSpace(request.Notes))
        {
            document.UpdateDocumentDetails(
                request.DocumentNumber,
                request.DocumentTitle,
                issueDateUtc,
                expiryDateUtc,
                request.IssuingAuthority,
                request.IssuingPlace,
                request.Notes,
                request.CreatedByUserId);
        }

        // Link to evidence if provided
        if (request.EvidenceId.HasValue)
        {
            document.LinkToEvidence(request.EvidenceId.Value, request.DocumentHash, request.CreatedByUserId);
        }

        // Link to entities if provided
        if (request.PersonId.HasValue)
        {
            document.LinkToPerson(request.PersonId.Value, request.CreatedByUserId);
        }

        if (request.PropertyUnitId.HasValue)
        {
            document.LinkToPropertyUnit(request.PropertyUnitId.Value, request.CreatedByUserId);
        }

        if (request.PersonPropertyRelationId.HasValue)
        {
            document.LinkToRelation(request.PersonPropertyRelationId.Value, request.CreatedByUserId);
        }

        if (request.ClaimId.HasValue)
        {
            document.LinkToClaim(request.ClaimId.Value, request.CreatedByUserId);
        }

        // Add to repository
        await _documentRepository.AddAsync(document, cancellationToken);
        await _documentRepository.SaveChangesAsync(cancellationToken);

        // Map to DTO
        return _mapper.Map<DocumentDto>(document);
    }
}
