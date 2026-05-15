namespace TRRCMS.Application.Reporting.Dtos;

public sealed class ImportPipelineReportDto
{
    public DateTime? FromUtc { get; set; }
    public DateTime? ToUtc { get; set; }
    public DateTime GeneratedAtUtc { get; set; }

    public int TotalPackages { get; set; }
    public int CompletedPackages { get; set; }
    public int FailedPackages { get; set; }
    public int PendingPackages { get; set; }
    public int CancelledPackages { get; set; }

    public int TotalRecordsImported { get; set; }
    public int TotalRecordsFailed { get; set; }
    public int TotalRecordsSkipped { get; set; }

    public List<ImportPipelineRow> Packages { get; set; } = new();
}

public sealed class ImportPipelineRow
{
    public string PackageNumber { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? ImportedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public int SuccessfulCount { get; set; }
    public int FailedCount { get; set; }
    public int SkippedCount { get; set; }
}
