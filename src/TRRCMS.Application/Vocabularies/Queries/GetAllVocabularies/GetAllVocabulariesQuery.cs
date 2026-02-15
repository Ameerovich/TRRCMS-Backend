using MediatR;
using TRRCMS.Application.Vocabularies.Dtos;

namespace TRRCMS.Application.Vocabularies.Queries.GetAllVocabularies;

/// <summary>
/// Query to retrieve all current vocabularies, optionally filtered by category.
/// Public endpoint — clients fetch on startup and cache locally.
/// </summary>
public record GetAllVocabulariesQuery : IRequest<List<VocabularyDto>>
{
    /// <summary>
    /// Optional category filter (e.g., "Demographics", "Property", "Legal").
    /// </summary>
    public string? Category { get; init; }
}
