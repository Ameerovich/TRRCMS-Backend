namespace TRRCMS.Domain.Enums;

/// <summary>
/// Relationship to head of household classification
/// Used for household composition and family structure
/// Referenced in Person entity and household profiling
/// </summary>
public enum RelationshipToHead
{
    /// <summary>
    /// Head of household - The primary household member (رب الأسرة)
    /// </summary>
    [ArabicLabel("رب الأسرة")]
    Head = 1,

    /// <summary>
    /// Spouse/Wife/Husband (زوج/زوجة)
    /// </summary>
    [ArabicLabel("زوج/زوجة")]
    Spouse = 2,

    /// <summary>
    /// Son (ابن)
    /// </summary>
    [ArabicLabel("ابن")]
    Son = 3,

    /// <summary>
    /// Daughter (ابنة)
    /// </summary>
    [ArabicLabel("ابنة")]
    Daughter = 4,

    /// <summary>
    /// Father (أب)
    /// </summary>
    [ArabicLabel("أب")]
    Father = 5,

    /// <summary>
    /// Mother (أم)
    /// </summary>
    [ArabicLabel("أم")]
    Mother = 6,

    /// <summary>
    /// Brother (أخ)
    /// </summary>
    [ArabicLabel("أخ")]
    Brother = 7,

    /// <summary>
    /// Sister (أخت)
    /// </summary>
    [ArabicLabel("أخت")]
    Sister = 8,

    /// <summary>
    /// Grandfather (جد)
    /// </summary>
    [ArabicLabel("جد")]
    Grandfather = 9,

    /// <summary>
    /// Grandmother (جدة)
    /// </summary>
    [ArabicLabel("جدة")]
    Grandmother = 10,

    /// <summary>
    /// Grandson (حفيد)
    /// </summary>
    [ArabicLabel("حفيد")]
    Grandson = 11,

    /// <summary>
    /// Granddaughter (حفيدة)
    /// </summary>
    [ArabicLabel("حفيدة")]
    Granddaughter = 12,

    /// <summary>
    /// Uncle (عم/خال)
    /// </summary>
    [ArabicLabel("عم/خال")]
    Uncle = 13,

    /// <summary>
    /// Aunt (عمة/خالة)
    /// </summary>
    [ArabicLabel("عمة/خالة")]
    Aunt = 14,

    /// <summary>
    /// Nephew (ابن الأخ/الأخت)
    /// </summary>
    [ArabicLabel("ابن الأخ/الأخت")]
    Nephew = 15,

    /// <summary>
    /// Niece (ابنة الأخ/الأخت)
    /// </summary>
    [ArabicLabel("ابنة الأخ/الأخت")]
    Niece = 16,

    /// <summary>
    /// Cousin (ابن/ابنة العم/الخال)
    /// </summary>
    [ArabicLabel("ابن/ابنة العم/الخال")]
    Cousin = 17,

    /// <summary>
    /// Son-in-law (صهر)
    /// </summary>
    [ArabicLabel("صهر")]
    SonInLaw = 18,

    /// <summary>
    /// Daughter-in-law (كنة)
    /// </summary>
    [ArabicLabel("كنة")]
    DaughterInLaw = 19,

    /// <summary>
    /// Father-in-law (حما)
    /// </summary>
    [ArabicLabel("حما")]
    FatherInLaw = 20,

    /// <summary>
    /// Mother-in-law (حماة)
    /// </summary>
    [ArabicLabel("حماة")]
    MotherInLaw = 21,

    /// <summary>
    /// Brother-in-law (صهر - أخ الزوج/الزوجة)
    /// </summary>
    [ArabicLabel("صهر - أخ الزوج/الزوجة")]
    BrotherInLaw = 22,

    /// <summary>
    /// Sister-in-law (سلفة - أخت الزوج/الزوجة)
    /// </summary>
    [ArabicLabel("سلفة - أخت الزوج/الزوجة")]
    SisterInLaw = 23,

    /// <summary>
    /// Stepfather (زوج الأم)
    /// </summary>
    [ArabicLabel("زوج الأم")]
    Stepfather = 24,

    /// <summary>
    /// Stepmother (زوجة الأب)
    /// </summary>
    [ArabicLabel("زوجة الأب")]
    Stepmother = 25,

    /// <summary>
    /// Stepson (ابن الزوج/الزوجة)
    /// </summary>
    [ArabicLabel("ابن الزوج/الزوجة")]
    Stepson = 26,

    /// <summary>
    /// Stepdaughter (ابنة الزوج/الزوجة)
    /// </summary>
    [ArabicLabel("ابنة الزوج/الزوجة")]
    Stepdaughter = 27,

    /// <summary>
    /// Adopted child (طفل متبنى)
    /// </summary>
    [ArabicLabel("طفل متبنى")]
    AdoptedChild = 28,

    /// <summary>
    /// Foster child (طفل كفيل)
    /// </summary>
    [ArabicLabel("طفل كفيل")]
    FosterChild = 29,

    /// <summary>
    /// Non-relative/Unrelated (غير قريب)
    /// </summary>
    [ArabicLabel("غير قريب")]
    NonRelative = 97,

    /// <summary>
    /// Domestic worker/Helper (عامل منزلي)
    /// </summary>
    [ArabicLabel("عامل منزلي")]
    DomesticWorker = 98,

    /// <summary>
    /// Other relationship
    /// </summary>
    [ArabicLabel("أخرى")]
    Other = 99
}