using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Models;
using TRRCMS.Application.Evidences.Dtos;

namespace TRRCMS.Application.Evidences.Queries.GetAllEvidences;

/// <summary>
/// Handler for GetAllEvidencesQuery
/// </summary>
public class GetAllEvidencesQueryHandler : IRequestHandler<GetAllEvidencesQuery, PagedResult<EvidenceDto>>
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

    public async Task<PagedResult<EvidenceDto>> Handle(GetAllEvidencesQuery request, CancellationToken cancellationToken)
    {
        var evidences = await _evidenceRepository.GetAllAsync(cancellationToken);
        var dtos = _mapper.Map<List<EvidenceDto>>(evidences);
        return PaginatedList.FromEnumerable(dtos, request.PageNumber, request.PageSize);
    }
}
