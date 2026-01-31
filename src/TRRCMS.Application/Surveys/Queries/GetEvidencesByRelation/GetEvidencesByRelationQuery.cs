using MediatR;
using TRRCMS.Application.Evidences.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Surveys.Queries.GetEvidencesByRelation;

/// <summary>
/// Query to get all evidences for a specific person-property relation (صور المستندات)
/// </summary>
public class GetEvidencesByRelationQuery : IRequest<List<EvidenceDto>>
{
    public Guid SurveyId { get; set; }
    public Guid RelationId { get; set; }

    /// <summary>
    /// Optional: Filter by evidence type enum
    /// </summary>
    public EvidenceType? EvidenceType { get; set; }

    /// <summary>
    /// Include only current versions (default: true)
    /// </summary>
    public bool OnlyCurrentVersions { get; set; } = true;
}
