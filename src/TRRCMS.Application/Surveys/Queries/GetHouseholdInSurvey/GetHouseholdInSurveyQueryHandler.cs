using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Services;
using TRRCMS.Application.Households.Dtos;

namespace TRRCMS.Application.Surveys.Queries.GetHouseholdInSurvey;

/// <summary>
/// Handler for GetHouseholdInSurveyQuery
/// </summary>
public class GetHouseholdInSurveyQueryHandler : IRequestHandler<GetHouseholdInSurveyQuery, HouseholdDto>
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly IHouseholdRepository _householdRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetHouseholdInSurveyQueryHandler(
        ISurveyRepository surveyRepository,
        IHouseholdRepository householdRepository,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _surveyRepository = surveyRepository ?? throw new ArgumentNullException(nameof(surveyRepository));
        _householdRepository = householdRepository ?? throw new ArgumentNullException(nameof(householdRepository));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<HouseholdDto> Handle(GetHouseholdInSurveyQuery request, CancellationToken cancellationToken)
    {
        // Get current user
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        // Get and validate survey
        var survey = await _surveyRepository.GetByIdAsync(request.SurveyId, cancellationToken);
        if (survey == null)
        {
            throw new NotFoundException($"Survey with ID {request.SurveyId} not found");
        }

        // Verify ownership
        if (survey.FieldCollectorId != currentUserId)
        {
            throw new UnauthorizedAccessException("You can only view households for your own surveys");
        }

        // Get household
        var household = await _householdRepository.GetByIdAsync(request.HouseholdId, cancellationToken);
        if (household == null)
        {
            throw new NotFoundException($"Household with ID {request.HouseholdId} not found");
        }

        // Map to DTO
        var result = _mapper.Map<HouseholdDto>(household);

        // Calculate computed properties
        result.DependencyRatio = household.CalculateDependencyRatio();
        result.IsVulnerable = household.IsVulnerable();

        return result;
    }
}