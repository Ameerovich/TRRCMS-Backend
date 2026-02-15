namespace TRRCMS.Domain.Enums;

/// <summary>
/// Building type classification (نوع البناء)
/// </summary>
public enum BuildingType
{
    /// <summary>
    /// Residential building (سكني)
    /// </summary>
    [ArabicLabel("سكني")]
    Residential = 1,

    /// <summary>
    /// Commercial building (تجاري)
    /// </summary>
    [ArabicLabel("تجاري")]
    Commercial = 2,

    /// <summary>
    /// Mixed-use building - residential and commercial (مختلط)
    /// </summary>
    [ArabicLabel("مختلط")]
    MixedUse = 3,

    /// <summary>
    /// Industrial building (صناعي)
    /// </summary>
    [ArabicLabel("صناعي")]
    Industrial = 4
}