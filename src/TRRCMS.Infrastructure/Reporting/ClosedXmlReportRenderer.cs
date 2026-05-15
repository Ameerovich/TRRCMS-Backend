using ClosedXML.Excel;
using TRRCMS.Application.Reporting.Common;
using TRRCMS.Application.Reporting.Dtos;

namespace TRRCMS.Infrastructure.Reporting;

/// <summary>
/// Renders the four report types to XLSX workbooks via ClosedXML.
/// One worksheet per report, with a metadata band at top and a styled
/// header row above the data table.
/// </summary>
public sealed class ClosedXmlReportRenderer : IExcelReportRenderer
{
    private readonly IReportLocalizer _loc;

    public ClosedXmlReportRenderer(IReportLocalizer localizer)
    {
        _loc = localizer;
    }

    // ── Survey Activity ─────────────────────────────────────────────────

    public byte[] RenderSurveyActivity(SurveyActivityReportDto data, ReportLocale locale)
    {
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet(_loc.T("sa.title", locale));
        int row = WriteMetadata(ws, locale,
            title: _loc.T("sa.title", locale),
            generatedAtUtc: data.GeneratedAtUtc,
            from: data.FromUtc, to: data.ToUtc);

        ws.Cell(row++, 1).Value = $"{_loc.T("sa.summary.total_done", locale)}: {data.TotalSurveysCompleted}";
        ws.Cell(row++, 1).Value = $"{_loc.T("sa.summary.total_draft", locale)}: {data.TotalSurveysDraft}";
        row++;

        ws.Cell(row, 1).Value = _loc.T("sa.field_collectors", locale);
        ws.Cell(row, 1).Style.Font.Bold = true; row++;

        var fcHeaders = new[]
        {
            _loc.T("sa.col.username", locale),
            _loc.T("sa.col.full_name", locale),
            _loc.T("sa.col.completed", locale),
            _loc.T("sa.col.draft", locale),
            _loc.T("sa.col.total", locale),
            _loc.T("sa.col.assigned_b", locale),
            _loc.T("sa.col.completed_b", locale)
        };
        row = WriteTableHeader(ws, row, fcHeaders);
        foreach (var r in data.FieldCollectors)
        {
            ws.Cell(row, 1).Value = r.Username;
            ws.Cell(row, 2).Value = r.FullName;
            ws.Cell(row, 3).Value = r.SurveysCompleted;
            ws.Cell(row, 4).Value = r.SurveysDraft;
            ws.Cell(row, 5).Value = r.TotalSurveys;
            ws.Cell(row, 6).Value = r.AssignedBuildings;
            ws.Cell(row, 7).Value = r.CompletedBuildings;
            row++;
        }
        row += 2;

        ws.Cell(row, 1).Value = _loc.T("sa.office_clerks", locale);
        ws.Cell(row, 1).Style.Font.Bold = true; row++;

        var ocHeaders = new[]
        {
            _loc.T("sa.col.username", locale),
            _loc.T("sa.col.full_name", locale),
            _loc.T("sa.col.completed", locale),
            _loc.T("sa.col.draft", locale),
            _loc.T("sa.col.total", locale)
        };
        row = WriteTableHeader(ws, row, ocHeaders);
        foreach (var r in data.OfficeClerks)
        {
            ws.Cell(row, 1).Value = r.Username;
            ws.Cell(row, 2).Value = r.FullName;
            ws.Cell(row, 3).Value = r.SurveysCompleted;
            ws.Cell(row, 4).Value = r.SurveysDraft;
            ws.Cell(row, 5).Value = r.TotalSurveys;
            row++;
        }

        if (locale == ReportLocale.Ar) ws.RightToLeft = true;
        ws.Columns().AdjustToContents();
        return ToBytes(wb);
    }

    // ── Building Inventory ──────────────────────────────────────────────

    public byte[] RenderBuildingInventory(BuildingInventoryReportDto data, ReportLocale locale)
    {
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet(_loc.T("bi.title", locale));
        int row = WriteMetadata(ws, locale,
            title: _loc.T("bi.title", locale),
            generatedAtUtc: data.GeneratedAtUtc,
            from: null, to: null,
            extras: new[]
            {
                (_loc.T("bi.filter.neighborhood", locale), data.NeighborhoodCodeFilter ?? string.Empty)
            });

        ws.Cell(row++, 1).Value = $"{_loc.T("bi.summary.neighborhoods", locale)}: {data.TotalNeighborhoods}";
        ws.Cell(row++, 1).Value = $"{_loc.T("bi.summary.with_b", locale)}: {data.NeighborhoodsWithBuildings}";
        ws.Cell(row++, 1).Value = $"{_loc.T("bi.summary.b_total", locale)}: {data.TotalBuildings}";
        ws.Cell(row++, 1).Value = $"{_loc.T("bi.summary.u_total", locale)}: {data.TotalPropertyUnits}";
        row++;

        var headers = new[]
        {
            _loc.T("bi.col.code", locale),
            _loc.T("bi.col.name_ar", locale),
            _loc.T("bi.col.name_en", locale),
            _loc.T("bi.col.buildings", locale),
            _loc.T("bi.col.units", locale)
        };
        row = WriteTableHeader(ws, row, headers);
        foreach (var r in data.Rows)
        {
            ws.Cell(row, 1).Value = r.NeighborhoodCode;
            ws.Cell(row, 2).Value = r.NameArabic;
            ws.Cell(row, 3).Value = r.NameEnglish;
            ws.Cell(row, 4).Value = r.BuildingCount;
            ws.Cell(row, 5).Value = r.PropertyUnitCount;
            row++;
        }

        if (locale == ReportLocale.Ar) ws.RightToLeft = true;
        ws.Columns().AdjustToContents();
        return ToBytes(wb);
    }

    // ── Import Pipeline ─────────────────────────────────────────────────

    public byte[] RenderImportPipeline(ImportPipelineReportDto data, ReportLocale locale)
    {
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet(_loc.T("ip.title", locale));
        int row = WriteMetadata(ws, locale,
            title: _loc.T("ip.title", locale),
            generatedAtUtc: data.GeneratedAtUtc,
            from: data.FromUtc, to: data.ToUtc);

        ws.Cell(row++, 1).Value = $"{_loc.T("ip.summary.total", locale)}: {data.TotalPackages}";
        ws.Cell(row++, 1).Value = $"{_loc.T("ip.summary.completed", locale)}: {data.CompletedPackages}";
        ws.Cell(row++, 1).Value = $"{_loc.T("ip.summary.failed", locale)}: {data.FailedPackages}";
        ws.Cell(row++, 1).Value = $"{_loc.T("ip.summary.pending", locale)}: {data.PendingPackages}";
        ws.Cell(row++, 1).Value = $"{_loc.T("ip.summary.cancelled", locale)}: {data.CancelledPackages}";
        ws.Cell(row++, 1).Value = $"{_loc.T("ip.summary.records", locale)}: {data.TotalRecordsImported} / {data.TotalRecordsFailed} / {data.TotalRecordsSkipped}";
        row++;

        var headers = new[]
        {
            _loc.T("ip.col.package", locale),
            _loc.T("ip.col.file", locale),
            _loc.T("ip.col.status", locale),
            _loc.T("ip.col.imported", locale),
            _loc.T("ip.col.completed", locale),
            _loc.T("ip.col.success", locale),
            _loc.T("ip.col.failed", locale),
            _loc.T("ip.col.skipped", locale)
        };
        row = WriteTableHeader(ws, row, headers);
        foreach (var p in data.Packages)
        {
            ws.Cell(row, 1).Value = p.PackageNumber;
            ws.Cell(row, 2).Value = p.FileName;
            ws.Cell(row, 3).Value = p.Status;
            ws.Cell(row, 4).Value = p.ImportedDate;
            if (p.ImportedDate.HasValue) ws.Cell(row, 4).Style.DateFormat.Format = "yyyy-mm-dd";
            ws.Cell(row, 5).Value = p.CompletedDate;
            if (p.CompletedDate.HasValue) ws.Cell(row, 5).Style.DateFormat.Format = "yyyy-mm-dd";
            ws.Cell(row, 6).Value = p.SuccessfulCount;
            ws.Cell(row, 7).Value = p.FailedCount;
            ws.Cell(row, 8).Value = p.SkippedCount;
            row++;
        }

        if (locale == ReportLocale.Ar) ws.RightToLeft = true;
        ws.Columns().AdjustToContents();
        return ToBytes(wb);
    }

    // ── Audit Export ────────────────────────────────────────────────────

    public byte[] RenderAuditExport(AuditExportReportDto data, ReportLocale locale)
    {
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet(_loc.T("ae.title", locale));
        int row = WriteMetadata(ws, locale,
            title: _loc.T("ae.title", locale),
            generatedAtUtc: data.GeneratedAtUtc,
            from: data.FromUtc, to: data.ToUtc,
            extras: new[]
            {
                (_loc.T("ae.entity_filter", locale), data.EntityTypeFilter ?? string.Empty),
                (_loc.T("ae.security_only", locale), data.SecurityOnly ? "Yes" : string.Empty)
            });

        ws.Cell(row++, 1).Value = $"{_loc.T("common.total", locale)}: {data.Entries.Count}";
        row++;

        var headers = new[]
        {
            _loc.T("ae.col.number", locale),
            _loc.T("ae.col.timestamp", locale),
            _loc.T("ae.col.action", locale),
            _loc.T("ae.col.result", locale),
            _loc.T("ae.col.user", locale),
            _loc.T("ae.col.role", locale),
            _loc.T("ae.col.entity", locale),
            _loc.T("ae.col.entity_id", locale),
            _loc.T("ae.col.ip", locale),
            _loc.T("ae.col.description", locale)
        };
        row = WriteTableHeader(ws, row, headers);
        foreach (var e in data.Entries)
        {
            ws.Cell(row, 1).Value = e.AuditLogNumber;
            ws.Cell(row, 2).Value = e.Timestamp;
            ws.Cell(row, 2).Style.DateFormat.Format = "yyyy-mm-dd hh:mm:ss";
            ws.Cell(row, 3).Value = e.ActionType;
            ws.Cell(row, 4).Value = e.ActionResult;
            ws.Cell(row, 5).Value = string.IsNullOrEmpty(e.UserFullName) ? e.Username : e.UserFullName;
            ws.Cell(row, 6).Value = e.UserRole;
            ws.Cell(row, 7).Value = e.EntityType ?? string.Empty;
            ws.Cell(row, 8).Value = e.EntityIdentifier ?? string.Empty;
            ws.Cell(row, 9).Value = e.IpAddress ?? string.Empty;
            ws.Cell(row, 10).Value = e.ActionDescription;
            row++;
        }

        if (locale == ReportLocale.Ar) ws.RightToLeft = true;
        ws.Columns().AdjustToContents();
        return ToBytes(wb);
    }

    // ── Helpers ─────────────────────────────────────────────────────────

    private int WriteMetadata(IXLWorksheet ws, ReportLocale locale, string title,
        DateTime generatedAtUtc, DateTime? from, DateTime? to,
        IEnumerable<(string Label, string Value)>? extras = null)
    {
        int row = 1;
        ws.Cell(row, 1).Value = title;
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 1).Style.Font.FontSize = 14;
        row++;

        ws.Cell(row, 1).Value = _loc.T("common.system", locale);
        ws.Cell(row, 1).Style.Font.FontColor = XLColor.Gray;
        row++;

        ws.Cell(row++, 1).Value = $"{_loc.T("common.generated_at", locale)}: {generatedAtUtc:yyyy-MM-dd HH:mm} UTC";

        string range = (from is null && to is null)
            ? _loc.T("common.all_time", locale)
            : $"{(from?.ToString("yyyy-MM-dd") ?? "—")} → {(to?.ToString("yyyy-MM-dd") ?? "—")}";
        ws.Cell(row++, 1).Value = $"{_loc.T("common.date_range", locale)}: {range}";

        if (extras != null)
        {
            foreach (var (label, value) in extras)
            {
                if (string.IsNullOrWhiteSpace(value)) continue;
                ws.Cell(row++, 1).Value = $"{label}: {value}";
            }
        }

        row++;
        return row;
    }

    private static int WriteTableHeader(IXLWorksheet ws, int row, string[] headers)
    {
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(row, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
            cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
        }
        return row + 1;
    }


    private static byte[] ToBytes(XLWorkbook wb)
    {
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }
}
