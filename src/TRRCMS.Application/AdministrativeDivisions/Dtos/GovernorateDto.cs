namespace TRRCMS.Application.AdministrativeDivisions.Dtos;

/// <summary>
/// DTO for Governorate (محافظة) - Top-level administrative division
/// </summary>
public class GovernorateDto
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Governorate code (2 digits)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Arabic name
    /// </summary>
    public string NameArabic { get; set; } = string.Empty;

    /// <summary>
    /// English name
    /// </summary>
    public string NameEnglish { get; set; } = string.Empty;

    /// <summary>
    /// Whether this governorate is active
    /// </summary>
    public bool IsActive { get; set; }
}
