using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Import.Dtos;
using TRRCMS.Domain.Entities.Staging;

namespace TRRCMS.Application.Import.Queries.GetStagedEntities;

/// <summary>
/// Handler for <see cref="GetStagedEntitiesQuery"/>.
/// Loads all staged entities for the given import package and maps them to DTOs.
///
/// Used by the conflict resolution UI to display entity details and allow
/// the data manager to identify staging vs production records during merge.
/// </summary>
public class GetStagedEntitiesQueryHandler
    : IRequestHandler<GetStagedEntitiesQuery, GetStagedEntitiesResponse>
{
    private readonly IStagingRepository<StagingBuilding> _buildingRepo;
    private readonly IStagingRepository<StagingPropertyUnit> _propertyUnitRepo;
    private readonly IStagingRepository<StagingPerson> _personRepo;
    private readonly IStagingRepository<StagingHousehold> _householdRepo;
    private readonly IStagingRepository<StagingPersonPropertyRelation> _relationRepo;
    private readonly IStagingRepository<StagingClaim> _claimRepo;
    private readonly IStagingRepository<StagingSurvey> _surveyRepo;
    private readonly IStagingRepository<StagingEvidence> _evidenceRepo;

    public GetStagedEntitiesQueryHandler(
        IStagingRepository<StagingBuilding> buildingRepo,
        IStagingRepository<StagingPropertyUnit> propertyUnitRepo,
        IStagingRepository<StagingPerson> personRepo,
        IStagingRepository<StagingHousehold> householdRepo,
        IStagingRepository<StagingPersonPropertyRelation> relationRepo,
        IStagingRepository<StagingClaim> claimRepo,
        IStagingRepository<StagingSurvey> surveyRepo,
        IStagingRepository<StagingEvidence> evidenceRepo)
    {
        _buildingRepo = buildingRepo;
        _propertyUnitRepo = propertyUnitRepo;
        _personRepo = personRepo;
        _householdRepo = householdRepo;
        _relationRepo = relationRepo;
        _claimRepo = claimRepo;
        _surveyRepo = surveyRepo;
        _evidenceRepo = evidenceRepo;
    }

    public async Task<GetStagedEntitiesResponse> Handle(
        GetStagedEntitiesQuery request,
        CancellationToken cancellationToken)
    {
        var pkgId = request.ImportPackageId;
        var filter = request.EntityTypeFilter?.ToLower();

        var response = new GetStagedEntitiesResponse { ImportPackageId = pkgId };

        // Load each entity type (skip if filter is specified and doesn't match)

        if (filter is null or "building")
        {
            var buildings = await _buildingRepo.GetByPackageIdAsync(pkgId, cancellationToken);
            response.Buildings = buildings.Select(b => new StagedEntityDto
            {
                StagingId = b.Id,
                OriginalEntityId = b.OriginalEntityId,
                EntityType = "Building",
                Identifier = b.BuildingId ?? $"{b.GovernorateCode}-{b.DistrictCode}-{b.SubDistrictCode}-{b.CommunityCode}-{b.NeighborhoodCode}-{b.BuildingNumber}",
                ValidationStatus = b.ValidationStatus.ToString(),
                IsApprovedForCommit = b.IsApprovedForCommit,
                CommittedEntityId = b.CommittedEntityId,
                DisplayInfo = $"{b.BuildingType}, {b.NumberOfPropertyUnits} units"
            }).ToList();
        }

        if (filter is null or "propertyunit")
        {
            var units = await _propertyUnitRepo.GetByPackageIdAsync(pkgId, cancellationToken);
            response.PropertyUnits = units.Select(u => new StagedEntityDto
            {
                StagingId = u.Id,
                OriginalEntityId = u.OriginalEntityId,
                EntityType = "PropertyUnit",
                Identifier = u.UnitIdentifier,
                ValidationStatus = u.ValidationStatus.ToString(),
                IsApprovedForCommit = u.IsApprovedForCommit,
                CommittedEntityId = u.CommittedEntityId,
                DisplayInfo = $"{u.UnitType}, Floor {u.FloorNumber?.ToString() ?? "N/A"}",
                ParentOriginalEntityId = u.OriginalBuildingId
            }).ToList();
        }

        if (filter is null or "person")
        {
            var persons = await _personRepo.GetByPackageIdAsync(pkgId, cancellationToken);
            response.Persons = persons.Select(p => new StagedEntityDto
            {
                StagingId = p.Id,
                OriginalEntityId = p.OriginalEntityId,
                EntityType = "Person",
                Identifier = p.NationalId ?? "N/A",
                ValidationStatus = p.ValidationStatus.ToString(),
                IsApprovedForCommit = p.IsApprovedForCommit,
                CommittedEntityId = p.CommittedEntityId,
                DisplayInfo = $"{p.FirstNameArabic} {p.FatherNameArabic} {p.FamilyNameArabic}".Trim()
            }).ToList();
        }

        if (filter is null or "household")
        {
            var households = await _householdRepo.GetByPackageIdAsync(pkgId, cancellationToken);
            response.Households = households.Select(h => new StagedEntityDto
            {
                StagingId = h.Id,
                OriginalEntityId = h.OriginalEntityId,
                EntityType = "Household",
                Identifier = h.HeadOfHouseholdName ?? "N/A",
                ValidationStatus = h.ValidationStatus.ToString(),
                IsApprovedForCommit = h.IsApprovedForCommit,
                CommittedEntityId = h.CommittedEntityId,
                DisplayInfo = $"Size: {h.HouseholdSize}",
                ParentOriginalEntityId = h.OriginalPropertyUnitId
            }).ToList();
        }

        if (filter is null or "personpropertyrelation")
        {
            var relations = await _relationRepo.GetByPackageIdAsync(pkgId, cancellationToken);
            response.PersonPropertyRelations = relations.Select(r => new StagedEntityDto
            {
                StagingId = r.Id,
                OriginalEntityId = r.OriginalEntityId,
                EntityType = "PersonPropertyRelation",
                Identifier = $"{r.OriginalPersonId} → {r.OriginalPropertyUnitId}",
                ValidationStatus = r.ValidationStatus.ToString(),
                IsApprovedForCommit = r.IsApprovedForCommit,
                CommittedEntityId = r.CommittedEntityId,
                DisplayInfo = r.RelationType.ToString()
            }).ToList();
        }

        if (filter is null or "claim")
        {
            var claims = await _claimRepo.GetByPackageIdAsync(pkgId, cancellationToken);
            response.Claims = claims.Select(c => new StagedEntityDto
            {
                StagingId = c.Id,
                OriginalEntityId = c.OriginalEntityId,
                EntityType = "Claim",
                Identifier = c.ClaimType,
                ValidationStatus = c.ValidationStatus.ToString(),
                IsApprovedForCommit = c.IsApprovedForCommit,
                CommittedEntityId = c.CommittedEntityId,
                DisplayInfo = c.ClaimSource.ToString(),
                ParentOriginalEntityId = c.OriginalPropertyUnitId
            }).ToList();
        }

        if (filter is null or "survey")
        {
            var surveys = await _surveyRepo.GetByPackageIdAsync(pkgId, cancellationToken);
            response.Surveys = surveys.Select(s => new StagedEntityDto
            {
                StagingId = s.Id,
                OriginalEntityId = s.OriginalEntityId,
                EntityType = "Survey",
                Identifier = s.ReferenceCode ?? "N/A",
                ValidationStatus = s.ValidationStatus.ToString(),
                IsApprovedForCommit = s.IsApprovedForCommit,
                CommittedEntityId = s.CommittedEntityId,
                DisplayInfo = s.SurveyDate.ToString("yyyy-MM-dd"),
                ParentOriginalEntityId = s.OriginalBuildingId
            }).ToList();
        }

        if (filter is null or "evidence")
        {
            var evidences = await _evidenceRepo.GetByPackageIdAsync(pkgId, cancellationToken);
            response.Evidences = evidences.Select(e => new StagedEntityDto
            {
                StagingId = e.Id,
                OriginalEntityId = e.OriginalEntityId,
                EntityType = "Evidence",
                Identifier = e.OriginalFileName,
                ValidationStatus = e.ValidationStatus.ToString(),
                IsApprovedForCommit = e.IsApprovedForCommit,
                CommittedEntityId = e.CommittedEntityId,
                DisplayInfo = e.Description,
                ParentOriginalEntityId = e.OriginalPersonId
            }).ToList();
        }

        return response;
    }
}
