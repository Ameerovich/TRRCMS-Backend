using TRRCMS.Application.Reporting.Common;

namespace TRRCMS.Infrastructure.Reporting;

/// <summary>
/// In-memory dictionary of dev-authored report labels in Arabic and English.
/// Keys are stable identifiers used by renderers; values are user-facing strings.
/// </summary>
public sealed class ReportLocalizer : IReportLocalizer
{
    private static readonly Dictionary<string, (string En, string Ar)> Labels = new()
    {
        // ── Shared ──
        ["common.generated_at"]      = ("Generated at", "تاريخ الإصدار"),
        ["common.date_range"]        = ("Date range", "النطاق الزمني"),
        ["common.from"]              = ("From", "من"),
        ["common.to"]                = ("To", "إلى"),
        ["common.all_time"]          = ("All time", "كل الفترات"),
        ["common.total"]             = ("Total", "الإجمالي"),
        ["common.page"]              = ("Page", "صفحة"),
        ["common.of"]                = ("of", "من"),
        ["common.no_data"]           = ("No data available for the selected filters.", "لا توجد بيانات للمعايير المحددة."),
        ["common.system"]            = ("TRRCMS — Tenure Rights Registration", "نظام تسجيل حقوق الحيازة"),

        // ── Survey Activity ──
        ["sa.title"]                 = ("Survey Activity Report", "تقرير نشاط المسوحات"),
        ["sa.field_collectors"]      = ("Field Collectors", "جامعو البيانات الميدانيون"),
        ["sa.office_clerks"]         = ("Office Clerks", "موظفو المكتب"),
        ["sa.col.username"]          = ("Username", "اسم المستخدم"),
        ["sa.col.full_name"]         = ("Full Name", "الاسم الكامل"),
        ["sa.col.completed"]         = ("Completed", "مكتملة"),
        ["sa.col.draft"]             = ("Draft", "مسودة"),
        ["sa.col.total"]             = ("Total Surveys", "إجمالي المسوحات"),
        ["sa.col.assigned_b"]        = ("Assigned Buildings", "المباني المعينة"),
        ["sa.col.completed_b"]       = ("Completed Buildings", "المباني المكتملة"),
        ["sa.summary.total_done"]    = ("Total surveys completed", "إجمالي المسوحات المكتملة"),
        ["sa.summary.total_draft"]   = ("Total surveys draft", "إجمالي المسوحات المسودة"),

        // ── Building Inventory ──
        ["bi.title"]                 = ("Building & Property Inventory", "جرد المباني والوحدات"),
        ["bi.filter.neighborhood"]   = ("Neighborhood filter", "تصفية حسب الحي"),
        ["bi.col.code"]              = ("Code", "الرمز"),
        ["bi.col.name_ar"]           = ("Name (Arabic)", "الاسم بالعربية"),
        ["bi.col.name_en"]           = ("Name (English)", "الاسم بالإنجليزية"),
        ["bi.col.buildings"]         = ("Buildings", "المباني"),
        ["bi.col.units"]             = ("Property Units", "الوحدات العقارية"),
        ["bi.summary.neighborhoods"] = ("Neighborhoods", "الأحياء"),
        ["bi.summary.with_b"]        = ("Neighborhoods with buildings", "الأحياء التي تحتوي مباني"),
        ["bi.summary.b_total"]       = ("Total buildings", "إجمالي المباني"),
        ["bi.summary.u_total"]       = ("Total property units", "إجمالي الوحدات"),

        // ── Import Pipeline ──
        ["ip.title"]                 = ("Import Pipeline Report", "تقرير خط الاستيراد"),
        ["ip.col.package"]           = ("Package #", "رقم الحزمة"),
        ["ip.col.file"]              = ("File", "الملف"),
        ["ip.col.status"]            = ("Status", "الحالة"),
        ["ip.col.imported"]          = ("Imported", "تاريخ الاستيراد"),
        ["ip.col.completed"]         = ("Completed", "تاريخ الإنجاز"),
        ["ip.col.success"]           = ("Successful", "ناجحة"),
        ["ip.col.failed"]            = ("Failed", "فاشلة"),
        ["ip.col.skipped"]           = ("Skipped", "متخطاة"),
        ["ip.summary.total"]         = ("Total packages", "إجمالي الحزم"),
        ["ip.summary.completed"]     = ("Completed", "مكتملة"),
        ["ip.summary.failed"]        = ("Failed", "فاشلة"),
        ["ip.summary.pending"]       = ("In progress", "قيد المعالجة"),
        ["ip.summary.cancelled"]     = ("Cancelled", "ملغاة"),
        ["ip.summary.records"]       = ("Records imported / failed / skipped", "السجلات: مستوردة / فاشلة / متخطاة"),

        // ── Audit Export ──
        ["ae.title"]                 = ("Audit Log Export", "تصدير السجل التدقيقي"),
        ["ae.security_only"]         = ("Security events only", "أحداث أمنية فقط"),
        ["ae.entity_filter"]         = ("Entity type filter", "تصفية حسب نوع الكيان"),
        ["ae.col.number"]            = ("#", "#"),
        ["ae.col.timestamp"]         = ("Timestamp (UTC)", "الطابع الزمني"),
        ["ae.col.action"]            = ("Action", "الإجراء"),
        ["ae.col.result"]            = ("Result", "النتيجة"),
        ["ae.col.user"]              = ("User", "المستخدم"),
        ["ae.col.role"]              = ("Role", "الدور"),
        ["ae.col.entity"]            = ("Entity", "الكيان"),
        ["ae.col.entity_id"]         = ("Identifier", "المعرف"),
        ["ae.col.ip"]                = ("IP", "العنوان"),
        ["ae.col.description"]      = ("Description", "الوصف")
    };

    public string T(string key, ReportLocale locale)
    {
        if (!Labels.TryGetValue(key, out var pair))
            return key;
        return locale == ReportLocale.Ar ? pair.Ar : pair.En;
    }
}
