using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Conflicts.Dtos;
using TRRCMS.Application.Documents.Dtos;
using TRRCMS.Application.Evidences.Dtos;

namespace TRRCMS.Application.Conflicts.Queries.GetConflictDocumentComparison;

/// <summary>
/// Handler for <see cref="GetConflictDocumentComparisonQuery"/>.
///
/// Loads documents and evidence for both entities in a conflict pair,
/// enabling side-by-side document comparison in the review UI.
///
/// UC-008 S04: Person duplicate — loads Evidence and Documents by PersonId.
/// UC-007: Property duplicate — loads Documents by PropertyUnitId.
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
        // ── 1. Load the conflict ────────────────────────────────────────────────
        var conflict = await _uow.ConflictResolutions.GetByIdAsync(
            request.ConflictId, cancellationToken);

        if (conflict is null)
            return null;

        // ── 2. Load documents/evidence for both entities ────────────────────────
        var firstEntity = await LoadEntityDocumentsAsync(
            conflict.EntityType, conflict.FirstEntityId,
            conflict.FirstEntityIdentifier, cancellationToken);

        var secondEntity = await LoadEntityDocumentsAsync(
            conflict.EntityType, conflict.SecondEntityId,
            conflict.SecondEntityIdentifier, cancellationToken);

        // ── 3. Build result ─────────────────────────────────────────────────────
        return new DocumentComparisonDto(
            ConflictId: conflict.Id,
            ConflictType: conflict.ConflictType,
            EntityType: conflict.EntityType,
            FirstEntity: firstEntity,
            SecondEntity: secondEntity
        );
    }

    private async Task<EntityDocumentsDto> LoadEntityDocumentsAsync(
        string entityType, Guid entityId, string? entityIdentifier,
        CancellationToken ct)
    {
        IReadOnlyList<EvidenceDto> evidenceDtos;
        IReadOnlyList<DocumentDto> documentDtos;

        if (entityType == "Person")
        {
            // Person: load evidence and documents directly by PersonId
            var evidences = await _uow.Evidences.GetByPersonIdAsync(entityId, ct);
            var documents = await _uow.Documents.GetByPersonIdAsync(entityId, ct);

            evidenceDtos = _mapper.Map<List<EvidenceDto>>(evidences);
            documentDtos = _mapper.Map<List<DocumentDto>>(documents);
        }
        else // PropertyUnit
        {
            // PropertyUnit: load documents by PropertyUnitId
            var documents = await _uow.Documents.GetByPropertyUnitIdAsync(entityId, ct);
            documentDtos = _mapper.Map<List<DocumentDto>>(documents);

            // Evidence for property units is linked via PersonPropertyRelations
            var relations = await _uow.PersonPropertyRelations
                .GetByPropertyUnitIdAsync(entityId, ct);

            var allEvidences = new List<EvidenceDto>();
            foreach (var relation in relations)
            {
                var relEvidences = await _uow.Evidences.GetByRelationIdAsync(relation.Id, ct);
                allEvidences.AddRange(_mapper.Map<List<EvidenceDto>>(relEvidences));
            }

            // Deduplicate by evidence ID (same evidence may be linked via multiple relations)
            evidenceDtos = allEvidences
                .GroupBy(e => e.Id)
                .Select(g => g.First())
                .ToList();
        }

        return new EntityDocumentsDto(
            EntityId: entityId,
            EntityIdentifier: entityIdentifier,
            Evidences: evidenceDtos,
            Documents: documentDtos
        );
    }
}
