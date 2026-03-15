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
        var dtos = _mapper.Map<List<ClaimDto>>(claims);

        // Enrich each claim DTO with evidence from the source PersonPropertyRelation
        foreach (var (dto, claim) in dtos.Zip(claims))
        {
            var relation = await _relationRepository.GetByPersonAndPropertyUnitAsync(
                claim.PrimaryClaimantId, claim.PropertyUnitId, cancellationToken);

            if (relation != null)
            {
                dto.SourceRelationId = relation.Id;
                var activeLinks = await _evidenceRelationRepository
                    .GetActiveByRelationIdAsync(relation.Id, cancellationToken);
                dto.EvidenceIds = activeLinks.Select(er => er.EvidenceId).ToList();
                dto.HasEvidence = dto.EvidenceIds.Count > 0 || (claim.Evidences != null && claim.Evidences.Any());
            }
        }

        return PaginatedList.FromEnumerable(dtos, request.PageNumber, request.PageSize);
    }
}
