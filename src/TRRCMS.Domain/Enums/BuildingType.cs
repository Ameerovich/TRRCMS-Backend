namespace TRRCMS.Domain.Enums;

/// <summary>
/// Building type classification (نوع البناء)
/// </summary>
public enum BuildingType
{
    /// <summary>
    /// Residential building (سكني)
    /// </summary>
    Residential = 1,

    /// <summary>
    /// Commercial building (تجاري)
    /// </summary>
    Commercial = 2,

    /// <summary>
    /// Mixed-use building - residential and commercial (مختلط)
    /// </summary>
    MixedUse = 3,

    /// <summary>
    /// Industrial building (صناعي)
    /// </summary>
    Industrial = 4
}