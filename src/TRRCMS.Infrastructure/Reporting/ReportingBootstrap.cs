using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuestPDF.Drawing;
using QuestPDF.Infrastructure;

namespace TRRCMS.Infrastructure.Reporting;

/// <summary>
/// One-shot startup tasks for the reporting subsystem:
///   1. Set QuestPDF license tier.
///   2. Register the configured Arabic font (if the file exists), so PDFs
///      generated with locale = Ar shape Arabic glyphs correctly.
///
/// Called once from WebAPI startup before any report is rendered.
/// </summary>
public static class ReportingBootstrap
{
    public static void Initialize(IServiceProvider services, ILogger logger)
    {
        // Community is the open-source tier ($0). Verify revenue tier (<$1M USD/yr)
        // with procurement before production deployment. If not eligible, switch to
        // LicenseType.Professional or .Enterprise and document the paid license key.
        QuestPDF.Settings.License = LicenseType.Community;

        var fonts = services.GetRequiredService<IOptions<ReportFontSettings>>().Value;

        if (string.IsNullOrWhiteSpace(fonts.ArabicFontPath))
        {
            logger.LogWarning(
                "Reports:ArabicFontPath is not configured. Arabic PDF reports will use the default font and may not shape glyphs correctly.");
            return;
        }

        if (!File.Exists(fonts.ArabicFontPath))
        {
            logger.LogWarning(
                "Reports:ArabicFontPath points to '{Path}', which does not exist. Arabic PDF reports will fall back to the default font.",
                fonts.ArabicFontPath);
            return;
        }

        try
        {
            using var stream = File.OpenRead(fonts.ArabicFontPath);
            FontManager.RegisterFont(stream);
            logger.LogInformation(
                "Registered Arabic report font '{Family}' from '{Path}'.",
                fonts.ArabicFontFamily, fonts.ArabicFontPath);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to register Arabic report font from '{Path}'. Arabic PDFs will fall back to the default font.",
                fonts.ArabicFontPath);
        }
    }
}
