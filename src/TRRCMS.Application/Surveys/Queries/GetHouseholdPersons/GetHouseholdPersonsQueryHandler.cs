using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Services;
using TRRCMS.Application.Persons.Dtos;

namespace TRRCMS.Application.Surveys.Queries.GetHouseholdPersons;

/// <summary>
/// Handler for GetHouseholdPersonsQuery
/// Returns all persons/members in a household
/// </summary>
public class GetHouseholdPersonsQueryHandler : IRequestHandler<GetHouseholdPersonsQuery, List<PersonDto>>
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly IHouseholdRepository _householdRepository;
    private readonly IPersonRepository _personRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetHouseholdPersonsQueryHandler(
        ISurveyRepository surveyRepository,
        IHouseholdRepository householdRepository,
        IPersonRepository personRepository,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _surveyRepository = surveyRepository ?? throw new ArgumentNullException(nameof(surveyRepository));
        _householdRepository = householdRepository ?? throw new ArgumentNullException(nameof(householdRepository));
        _personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<List<PersonDto>> Handle(GetHouseholdPersonsQuery request, CancellationToken cancellationToken)
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
            throw new UnauthorizedAccessException("You can only view household members for your own surveys");
        }

        // Verify household exists
        var household = await _householdRepository.GetByIdAsync(request.HouseholdId, cancellationToken);
        if (household == null)
        {
            throw new NotFoundException($"Household with ID {request.HouseholdId} not found");
        }

        // Get all persons in the household using repository
        var persons = await _personRepository.GetByHouseholdIdAsync(request.HouseholdId, cancellationToken);

        // Map to DTOs
        var result = _mapper.Map<List<PersonDto>>(persons);

        return result;
    }
}