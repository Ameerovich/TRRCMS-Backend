using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Services;
using TRRCMS.Application.PersonPropertyRelations.Dtos;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Application.Surveys.Queries.GetRelationsForPropertyUnitInSurvey;

/// <summary>
/// Handler for GetRelationsForPropertyUnitInSurveyQuery.
/// Returns all person-property relations for a property unit,
/// scoped to a survey for authorization.
/// </summary>
public class GetRelationsForPropertyUnitInSurveyQueryHandler
    : IRequestHandler<GetRelationsForPropertyUnitInSurveyQuery, List<PersonPropertyRelationDto>>
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly IPropertyUnitRepository _propertyUnitRepository;
    private readonly IPersonPropertyRelationRepository _relationRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetRelationsForPropertyUnitInSurveyQueryHandler(
        ISurveyRepository surveyRepository,
        IPropertyUnitRepository propertyUnitRepository,
        IPersonPropertyRelationRepository relationRepository,
        ICurrentUserService currentUserService)
    {
        _surveyRepository = surveyRepository ?? throw new ArgumentNullException(nameof(surveyRepository));
        _propertyUnitRepository = propertyUnitRepository ?? throw new ArgumentNullException(nameof(propertyUnitRepository));
        _relationRepository = relationRepository ?? throw new ArgumentNullException(nameof(relationRepository));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    public async Task<List<PersonPropertyRelationDto>> Handle(
        GetRelationsForPropertyUnitInSurveyQuery request,
        CancellationToken cancellationToken)
    {
        // ==================== AUTHORIZATION ====================

        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var survey = await _surveyRepository.GetByIdAsync(request.SurveyId, cancellationToken)
            ?? throw new NotFoundException($"Survey with ID {request.SurveyId} not found");

        if (survey.FieldCollectorId != currentUserId)
        {
            throw new UnauthorizedAccessException("You can only view relations for your own surveys");
        }

        // ==================== VALIDATE PROPERTY UNIT ====================

        var propertyUnit = await _propertyUnitRepository.GetByIdAsync(request.PropertyUnitId, cancellationToken)
            ?? throw new NotFoundException($"Property unit with ID {request.PropertyUnitId} not found");

        if (propertyUnit.BuildingId != survey.BuildingId)
        {
            throw new ValidationException("Property unit does not belong to the survey's building");
        }

        // ==================== FETCH RELATIONS ====================

        // Use WithEvidences variant so EvidenceCount is populated correctly
        var relations = await _relationRepository.GetByPropertyUnitIdWithEvidencesAsync(
            request.PropertyUnitId, cancellationToken);

        // ==================== MAP TO DTOs ====================

        return relations.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Maps PersonPropertyRelation entity to PersonPropertyRelationDto.
    /// Updated for office survey workflow - removed deprecated fields.
    /// </summary>
    private static PersonPropertyRelationDto MapToDto(PersonPropertyRelation r)
    {
        return new PersonPropertyRelationDto
        {
            Id = r.Id,
            PersonId = r.PersonId,
            PropertyUnitId = r.PropertyUnitId,
            RelationType = (int)r.RelationType,
            OccupancyType = r.OccupancyType.HasValue ? (int?)r.OccupancyType : null,
            HasEvidence = r.HasEvidence,
            OwnershipShare = r.OwnershipShare,
            ContractDetails = r.ContractDetails,
            Notes = r.Notes,
            IsActive = r.IsActive,
            CreatedAtUtc = r.CreatedAtUtc,
            CreatedBy = r.CreatedBy,
            LastModifiedAtUtc = r.LastModifiedAtUtc,
            LastModifiedBy = r.LastModifiedBy,
            IsDeleted = r.IsDeleted,
            DeletedAtUtc = r.DeletedAtUtc,
            DeletedBy = r.DeletedBy,
            IsOngoing = r.IsActive,
            EvidenceCount = r.Evidences?.Count ?? 0
        };
    }
}
