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
    Head = 1,

    /// <summary>
    /// Spouse/Wife/Husband (زوج/زوجة)
    /// </summary>
    Spouse = 2,

    /// <summary>
    /// Son (ابن)
    /// </summary>
    Son = 3,

    /// <summary>
    /// Daughter (ابنة)
    /// </summary>
    Daughter = 4,

    /// <summary>
    /// Father (أب)
    /// </summary>
    Father = 5,

    /// <summary>
    /// Mother (أم)
    /// </summary>
    Mother = 6,

    /// <summary>
    /// Brother (أخ)
    /// </summary>
    Brother = 7,

    /// <summary>
    /// Sister (أخت)
    /// </summary>
    Sister = 8,

    /// <summary>
    /// Grandfather (جد)
    /// </summary>
    Grandfather = 9,

    /// <summary>
    /// Grandmother (جدة)
    /// </summary>
    Grandmother = 10,

    /// <summary>
    /// Grandson (حفيد)
    /// </summary>
    Grandson = 11,

    /// <summary>
    /// Granddaughter (حفيدة)
    /// </summary>
    Granddaughter = 12,

    /// <summary>
    /// Uncle (عم/خال)
    /// </summary>
    Uncle = 13,

    /// <summary>
    /// Aunt (عمة/خالة)
    /// </summary>
    Aunt = 14,

    /// <summary>
    /// Nephew (ابن الأخ/الأخت)
    /// </summary>
    Nephew = 15,

    /// <summary>
    /// Niece (ابنة الأخ/الأخت)
    /// </summary>
    Niece = 16,

    /// <summary>
    /// Cousin (ابن/ابنة العم/الخال)
    /// </summary>
    Cousin = 17,

    /// <summary>
    /// Son-in-law (صهر)
    /// </summary>
    SonInLaw = 18,

    /// <summary>
    /// Daughter-in-law (كنة)
    /// </summary>
    DaughterInLaw = 19,

    /// <summary>
    /// Father-in-law (حما)
    /// </summary>
    FatherInLaw = 20,

    /// <summary>
    /// Mother-in-law (حماة)
    /// </summary>
    MotherInLaw = 21,

    /// <summary>
    /// Brother-in-law (صهر - أخ الزوج/الزوجة)
    /// </summary>
    BrotherInLaw = 22,

    /// <summary>
    /// Sister-in-law (سلفة - أخت الزوج/الزوجة)
    /// </summary>
    SisterInLaw = 23,

    /// <summary>
    /// Stepfather (زوج الأم)
    /// </summary>
    Stepfather = 24,

    /// <summary>
    /// Stepmother (زوجة الأب)
    /// </summary>
    Stepmother = 25,

    /// <summary>
    /// Stepson (ابن الزوج/الزوجة)
    /// </summary>
    Stepson = 26,

    /// <summary>
    /// Stepdaughter (ابنة الزوج/الزوجة)
    /// </summary>
    Stepdaughter = 27,

    /// <summary>
    /// Adopted child (طفل متبنى)
    /// </summary>
    AdoptedChild = 28,

    /// <summary>
    /// Foster child (طفل كفيل)
    /// </summary>
    FosterChild = 29,

    /// <summary>
    /// Non-relative/Unrelated (غير قريب)
    /// </summary>
    NonRelative = 97,

    /// <summary>
    /// Domestic worker/Helper (عامل منزلي)
    /// </summary>
    DomesticWorker = 98,

    /// <summary>
    /// Other relationship
    /// </summary>
    Other = 99
}