using TRRCMS.Application.Documents.Dtos;
using TRRCMS.Application.Evidences.Dtos;

namespace TRRCMS.Application.Conflicts.Dtos;

/// <summary>
/// Side-by-side document/evidence comparison for a conflict pair.
/// UC-008 S04: Document viewer data for person duplicate review.
/// Also supports UC-007 for property unit document comparison.
/// </summary>
public sealed record DocumentComparisonDto(
    Guid ConflictId,
    string ConflictType,
    string EntityType,
    EntityDocumentsDto FirstEntity,
    EntityDocumentsDto SecondEntity
);

/// <summary>
/// All documents and evidence belonging to one entity in a conflict pair.
/// </summary>
public sealed record EntityDocumentsDto(
    Guid EntityId,
    string? EntityIdentifier,
    IReadOnlyList<EvidenceDto> Evidences,
    IReadOnlyList<DocumentDto> Documents
);
