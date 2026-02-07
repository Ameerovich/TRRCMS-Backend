using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Import.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Import.Commands.DetectDuplicates;

/// <summary>
/// Handler for DetectDuplicatesCommand.
/// Orchestrates:
///   1. Validate package exists and is in correct status
///   2. If re-running, clean up previous conflict records for this package
///   3. Run IDuplicateDetectionService.DetectAsync()
///   4. Update ImportPackage with conflict results
///      (transitions to ReviewingConflicts or ReadyToCommit)
///   5. Return DuplicateDetectionResultDto
///
/// UC-003 Stage 2 — S14 (Detect Anomalies and Potential Duplicates).
/// </summary>
public class DetectDuplicatesCommandHandler
    : IRequestHandler<DetectDuplicatesCommand, DuplicateDetectionResultDto>
{
    private readonly IImportPackageRepository _packageRepository;
    private readonly IDuplicateDetectionService _detectionService;
    private readonly IConflictResolutionRepository _conflictRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DetectDuplicatesCommandHandler> _logger;

    public DetectDuplicatesCommandHandler(
        IImportPackageRepository packageRepository,
        IDuplicateDetectionService detectionService,
        IConflictResolutionRepository conflictRepository,
        ICurrentUserService currentUserService,
        ILogger<DetectDuplicatesCommandHandler> logger)
    {
        _packageRepository = packageRepository;
        _detectionService = detectionService;
        _conflictRepository = conflictRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<DuplicateDetectionResultDto> Handle(
        DetectDuplicatesCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated");

        // ============================================================
        // STEP 1: Load and validate package
        // ============================================================
        var package = await _packageRepository.GetByIdAsync(request.ImportPackageId, cancellationToken)
            ?? throw new KeyNotFoundException(
                $"Import package not found: {request.ImportPackageId}");

        if (package.Status != ImportStatus.Staging &&
            package.Status != ImportStatus.ReviewingConflicts)
        {
            throw new InvalidOperationException(
                $"Cannot run duplicate detection on package with status '{package.Status}'. " +
                $"Expected Staging or ReviewingConflicts.");
        }

        _logger.LogInformation(
            "Starting duplicate detection for package {PackageId} (Id={Id}, Status={Status})",
            package.PackageId, package.Id, package.Status);

        // ============================================================
        // STEP 2: If re-running, clean up previous conflicts for this package
        // ============================================================
        if (package.Status == ImportStatus.ReviewingConflicts)
        {
            _logger.LogInformation(
                "Re-run detected — cleaning up previous conflict records for package {PackageId}",
                package.PackageId);

            var previousConflicts = await _conflictRepository
                .GetByPackageIdAsync(package.Id, cancellationToken);

            foreach (var conflict in previousConflicts.Where(c => c.Status == "PendingReview"))
            {
                conflict.Ignore("Superseded by re-run of duplicate detection", userId);
            }

            if (previousConflicts.Count > 0)
            {
                await _conflictRepository.SaveChangesAsync(cancellationToken);
            }
        }

        try
        {
            // ============================================================
            // STEP 3: Run duplicate detection
            // ============================================================
            var result = await _detectionService.DetectAsync(
                package.Id, userId, cancellationToken);

            _logger.LogInformation(
                "Duplicate detection complete for package {PackageId}: " +
                "{PersonDupes} person duplicates, {PropertyDupes} property duplicates, " +
                "{Total} total conflicts (in {Duration}ms)",
                package.PackageId,
                result.PersonDuplicatesFound,
                result.PropertyDuplicatesFound,
                result.TotalConflictsCreated,
                result.Duration.TotalMilliseconds);

            // ============================================================
            // STEP 4: Update ImportPackage with conflict results
            // ============================================================
            // SetConflictResults transitions to:
            //   - ReviewingConflicts if totalConflicts > 0
            //   - ReadyToCommit if totalConflicts == 0
            package.SetConflictResults(
                result.PersonDuplicatesFound,
                result.PropertyDuplicatesFound,
                result.TotalConflictsCreated,
                userId);

            await _packageRepository.UpdateAsync(package, cancellationToken);
            await _packageRepository.SaveChangesAsync(cancellationToken);

            // ============================================================
            // STEP 5: Build response DTO
            // ============================================================
            var message = result.TotalConflictsCreated > 0
                ? $"Found {result.TotalConflictsCreated} potential duplicates requiring review."
                : "No duplicates detected. Package is ready to commit.";

            return new DuplicateDetectionResultDto
            {
                ImportPackageId = package.Id,
                PackageNumber = package.PackageNumber,
                Status = package.Status.ToString(),
                PersonDuplicatesFound = result.PersonDuplicatesFound,
                PropertyDuplicatesFound = result.PropertyDuplicatesFound,
                TotalConflictsCreated = result.TotalConflictsCreated,
                PersonsScanned = result.PersonsScanned,
                BuildingsScanned = result.BuildingsScanned,
                DurationMs = result.Duration.TotalMilliseconds,
                ConflictIds = result.ConflictIds,
                Message = message
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Duplicate detection failed for package {PackageId}", package.PackageId);

            package.MarkAsFailed(
                $"Duplicate detection failed: {ex.Message}",
                JsonSerializer.Serialize(new { ex.Message, ex.StackTrace }),
                userId);

            await _packageRepository.UpdateAsync(package, cancellationToken);
            await _packageRepository.SaveChangesAsync(cancellationToken);

            throw;
        }
    }
}
