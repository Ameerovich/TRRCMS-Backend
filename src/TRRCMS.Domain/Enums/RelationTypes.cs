namespace TRRCMS.Domain.Enums;

/// <summary>
/// Person-Property relation types
/// </summary>
public enum RelationType
{
    /// <summary>
    /// Property owner (مالك)
    /// </summary>
    Owner = 1,

    /// <summary>
    /// Current occupant (ساكن)
    /// </summary>
    Occupant = 2,

    /// <summary>
    /// Tenant/Renter (مستأجر)
    /// </summary>
    Tenant = 3,

    /// <summary>
    /// Guest/Temporary resident (ضيف)
    /// </summary>
    Guest = 4,

    /// <summary>
    /// Heir/Inheritor (وريث)
    /// </summary>
    Heir = 5,

    /// <summary>
    /// Other relation type
    /// </summary>
    Other = 99
}