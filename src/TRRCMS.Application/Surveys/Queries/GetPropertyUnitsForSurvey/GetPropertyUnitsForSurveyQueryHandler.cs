using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Services;
using TRRCMS.Application.PropertyUnits.Dtos;

namespace TRRCMS.Application.Surveys.Queries.GetPropertyUnitsForSurvey;

/// <summary>
/// Handler for GetPropertyUnitsForSurveyQuery
/// Retrieves all property units for the building being surveyed
/// </summary>
public class GetPropertyUnitsForSurveyQueryHandler : IRequestHandler<GetPropertyUnitsForSurveyQuery, List<PropertyUnitDto>>
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly IPropertyUnitRepository _propertyUnitRepository;
    private readonly IBuildingRepository _buildingRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetPropertyUnitsForSurveyQueryHandler(
        ISurveyRepository surveyRepository,
        IPropertyUnitRepository propertyUnitRepository,
        IBuildingRepository buildingRepository,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _surveyRepository = surveyRepository ?? throw new ArgumentNullException(nameof(surveyRepository));
        _propertyUnitRepository = propertyUnitRepository ?? throw new ArgumentNullException(nameof(propertyUnitRepository));
        _buildingRepository = buildingRepository ?? throw new ArgumentNullException(nameof(buildingRepository));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<List<PropertyUnitDto>> Handle(GetPropertyUnitsForSurveyQuery request, CancellationToken cancellationToken)
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

        // Verify ownership (users can only view property units for their own surveys)
        if (survey.FieldCollectorId != currentUserId)
        {
            throw new UnauthorizedAccessException("You can only view property units for your own surveys");
        }

        // Get all property units for the survey's building
        var propertyUnits = await _propertyUnitRepository.GetByBuildingIdAsync(survey.BuildingId, cancellationToken);

        // Get building for DTO enrichment
        var building = await _buildingRepository.GetByIdAsync(survey.BuildingId, cancellationToken);

        // Map to DTOs
        var result = propertyUnits.Select(unit =>
        {
            var dto = _mapper.Map<PropertyUnitDto>(unit);
            dto.BuildingNumber = building?.BuildingNumber;
            return dto;
        }).ToList();

        return result;
    }
}