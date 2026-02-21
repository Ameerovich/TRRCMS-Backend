namespace TRRCMS.Application.AdministrativeDivisions.Dtos;

/// <summary>
/// DTO for District (منطقة/قضاء) - Second-level administrative division
/// </summary>
public class DistrictDto
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// District code (2 digits)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Parent governorate code (2 digits)
    /// </summary>
    public string GovernorateCode { get; set; } = string.Empty;

    /// <summary>
    /// Arabic name
    /// </summary>
    public string NameArabic { get; set; } = string.Empty;

    /// <summary>
    /// English name
    /// </summary>
    public string NameEnglish { get; set; } = string.Empty;

    /// <summary>
    /// Whether this district is active
    /// </summary>
    public bool IsActive { get; set; }
}
