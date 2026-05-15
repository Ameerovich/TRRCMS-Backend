namespace TRRCMS.Infrastructure.Reporting;

/// <summary>
/// Bound from the "Reports" section in appsettings.json.
/// </summary>
public sealed class ReportFontSettings
{
    public const string SectionName = "Reports";

    /// <summary>
    /// Filesystem path to an Arabic-shaping OTF/TTF font (e.g. Noto Naskh Arabic).
    /// Loaded once at startup and registered with QuestPDF's FontManager.
    /// When null/empty or missing on disk, AR PDFs fall back to QuestPDF's
    /// default font — Arabic glyphs may not shape correctly.
    /// </summary>
    public string? ArabicFontPath { get; set; }

    /// <summary>
    /// Font family name used by the renderer when locale = Ar.
    /// Should match the family name embedded in the font file.
    /// </summary>
    public string ArabicFontFamily { get; set; } = "Noto Naskh Arabic";

    /// <summary>
    /// Font family used for English/Latin text. Defaults to QuestPDF's bundled default.
    /// </summary>
    public string LatinFontFamily { get; set; } = "Lato";
}
