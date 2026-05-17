using Microsoft.Extensions.Options;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TRRCMS.Application.Reporting.Common;
using TRRCMS.Application.Reporting.Dtos;

namespace TRRCMS.Infrastructure.Reporting;

/// <summary>
/// Renders the four report types to PDF using QuestPDF. Documents are built with
/// a shared header/footer/title layout; only the body section varies per report.
///
/// IMPORTANT: QuestPDF.Settings.License must be configured at application startup
/// (see DI registration). The Community license is selected here; verify the
/// organization's revenue tier (<$1M USD/yr) before production deployment.
/// </summary>
public sealed class QuestPdfReportRenderer : IPdfReportRenderer
{
    private readonly IReportLocalizer _loc;
    private readonly ReportFontSettings _fonts;

    public QuestPdfReportRenderer(
        IReportLocalizer localizer,
        IOptions<ReportFontSettings> fonts)
    {
        _loc = localizer;
        _fonts = fonts.Value;
    }

    // ── Public API ───────────────────────────────────────────────────────

    public byte[] RenderSurveyActivity(SurveyActivityReportDto data, ReportLocale locale)
        => Build(locale, _loc.T("sa.title", locale),
            container => ComposeSurveyActivity(container, data, locale));

    public byte[] RenderBuildingInventory(BuildingInventoryReportDto data, ReportLocale locale)
        => Build(locale, _loc.T("bi.title", locale),
            container => ComposeBuildingInventory(container, data, locale));

    public byte[] RenderImportPipeline(ImportPipelineReportDto data, ReportLocale locale)
        => Build(locale, _loc.T("ip.title", locale),
            container => ComposeImportPipeline(container, data, locale));

    public byte[] RenderAuditExport(AuditExportReportDto data, ReportLocale locale)
        => Build(locale, _loc.T("ae.title", locale),
            container => ComposeAuditExport(container, data, locale));

    // ── Shared document scaffolding ─────────────────────────────────────

    private byte[] Build(ReportLocale locale, string title, Action<IContainer> bodyBuilder)
    {
        string family = locale == ReportLocale.Ar ? _fonts.ArabicFontFamily : _fonts.LatinFontFamily;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                if (locale == ReportLocale.Ar)
                    page.ContentFromRightToLeft();
                else
                    page.ContentFromLeftToRight();

                page.DefaultTextStyle(t => t.FontFamily(family).FontSize(10));

                page.Header().Element(c => ComposeHeader(c, title, locale));
                page.Content().PaddingVertical(10).Element(bodyBuilder);
                page.Footer().Element(c => ComposeFooter(c, locale));
            });
        }).GeneratePdf();
    }

    private void ComposeHeader(IContainer container, string title, ReportLocale locale)
    {
        container.Column(col =>
        {
            col.Item().Text(_loc.T("common.system", locale))
                .FontSize(9).FontColor(Colors.Grey.Darken1);
            col.Item().Text(title)
                .FontSize(16).Bold();
            col.Item().PaddingTop(2).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
        });
    }

    private void ComposeFooter(IContainer container, ReportLocale locale)
    {
        container.AlignCenter().Text(t =>
        {
            t.DefaultTextStyle(s => s.FontSize(8).FontColor(Colors.Grey.Darken1));
            t.Span($"{_loc.T("common.page", locale)} ");
            t.CurrentPageNumber();
            t.Span($" {_loc.T("common.of", locale)} ");
            t.TotalPages();
        });
    }

    private void ComposeMeta(IContainer container, ReportLocale locale,
        DateTime generatedAtUtc, DateTime? from, DateTime? to,
        IEnumerable<(string Label, string Value)>? extras = null)
    {
        container.Column(col =>
        {
            col.Spacing(2);
            col.Item().Text(t =>
            {
                t.Span($"{_loc.T("common.generated_at", locale)}: ").Bold();
                t.Span(generatedAtUtc.ToString("yyyy-MM-dd HH:mm 'UTC'"));
            });

            col.Item().Text(t =>
            {
                t.Span($"{_loc.T("common.date_range", locale)}: ").Bold();
                if (from is null && to is null)
                    t.Span(_loc.T("common.all_time", locale));
                else
                    t.Span($"{(from?.ToString("yyyy-MM-dd") ?? "—")}  →  {(to?.ToString("yyyy-MM-dd") ?? "—")}");
            });

            if (extras != null)
            {
                foreach (var (label, value) in extras)
                {
                    if (string.IsNullOrWhiteSpace(value)) continue;
                    col.Item().Text(t =>
                    {
                        t.Span($"{label}: ").Bold();
                        t.Span(value);
                    });
                }
            }
        });
    }

    private void EmptyState(IContainer container, ReportLocale locale)
    {
        container.PaddingVertical(20).AlignCenter().Text(_loc.T("common.no_data", locale))
            .FontColor(Colors.Grey.Darken1).Italic();
    }

    // ── Survey Activity ─────────────────────────────────────────────────

    private void ComposeSurveyActivity(IContainer body, SurveyActivityReportDto data, ReportLocale locale)
    {
        body.Column(col =>
        {
            col.Spacing(10);
            col.Item().Element(c => ComposeMeta(c, locale, data.GeneratedAtUtc, data.FromUtc, data.ToUtc));

            col.Item().Text(t =>
            {
                t.Span($"{_loc.T("sa.summary.total_done", locale)}: ").Bold();
                t.Span(data.TotalSurveysCompleted.ToString());
                t.Span("   ");
                t.Span($"{_loc.T("sa.summary.total_draft", locale)}: ").Bold();
                t.Span(data.TotalSurveysDraft.ToString());
            });

            col.Item().PaddingTop(6).Text(_loc.T("sa.field_collectors", locale)).Bold().FontSize(12);
            col.Item().Element(c => SurveyActivityTable(c, data.FieldCollectors, locale, includeBuildings: true));

            col.Item().PaddingTop(6).Text(_loc.T("sa.office_clerks", locale)).Bold().FontSize(12);
            col.Item().Element(c => SurveyActivityTable(c, data.OfficeClerks, locale, includeBuildings: false));
        });
    }

    private void SurveyActivityTable(IContainer container, List<SurveyActivityRow> rows,
        ReportLocale locale, bool includeBuildings)
    {
        if (rows.Count == 0) { EmptyState(container, locale); return; }

        container.Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.RelativeColumn(2); // username
                c.RelativeColumn(3); // full name
                c.RelativeColumn(1); // completed
                c.RelativeColumn(1); // draft
                c.RelativeColumn(1); // total
                if (includeBuildings)
                {
                    c.RelativeColumn(1); // assigned b
                    c.RelativeColumn(1); // completed b
                }
            });

            table.Header(h =>
            {
                HeaderCell(h, _loc.T("sa.col.username", locale));
                HeaderCell(h, _loc.T("sa.col.full_name", locale));
                HeaderCell(h, _loc.T("sa.col.completed", locale));
                HeaderCell(h, _loc.T("sa.col.draft", locale));
                HeaderCell(h, _loc.T("sa.col.total", locale));
                if (includeBuildings)
                {
                    HeaderCell(h, _loc.T("sa.col.assigned_b", locale));
                    HeaderCell(h, _loc.T("sa.col.completed_b", locale));
                }
            });

            foreach (var r in rows)
            {
                BodyCell(table, r.Username);
                BodyCell(table, r.FullName);
                BodyCell(table, r.SurveysCompleted.ToString());
                BodyCell(table, r.SurveysDraft.ToString());
                BodyCell(table, r.TotalSurveys.ToString());
                if (includeBuildings)
                {
                    BodyCell(table, r.AssignedBuildings.ToString());
                    BodyCell(table, r.CompletedBuildings.ToString());
                }
            }
        });
    }

    // ── Building Inventory ──────────────────────────────────────────────

    private void ComposeBuildingInventory(IContainer body, BuildingInventoryReportDto data, ReportLocale locale)
    {
        body.Column(col =>
        {
            col.Spacing(10);
            col.Item().Element(c => ComposeMeta(c, locale, data.GeneratedAtUtc, null, null,
                extras: new[] { (_loc.T("bi.filter.neighborhood", locale), data.NeighborhoodCodeFilter ?? string.Empty) }));

            col.Item().Text(t =>
            {
                t.Span($"{_loc.T("bi.summary.neighborhoods", locale)}: ").Bold();
                t.Span(data.TotalNeighborhoods.ToString());
                t.Span("   ");
                t.Span($"{_loc.T("bi.summary.with_b", locale)}: ").Bold();
                t.Span(data.NeighborhoodsWithBuildings.ToString());
                t.Span("   ");
                t.Span($"{_loc.T("bi.summary.b_total", locale)}: ").Bold();
                t.Span(data.TotalBuildings.ToString());
                t.Span("   ");
                t.Span($"{_loc.T("bi.summary.u_total", locale)}: ").Bold();
                t.Span(data.TotalPropertyUnits.ToString());
            });

            if (data.Rows.Count == 0) { col.Item().Element(c => EmptyState(c, locale)); return; }

            col.Item().Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(3);
                    c.RelativeColumn(3);
                    c.RelativeColumn(3);
                    c.RelativeColumn(1);
                    c.RelativeColumn(1);
                });
                table.Header(h =>
                {
                    HeaderCell(h, _loc.T("bi.col.code", locale));
                    HeaderCell(h, _loc.T("bi.col.name_ar", locale));
                    HeaderCell(h, _loc.T("bi.col.name_en", locale));
                    HeaderCell(h, _loc.T("bi.col.buildings", locale));
                    HeaderCell(h, _loc.T("bi.col.units", locale));
                });

                foreach (var r in data.Rows)
                {
                    BodyCell(table, r.NeighborhoodCode);
                    BodyCell(table, r.NameArabic);
                    BodyCell(table, r.NameEnglish);
                    BodyCell(table, r.BuildingCount.ToString());
                    BodyCell(table, r.PropertyUnitCount.ToString());
                }
            });
        });
    }

    // ── Import Pipeline ─────────────────────────────────────────────────

    private void ComposeImportPipeline(IContainer body, ImportPipelineReportDto data, ReportLocale locale)
    {
        body.Column(col =>
        {
            col.Spacing(10);
            col.Item().Element(c => ComposeMeta(c, locale, data.GeneratedAtUtc, data.FromUtc, data.ToUtc));

            col.Item().Text(t =>
            {
                t.Span($"{_loc.T("ip.summary.total", locale)}: ").Bold();
                t.Span(data.TotalPackages.ToString());
                t.Span("   ");
                t.Span($"{_loc.T("ip.summary.completed", locale)}: ").Bold();
                t.Span(data.CompletedPackages.ToString());
                t.Span("   ");
                t.Span($"{_loc.T("ip.summary.failed", locale)}: ").Bold();
                t.Span(data.FailedPackages.ToString());
                t.Span("   ");
                t.Span($"{_loc.T("ip.summary.pending", locale)}: ").Bold();
                t.Span(data.PendingPackages.ToString());
                t.Span("   ");
                t.Span($"{_loc.T("ip.summary.cancelled", locale)}: ").Bold();
                t.Span(data.CancelledPackages.ToString());
            });

            col.Item().Text(t =>
            {
                t.Span($"{_loc.T("ip.summary.records", locale)}: ").Bold();
                t.Span($"{data.TotalRecordsImported} / {data.TotalRecordsFailed} / {data.TotalRecordsSkipped}");
            });

            if (data.Packages.Count == 0) { col.Item().Element(c => EmptyState(c, locale)); return; }

            col.Item().Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(2);
                    c.RelativeColumn(3);
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                    c.RelativeColumn(1);
                    c.RelativeColumn(1);
                    c.RelativeColumn(1);
                });
                table.Header(h =>
                {
                    HeaderCell(h, _loc.T("ip.col.package", locale));
                    HeaderCell(h, _loc.T("ip.col.file", locale));
                    HeaderCell(h, _loc.T("ip.col.status", locale));
                    HeaderCell(h, _loc.T("ip.col.imported", locale));
                    HeaderCell(h, _loc.T("ip.col.completed", locale));
                    HeaderCell(h, _loc.T("ip.col.success", locale));
                    HeaderCell(h, _loc.T("ip.col.failed", locale));
                    HeaderCell(h, _loc.T("ip.col.skipped", locale));
                });

                foreach (var p in data.Packages)
                {
                    BodyCell(table, p.PackageNumber);
                    BodyCell(table, p.FileName);
                    BodyCell(table, p.Status);
                    BodyCell(table, p.ImportedDate?.ToString("yyyy-MM-dd") ?? string.Empty);
                    BodyCell(table, p.CompletedDate?.ToString("yyyy-MM-dd") ?? string.Empty);
                    BodyCell(table, p.SuccessfulCount.ToString());
                    BodyCell(table, p.FailedCount.ToString());
                    BodyCell(table, p.SkippedCount.ToString());
                }
            });
        });
    }

    // ── Audit Export ────────────────────────────────────────────────────

    private void ComposeAuditExport(IContainer body, AuditExportReportDto data, ReportLocale locale)
    {
        body.Column(col =>
        {
            col.Spacing(10);
            col.Item().Element(c => ComposeMeta(c, locale, data.GeneratedAtUtc, data.FromUtc, data.ToUtc,
                extras: new[]
                {
                    (_loc.T("ae.entity_filter", locale), data.EntityTypeFilter ?? string.Empty),
                    (_loc.T("ae.security_only", locale), data.SecurityOnly ? "✓" : string.Empty)
                }));

            col.Item().Text(t =>
            {
                t.Span($"{_loc.T("common.total", locale)}: ").Bold();
                t.Span(data.Entries.Count.ToString());
            });

            if (data.Entries.Count == 0) { col.Item().Element(c => EmptyState(c, locale)); return; }

            col.Item().Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(1);
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                    c.RelativeColumn(3);
                });
                table.Header(h =>
                {
                    HeaderCell(h, _loc.T("ae.col.number", locale));
                    HeaderCell(h, _loc.T("ae.col.timestamp", locale));
                    HeaderCell(h, _loc.T("ae.col.action", locale));
                    HeaderCell(h, _loc.T("ae.col.user", locale));
                    HeaderCell(h, _loc.T("ae.col.role", locale));
                    HeaderCell(h, _loc.T("ae.col.entity", locale));
                    HeaderCell(h, _loc.T("ae.col.description", locale));
                });

                foreach (var e in data.Entries)
                {
                    BodyCell(table, e.AuditLogNumber.ToString());
                    BodyCell(table, e.Timestamp.ToString("yyyy-MM-dd HH:mm"));
                    BodyCell(table, e.ActionType);
                    BodyCell(table, string.IsNullOrEmpty(e.UserFullName) ? e.Username : e.UserFullName);
                    BodyCell(table, e.UserRole);
                    BodyCell(table, $"{e.EntityType} {e.EntityIdentifier}".Trim());
                    BodyCell(table, e.ActionDescription);
                }
            });
        });
    }

    // ── Cell helpers ────────────────────────────────────────────────────

    private static void HeaderCell(TableCellDescriptor header, string text)
    {
        header.Cell().Background(Colors.Grey.Lighten3).Padding(4)
            .Text(text).Bold().FontSize(10);
    }

    private static void BodyCell(TableDescriptor table, string text)
    {
        table.Cell().BorderBottom(0.25f).BorderColor(Colors.Grey.Lighten2)
            .Padding(4).Text(text).FontSize(9);
    }
}
