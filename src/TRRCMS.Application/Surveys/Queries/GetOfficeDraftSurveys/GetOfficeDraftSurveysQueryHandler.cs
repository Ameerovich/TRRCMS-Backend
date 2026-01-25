using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Services;
using TRRCMS.Application.Surveys.Dtos;

namespace TRRCMS.Application.Surveys.Queries.GetOfficeDraftSurveys;

/// <summary>
/// Handler for GetOfficeDraftSurveysQuery
/// Returns draft office surveys for the current clerk to resume
/// </summary>
public class GetOfficeDraftSurveysQueryHandler : IRequestHandler<GetOfficeDraftSurveysQuery, List<SurveyDto>>
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetOfficeDraftSurveysQueryHandler(
        ISurveyRepository surveyRepository,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _surveyRepository = surveyRepository ?? throw new ArgumentNullException(nameof(surveyRepository));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<List<SurveyDto>> Handle(
        GetOfficeDraftSurveysQuery request, 
        CancellationToken cancellationToken)
    {
        // Get current user (office clerk)
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        // Get draft office surveys for this clerk
        var surveys = await _surveyRepository.GetOfficeDraftsByClerkAsync(
            currentUserId, 
            cancellationToken);

        // Map to DTOs
        var surveyDtos = _mapper.Map<List<SurveyDto>>(surveys);

        // Set clerk name
        foreach (var dto in surveyDtos)
        {
            dto.FieldCollectorName = _currentUserService.Username;
        }

        return surveyDtos;
    }
}
