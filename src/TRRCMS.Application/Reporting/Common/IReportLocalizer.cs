namespace TRRCMS.Application.Reporting.Common;

/// <summary>
/// Resolves report label keys to localized strings (AR / EN).
/// Backed by a static in-memory dictionary — report labels are dev-authored
/// and shipped with the code, not user-managed vocabulary.
/// </summary>
public interface IReportLocalizer
{
    string T(string key, ReportLocale locale);
}
