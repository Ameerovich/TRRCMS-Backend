using AutoMapper;
using MediatR;
using TRRCMS.Application.Claims.Dtos;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Services;

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
    private readonly ISurveyRepository _surveyRepository;
    private readonly IPersonDesignationService _designationService;

    public GetClaimQueryHandler(
        IClaimRepository claimRepository,
        IPersonPropertyRelationRepository relationRepository,
        IEvidenceRelationRepository evidenceRelationRepository,
        IMapper mapper,
        ISurveyRepository surveyRepository,
        IPersonDesignationService designationService)
    {
        _surveyRepository = surveyRepository;
        _designationService = designationService;
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
        var relation = ClaimSourceRelationResolver.Resolve(claim,
            await _relationRepository.GetAllByPersonAndPropertyUnitAsync(
                claim.PrimaryClaimantId, claim.PropertyUnitId, cancellationToken));

        if (relation != null)
        {
            dto.SourceRelationId = relation.Id;
            var activeLinks = await _evidenceRelationRepository
                .GetActiveByRelationIdAsync(relation.Id, cancellationToken);
            dto.EvidenceIds = activeLinks.Select(er => er.EvidenceId).ToList();
            dto.HasEvidence = dto.EvidenceIds.Count > 0 || (claim.Evidences != null && claim.Evidences.Any());
        }

        var survey = claim.OriginatingSurveyId.HasValue
            ? await _surveyRepository.GetByIdAsync(claim.OriginatingSurveyId.Value, cancellationToken)
            : null;
        if (survey?.ContactPersonId == null)
        {
            survey = (await _surveyRepository.GetByPropertyUnitAsync(claim.PropertyUnitId, cancellationToken))
                .FirstOrDefault(s => s.ContactPersonId.HasValue);
        }
        if (survey != null)
        {
            dto.ContactPoint = await _designationService.GetContactPointAsync(survey, cancellationToken);
            dto.PrimaryClaimantIsContactPoint = dto.ContactPoint?.PersonId == claim.PrimaryClaimantId;
        }

        return dto;
    }
}
