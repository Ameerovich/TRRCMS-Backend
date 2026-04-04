using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Conflicts.Dtos;
using TRRCMS.Application.Evidences.Dtos;
using TRRCMS.Application.IdentificationDocuments.Dtos;

namespace TRRCMS.Application.Conflicts.Queries.GetConflictDocumentComparison;

/// <summary>
/// Handler for <see cref="GetConflictDocumentComparisonQuery"/>.
///
/// Loads evidence for both entities in a conflict pair,
/// enabling side-by-side comparison in the review UI.
/// </summary>
public class GetConflictDocumentComparisonQueryHandler
    : IRequestHandler<GetConflictDocumentComparisonQuery, DocumentComparisonDto?>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetConflictDocumentComparisonQueryHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<DocumentComparisonDto?> Handle(
        GetConflictDocumentComparisonQuery request,
        CancellationToken cancellationToken)
    {
        var conflict = await _uow.ConflictResolutions.GetByIdAsync(
            request.ConflictId, cancellationToken);

        if (conflict is null)
            return null;

        var firstEntity = await LoadEntityEvidenceAsync(
            conflict.EntityType, conflict.FirstEntityId,
            conflict.FirstEntityIdentifier, cancellationToken);

        var secondEntity = await LoadEntityEvidenceAsync(
            conflict.EntityType, conflict.SecondEntityId,
            conflict.SecondEntityIdentifier, cancellationToken);

        return new DocumentComparisonDto(
            ConflictId: conflict.Id,
            ConflictType: conflict.ConflictType,
            EntityType: conflict.EntityType,
            FirstEntity: firstEntity,
            SecondEntity: secondEntity
        );
    }

    private async Task<EntityDocumentsDto> LoadEntityEvidenceAsync(
        string entityType, Guid entityId, string? entityIdentifier,
        CancellationToken ct)
    {
        IReadOnlyList<EvidenceDto> evidenceDtos;
        IReadOnlyList<IdentificationDocumentDto> idDocDtos = Array.Empty<IdentificationDocumentDto>();

        if (entityType == "Person")
        {
            // Evidence is linked through EvidenceRelation, not directly to Person.
            // Load identification documents for person-level document comparison.
            var idDocs = await _uow.IdentificationDocuments.GetByPersonIdAsync(entityId, ct);
            idDocDtos = _mapper.Map<List<IdentificationDocumentDto>>(idDocs);
            evidenceDtos = Array.Empty<EvidenceDto>();
        }
        else // PropertyUnit
        {
            var relations = await _uow.PersonPropertyRelations
                .GetByPropertyUnitIdAsync(entityId, ct);

            var allEvidences = new List<EvidenceDto>();
            foreach (var relation in relations)
            {
                var relEvidences = await _uow.Evidences.GetByRelationIdAsync(relation.Id, ct);
                allEvidences.AddRange(_mapper.Map<List<EvidenceDto>>(relEvidences));
            }

            evidenceDtos = allEvidences
                .GroupBy(e => e.Id)
                .Select(g => g.First())
                .ToList();
        }

        return new EntityDocumentsDto(
            EntityId: entityId,
            EntityIdentifier: entityIdentifier,
            Evidences: evidenceDtos,
            IdentificationDocuments: idDocDtos
        );
    }
}
