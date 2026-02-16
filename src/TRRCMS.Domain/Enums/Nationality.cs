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
    [ArabicLabel("سوري")]
    Syrian = 1,

    /// <summary>
    /// Palestinian (فلسطيني)
    /// </summary>
    [ArabicLabel("فلسطيني")]
    Palestinian = 2,

    /// <summary>
    /// Iraqi (عراقي)
    /// </summary>
    [ArabicLabel("عراقي")]
    Iraqi = 3,

    /// <summary>
    /// Lebanese (لبناني)
    /// </summary>
    [ArabicLabel("لبناني")]
    Lebanese = 4,

    /// <summary>
    /// Jordanian (أردني)
    /// </summary>
    [ArabicLabel("أردني")]
    Jordanian = 5,

    /// <summary>
    /// Egyptian (مصري)
    /// </summary>
    [ArabicLabel("مصري")]
    Egyptian = 6,

    /// <summary>
    /// Turkish (تركي)
    /// </summary>
    [ArabicLabel("تركي")]
    Turkish = 7,

    /// <summary>
    /// Saudi (سعودي)
    /// </summary>
    [ArabicLabel("سعودي")]
    Saudi = 8,

    /// <summary>
    /// Yemeni (يمني)
    /// </summary>
    [ArabicLabel("يمني")]
    Yemeni = 9,

    /// <summary>
    /// Sudanese (سوداني)
    /// </summary>
    [ArabicLabel("سوداني")]
    Sudanese = 10,

    /// <summary>
    /// Iranian (إيراني)
    /// </summary>
    [ArabicLabel("إيراني")]
    Iranian = 11,

    /// <summary>
    /// Stateless (عديم الجنسية)
    /// </summary>
    [ArabicLabel("عديم الجنسية")]
    Stateless = 97,

    /// <summary>
    /// Refugee - no specific nationality documented (لاجئ)
    /// </summary>
    [ArabicLabel("لاجئ")]
    Refugee = 98,

    /// <summary>
    /// Other nationality not listed
    /// </summary>
    [ArabicLabel("أخرى")]
    Other = 99
}