using TRRCMS.Application.Reporting.Dtos;

namespace TRRCMS.Application.Reporting.Common;

public interface IPdfReportRenderer
{
    byte[] RenderSurveyActivity(SurveyActivityReportDto data, ReportLocale locale);
    byte[] RenderBuildingInventory(BuildingInventoryReportDto data, ReportLocale locale);
    byte[] RenderImportPipeline(ImportPipelineReportDto data, ReportLocale locale);
    byte[] RenderAuditExport(AuditExportReportDto data, ReportLocale locale);
}
