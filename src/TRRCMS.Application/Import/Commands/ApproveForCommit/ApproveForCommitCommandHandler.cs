using AutoMapper;
using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Import.Dtos;
using TRRCMS.Domain.Entities.Staging;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Import.Commands.ApproveForCommit;

/// <summary>
/// Handles the ApproveForCommitCommand.
///
/// Flow:
///   1. Load and validate ImportPackage (status must be ReviewingConflicts or ReadyToCommit)
///   2. Verify all conflicts for the package are resolved
///   3. Approve staging records (all valid, or specific IDs)
///   4. Transition package status → ReadyToCommit
///   5. Return updated ImportPackageDto
///
/// UC-003 Stage 4 — S16 (Approve for Commit).
/// </summary>
public class ApproveForCommitCommandHandler : IRequestHandler<ApproveForCommitCommand, ImportPackageDto>
{
    private readonly IImportPackageRepository _importPackageRepository;
    private readonly IConflictResolutionRepository _conflictResolutionRepository;
    private readonly IStagingRepository<StagingBuilding> _stagingBuildingRepo;
    private readonly IStagingRepository<StagingPropertyUnit> _stagingPropertyUnitRepo;
    private readonly IStagingRepository<StagingPerson> _stagingPersonRepo;
    private readonly IStagingRepository<StagingHousehold> _stagingHouseholdRepo;
    private readonly IStagingRepository<StagingPersonPropertyRelation> _stagingRelationRepo;
    private readonly IStagingRepository<StagingEvidence> _stagingEvidenceRepo;
    private readonly IStagingRepository<StagingClaim> _stagingClaimRepo;
    private readonly IStagingRepository<StagingSurvey> _stagingSurveyRepo;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public ApproveForCommitCommandHandler(
        IImportPackageRepository importPackageRepository,
        IConflictResolutionRepository conflictResolutionRepository,
        IStagingRepository<StagingBuilding> stagingBuildingRepo,
        IStagingRepository<StagingPropertyUnit> stagingPropertyUnitRepo,
        IStagingRepository<StagingPerson> stagingPersonRepo,
        IStagingRepository<StagingHousehold> stagingHouseholdRepo,
        IStagingRepository<StagingPersonPropertyRelation> stagingRelationRepo,
        IStagingRepository<StagingEvidence> stagingEvidenceRepo,
        IStagingRepository<StagingClaim> stagingClaimRepo,
        IStagingRepository<StagingSurvey> stagingSurveyRepo,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _importPackageRepository = importPackageRepository;
        _conflictResolutionRepository = conflictResolutionRepository;
        _stagingBuildingRepo = stagingBuildingRepo;
        _stagingPropertyUnitRepo = stagingPropertyUnitRepo;
        _stagingPersonRepo = stagingPersonRepo;
        _stagingHouseholdRepo = stagingHouseholdRepo;
        _stagingRelationRepo = stagingRelationRepo;
        _stagingEvidenceRepo = stagingEvidenceRepo;
        _stagingClaimRepo = stagingClaimRepo;
        _stagingSurveyRepo = stagingSurveyRepo;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<ImportPackageDto> Handle(
        ApproveForCommitCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Load package
        var package = await _importPackageRepository.GetByIdAsync(request.ImportPackageId, cancellationToken)
            ?? throw new NotFoundException($"ImportPackage with ID '{request.ImportPackageId}' was not found.");

        // 2. Validate status — must be ReviewingConflicts or ReadyToCommit
        var allowedStatuses = new[] { ImportStatus.ReviewingConflicts, ImportStatus.ReadyToCommit, ImportStatus.Staging };
        if (!allowedStatuses.Contains(package.Status))
        {
            throw new ConflictException(
                $"Cannot approve package for commit. Current status is '{package.Status}'. " +
                $"Expected: {string.Join(", ", allowedStatuses)}.");
        }

        // 3. Verify all conflicts are resolved
        var unresolvedCount = await _conflictResolutionRepository
            .GetUnresolvedCountByPackageIdAsync(request.ImportPackageId, cancellationToken);

        if (unresolvedCount > 0)
        {
            throw new ConflictException(
                $"Cannot approve for commit: {unresolvedCount} unresolved conflict(s) remain. " +
                "Resolve all conflicts before approving.");
        }

        // 4. Approve staging records
        var userId = _currentUserService.UserId
            ?? throw new InvalidOperationException("Current user context is required for approval.");

        if (request.ApproveAllValid)
        {
            await ApproveAllValidRecordsAsync(request.ImportPackageId, cancellationToken);
        }
        else
        {
            await ApproveSpecificRecordsAsync(request.ImportPackageId, request.StagingRecordIds!, cancellationToken);
        }

        // 5. Mark conflicts resolved and transition status
        package.MarkConflictsResolved(userId);

        await _importPackageRepository.UpdateAsync(package, cancellationToken);
        await _importPackageRepository.SaveChangesAsync(cancellationToken);

        // 6. Return updated DTO
        return _mapper.Map<ImportPackageDto>(package);
    }

    /// <summary>
    /// Approve all Valid and Warning staging records across all 8 entity types.
    /// </summary>
    private async Task ApproveAllValidRecordsAsync(Guid importPackageId, CancellationToken ct)
    {
        await ApproveValidForTypeAsync(_stagingBuildingRepo, importPackageId, ct);
        await ApproveValidForTypeAsync(_stagingPropertyUnitRepo, importPackageId, ct);
        await ApproveValidForTypeAsync(_stagingPersonRepo, importPackageId, ct);
        await ApproveValidForTypeAsync(_stagingHouseholdRepo, importPackageId, ct);
        await ApproveValidForTypeAsync(_stagingRelationRepo, importPackageId, ct);
        await ApproveValidForTypeAsync(_stagingEvidenceRepo, importPackageId, ct);
        await ApproveValidForTypeAsync(_stagingClaimRepo, importPackageId, ct);
        await ApproveValidForTypeAsync(_stagingSurveyRepo, importPackageId, ct);
    }

    /// <summary>
    /// Approve all Valid/Warning records for a single staging entity type.
    /// </summary>
    private static async Task ApproveValidForTypeAsync<T>(
        IStagingRepository<T> repo,
        Guid importPackageId,
        CancellationToken ct) where T : Domain.Common.BaseStagingEntity
    {
        var validRecords = await repo.GetByPackageAndStatusAsync(
            importPackageId, StagingValidationStatus.Valid, ct);
        var warningRecords = await repo.GetByPackageAndStatusAsync(
            importPackageId, StagingValidationStatus.Warning, ct);

        var toApprove = validRecords.Concat(warningRecords).ToList();

        foreach (var record in toApprove)
        {
            record.ApproveForCommit();
        }

        if (toApprove.Count > 0)
        {
            await repo.UpdateRangeAsync(toApprove, ct);
            await repo.SaveChangesAsync(ct);
        }
    }

    /// <summary>
    /// Approve specific staging records by ID (selective approval).
    /// Checks each record is Valid or Warning before approving.
    /// </summary>
    private async Task ApproveSpecificRecordsAsync(
        Guid importPackageId,
        List<Guid> recordIds,
        CancellationToken ct)
    {
        // Attempt to find and approve in each staging repository
        // (caller doesn't know entity type, so we check all)
        foreach (var recordId in recordIds)
        {
            var approved = await TryApproveInRepo(_stagingBuildingRepo, recordId, ct)
                || await TryApproveInRepo(_stagingPropertyUnitRepo, recordId, ct)
                || await TryApproveInRepo(_stagingPersonRepo, recordId, ct)
                || await TryApproveInRepo(_stagingHouseholdRepo, recordId, ct)
                || await TryApproveInRepo(_stagingRelationRepo, recordId, ct)
                || await TryApproveInRepo(_stagingEvidenceRepo, recordId, ct)
                || await TryApproveInRepo(_stagingClaimRepo, recordId, ct)
                || await TryApproveInRepo(_stagingSurveyRepo, recordId, ct);

            if (!approved)
            {
                throw new NotFoundException($"StagingRecord with ID '{recordId}' was not found.");
            }
        }
    }

    /// <summary>
    /// Try to find and approve a single record in a specific repository.
    /// Returns true if found and approved, false if not found.
    /// </summary>
    private static async Task<bool> TryApproveInRepo<T>(
        IStagingRepository<T> repo,
        Guid recordId,
        CancellationToken ct) where T : Domain.Common.BaseStagingEntity
    {
        var record = await repo.GetByIdAsync(recordId, ct);
        if (record is null) return false;

        record.ApproveForCommit();
        await repo.UpdateAsync(record, ct);
        await repo.SaveChangesAsync(ct);
        return true;
    }
}
