namespace TRRCMS.Domain.Enums;

/// <summary>
/// Gender classification
/// </summary>
public enum Gender
{
    /// <summary>
    /// Male (ذكر)
    /// </summary>
    [ArabicLabel("ذكر")]
    Male = 1,

    /// <summary>
    /// Female (أنثى)
    /// </summary>
    [ArabicLabel("أنثى")]
    Female = 2
}