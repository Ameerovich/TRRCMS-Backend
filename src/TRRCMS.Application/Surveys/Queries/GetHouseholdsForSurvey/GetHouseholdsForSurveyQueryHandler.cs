using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Services;
using TRRCMS.Application.Households.Dtos;

namespace TRRCMS.Application.Surveys.Queries.GetHouseholdsForSurvey;

/// <summary>
/// Handler for GetHouseholdsForSurveyQuery
/// </summary>
public class GetHouseholdsForSurveyQueryHandler : IRequestHandler<GetHouseholdsForSurveyQuery, List<HouseholdDto>>
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly IHouseholdRepository _householdRepository;
    private readonly IPropertyUnitRepository _propertyUnitRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetHouseholdsForSurveyQueryHandler(
        ISurveyRepository surveyRepository,
        IHouseholdRepository householdRepository,
        IPropertyUnitRepository propertyUnitRepository,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _surveyRepository = surveyRepository ?? throw new ArgumentNullException(nameof(surveyRepository));
        _householdRepository = householdRepository ?? throw new ArgumentNullException(nameof(householdRepository));
        _propertyUnitRepository = propertyUnitRepository ?? throw new ArgumentNullException(nameof(propertyUnitRepository));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<List<HouseholdDto>> Handle(GetHouseholdsForSurveyQuery request, CancellationToken cancellationToken)
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

        // Check if survey has property unit
        if (!survey.PropertyUnitId.HasValue)
        {
            return new List<HouseholdDto>();
        }

        // Get property unit
        var propertyUnit = await _propertyUnitRepository.GetByIdAsync(survey.PropertyUnitId.Value, cancellationToken);

        // Get households for this property unit
        var households = await _householdRepository.GetByPropertyUnitIdAsync(survey.PropertyUnitId.Value, cancellationToken);

        // Map to DTOs
        var result = households.Select(household =>
        {
            var dto = _mapper.Map<HouseholdDto>(household);
            dto.PropertyUnitIdentifier = propertyUnit?.UnitIdentifier;
            return dto;
        }).ToList();

        return result;
    }
}
