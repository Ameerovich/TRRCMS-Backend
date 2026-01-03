namespace TRRCMS.Domain.Enums;

/// <summary>
/// Nationality classification (الجنسية)
/// Common nationalities for the Syrian context
/// Referenced in FSD section 6.1.3
/// </summary>
public enum Nationality
{
    /// <summary>
    /// Syrian national (سوري)
    /// </summary>
    Syrian = 1,

    /// <summary>
    /// Palestinian (فلسطيني)
    /// </summary>
    Palestinian = 2,

    /// <summary>
    /// Iraqi (عراقي)
    /// </summary>
    Iraqi = 3,

    /// <summary>
    /// Lebanese (لبناني)
    /// </summary>
    Lebanese = 4,

    /// <summary>
    /// Jordanian (أردني)
    /// </summary>
    Jordanian = 5,

    /// <summary>
    /// Egyptian (مصري)
    /// </summary>
    Egyptian = 6,

    /// <summary>
    /// Turkish (تركي)
    /// </summary>
    Turkish = 7,

    /// <summary>
    /// Saudi (سعودي)
    /// </summary>
    Saudi = 8,

    /// <summary>
    /// Yemeni (يمني)
    /// </summary>
    Yemeni = 9,

    /// <summary>
    /// Sudanese (سوداني)
    /// </summary>
    Sudanese = 10,

    /// <summary>
    /// Iranian (إيراني)
    /// </summary>
    Iranian = 11,

    /// <summary>
    /// Kurdish (كردي)
    /// </summary>
    Kurdish = 12,

    /// <summary>
    /// Stateless (عديم الجنسية)
    /// </summary>
    Stateless = 97,

    /// <summary>
    /// Refugee - no specific nationality documented (لاجئ)
    /// </summary>
    Refugee = 98,

    /// <summary>
    /// Other nationality not listed
    /// </summary>
    Other = 99
}