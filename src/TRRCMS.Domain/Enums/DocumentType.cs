namespace TRRCMS.Domain.Enums;

/// <summary>
/// Document type classification for identification documents (نوع الوثيقة)
/// Vocabulary-driven — seeded from this enum, admin can extend via vocabulary management.
/// </summary>
public enum DocumentType
{
    /// <summary>
    /// Personal ID photo (صورة الهوية الشخصية)
    /// </summary>
    [ArabicLabel("صورة الهوية الشخصية")]
    PersonalIdPhoto = 1,

    /// <summary>
    /// Family record extract (إخراج قيد)
    /// </summary>
    [ArabicLabel("إخراج قيد")]
    FamilyRecord = 2,

    /// <summary>
    /// Photo (صورة)
    /// </summary>
    [ArabicLabel("صورة")]
    Photo = 3
}
