using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Services;
using TRRCMS.Application.Evidences.Dtos;

namespace TRRCMS.Application.Surveys.Queries.GetSurveyEvidence;

/// <summary>
/// Handler for GetSurveyEvidenceQuery
/// </summary>
public class GetSurveyEvidenceQueryHandler : IRequestHandler<GetSurveyEvidenceQuery, List<EvidenceDto>>
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly IEvidenceRepository _evidenceRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetSurveyEvidenceQueryHandler(
        ISurveyRepository surveyRepository,
        IEvidenceRepository evidenceRepository,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _surveyRepository = surveyRepository ?? throw new ArgumentNullException(nameof(surveyRepository));
        _evidenceRepository = evidenceRepository ?? throw new ArgumentNullException(nameof(evidenceRepository));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<List<EvidenceDto>> Handle(GetSurveyEvidenceQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var survey = await _surveyRepository.GetByIdAsync(request.SurveyId, cancellationToken)
            ?? throw new NotFoundException($"Survey with ID {request.SurveyId} not found");

        if (survey.FieldCollectorId != currentUserId)
            throw new UnauthorizedAccessException("You can only view evidence for your own surveys");

        // Get evidence using EvidenceType? enum
        var evidences = await _evidenceRepository.GetBySurveyContextAsync(
            survey.BuildingId,
            request.EvidenceType,
            cancellationToken);

        var result = evidences.Select(evidence =>
        {
            var dto = _mapper.Map<EvidenceDto>(evidence);
            dto.IsExpired = evidence.IsExpired();
            return dto;
        }).ToList();

        return result;
    }
}
