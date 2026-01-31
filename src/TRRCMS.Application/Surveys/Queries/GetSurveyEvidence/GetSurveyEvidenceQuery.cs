using MediatR;
using TRRCMS.Application.Evidences.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Surveys.Queries.GetSurveyEvidence;

/// <summary>
/// Query to get all evidence for a survey
/// </summary>
public class GetSurveyEvidenceQuery : IRequest<List<EvidenceDto>>
{
    public Guid SurveyId { get; set; }

    /// <summary>
    /// Optional filter by evidence type enum
    /// </summary>
    public EvidenceType? EvidenceType { get; set; }
}
