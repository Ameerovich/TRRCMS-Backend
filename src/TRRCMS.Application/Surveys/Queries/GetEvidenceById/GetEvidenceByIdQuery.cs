using MediatR;
using TRRCMS.Application.Evidences.Dtos;

namespace TRRCMS.Application.Surveys.Queries.GetEvidenceById;

/// <summary>
/// Query to get specific evidence details
/// </summary>
public class GetEvidenceByIdQuery : IRequest<EvidenceDto>
{
    /// <summary>
    /// Evidence ID to retrieve
    /// </summary>
    public Guid EvidenceId { get; set; }
}