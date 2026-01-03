namespace TRRCMS.Domain.Enums;

/// <summary>
/// Case priority classification for claim prioritization
/// </summary>
public enum CasePriority
{
    /// <summary>
    /// Low priority - Standard processing (أولوية منخفضة)
    /// </summary>
    Low = 1,

    /// <summary>
    /// Normal priority - Regular processing timeline (أولوية عادية)
    /// </summary>
    Normal = 2,

    /// <summary>
    /// Medium priority - Expedited processing (أولوية متوسطة)
    /// </summary>
    Medium = 3,

    /// <summary>
    /// High priority - Requires urgent attention (أولوية عالية)
    /// Examples: Vulnerable persons, imminent eviction
    /// </summary>
    High = 4,

    /// <summary>
    /// Critical/Emergency - Immediate action required (طوارئ)
    /// Examples: Safety concerns, legal deadlines
    /// </summary>
    Critical = 5,

    /// <summary>
    /// VIP - Special handling required (كبار الشخصيات)
    /// </summary>
    VIP = 6,

    /// <summary>
    /// Escalated - Priority raised by supervisor (مصعد)
    /// </summary>
    Escalated = 7
}