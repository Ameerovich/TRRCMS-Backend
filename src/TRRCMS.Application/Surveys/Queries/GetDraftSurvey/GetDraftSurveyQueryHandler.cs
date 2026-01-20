using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Services;
using TRRCMS.Application.Surveys.Dtos;

namespace TRRCMS.Application.Surveys.Queries.GetDraftSurvey;

/// <summary>
/// Handler for GetDraftSurveyQuery
/// Retrieves survey with all related data for resuming work
/// </summary>
public class GetDraftSurveyQueryHandler : IRequestHandler<GetDraftSurveyQuery, SurveyDto>
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetDraftSurveyQueryHandler(
        ISurveyRepository surveyRepository,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _surveyRepository = surveyRepository ?? throw new ArgumentNullException(nameof(surveyRepository));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<SurveyDto> Handle(GetDraftSurveyQuery request, CancellationToken cancellationToken)
    {
        // Get current user
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        // Get survey with related data
        var survey = await _surveyRepository.GetByIdAsync(request.SurveyId, cancellationToken);

        if (survey == null)
        {
            throw new NotFoundException($"Survey with ID {request.SurveyId} not found");
        }

        // Verify ownership (users can only view their own surveys unless they have ViewAll permission)
        // For now, enforce strict ownership for field collectors
        if (survey.FieldCollectorId != currentUserId)
        {
            // TODO: Add role-based check to allow supervisors/admins to view all surveys
            throw new UnauthorizedAccessException("You can only view your own surveys");
        }

        // Map to DTO
        var result = _mapper.Map<SurveyDto>(survey);

        // Enrich with additional data
        result.FieldCollectorName = _currentUserService.Username;

        if (survey.Building != null)
        {
            result.BuildingNumber = survey.Building.BuildingNumber;
            result.BuildingAddress = survey.Building.Address;
        }

        if (survey.PropertyUnit != null)
        {
            result.UnitIdentifier = survey.PropertyUnit.UnitIdentifier;
        }

        return result;
    }
}