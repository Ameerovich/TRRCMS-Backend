using TRRCMS.Domain.Common;

namespace TRRCMS.Domain.Entities;

/// <summary>
/// Community (قرية/مجتمع) - Level 4 of administrative hierarchy
/// Fourth-level administrative division under a SubDistrict
/// </summary>
public class Community : BaseAuditableEntity
{
    /// <summary>
    /// Community code (3 digits)
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
    /// Parent sub-district code (2 digits)
    /// </summary>
    public string SubDistrictCode { get; private set; } = string.Empty;

    /// <summary>
    /// Arabic name (e.g., "القطاع الغربي")
    /// </summary>
    public string NameArabic { get; private set; } = string.Empty;

    /// <summary>
    /// English name (e.g., "Western Sector")
    /// </summary>
    public string NameEnglish { get; private set; } = string.Empty;

    /// <summary>
    /// Whether this community is active in the system
    /// </summary>
    public bool IsActive { get; private set; }

    // Navigation property
    public SubDistrict SubDistrict { get; private set; } = null!;

    // Private constructor for EF Core
    private Community() { }

    /// <summary>
    /// Factory method to create a new Community
    /// </summary>
    public static Community Create(
        string code,
        string governorateCode,
        string districtCode,
        string subDistrictCode,
        string nameArabic,
        string nameEnglish,
        Guid createdByUserId)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length != 3)
            throw new ArgumentException("Community code must be exactly 3 digits", nameof(code));

        if (string.IsNullOrWhiteSpace(governorateCode) || governorateCode.Length != 2)
            throw new ArgumentException("Governorate code must be exactly 2 digits", nameof(governorateCode));

        if (string.IsNullOrWhiteSpace(districtCode) || districtCode.Length != 2)
            throw new ArgumentException("District code must be exactly 2 digits", nameof(districtCode));

        if (string.IsNullOrWhiteSpace(subDistrictCode) || subDistrictCode.Length != 2)
            throw new ArgumentException("Sub-district code must be exactly 2 digits", nameof(subDistrictCode));

        if (string.IsNullOrWhiteSpace(nameArabic))
            throw new ArgumentException("Arabic name is required", nameof(nameArabic));

        if (string.IsNullOrWhiteSpace(nameEnglish))
            throw new ArgumentException("English name is required", nameof(nameEnglish));

        var community = new Community
        {
            Id = Guid.NewGuid(),
            Code = code,
            GovernorateCode = governorateCode,
            DistrictCode = districtCode,
            SubDistrictCode = subDistrictCode,
            NameArabic = nameArabic,
            NameEnglish = nameEnglish,
            IsActive = true
        };
        community.MarkAsCreated(createdByUserId);
        return community;
    }

    /// <summary>
    /// Deactivate this community
    /// </summary>
    public void Deactivate(Guid modifiedByUserId)
    {
        IsActive = false;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Activate this community
    /// </summary>
    public void Activate(Guid modifiedByUserId)
    {
        IsActive = true;
        MarkAsModified(modifiedByUserId);
    }
}
