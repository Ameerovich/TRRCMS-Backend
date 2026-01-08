using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Evidences.Dtos;

namespace TRRCMS.Application.Evidences.Queries.GetEvidence;

/// <summary>
/// Handler for GetEvidenceQuery
/// </summary>
public class GetEvidenceQueryHandler : IRequestHandler<GetEvidenceQuery, EvidenceDto?>
{
    private readonly IEvidenceRepository _evidenceRepository;
    private readonly IMapper _mapper;

    public GetEvidenceQueryHandler(
        IEvidenceRepository evidenceRepository,
        IMapper mapper)
    {
        _evidenceRepository = evidenceRepository;
        _mapper = mapper;
    }

    public async Task<EvidenceDto?> Handle(GetEvidenceQuery request, CancellationToken cancellationToken)
    {
        var evidence = await _evidenceRepository.GetByIdAsync(request.Id, cancellationToken);
        return evidence == null ? null : _mapper.Map<EvidenceDto>(evidence);
    }
}
