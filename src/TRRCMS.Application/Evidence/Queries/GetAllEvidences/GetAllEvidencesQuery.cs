using MediatR;
using TRRCMS.Application.Common.Models;
using TRRCMS.Application.Evidences.Dtos;

namespace TRRCMS.Application.Evidences.Queries.GetAllEvidences;

/// <summary>
/// Query to get all evidences with pagination
/// </summary>
public class GetAllEvidencesQuery : PagedQuery, IRequest<PagedResult<EvidenceDto>>
{
}
