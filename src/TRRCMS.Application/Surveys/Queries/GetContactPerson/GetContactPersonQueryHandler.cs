using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Persons.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Surveys.Queries.GetContactPerson;

public class GetContactPersonQueryHandler : IRequestHandler<GetContactPersonQuery, PersonDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetContactPersonQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<PersonDto> Handle(GetContactPersonQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var survey = await _unitOfWork.Surveys.GetByIdAsync(request.SurveyId, cancellationToken)
            ?? throw new NotFoundException($"Survey with ID {request.SurveyId} not found");

        if (survey.FieldCollectorId != currentUserId)
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync(cancellationToken);
            if (currentUser == null || !currentUser.HasPermission(Permission.Surveys_ViewAll))
                throw new UnauthorizedAccessException("You can only view contact persons for your own surveys");
        }

        if (!survey.ContactPersonId.HasValue)
            throw new NotFoundException("No contact person is set for this survey.");

        var person = await _unitOfWork.Persons.GetByIdAsync(survey.ContactPersonId.Value, cancellationToken)
            ?? throw new NotFoundException($"Contact person with ID {survey.ContactPersonId.Value} not found");

        return _mapper.Map<PersonDto>(person);
    }
}
