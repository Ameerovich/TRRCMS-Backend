using MediatR;
using TRRCMS.Application.BuildingAssignments.Dtos;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.BuildingAssignments.Queries.GetPropertyUnitsForRevisit;

/// <summary>
/// Handler for GetPropertyUnitsForRevisitQuery
/// UC-012: S04-S05 - Review property units and select for revisit
/// </summary>
public class GetPropertyUnitsForRevisitQueryHandler 
    : IRequestHandler<GetPropertyUnitsForRevisitQuery, List<PropertyUnitForRevisitDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPropertyUnitsForRevisitQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<List<PropertyUnitForRevisitDto>> Handle(
        GetPropertyUnitsForRevisitQuery request, 
        CancellationToken cancellationToken)
    {
        // Validate building exists
        var building = await _unitOfWork.Buildings.GetByIdAsync(request.BuildingId, cancellationToken);
        if (building == null)
        {
            throw new NotFoundException($"Building with ID {request.BuildingId} not found");
        }

        // Get property units for the building
        var propertyUnits = await _unitOfWork.PropertyUnits.GetByBuildingIdAsync(
            request.BuildingId, cancellationToken);

        var result = new List<PropertyUnitForRevisitDto>();

        foreach (var unit in propertyUnits)
        {
            // Get surveys for this unit to check completion status
            var surveys = await _unitOfWork.Surveys.GetByPropertyUnitAsync(unit.Id, cancellationToken);
            var completedSurvey = surveys
                .Where(s => s.Status == SurveyStatus.Finalized)
                .OrderByDescending(s => s.SurveyDate)
                .FirstOrDefault();

            bool hasCompletedSurvey = completedSurvey != null;

            // Apply filter if requested
            if (request.OnlyWithCompletedSurveys && !hasCompletedSurvey)
                continue;

            // Get related data counts
            var households = await _unitOfWork.Households.GetByPropertyUnitIdAsync(unit.Id, cancellationToken);
            var relations = await _unitOfWork.PersonPropertyRelations.GetByPropertyUnitIdAsync(unit.Id, cancellationToken);
            
            // Check if property unit has claims
            var hasClaims = await _unitOfWork.Claims.HasClaimsAsync(unit.Id, cancellationToken);
            var claimCount = hasClaims ? 1 : 0; // Simplified - could be enhanced if multiple claims per unit are supported

            // Get persons count through relations
            var personIds = relations.Select(r => r.PersonId).Distinct().ToList();

            result.Add(new PropertyUnitForRevisitDto
            {
                Id = unit.Id,
                UnitCode = unit.UnitIdentifier,
                UnitType = unit.UnitType.ToString(),
                FloorNumber = unit.FloorNumber,
                Description = unit.Description,
                HasCompletedSurvey = hasCompletedSurvey,
                LastSurveyDate = completedSurvey?.SurveyDate,
                PersonCount = personIds.Count,
                HouseholdCount = households.Count(),
                ClaimCount = claimCount
            });
        }

        return result.OrderBy(u => u.FloorNumber).ThenBy(u => u.UnitCode).ToList();
    }
}
