using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Reporting.Common;
using TRRCMS.Application.Reporting.Queries.AuditExport;
using TRRCMS.Application.Reporting.Queries.BuildingInventory;
using TRRCMS.Application.Reporting.Queries.ImportPipeline;
using TRRCMS.Application.Reporting.Queries.SurveyActivity;
using TRRCMS.Domain.Enums;

namespace TRRCMS.WebAPI.Controllers;

/// <summary>
/// Statistical &amp; operational reports — PDF and XLSX output.
///
/// Endpoints:
///   GET /api/v1/reports/survey-activity     (Reports.ExportSurveys)
///   GET /api/v1/reports/building-inventory  (Reports.ExportBuildings)
///   GET /api/v1/reports/import-pipeline     (Reports.ExportImports)
///   GET /api/v1/reports/audit-export        (Reports.ExportAudit)
///
/// All endpoints accept `format=pdf|xlsx` and `locale=ar|en` query parameters
/// and stream the generated file with a Content-Disposition: attachment header.
/// Every successful generation emits an audit event (AuditActionType.ReportGenerated).
/// </summary>
[ApiController]
[Route("api/v1/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private const string XlsxMime = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    private const string PdfMime = "application/pdf";

    private readonly IMediator _mediator;
    private readonly IPdfReportRenderer _pdf;
    private readonly IExcelReportRenderer _xlsx;
    private readonly IAuditService _audit;

    public ReportsController(
        IMediator mediator,
        IPdfReportRenderer pdf,
        IExcelReportRenderer xlsx,
        IAuditService audit)
    {
        _mediator = mediator;
        _pdf = pdf;
        _xlsx = xlsx;
        _audit = audit;
    }

    /// <summary>
    /// Survey activity report — per field collector and office clerk.
    /// </summary>
    [HttpGet("survey-activity")]
    [Authorize(Policy = "CanExportSurveysReport")]
    public async Task<IActionResult> SurveyActivity(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] ReportFormat format = ReportFormat.Pdf,
        [FromQuery] ReportLocale locale = ReportLocale.En,
        CancellationToken ct = default)
    {
        var data = await _mediator.Send(new GetSurveyActivityReportQuery(from, to), ct);

        var (bytes, mime, ext) = format == ReportFormat.Xlsx
            ? (_xlsx.RenderSurveyActivity(data, locale), XlsxMime, "xlsx")
            : (_pdf.RenderSurveyActivity(data, locale), PdfMime, "pdf");

        await LogExport("survey-activity", format, locale,
            $"from={from:yyyy-MM-dd};to={to:yyyy-MM-dd};rows={data.FieldCollectors.Count + data.OfficeClerks.Count}", ct);

        return File(bytes, mime, FileName("survey-activity", locale, ext));
    }

    /// <summary>
    /// Building &amp; property unit inventory report — optionally filtered by neighborhood code.
    /// </summary>
    [HttpGet("building-inventory")]
    [Authorize(Policy = "CanExportBuildingsReport")]
    public async Task<IActionResult> BuildingInventory(
        [FromQuery] string? neighborhoodCode,
        [FromQuery] ReportFormat format = ReportFormat.Pdf,
        [FromQuery] ReportLocale locale = ReportLocale.En,
        CancellationToken ct = default)
    {
        var data = await _mediator.Send(new GetBuildingInventoryReportQuery(neighborhoodCode), ct);

        var (bytes, mime, ext) = format == ReportFormat.Xlsx
            ? (_xlsx.RenderBuildingInventory(data, locale), XlsxMime, "xlsx")
            : (_pdf.RenderBuildingInventory(data, locale), PdfMime, "pdf");

        await LogExport("building-inventory", format, locale,
            $"neighborhood={neighborhoodCode};rows={data.Rows.Count}", ct);

        return File(bytes, mime, FileName("building-inventory", locale, ext));
    }

    /// <summary>
    /// Import pipeline report — packages imported within the date range, with status totals.
    /// </summary>
    [HttpGet("import-pipeline")]
    [Authorize(Policy = "CanExportImportsReport")]
    public async Task<IActionResult> ImportPipeline(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] ReportFormat format = ReportFormat.Pdf,
        [FromQuery] ReportLocale locale = ReportLocale.En,
        CancellationToken ct = default)
    {
        var data = await _mediator.Send(new GetImportPipelineReportQuery(from, to), ct);

        var (bytes, mime, ext) = format == ReportFormat.Xlsx
            ? (_xlsx.RenderImportPipeline(data, locale), XlsxMime, "xlsx")
            : (_pdf.RenderImportPipeline(data, locale), PdfMime, "pdf");

        await LogExport("import-pipeline", format, locale,
            $"from={from:yyyy-MM-dd};to={to:yyyy-MM-dd};packages={data.Packages.Count}", ct);

        return File(bytes, mime, FileName("import-pipeline", locale, ext));
    }

    /// <summary>
    /// Audit log export — capped at 10,000 entries, optionally filtered by entity type or security-only.
    /// </summary>
    [HttpGet("audit-export")]
    [Authorize(Policy = "CanExportAuditReport")]
    public async Task<IActionResult> AuditExport(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? entityType,
        [FromQuery] bool securityOnly = false,
        [FromQuery] int maxRows = 10_000,
        [FromQuery] ReportFormat format = ReportFormat.Pdf,
        [FromQuery] ReportLocale locale = ReportLocale.En,
        CancellationToken ct = default)
    {
        var data = await _mediator.Send(
            new GetAuditExportReportQuery(from, to, entityType, securityOnly, maxRows), ct);

        var (bytes, mime, ext) = format == ReportFormat.Xlsx
            ? (_xlsx.RenderAuditExport(data, locale), XlsxMime, "xlsx")
            : (_pdf.RenderAuditExport(data, locale), PdfMime, "pdf");

        await LogExport("audit-export", format, locale,
            $"from={from:yyyy-MM-dd};to={to:yyyy-MM-dd};entity={entityType};securityOnly={securityOnly};rows={data.Entries.Count}", ct);

        return File(bytes, mime, FileName("audit-export", locale, ext));
    }

    // ── Helpers ─────────────────────────────────────────────────────────

    private static string FileName(string slug, ReportLocale locale, string ext) =>
        $"{slug}_{DateTime.UtcNow:yyyyMMdd}_{locale.ToString().ToLowerInvariant()}.{ext}";

    private Task LogExport(string reportSlug, ReportFormat format, ReportLocale locale, string filters, CancellationToken ct) =>
        _audit.LogActionAsync(
            actionType: AuditActionType.ReportGenerated,
            actionDescription: $"Report '{reportSlug}' generated ({format}, {locale}) — {filters}",
            entityType: "Report",
            entityIdentifier: reportSlug,
            cancellationToken: ct);
}
