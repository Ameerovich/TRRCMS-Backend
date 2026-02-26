using MediatR;
using Microsoft.Extensions.Logging;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Import.Dtos;
using TRRCMS.Domain.Enums;
using System.Diagnostics;

namespace TRRCMS.Application.Import.Commands.CommitPackage;

/// <summary>
/// Handles the CommitPackageCommand — the core commit orchestrator.
///
/// Delegates actual commit work to <see cref="ICommitService"/> which handles:
///   - Staging → production entity mapping
///   - Record ID generation (FR-D-8)
///   - Attachment deduplication (FR-D-9)
///   - Transactional insert of production entities
///
/// This handler is responsible for:
///   - Pre-condition validation (status, conflicts, approvals)
///   - Package status transitions (Committing → Completed/Failed)
///   - Archiving the .uhc package after commit
///   - Optional staging data cleanup
///   - Error handling and reporting
///
/// UC-003 Stage 4 — S17 (Commit to Production), S11 (Archive).
/// </summary>
public class CommitPackageCommandHandler : IRequestHandler<CommitPackageCommand, CommitReportDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICommitService _commitService;
    private readonly IStagingService _stagingService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CommitPackageCommandHandler> _logger;

    public CommitPackageCommandHandler(
        IUnitOfWork unitOfWork,
        ICommitService commitService,
        IStagingService stagingService,
        ICurrentUserService currentUserService,
        ILogger<CommitPackageCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _commitService = commitService;
        _stagingService = stagingService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<CommitReportDto> Handle(
        CommitPackageCommand request,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        // 1. Load and validate package
        var package = await _unitOfWork.ImportPackages.GetByIdAsync(request.ImportPackageId, cancellationToken)
            ?? throw new NotFoundException($"ImportPackage with ID '{request.ImportPackageId}' was not found.");

        var userId = _currentUserService.UserId
            ?? throw new InvalidOperationException("Current user context is required for commit.");

        // 2. Validate pre-conditions
        if (package.Status != ImportStatus.ReadyToCommit)
        {
            throw new ConflictException(
                $"Cannot commit package. Current status is '{package.Status}'. " +
                "Package must be in 'ReadyToCommit' status. Call approve endpoint first.");
        }

        var unresolvedCount = await _unitOfWork.ConflictResolutions
            .GetUnresolvedCountByPackageIdAsync(request.ImportPackageId, cancellationToken);

        if (unresolvedCount > 0)
        {
            throw new ConflictException(
                $"Cannot commit: {unresolvedCount} unresolved conflict(s) remain.");
        }

        // 3. Transition to Committing
        _logger.LogInformation(
            "Starting commit for package {PackageNumber} ({PackageId})",
            package.PackageNumber, package.PackageId);

        package.StartCommit(userId);
        await _unitOfWork.ImportPackages.UpdateAsync(package, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        CommitReportDto report;

        try
        {
            // 4. Execute commit via ICommitService (transactional)
            report = await _commitService.CommitAsync(
                request.ImportPackageId, userId, cancellationToken);

            // 5. Update package based on commit outcome
            if (report.IsFullySuccessful)
            {
                package.MarkAsCompleted(
                    successCount: report.TotalRecordsCommitted,
                    failedCount: 0,
                    skippedCount: report.TotalRecordsSkipped,
                    importSummary: BuildImportSummary(report),
                    committedByUserId: userId,
                    modifiedByUserId: userId);

                _logger.LogInformation(
                    "Commit completed successfully for package {PackageNumber}. " +
                    "{Committed} records committed.",
                    package.PackageNumber, report.TotalRecordsCommitted);
            }
            else if (report.TotalRecordsCommitted > 0)
            {
                package.MarkAsPartiallyCompleted(
                    successCount: report.TotalRecordsCommitted,
                    failedCount: report.TotalRecordsFailed,
                    skippedCount: report.TotalRecordsSkipped,
                    importSummary: BuildImportSummary(report),
                    modifiedByUserId: userId);

                _logger.LogWarning(
                    "Commit partially completed for package {PackageNumber}. " +
                    "{Committed} committed, {Failed} failed.",
                    package.PackageNumber, report.TotalRecordsCommitted, report.TotalRecordsFailed);
            }
            else
            {
                package.MarkAsFailed(
                    "All records failed during commit.",
                    System.Text.Json.JsonSerializer.Serialize(report.Errors),
                    userId);

                _logger.LogError(
                    "Commit failed for package {PackageNumber}. All records failed.",
                    package.PackageNumber);
            }

            await _unitOfWork.ImportPackages.UpdateAsync(package, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 6. Archive .uhc package (post-commit, non-transactional)
            if (report.TotalRecordsCommitted > 0)
            {
                try
                {
                    var archivePath = await _commitService.ArchivePackageAsync(
                        request.ImportPackageId, userId, cancellationToken);

                    package.Archive(archivePath, userId);
                    await _unitOfWork.ImportPackages.UpdateAsync(package, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    report.IsArchived = true;
                    report.ArchivePath = archivePath;

                    _logger.LogInformation(
                        "Package {PackageNumber} archived to {ArchivePath}",
                        package.PackageNumber, archivePath);
                }
                catch (Exception ex)
                {
                    // Archive failure is non-critical — log but don't fail the commit
                    _logger.LogWarning(ex,
                        "Failed to archive package {PackageNumber}. Commit data is safe.",
                        package.PackageNumber);
                }
            }

            // 7. Optional staging cleanup
            if (request.CleanupStagingAfterCommit && report.IsFullySuccessful)
            {
                try
                {
                    await _stagingService.CleanupStagingAsync(request.ImportPackageId, cancellationToken);
                    _logger.LogInformation(
                        "Staging data cleaned up for package {PackageNumber}",
                        package.PackageNumber);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Failed to cleanup staging data for package {PackageNumber}.",
                        package.PackageNumber);
                }
            }
        }
        catch (Exception ex)
        {
            // Log the full error details server-side (inner exceptions, stack traces)
            _logger.LogError(ex,
                "Commit failed for package {PackageNumber}: {ErrorMessage}",
                package.PackageNumber, ex.Message);

            try
            {
                // CRITICAL: Clear the change tracker before saving MarkAsFailed.
                // After a failed commit, the tracker still has dirty entities (e.g., the Building
                // that caused the unique constraint violation) in Added state. If we try to
                // SaveChanges without clearing, EF re-attempts all pending changes → same error.
                _unitOfWork.DetachAllEntities();

                // Re-fetch the package fresh (it was detached above)
                package = await _unitOfWork.ImportPackages.GetByIdAsync(request.ImportPackageId, cancellationToken)
                ?? throw new NotFoundException($"ImportPackage with ID '{request.ImportPackageId}' was not found.");

                // Store technical details in ErrorLog (internal, not exposed via API)
                var rootCause = ex;
                while (rootCause.InnerException != null)
                    rootCause = rootCause.InnerException;

                package.MarkAsFailed(
                    "An error occurred during the commit process. Please check the server logs or contact support.",
                    System.Text.Json.JsonSerializer.Serialize(new
                    {
                        Message = ex.Message,
                        InnerException = rootCause.Message,
                        InnerExceptionType = rootCause.GetType().FullName,
                        ex.StackTrace
                    }),
                    userId);

                await _unitOfWork.ImportPackages.UpdateAsync(package, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (Exception saveEx)
            {
                _logger.LogError(saveEx,
                    "Failed to save MarkAsFailed status for package {PackageNumber}. " +
                    "Use reset-commit endpoint to recover.",
                    package.PackageNumber);
            }

            report = new CommitReportDto
            {
                ImportPackageId = request.ImportPackageId,
                PackageNumber = package.PackageNumber,
                Status = ImportStatus.Failed.ToString(),
                CommittedByUserId = userId,
                CommittedAtUtc = DateTime.UtcNow,
                Errors = { new CommitErrorDto
                {
                    ErrorMessage = "An error occurred during the commit process. " +
                                   "Check server logs for details."
                }}
            };
        }

        stopwatch.Stop();
        report.Duration = stopwatch.Elapsed;

        return report;
    }

    /// <summary>
    /// Build a human-readable import summary for the ImportPackage.ImportSummary field.
    /// </summary>
    private static string BuildImportSummary(CommitReportDto report)
    {
        var lines = new List<string>
        {
            $"Commit completed at {report.CommittedAtUtc:yyyy-MM-dd HH:mm:ss} UTC",
            $"Total committed: {report.TotalRecordsCommitted}",
            $"Total failed: {report.TotalRecordsFailed}",
            $"Total skipped: {report.TotalRecordsSkipped}",
            $"Success rate: {report.SuccessRate}%",
            $"Duplicate attachments deduplicated: {report.DuplicateAttachmentsFound}"
        };

        if (report.Buildings.Committed > 0) lines.Add($"  Buildings: {report.Buildings.Committed}");
        if (report.PropertyUnits.Committed > 0) lines.Add($"  Property Units: {report.PropertyUnits.Committed}");
        if (report.Persons.Committed > 0) lines.Add($"  Persons: {report.Persons.Committed}");
        if (report.Households.Committed > 0) lines.Add($"  Households: {report.Households.Committed}");
        if (report.PersonPropertyRelations.Committed > 0) lines.Add($"  Relations: {report.PersonPropertyRelations.Committed}");
        if (report.Evidences.Committed > 0) lines.Add($"  Evidence: {report.Evidences.Committed}");
        if (report.Claims.Committed > 0) lines.Add($"  Claims: {report.Claims.Committed}");
        if (report.Surveys.Committed > 0) lines.Add($"  Surveys: {report.Surveys.Committed}");

        return string.Join("\n", lines);
    }
}