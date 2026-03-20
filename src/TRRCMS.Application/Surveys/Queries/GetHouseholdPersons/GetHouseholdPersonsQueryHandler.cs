using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Services;
using TRRCMS.Application.Persons.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Surveys.Queries.GetHouseholdPersons;

/// <summary>
/// Handler for GetHouseholdPersonsQuery
/// </summary>
public class GetHouseholdPersonsQueryHandler : IRequestHandler<GetHouseholdPersonsQuery, List<PersonDto>>
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly IPersonRepository _personRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetHouseholdPersonsQueryHandler(
        ISurveyRepository surveyRepository,
        IPersonRepository personRepository,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _surveyRepository = surveyRepository ?? throw new ArgumentNullException(nameof(surveyRepository));
        _personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<List<PersonDto>> Handle(GetHouseholdPersonsQuery request, CancellationToken cancellationToken)
    {
        // Get current user
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        // Validate survey exists and user has access
        var survey = await _surveyRepository.GetByIdAsync(request.SurveyId, cancellationToken);
        if (survey == null)
        {
            throw new NotFoundException($"Survey with ID {request.SurveyId} not found");
        }

        // Verify ownership
        if (survey.FieldCollectorId != currentUserId)
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync(cancellationToken);
            if (currentUser == null || !currentUser.HasPermission(Permission.Surveys_ViewAll))
                throw new UnauthorizedAccessException("You can only view persons in your own surveys");
        }

        // Get persons for this household
        var persons = await _personRepository.GetByHouseholdIdAsync(request.HouseholdId, cancellationToken);
        var personList = persons.ToList();

        // Include the survey's contact person (has no household but belongs to the survey)
        if (survey.ContactPersonId.HasValue)
        {
            var alreadyIncluded = personList.Any(p => p.Id == survey.ContactPersonId.Value);
            if (!alreadyIncluded)
            {
                var contactPerson = await _personRepository.GetByIdAsync(survey.ContactPersonId.Value, cancellationToken);
                if (contactPerson != null)
                {
                    personList.Add(contactPerson);
                }
            }
        }

        // Map to DTOs
        return _mapper.Map<List<PersonDto>>(personList);
    }
}
