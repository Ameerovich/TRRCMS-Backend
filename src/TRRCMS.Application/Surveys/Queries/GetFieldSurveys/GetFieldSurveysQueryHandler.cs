using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Surveys.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Surveys.Queries.GetFieldSurveys;

/// <summary>
/// Handler for GetFieldSurveysQuery
/// Returns paginated list of field surveys with filtering
/// UC-001/UC-002: Field Survey listing
/// </summary>
public class GetFieldSurveysQueryHandler : IRequestHandler<GetFieldSurveysQuery, GetFieldSurveysResponse>
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public GetFieldSurveysQueryHandler(
        ISurveyRepository surveyRepository,
        IUserRepository userRepository,
        IMapper mapper)
    {
        _surveyRepository = surveyRepository ?? throw new ArgumentNullException(nameof(surveyRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<GetFieldSurveysResponse> Handle(
        GetFieldSurveysQuery request,
        CancellationToken cancellationToken)
    {
        // Validate and normalize page size
        if (request.PageSize < 1) request.PageSize = 20;
        if (request.PageSize > 100) request.PageSize = 100;
        if (request.Page < 1) request.Page = 1;

        // Parse status filter if provided
        SurveyStatus? statusFilter = null;
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            if (Enum.TryParse<SurveyStatus>(request.Status, true, out var parsedStatus))
            {
                statusFilter = parsedStatus;
            }
        }

        // Build filter criteria - uses FieldSurveyFilterCriteria from Common.Interfaces namespace
        var filterCriteria = new FieldSurveyFilterCriteria
        {
            Status = statusFilter,
            BuildingId = request.BuildingId,
            FieldCollectorId = request.FieldCollectorId,
            PropertyUnitId = request.PropertyUnitId,
            FromDate = request.FromDate,
            ToDate = request.ToDate,
            ReferenceCode = request.ReferenceCode,
            IntervieweeName = request.IntervieweeName,
            SortBy = request.SortBy,
            SortDirection = request.SortDirection
        };

        // Get total count for pagination
        var totalCount = await _surveyRepository.GetFieldSurveysCountAsync(filterCriteria, cancellationToken);

        // Get paginated surveys
        var surveys = await _surveyRepository.GetFieldSurveysAsync(
            filterCriteria,
            request.Page,
            request.PageSize,
            cancellationToken);

        // Map to DTOs
        var surveyDtos = new List<SurveyDto>();
        foreach (var survey in surveys)
        {
            var dto = _mapper.Map<SurveyDto>(survey);

            // Building info is already mapped by AutoMapper profile
            // Property unit info is already mapped by AutoMapper profile

            // Get field collector name
            var fieldCollector = await _userRepository.GetByIdAsync(survey.FieldCollectorId, cancellationToken);
            if (fieldCollector != null)
            {
                dto.FieldCollectorName = fieldCollector.FullNameArabic ?? fieldCollector.Username;
            }

            surveyDtos.Add(dto);
        }

        return new GetFieldSurveysResponse
        {
            Surveys = surveyDtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}