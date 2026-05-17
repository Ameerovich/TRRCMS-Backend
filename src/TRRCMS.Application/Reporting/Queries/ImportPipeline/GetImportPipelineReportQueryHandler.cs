using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Reporting.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Reporting.Queries.ImportPipeline;

/// <summary>
/// Builds the import pipeline report from ImportPackage rows filtered by import date.
/// </summary>
public sealed class GetImportPipelineReportQueryHandler
    : IRequestHandler<GetImportPipelineReportQuery, ImportPipelineReportDto>
{
    private readonly IUnitOfWork _uow;

    public GetImportPipelineReportQueryHandler(IUnitOfWork uow)
    {
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
    }

    public async Task<ImportPipelineReportDto> Handle(
        GetImportPipelineReportQuery request,
        CancellationToken cancellationToken)
    {
        var (packages, _) = await _uow.ImportPackages.SearchAsync(
            status: null,
            exportedByUserId: null,
            importedByUserId: null,
            importedAfter: request.From,
            importedBefore: request.To,
            searchTerm: null,
            page: 1,
            pageSize: 10_000,
            sortBy: "ImportedDate",
            sortDescending: true,
            cancellationToken: cancellationToken);

        var rows = packages.Select(p => new ImportPipelineRow
        {
            PackageNumber = p.PackageNumber,
            FileName = p.FileName,
            Status = p.Status.ToString(),
            ImportedDate = p.ImportedDate,
            CompletedDate = p.CommittedDate,
            SuccessfulCount = p.SuccessfulImportCount,
            FailedCount = p.FailedImportCount,
            SkippedCount = p.SkippedRecordCount
        }).ToList();

        return new ImportPipelineReportDto
        {
            FromUtc = request.From,
            ToUtc = request.To,
            GeneratedAtUtc = DateTime.UtcNow,
            TotalPackages = packages.Count,
            CompletedPackages = packages.Count(p => p.Status == ImportStatus.Completed),
            FailedPackages = packages.Count(p => p.Status == ImportStatus.Failed),
            PendingPackages = packages.Count(p =>
                p.Status == ImportStatus.Pending ||
                p.Status == ImportStatus.Validating ||
                p.Status == ImportStatus.Staging ||
                p.Status == ImportStatus.ReviewingConflicts),
            CancelledPackages = packages.Count(p => p.Status == ImportStatus.Cancelled),
            TotalRecordsImported = packages.Sum(p => p.SuccessfulImportCount),
            TotalRecordsFailed = packages.Sum(p => p.FailedImportCount),
            TotalRecordsSkipped = packages.Sum(p => p.SkippedRecordCount),
            Packages = rows
        };
    }
}
