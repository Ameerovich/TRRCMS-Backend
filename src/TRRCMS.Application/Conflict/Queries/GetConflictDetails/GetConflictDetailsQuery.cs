using MediatR;
using TRRCMS.Application.Conflicts.Dtos;

namespace TRRCMS.Application.Conflicts.Queries.GetConflictDetails;

/// <summary>
/// Query to retrieve full conflict details for the side-by-side review screen.
/// /// </summary>
public class GetConflictDetailsQuery : IRequest<ConflictDetailDto>
{
    /// <summary>
    /// Conflict resolution surrogate ID.
    /// </summary>
    public Guid Id { get; set; }
}
