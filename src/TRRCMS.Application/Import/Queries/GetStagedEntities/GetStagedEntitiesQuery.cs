using MediatR;
using TRRCMS.Application.Import.Dtos;

namespace TRRCMS.Application.Import.Queries.GetStagedEntities;

/// <summary>
/// Query to retrieve all staged entities for an import package.
/// Returns entities grouped by type with their IDs, identifiers, and validation status.
///
/// Used by the conflict resolution UI to display entity details and allow
/// the data manager to identify which record is staging vs production.
/// </summary>
public class GetStagedEntitiesQuery : IRequest<GetStagedEntitiesResponse>
{
    /// <summary>The import package to retrieve staged entities for.</summary>
    public Guid ImportPackageId { get; init; }

    /// <summary>Optional filter: only return entities of this type (e.g. "Person", "PropertyUnit").</summary>
    public string? EntityTypeFilter { get; init; }
}
