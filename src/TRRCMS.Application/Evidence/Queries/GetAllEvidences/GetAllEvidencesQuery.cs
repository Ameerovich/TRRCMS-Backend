using MediatR;
using TRRCMS.Application.Evidences.Dtos;

namespace TRRCMS.Application.Evidences.Queries.GetAllEvidences;

/// <summary>
/// Query to get all evidences
/// </summary>
public class GetAllEvidencesQuery : IRequest<IEnumerable<EvidenceDto>>
{
}
