using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Evidences.Dtos;

namespace TRRCMS.Application.Evidences.Queries.GetAllEvidences;

/// <summary>
/// Handler for GetAllEvidencesQuery
/// </summary>
public class GetAllEvidencesQueryHandler : IRequestHandler<GetAllEvidencesQuery, IEnumerable<EvidenceDto>>
{
    private readonly IEvidenceRepository _evidenceRepository;
    private readonly IMapper _mapper;

    public GetAllEvidencesQueryHandler(
        IEvidenceRepository evidenceRepository,
        IMapper mapper)
    {
        _evidenceRepository = evidenceRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<EvidenceDto>> Handle(GetAllEvidencesQuery request, CancellationToken cancellationToken)
    {
        var evidences = await _evidenceRepository.GetAllAsync(cancellationToken);
        return _mapper.Map<IEnumerable<EvidenceDto>>(evidences);
    }
}
