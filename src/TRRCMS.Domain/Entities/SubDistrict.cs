using TRRCMS.Domain.Common;

namespace TRRCMS.Domain.Entities;

/// <summary>
/// SubDistrict (ناحية/بلدة) - Level 3 of administrative hierarchy
/// Third-level administrative division under a District
/// </summary>
public class SubDistrict : BaseAuditableEntity
{
    /// <summary>
    /// Sub-district code (2 digits)
    /// </summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>
    /// Parent governorate code (2 digits)
    /// </summary>
    public string GovernorateCode { get; private set; } = string.Empty;

    /// <summary>
    /// Parent district code (2 digits)
    /// </summary>
    public string DistrictCode { get; private set; } = string.Empty;

    /// <summary>
    /// Arabic name (e.g., "مدينة حلب")
    /// </summary>
    public string NameArabic { get; private set; } = string.Empty;

    /// <summary>
    /// English name (e.g., "Aleppo City")
    /// </summary>
    public string NameEnglish { get; private set; } = string.Empty;

    /// <summary>
    /// Whether this sub-district is active in the system
    /// </summary>
    public bool IsActive { get; private set; }

    // Navigation properties
    public District District { get; private set; } = null!;
    public ICollection<Community> Communities { get; private set; } = new List<Community>();

    // Private constructor for EF Core
    private SubDistrict() { }

    /// <summary>
    /// Factory method to create a new SubDistrict
    /// </summary>
    public static SubDistrict Create(
        string code,
        string governorateCode,
        string districtCode,
        string nameArabic,
        string nameEnglish,
        Guid createdByUserId)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length != 2)
            throw new ArgumentException("Sub-district code must be exactly 2 digits", nameof(code));

        if (string.IsNullOrWhiteSpace(governorateCode) || governorateCode.Length != 2)
            throw new ArgumentException("Governorate code must be exactly 2 digits", nameof(governorateCode));

        if (string.IsNullOrWhiteSpace(districtCode) || districtCode.Length != 2)
            throw new ArgumentException("District code must be exactly 2 digits", nameof(districtCode));

        if (string.IsNullOrWhiteSpace(nameArabic))
            throw new ArgumentException("Arabic name is required", nameof(nameArabic));

        if (string.IsNullOrWhiteSpace(nameEnglish))
            throw new ArgumentException("English name is required", nameof(nameEnglish));

        var subDistrict = new SubDistrict
        {
            Id = Guid.NewGuid(),
            Code = code,
            GovernorateCode = governorateCode,
            DistrictCode = districtCode,
            NameArabic = nameArabic,
            NameEnglish = nameEnglish,
            IsActive = true
        };
        subDistrict.MarkAsCreated(createdByUserId);
        return subDistrict;
    }

    /// <summary>
    /// Deactivate this sub-district
    /// </summary>
    public void Deactivate(Guid modifiedByUserId)
    {
        IsActive = false;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Activate this sub-district
    /// </summary>
    public void Activate(Guid modifiedByUserId)
    {
        IsActive = true;
        MarkAsModified(modifiedByUserId);
    }
}
