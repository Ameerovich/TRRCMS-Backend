using TRRCMS.Domain.Common;

namespace TRRCMS.Domain.Entities;

/// <summary>
/// Governorate (محافظة) - Level 1 of administrative hierarchy
/// Top-level administrative division (e.g., Aleppo Governorate)
/// </summary>
public class Governorate : BaseAuditableEntity
{
    /// <summary>
    /// Governorate code (2 digits, e.g., "01" for Aleppo)
    /// </summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>
    /// Arabic name (e.g., "حلب")
    /// </summary>
    public string NameArabic { get; private set; } = string.Empty;

    /// <summary>
    /// English name (e.g., "Aleppo")
    /// </summary>
    public string NameEnglish { get; private set; } = string.Empty;

    /// <summary>
    /// Whether this governorate is active in the system
    /// </summary>
    public bool IsActive { get; private set; }

    // Navigation property
    public ICollection<District> Districts { get; private set; } = new List<District>();

    // Private constructor for EF Core
    private Governorate() { }

    /// <summary>
    /// Factory method to create a new Governorate
    /// </summary>
    public static Governorate Create(
        string code,
        string nameArabic,
        string nameEnglish,
        Guid createdByUserId)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length != 2)
            throw new ArgumentException("Governorate code must be exactly 2 digits", nameof(code));

        if (string.IsNullOrWhiteSpace(nameArabic))
            throw new ArgumentException("Arabic name is required", nameof(nameArabic));

        if (string.IsNullOrWhiteSpace(nameEnglish))
            throw new ArgumentException("English name is required", nameof(nameEnglish));

        var governorate = new Governorate
        {
            Id = Guid.NewGuid(),
            Code = code,
            NameArabic = nameArabic,
            NameEnglish = nameEnglish,
            IsActive = true
        };
        governorate.MarkAsCreated(createdByUserId);
        return governorate;
    }

    /// <summary>
    /// Deactivate this governorate
    /// </summary>
    public void Deactivate(Guid modifiedByUserId)
    {
        IsActive = false;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Activate this governorate
    /// </summary>
    public void Activate(Guid modifiedByUserId)
    {
        IsActive = true;
        MarkAsModified(modifiedByUserId);
    }
}
