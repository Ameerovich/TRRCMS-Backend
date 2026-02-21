namespace TRRCMS.Application.AdministrativeDivisions.Dtos;

/// <summary>
/// DTO for Community (قرية/مجتمع) - Fourth-level administrative division
/// </summary>
public class CommunityDto
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Community code (3 digits)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Parent governorate code (2 digits)
    /// </summary>
    public string GovernorateCode { get; set; } = string.Empty;

    /// <summary>
    /// Parent district code (2 digits)
    /// </summary>
    public string DistrictCode { get; set; } = string.Empty;

    /// <summary>
    /// Parent sub-district code (2 digits)
    /// </summary>
    public string SubDistrictCode { get; set; } = string.Empty;

    /// <summary>
    /// Arabic name
    /// </summary>
    public string NameArabic { get; set; } = string.Empty;

    /// <summary>
    /// English name
    /// </summary>
    public string NameEnglish { get; set; } = string.Empty;

    /// <summary>
    /// Whether this community is active
    /// </summary>
    public bool IsActive { get; set; }
}
