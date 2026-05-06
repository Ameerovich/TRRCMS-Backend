namespace TRRCMS.Domain.Enums;

/// <summary>
/// Property unit type classification
/// </summary>
public enum PropertyUnitType
{
    /// <summary>
    /// Residential apartment (شقة سكنية)
    /// </summary>
    [ArabicLabel("شقة سكنية")]
    Apartment = 1,

    /// <summary>
    /// Commercial shop (محل تجاري)
    /// </summary>
    [ArabicLabel("محل تجاري")]
    Shop = 2,

    /// <summary>
    /// Office space (مكتب)
    /// </summary>
    [ArabicLabel("مكتب")]
    Office = 3,

    /// <summary>
    /// Warehouse (مستودع)
    /// </summary>
    [ArabicLabel("مستودع")]
    Warehouse = 4,

    /// <summary>
    /// Other type
    /// </summary>
    [ArabicLabel("أخرى")]
    Other = 5,

    /// <summary>
    /// Unknown - used when the unit type could not be determined
    /// (e.g., field collector was obstructed and could not inspect the unit).
    /// </summary>
    [ArabicLabel("غير معروف")]
    Unknown = 99,
}