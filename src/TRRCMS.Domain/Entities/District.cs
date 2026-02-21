using TRRCMS.Domain.Common;

namespace TRRCMS.Domain.Entities;

/// <summary>
/// District (منطقة/مدينة) - Level 2 of administrative hierarchy
/// Second-level administrative division under a Governorate
/// </summary>
public class District : BaseAuditableEntity
{
    /// <summary>
    /// District code (2 digits)
    /// </summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>
    /// Parent governorate code (2 digits)
    /// </summary>
    public string GovernorateCode { get; private set; } = string.Empty;

    /// <summary>
    /// Arabic name (e.g., "جبل سمعان")
    /// </summary>
    public string NameArabic { get; private set; } = string.Empty;

    /// <summary>
    /// English name (e.g., "Mount Simeon")
    /// </summary>
    public string NameEnglish { get; private set; } = string.Empty;

    /// <summary>
    /// Whether this district is active in the system
    /// </summary>
    public bool IsActive { get; private set; }

    // Navigation properties
    public Governorate Governorate { get; private set; } = null!;
    public ICollection<SubDistrict> SubDistricts { get; private set; } = new List<SubDistrict>();

    // Private constructor for EF Core
    private District() { }

    /// <summary>
    /// Factory method to create a new District
    /// </summary>
    public static District Create(
        string code,
        string governorateCode,
        string nameArabic,
        string nameEnglish,
        Guid createdByUserId)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length != 2)
            throw new ArgumentException("District code must be exactly 2 digits", nameof(code));

        if (string.IsNullOrWhiteSpace(governorateCode) || governorateCode.Length != 2)
            throw new ArgumentException("Governorate code must be exactly 2 digits", nameof(governorateCode));

        if (string.IsNullOrWhiteSpace(nameArabic))
            throw new ArgumentException("Arabic name is required", nameof(nameArabic));

        if (string.IsNullOrWhiteSpace(nameEnglish))
            throw new ArgumentException("English name is required", nameof(nameEnglish));

        var district = new District
        {
            Id = Guid.NewGuid(),
            Code = code,
            GovernorateCode = governorateCode,
            NameArabic = nameArabic,
            NameEnglish = nameEnglish,
            IsActive = true
        };
        district.MarkAsCreated(createdByUserId);
        return district;
    }

    /// <summary>
    /// Deactivate this district
    /// </summary>
    public void Deactivate(Guid modifiedByUserId)
    {
        IsActive = false;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Activate this district
    /// </summary>
    public void Activate(Guid modifiedByUserId)
    {
        IsActive = true;
        MarkAsModified(modifiedByUserId);
    }
}
