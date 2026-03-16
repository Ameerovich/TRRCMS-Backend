using TRRCMS.Application.Evidences.Dtos;

namespace TRRCMS.Application.Conflicts.Dtos;

/// <summary>
/// Side-by-side evidence comparison for a conflict pair.
/// </summary>
public sealed record DocumentComparisonDto(
    Guid ConflictId,
    string ConflictType,
    string EntityType,
    EntityDocumentsDto FirstEntity,
    EntityDocumentsDto SecondEntity
);

/// <summary>
/// All evidence belonging to one entity in a conflict pair.
/// </summary>
public sealed record EntityDocumentsDto(
    Guid EntityId,
    string? EntityIdentifier,
    IReadOnlyList<EvidenceDto> Evidences
);
