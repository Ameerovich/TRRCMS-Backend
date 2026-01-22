using MediatR;
using TRRCMS.Application.Evidences.Dtos;

namespace TRRCMS.Application.Surveys.Queries.GetSurveyEvidence;

/// <summary>
/// Query to get all evidence for a survey
/// Returns all photos and documents uploaded in survey context
/// </summary>
public class GetSurveyEvidenceQuery : IRequest<List<EvidenceDto>>
{
    /// <summary>
    /// Survey ID to get evidence for
    /// </summary>
    public Guid SurveyId { get; set; }

    /// <summary>
    /// Optional filter by evidence type
    /// </summary>
    public string? EvidenceType { get; set; }
}