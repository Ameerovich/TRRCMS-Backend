namespace TRRCMS.Domain.Enums;

/// <summary>
/// Person-Property relation types
/// </summary>
public enum RelationType
{
    /// <summary>
    /// Property owner (مالك)
    /// </summary>
    [ArabicLabel("مالك")]
    Owner = 1,

    /// <summary>
    /// Current occupant (ساكن)
    /// </summary>
    [ArabicLabel("ساكن")]
    Occupant = 2,

    /// <summary>
    /// Tenant/Renter (مستأجر)
    /// </summary>
    [ArabicLabel("مستأجر")]
    Tenant = 3,

    /// <summary>
    /// Guest/Temporary resident (ضيف)
    /// </summary>
    [ArabicLabel("ضيف")]
    Guest = 4,

    /// <summary>
    /// Heir/Inheritor (وريث)
    /// </summary>
    [ArabicLabel("وريث")]
    Heir = 5,

    /// <summary>
    /// Other relation type
    /// </summary>
    [ArabicLabel("أخرى")]
    Other = 99
}