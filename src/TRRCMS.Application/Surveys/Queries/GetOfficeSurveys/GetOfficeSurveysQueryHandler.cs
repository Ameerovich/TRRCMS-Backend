using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Services;
using TRRCMS.Application.Surveys.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Surveys.Queries.GetOfficeSurveys;

/// <summary>
/// Handler for GetOfficeSurveysQuery
/// Returns paginated list of office surveys with filtering
/// </summary>
public class GetOfficeSurveysQueryHandler : IRequestHandler<GetOfficeSurveysQuery, GetOfficeSurveysResponse>
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetOfficeSurveysQueryHandler(
        ISurveyRepository surveyRepository,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _surveyRepository = surveyRepository ?? throw new ArgumentNullException(nameof(surveyRepository));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<GetOfficeSurveysResponse> Handle(
        GetOfficeSurveysQuery request, 
        CancellationToken cancellationToken)
    {
        // Validate pagination parameters
        if (request.Page < 1) request.Page = 1;
        if (request.PageSize < 1) request.PageSize = 20;
        if (request.PageSize > 100) request.PageSize = 100;

        // Get office surveys with filters
        var (surveys, totalCount) = await _surveyRepository.GetOfficeSurveysAsync(
            status: request.Status,
            buildingId: request.BuildingId,
            clerkId: request.ClerkId,
            fromDate: request.FromDate,
            toDate: request.ToDate,
            referenceCode: request.ReferenceCode,
            intervieweeName: request.IntervieweeName,
            page: request.Page,
            pageSize: request.PageSize,
            sortBy: request.SortBy,
            sortDirection: request.SortDirection,
            cancellationToken: cancellationToken
        );

        // Map to DTOs
        var surveyDtos = _mapper.Map<List<SurveyDto>>(surveys);

        // Enhance DTOs with additional info
        foreach (var dto in surveyDtos)
        {
            dto.FieldCollectorName = _currentUserService.Username; // Will be fetched properly in future
        }

        return new GetOfficeSurveysResponse
        {
            Surveys = surveyDtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
