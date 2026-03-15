using AutoMapper;
using MediatR;
using TRRCMS.Application.Claims.Dtos;
using TRRCMS.Application.Common.Interfaces;

namespace TRRCMS.Application.Claims.Queries.GetClaim;

/// <summary>
/// Handler for GetClaimQuery
/// Retrieves a single claim by ID with all navigation properties
/// </summary>
public class GetClaimQueryHandler : IRequestHandler<GetClaimQuery, ClaimDto?>
{
    private readonly IClaimRepository _claimRepository;
    private readonly IPersonPropertyRelationRepository _relationRepository;
    private readonly IEvidenceRelationRepository _evidenceRelationRepository;
    private readonly IMapper _mapper;

    public GetClaimQueryHandler(
        IClaimRepository claimRepository,
        IPersonPropertyRelationRepository relationRepository,
        IEvidenceRelationRepository evidenceRelationRepository,
        IMapper mapper)
    {
        _claimRepository = claimRepository ?? throw new ArgumentNullException(nameof(claimRepository));
        _relationRepository = relationRepository ?? throw new ArgumentNullException(nameof(relationRepository));
        _evidenceRelationRepository = evidenceRelationRepository ?? throw new ArgumentNullException(nameof(evidenceRelationRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<ClaimDto?> Handle(GetClaimQuery request, CancellationToken cancellationToken)
    {
        var claim = await _claimRepository.GetByIdAsync(request.Id, cancellationToken);

        if (claim == null)
        {
            return null;
        }

        var dto = _mapper.Map<ClaimDto>(claim);

        // Enrich with evidence from the source PersonPropertyRelation
        if (claim.PrimaryClaimantId.HasValue)
        {
            var relation = await _relationRepository.GetByPersonAndPropertyUnitAsync(
                claim.PrimaryClaimantId.Value, claim.PropertyUnitId, cancellationToken);

            if (relation != null)
            {
                dto.SourceRelationId = relation.Id;
                var activeLinks = await _evidenceRelationRepository
                    .GetActiveByRelationIdAsync(relation.Id, cancellationToken);
                dto.EvidenceIds = activeLinks.Select(er => er.EvidenceId).ToList();
                dto.HasEvidence = dto.EvidenceIds.Count > 0 || (claim.Evidences != null && claim.Evidences.Any());
            }
        }

        return dto;
    }
}
