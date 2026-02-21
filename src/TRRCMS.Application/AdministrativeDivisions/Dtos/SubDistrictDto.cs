namespace TRRCMS.Application.AdministrativeDivisions.Dtos;

/// <summary>
/// DTO for SubDistrict (ناحية) - Third-level administrative division
/// </summary>
public class SubDistrictDto
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Sub-district code (2 digits)
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
    /// Arabic name
    /// </summary>
    public string NameArabic { get; set; } = string.Empty;

    /// <summary>
    /// English name
    /// </summary>
    public string NameEnglish { get; set; } = string.Empty;

    /// <summary>
    /// Whether this sub-district is active
    /// </summary>
    public bool IsActive { get; set; }
}
