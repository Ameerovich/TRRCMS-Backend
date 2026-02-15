namespace TRRCMS.Domain.Enums;

/// <summary>
/// Case priority classification for claim prioritization
/// </summary>
public enum CasePriority
{
    /// <summary>
    /// Low priority - Standard processing (أولوية منخفضة)
    /// </summary>
    [ArabicLabel("أولوية منخفضة")]
    Low = 1,

    /// <summary>
    /// Normal priority - Regular processing timeline (أولوية عادية)
    /// </summary>
    [ArabicLabel("أولوية عادية")]
    Normal = 2,

    /// <summary>
    /// Medium priority - Expedited processing (أولوية متوسطة)
    /// </summary>
    [ArabicLabel("أولوية متوسطة")]
    Medium = 3,

    /// <summary>
    /// High priority - Requires urgent attention (أولوية عالية)
    /// Examples: Vulnerable persons, imminent eviction
    /// </summary>
    [ArabicLabel("أولوية عالية")]
    High = 4,

    /// <summary>
    /// Critical/Emergency - Immediate action required (طوارئ)
    /// Examples: Safety concerns, legal deadlines
    /// </summary>
    [ArabicLabel("طوارئ")]
    Critical = 5,

    /// <summary>
    /// VIP - Special handling required (كبار الشخصيات)
    /// </summary>
    [ArabicLabel("كبار الشخصيات")]
    VIP = 6,

    /// <summary>
    /// Escalated - Priority raised by supervisor (مصعد)
    /// </summary>
    [ArabicLabel("مصعد")]
    Escalated = 7
}