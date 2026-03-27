using TRRCMS.Domain.Common;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Domain.Entities;

/// <summary>
/// Stores an SVG icon for a landmark type.
/// One icon per type — admin can update the SVG over time.
/// </summary>
public class LandmarkTypeIcon : BaseAuditableEntity
{
    public LandmarkType Type { get; private set; }
    public string SvgContent { get; private set; } = string.Empty;
    public string DisplayNameArabic { get; private set; } = string.Empty;
    public string DisplayNameEnglish { get; private set; } = string.Empty;

    private LandmarkTypeIcon() { }

    public static LandmarkTypeIcon Create(
        LandmarkType type,
        string svgContent,
        string displayNameArabic,
        string displayNameEnglish,
        Guid createdByUserId)
    {
        var icon = new LandmarkTypeIcon
        {
            Type = type,
            SvgContent = svgContent,
            DisplayNameArabic = displayNameArabic,
            DisplayNameEnglish = displayNameEnglish
        };
        icon.MarkAsCreated(createdByUserId);
        return icon;
    }

    public void UpdateIcon(string svgContent, Guid modifiedByUserId)
    {
        SvgContent = svgContent;
        MarkAsModified(modifiedByUserId);
    }

    public void UpdateDisplayNames(string displayNameArabic, string displayNameEnglish, Guid modifiedByUserId)
    {
        DisplayNameArabic = displayNameArabic;
        DisplayNameEnglish = displayNameEnglish;
        MarkAsModified(modifiedByUserId);
    }
}
