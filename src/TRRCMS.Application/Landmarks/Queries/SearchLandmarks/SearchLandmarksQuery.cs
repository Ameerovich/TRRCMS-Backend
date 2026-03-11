using MediatR;
using TRRCMS.Application.Landmarks.Dtos;

namespace TRRCMS.Application.Landmarks.Queries.SearchLandmarks;

/// <summary>
/// Search landmarks by name with optional type filter.
/// Used by desk officers to locate landmarks based on applicant descriptions.
/// </summary>
public record SearchLandmarksQuery : IRequest<List<LandmarkDto>>
{
    /// <summary>Search query (partial name match)</summary>
    /// <example>جامع</example>
    public string Query { get; init; } = string.Empty;

    /// <summary>Optional filter by landmark type</summary>
    public int? Type { get; init; }

    /// <summary>Max results to return (default 50)</summary>
    public int MaxResults { get; init; } = 50;
}
