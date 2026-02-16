using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Surveys.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Claims.Queries.GetClaimSummaries;

public class GetClaimSummariesQueryHandler
    : IRequestHandler<GetClaimSummariesQuery, List<CreatedClaimSummaryDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetClaimSummariesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<CreatedClaimSummaryDto>> Handle(
        GetClaimSummariesQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Resolve SurveyVisitId → ClaimId (if provided)
        Guid? surveyLinkedClaimId = null;

        if (request.SurveyVisitId.HasValue)
        {
            var survey = await _unitOfWork.Surveys.GetByIdAsync(
                request.SurveyVisitId.Value, cancellationToken);

            if (survey?.ClaimId == null)
                return new List<CreatedClaimSummaryDto>();

            surveyLinkedClaimId = survey.ClaimId;
        }

        // 2. Cast int filters to enum types for the repository
        ClaimStatus? statusFilter = request.ClaimStatus.HasValue
            ? (ClaimStatus)request.ClaimStatus.Value
            : null;

        ClaimSource? sourceFilter = request.ClaimSource.HasValue
            ? (ClaimSource)request.ClaimSource.Value
            : null;

        // 3. Get filtered claims (server-side, single DB query)
        var claims = await _unitOfWork.Claims.GetFilteredAsync(
            statusFilter,
            sourceFilter,
            request.CreatedByUserId,
            surveyLinkedClaimId,
            request.BuildingCode,
            cancellationToken);

        if (claims.Count == 0)
            return new List<CreatedClaimSummaryDto>();

        // 4. Batch-load PersonPropertyRelations for SourceRelationId, RelationType, HasEvidence
        //    Key: (PersonId, PropertyUnitId) → Relation
        var propertyUnitIds = claims
            .Select(c => c.PropertyUnitId)
            .Distinct()
            .ToList();

        var allRelations = new List<Domain.Entities.PersonPropertyRelation>();
        foreach (var puId in propertyUnitIds)
        {
            var relations = await _unitOfWork.PersonPropertyRelations
                .GetByPropertyUnitIdAsync(puId, cancellationToken);
            allRelations.AddRange(relations);
        }

        var relationLookup = allRelations
            .GroupBy(r => (r.PersonId, r.PropertyUnitId))
            .ToDictionary(g => g.Key, g => g.First());

        // 5. Batch-load Surveys linked to these claims (Survey.ClaimId → Survey)
        var claimIds = claims.Select(c => c.Id).ToList();
        var surveyByClaimId = await _unitOfWork.Surveys.GetByClaimIdsAsync(claimIds, cancellationToken);

        // 6. Map to DTOs
        var result = new List<CreatedClaimSummaryDto>(claims.Count);

        foreach (var claim in claims)
        {
            var person = claim.PrimaryClaimant;
            var propertyUnit = claim.PropertyUnit;

            // Find the PersonPropertyRelation that generated this claim
            Domain.Entities.PersonPropertyRelation? sourceRelation = null;
            if (claim.PrimaryClaimantId.HasValue)
            {
                relationLookup.TryGetValue(
                    (claim.PrimaryClaimantId.Value, claim.PropertyUnitId),
                    out sourceRelation);
            }

            // Get linked survey (if any)
            surveyByClaimId.TryGetValue(claim.Id, out var linkedSurvey);

            result.Add(new CreatedClaimSummaryDto
            {
                ClaimId = claim.Id,
                ClaimNumber = claim.ClaimNumber,
                PropertyUnitIdNumber = propertyUnit?.UnitIdentifier ?? string.Empty,
                FullNameArabic = person?.GetFullNameArabic() ?? string.Empty,
                ClaimSource = (int)claim.ClaimSource,
                CasePriority = (int)claim.Priority,
                ClaimStatus = (int)claim.Status,
                SurveyDate = linkedSurvey?.SurveyDate ?? default,
                TypeOfWorks = MapPropertyUnitTypeToTypeOfWorks(propertyUnit?.UnitType),
                HasEvidence = sourceRelation?.HasEvidence ?? false,
                SourceRelationId = sourceRelation?.Id ?? Guid.Empty,
                RelationType = sourceRelation != null ? (int)sourceRelation.RelationType : 0,
                PersonId = claim.PrimaryClaimantId ?? Guid.Empty,
                PropertyUnitId = claim.PropertyUnitId,
                BuildingCode = propertyUnit?.Building?.BuildingId ?? string.Empty,
                SurveyId = linkedSurvey?.Id
            });
        }

        return result;
    }

    private static string MapPropertyUnitTypeToTypeOfWorks(PropertyUnitType? unitType)
    {
        return unitType switch
        {
            PropertyUnitType.Apartment => "Residential",
            PropertyUnitType.Shop => "Commercial",
            PropertyUnitType.Office => "Commercial",
            PropertyUnitType.Warehouse => "Factorial",
            _ => "Other"
        };
    }
}
