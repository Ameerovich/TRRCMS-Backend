using MediatR;
using Microsoft.Extensions.Logging;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Import.Dtos;
using TRRCMS.Domain.Common;
using TRRCMS.Domain.Entities.Staging;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Import.Queries.GetStagingSummary;

public class GetStagingSummaryQueryHandler : IRequestHandler<GetStagingSummaryQuery, StagingSummaryDto>
{
    private readonly IImportPackageRepository _packageRepository;
    private readonly IStagingRepository<StagingBuilding> _buildingRepo;
    private readonly IStagingRepository<StagingPropertyUnit> _unitRepo;
    private readonly IStagingRepository<StagingPerson> _personRepo;
    private readonly IStagingRepository<StagingHousehold> _householdRepo;
    private readonly IStagingRepository<StagingPersonPropertyRelation> _relationRepo;
    private readonly IStagingRepository<StagingEvidence> _evidenceRepo;
    private readonly IStagingRepository<StagingClaim> _claimRepo;
    private readonly IStagingRepository<StagingSurvey> _surveyRepo;
    private readonly ILogger<GetStagingSummaryQueryHandler> _logger;

    public GetStagingSummaryQueryHandler(
        IImportPackageRepository packageRepository,
        IStagingRepository<StagingBuilding> buildingRepo,
        IStagingRepository<StagingPropertyUnit> unitRepo,
        IStagingRepository<StagingPerson> personRepo,
        IStagingRepository<StagingHousehold> householdRepo,
        IStagingRepository<StagingPersonPropertyRelation> relationRepo,
        IStagingRepository<StagingEvidence> evidenceRepo,
        IStagingRepository<StagingClaim> claimRepo,
        IStagingRepository<StagingSurvey> surveyRepo,
        ILogger<GetStagingSummaryQueryHandler> logger)
    {
        _packageRepository = packageRepository;
        _buildingRepo = buildingRepo;
        _unitRepo = unitRepo;
        _personRepo = personRepo;
        _householdRepo = householdRepo;
        _relationRepo = relationRepo;
        _evidenceRepo = evidenceRepo;
        _claimRepo = claimRepo;
        _surveyRepo = surveyRepo;
        _logger = logger;
    }

    public async Task<StagingSummaryDto> Handle(
        GetStagingSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var package = await _packageRepository.GetByIdAsync(request.ImportPackageId, cancellationToken)
            ?? throw new NotFoundException(
                $"Import package not found: {request.ImportPackageId}");

        var dto = new StagingSummaryDto
        {
            ImportPackageId = package.Id,
            PackageNumber = package.PackageNumber,
            Status = package.Status.ToString()
        };

        // Build per-entity-type summaries
        dto.Buildings = await BuildEntitySummaryAsync(_buildingRepo, "Building", package.Id, cancellationToken);
        dto.PropertyUnits = await BuildEntitySummaryAsync(_unitRepo, "PropertyUnit", package.Id, cancellationToken);
        dto.Persons = await BuildEntitySummaryAsync(_personRepo, "Person", package.Id, cancellationToken);
        dto.Households = await BuildEntitySummaryAsync(_householdRepo, "Household", package.Id, cancellationToken);
        dto.PersonPropertyRelations = await BuildEntitySummaryAsync(_relationRepo, "PersonPropertyRelation", package.Id, cancellationToken);
        dto.Evidences = await BuildEntitySummaryAsync(_evidenceRepo, "Evidence", package.Id, cancellationToken);
        dto.Claims = await BuildEntitySummaryAsync(_claimRepo, "Claim", package.Id, cancellationToken);
        dto.Surveys = await BuildEntitySummaryAsync(_surveyRepo, "Survey", package.Id, cancellationToken);

        // Aggregate counts
        var allSummaries = new[]
        {
            dto.Buildings, dto.PropertyUnits, dto.Persons, dto.Households,
            dto.PersonPropertyRelations, dto.Evidences, dto.Claims, dto.Surveys
        };

        dto.TotalRecords = allSummaries.Sum(s => s.Total);
        dto.TotalValid = allSummaries.Sum(s => s.Valid);
        dto.TotalInvalid = allSummaries.Sum(s => s.Invalid);
        dto.TotalWarning = allSummaries.Sum(s => s.Warning);
        dto.TotalSkipped = allSummaries.Sum(s => s.Skipped);
        dto.TotalPending = allSummaries.Sum(s => s.Pending);

        return dto;
    }

    private static async Task<EntityTypeSummary> BuildEntitySummaryAsync<T>(
        IStagingRepository<T> repo,
        string entityTypeName,
        Guid importPackageId,
        CancellationToken cancellationToken) where T : BaseStagingEntity
    {
        var counts = await repo.GetStatusCountsByPackageAsync(importPackageId, cancellationToken);

        return new EntityTypeSummary
        {
            EntityType = entityTypeName,
            Total = counts.Values.Sum(),
            Valid = counts.GetValueOrDefault(StagingValidationStatus.Valid, 0),
            Invalid = counts.GetValueOrDefault(StagingValidationStatus.Invalid, 0),
            Warning = counts.GetValueOrDefault(StagingValidationStatus.Warning, 0),
            Skipped = counts.GetValueOrDefault(StagingValidationStatus.Skipped, 0),
            Pending = counts.GetValueOrDefault(StagingValidationStatus.Pending, 0)
        };
    }
}
