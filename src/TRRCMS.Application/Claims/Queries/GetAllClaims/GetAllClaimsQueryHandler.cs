using AutoMapper;
using MediatR;
using TRRCMS.Application.Claims.Dtos;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Models;

namespace TRRCMS.Application.Claims.Queries.GetAllClaims;

/// <summary>
/// Handler for GetAllClaimsQuery
/// Retrieves claims with optional filtering and pagination
/// </summary>
public class GetAllClaimsQueryHandler : IRequestHandler<GetAllClaimsQuery, PagedResult<ClaimDto>>
{
    private readonly IClaimRepository _claimRepository;
    private readonly IPersonPropertyRelationRepository _relationRepository;
    private readonly IEvidenceRelationRepository _evidenceRelationRepository;
    private readonly IMapper _mapper;

    public GetAllClaimsQueryHandler(
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

    public async Task<PagedResult<ClaimDto>> Handle(GetAllClaimsQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<Domain.Entities.Claim> claims;

        // Apply filtering based on provided parameters
        if (request.PropertyUnitId.HasValue)
        {
            var claim = await _claimRepository.GetByPropertyUnitIdAsync(
                request.PropertyUnitId.Value,
                cancellationToken
            );
            claims = claim != null ? new[] { claim } : Array.Empty<Domain.Entities.Claim>();
        }
        else if (request.PrimaryClaimantId.HasValue)
        {
            claims = await _claimRepository.GetByPrimaryClaimantIdAsync(
                request.PrimaryClaimantId.Value,
                cancellationToken
            );
        }
        else if (request.CaseStatus.HasValue)
        {
            claims = await _claimRepository.GetByCaseStatusAsync(
                request.CaseStatus.Value,
                cancellationToken
            );
        }
        else
        {
            claims = await _claimRepository.GetAllAsync(cancellationToken);
        }

        // Map to DTOs
        var claimList = claims.ToList();
        var dtos = _mapper.Map<List<ClaimDto>>(claimList);

        // Batch load relations and evidence links (avoids N+1 queries)
        var pairs = claimList.Select(c => (c.PrimaryClaimantId, c.PropertyUnitId)).ToList();
        var relationsDict = await _relationRepository.GetByPersonPropertyPairsBatchAsync(pairs, cancellationToken);

        var relationIds = relationsDict.Values.Select(r => r.Id).ToList();
        var evidenceDict = await _evidenceRelationRepository.GetActiveByRelationIdsBatchAsync(relationIds, cancellationToken);

        // Enrich DTOs from batch-loaded data
        foreach (var (dto, claim) in dtos.Zip(claimList))
        {
            if (relationsDict.TryGetValue((claim.PrimaryClaimantId, claim.PropertyUnitId), out var relation))
            {
                dto.SourceRelationId = relation.Id;
                var activeLinks = evidenceDict.GetValueOrDefault(relation.Id, new List<Domain.Entities.EvidenceRelation>());
                dto.EvidenceIds = activeLinks.Select(er => er.EvidenceId).ToList();
                dto.HasEvidence = dto.EvidenceIds.Count > 0 || (claim.Evidences != null && claim.Evidences.Any());
            }
        }

        return PaginatedList.FromEnumerable(dtos, request.PageNumber, request.PageSize);
    }
}
