using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Evidences.Dtos;

namespace TRRCMS.Application.Surveys.Queries.GetEvidenceById;

/// <summary>
/// Handler for GetEvidenceByIdQuery
/// </summary>
public class GetEvidenceByIdQueryHandler : IRequestHandler<GetEvidenceByIdQuery, EvidenceDto>
{
    private readonly IEvidenceRepository _evidenceRepository;
    private readonly IMapper _mapper;

    public GetEvidenceByIdQueryHandler(
        IEvidenceRepository evidenceRepository,
        IMapper mapper)
    {
        _evidenceRepository = evidenceRepository ?? throw new ArgumentNullException(nameof(evidenceRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<EvidenceDto> Handle(GetEvidenceByIdQuery request, CancellationToken cancellationToken)
    {
        var evidence = await _evidenceRepository.GetByIdAsync(request.EvidenceId, cancellationToken);
        if (evidence == null)
        {
            throw new NotFoundException($"Evidence with ID {request.EvidenceId} not found");
        }

        var result = _mapper.Map<EvidenceDto>(evidence);
        result.IsExpired = evidence.IsExpired();

        return result;
    }
}