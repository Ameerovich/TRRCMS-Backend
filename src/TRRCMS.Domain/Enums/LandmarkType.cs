namespace TRRCMS.Domain.Enums;

/// <summary>
/// Landmark type classification (نوع المعلم)
/// Used for map reference layers to help applicants identify buildings.
/// </summary>
public enum LandmarkType
{
    /// <summary>
    /// Police station (مركز شرطة)
    /// </summary>
    [ArabicLabel("مركز شرطة")]
    PoliceStation = 1,

    /// <summary>
    /// Mosque (مسجد)
    /// </summary>
    [ArabicLabel("مسجد")]
    Mosque = 2,

    /// <summary>
    /// Square (ساحة)
    /// </summary>
    [ArabicLabel("ساحة")]
    Square = 3,

    /// <summary>
    /// Shop (محل تجاري)
    /// </summary>
    [ArabicLabel("محل تجاري")]
    Shop = 4,

    /// <summary>
    /// School (مدرسة)
    /// </summary>
    [ArabicLabel("مدرسة")]
    School = 5,

    /// <summary>
    /// Clinic (عيادة)
    /// </summary>
    [ArabicLabel("عيادة")]
    Clinic = 6,

    /// <summary>
    /// Water tank (خزان مياه)
    /// </summary>
    [ArabicLabel("خزان مياه")]
    WaterTank = 7,

    /// <summary>
    /// Fuel station (محطة وقود)
    /// </summary>
    [ArabicLabel("محطة وقود")]
    FuelStation = 8,

    /// <summary>
    /// Hospital (مستشفى)
    /// </summary>
    [ArabicLabel("مستشفى")]
    Hospital = 9,

    /// <summary>
    /// Park (حديقة)
    /// </summary>
    [ArabicLabel("حديقة")]
    Park = 10
}
