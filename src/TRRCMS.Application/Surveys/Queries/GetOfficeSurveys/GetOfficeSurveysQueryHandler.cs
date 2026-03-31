using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Models;
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
        request.Page = PagedQuery.ClampPageNumber(request.Page);
        request.PageSize = PagedQuery.ClampPageSize(request.PageSize);

        // Get office surveys with filters
        var (surveys, totalCount) = await _surveyRepository.GetOfficeSurveysAsync(
            status: request.Status,
            buildingId: request.BuildingId,
            clerkId: request.ClerkId,
            fromDate: request.FromDate,
            toDate: request.ToDate,
            referenceCode: request.ReferenceCode,
            contactPersonName: request.ContactPersonName,
            page: request.Page,
            pageSize: request.PageSize,
            sortBy: request.SortBy,
            sortDirection: request.SortDirection,
            cancellationToken: cancellationToken
        );

        // Map to DTOs
        var surveyDtos = _mapper.Map<List<SurveyDto>>(surveys);

        // Enhance DTOs with additional info
        foreach (var (dto, survey) in surveyDtos.Zip(surveys))
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
