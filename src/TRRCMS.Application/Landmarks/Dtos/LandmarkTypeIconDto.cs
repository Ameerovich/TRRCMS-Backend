namespace TRRCMS.Application.Landmarks.Dtos;

public record LandmarkTypeIconDto
{
    public int Type { get; init; }
    public string TypeName { get; init; } = string.Empty;
    public string DisplayNameArabic { get; init; } = string.Empty;
    public string DisplayNameEnglish { get; init; } = string.Empty;
    public string SvgContent { get; init; } = string.Empty;
    public DateTime LastModifiedAtUtc { get; init; }
}
