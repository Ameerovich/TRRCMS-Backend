using MediatR;
using TRRCMS.Application.Evidences.Dtos;

namespace TRRCMS.Application.Evidences.Queries.GetEvidence;

/// <summary>
/// Query to get evidence by ID
/// </summary>
public class GetEvidenceQuery : IRequest<EvidenceDto?>
{
    public Guid Id { get; }

    public GetEvidenceQuery(Guid id)
    {
        Id = id;
    }
}
