using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Import.Dtos;
using TRRCMS.Domain.Entities.Staging;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Import.Queries.GetCommitReport;

/// <summary>
/// Handles GetCommitReportQuery by reconstructing the commit report
/// from ImportPackage metadata and staging traceability data.
/// </summary>
public class GetCommitReportQueryHandler : IRequestHandler<GetCommitReportQuery, CommitReportDto>
{
    private readonly IImportPackageRepository _importPackageRepository;
    private readonly IStagingRepository<StagingBuilding> _stagingBuildingRepo;
    private readonly IStagingRepository<StagingPropertyUnit> _stagingPropertyUnitRepo;
    private readonly IStagingRepository<StagingPerson> _stagingPersonRepo;
    private readonly IStagingRepository<StagingHousehold> _stagingHouseholdRepo;
    private readonly IStagingRepository<StagingPersonPropertyRelation> _stagingRelationRepo;
    private readonly IStagingRepository<StagingEvidence> _stagingEvidenceRepo;
    private readonly IStagingRepository<StagingClaim> _stagingClaimRepo;
    private readonly IStagingRepository<StagingSurvey> _stagingSurveyRepo;
    private readonly IConflictResolutionRepository _conflictResolutionRepository;

    public GetCommitReportQueryHandler(
        IImportPackageRepository importPackageRepository,
        IStagingRepository<StagingBuilding> stagingBuildingRepo,
        IStagingRepository<StagingPropertyUnit> stagingPropertyUnitRepo,
        IStagingRepository<StagingPerson> stagingPersonRepo,
        IStagingRepository<StagingHousehold> stagingHouseholdRepo,
        IStagingRepository<StagingPersonPropertyRelation> stagingRelationRepo,
        IStagingRepository<StagingEvidence> stagingEvidenceRepo,
        IStagingRepository<StagingClaim> stagingClaimRepo,
        IStagingRepository<StagingSurvey> stagingSurveyRepo,
        IConflictResolutionRepository conflictResolutionRepository)
    {
        _importPackageRepository = importPackageRepository;
        _stagingBuildingRepo = stagingBuildingRepo;
        _stagingPropertyUnitRepo = stagingPropertyUnitRepo;
        _stagingPersonRepo = stagingPersonRepo;
        _stagingHouseholdRepo = stagingHouseholdRepo;
        _stagingRelationRepo = stagingRelationRepo;
        _stagingEvidenceRepo = stagingEvidenceRepo;
        _stagingClaimRepo = stagingClaimRepo;
        _stagingSurveyRepo = stagingSurveyRepo;
        _conflictResolutionRepository = conflictResolutionRepository;
    }

    public async Task<CommitReportDto> Handle(
        GetCommitReportQuery request,
        CancellationToken cancellationToken)
    {
        var package = await _importPackageRepository.GetByIdAsync(request.ImportPackageId, cancellationToken)
            ?? throw new NotFoundException($"ImportPackage with ID '{request.ImportPackageId}' was not found.");

        // Only completed/partially-completed/failed packages have commit reports
        var validStatuses = new[] { ImportStatus.Completed, ImportStatus.PartiallyCompleted, ImportStatus.Failed };
        if (!validStatuses.Contains(package.Status))
        {
            throw new ConflictException(
                $"No commit report available. Package status is '{package.Status}'. " +
                "Commit report is only available after commit has been attempted.");
        }

        var report = new CommitReportDto
        {
            ImportPackageId = package.Id,
            PackageNumber = package.PackageNumber,
            Status = package.Status.ToString(),
            CommittedByUserId = package.CommittedByUserId ?? Guid.Empty,
            CommittedAtUtc = package.CommittedDate ?? DateTime.UtcNow,
            TotalRecordsCommitted = package.SuccessfulImportCount,
            TotalRecordsFailed = package.FailedImportCount,
            TotalRecordsSkipped = package.SkippedRecordCount,
            IsArchived = package.IsArchived,
            ArchivePath = package.ArchivePath
        };

        // Build per-entity-type breakdown from staging data
        report.Buildings = await BuildEntitySummaryAsync(_stagingBuildingRepo, request.ImportPackageId, "Building", cancellationToken);
        report.PropertyUnits = await BuildEntitySummaryAsync(_stagingPropertyUnitRepo, request.ImportPackageId, "PropertyUnit", cancellationToken);
        report.Persons = await BuildEntitySummaryAsync(_stagingPersonRepo, request.ImportPackageId, "Person", cancellationToken);
        report.Households = await BuildEntitySummaryAsync(_stagingHouseholdRepo, request.ImportPackageId, "Household", cancellationToken);
        report.PersonPropertyRelations = await BuildEntitySummaryAsync(_stagingRelationRepo, request.ImportPackageId, "PersonPropertyRelation", cancellationToken);
        report.Evidences = await BuildEntitySummaryAsync(_stagingEvidenceRepo, request.ImportPackageId, "Evidence", cancellationToken);
        report.Claims = await BuildEntitySummaryAsync(_stagingClaimRepo, request.ImportPackageId, "Claim", cancellationToken);
        report.Surveys = await BuildEntitySummaryAsync(_stagingSurveyRepo, request.ImportPackageId, "Survey", cancellationToken);

        report.TotalRecordsApproved =
            report.Buildings.Approved + report.PropertyUnits.Approved +
            report.Persons.Approved + report.Households.Approved +
            report.PersonPropertyRelations.Approved + report.Evidences.Approved +
            report.Claims.Approved + report.Surveys.Approved;

        // Conflict resolutions applied
        var resolvedConflicts = await _conflictResolutionRepository
            .GetByPackageIdAsync(request.ImportPackageId, cancellationToken);
        report.ConflictResolutionsApplied = resolvedConflicts.Count(c => c.Status == "Resolved");
        report.MergesPerformed = resolvedConflicts.Count(c =>
            c.Status == "Resolved" && c.ResolutionAction == ConflictResolutionAction.Merge);

        return report;
    }

    /// <summary>
    /// Build commit summary for a single staging entity type.
    /// </summary>
    private static async Task<CommitEntityTypeSummary> BuildEntitySummaryAsync<T>(
        IStagingRepository<T> repo,
        Guid importPackageId,
        string entityTypeName,
        CancellationToken ct) where T : Domain.Common.BaseStagingEntity
    {
        var allRecords = await repo.GetByPackageIdAsync(importPackageId, ct);

        var committed = allRecords.Where(r => r.CommittedEntityId.HasValue).ToList();
        var approved = allRecords.Where(r => r.IsApprovedForCommit).ToList();
        var skipped = allRecords.Where(r =>
            r.ValidationStatus == StagingValidationStatus.Skipped).ToList();
        var failed = approved.Where(r => !r.CommittedEntityId.HasValue).ToList();

        return new CommitEntityTypeSummary
        {
            EntityType = entityTypeName,
            Approved = approved.Count,
            Committed = committed.Count,
            Failed = failed.Count,
            Skipped = skipped.Count,
            IdMappings = committed.ToDictionary(
                r => r.OriginalEntityId,
                r => r.CommittedEntityId!.Value)
        };
    }
}
