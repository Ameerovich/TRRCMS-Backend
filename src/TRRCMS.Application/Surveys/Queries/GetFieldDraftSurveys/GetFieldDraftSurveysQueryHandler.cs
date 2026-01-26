using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Surveys.Dtos;

namespace TRRCMS.Application.Surveys.Queries.GetFieldDraftSurveys;

/// <summary>
/// Handler for GetFieldDraftSurveysQuery
/// Returns draft field surveys for the current field collector
/// UC-002: Resume draft field survey
/// </summary>
public class GetFieldDraftSurveysQueryHandler : IRequestHandler<GetFieldDraftSurveysQuery, GetFieldDraftSurveysResponse>
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetFieldDraftSurveysQueryHandler(
        ISurveyRepository surveyRepository,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _surveyRepository = surveyRepository ?? throw new ArgumentNullException(nameof(surveyRepository));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<GetFieldDraftSurveysResponse> Handle(
        GetFieldDraftSurveysQuery request,
        CancellationToken cancellationToken)
    {
        // Get current user (field collector)
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        // Validate and normalize page size
        if (request.PageSize < 1) request.PageSize = 20;
        if (request.PageSize > 50) request.PageSize = 50;
        if (request.Page < 1) request.Page = 1;

        // Get draft field surveys for current user with pagination
        var (surveys, totalCount) = await _surveyRepository.GetFieldDraftSurveysByCollectorAsync(
            currentUserId,
            request.BuildingId,
            request.Page,
            request.PageSize,
            request.SortBy,
            request.SortDirection,
            cancellationToken);

        // Map to DTOs
        var surveyDtos = new List<SurveyDto>();
        foreach (var survey in surveys)
        {
            var dto = _mapper.Map<SurveyDto>(survey);

            // Building info is already mapped by AutoMapper profile
            // Property unit info is already mapped by AutoMapper profile

            // Set field collector name to current user
            dto.FieldCollectorName = _currentUserService.Username;

            surveyDtos.Add(dto);
        }

        return new GetFieldDraftSurveysResponse
        {
            Surveys = surveyDtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}