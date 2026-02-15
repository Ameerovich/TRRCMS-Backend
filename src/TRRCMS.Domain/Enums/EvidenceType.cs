namespace TRRCMS.Domain.Enums;

/// <summary>
/// Evidence/Document type classification
/// </summary>
public enum EvidenceType
{
    /// <summary>
    /// Personal identification document (National ID, Passport)
    /// (بطاقة هوية شخصية)
    /// </summary>
    [ArabicLabel("بطاقة هوية شخصية")]
    IdentificationDocument = 1,

    /// <summary>
    /// Property ownership deed (سند ملكية)
    /// </summary>
    [ArabicLabel("سند ملكية")]
    OwnershipDeed = 2,

    /// <summary>
    /// Rental/Lease contract (عقد إيجار)
    /// </summary>
    [ArabicLabel("عقد إيجار")]
    RentalContract = 3,

    /// <summary>
    /// Utility bill (electricity, water)
    /// (فاتورة مرافق)
    /// </summary>
    [ArabicLabel("فاتورة مرافق")]
    UtilityBill = 4,

    /// <summary>
    /// Building or property photo (صورة)
    /// </summary>
    [ArabicLabel("صورة")]
    Photo = 5,

    /// <summary>
    /// Official letter or government document
    /// (رسالة رسمية)
    /// </summary>
    [ArabicLabel("رسالة رسمية")]
    OfficialLetter = 6,

    /// <summary>
    /// Court order or legal document
    /// (أمر محكمة)
    /// </summary>
    [ArabicLabel("أمر محكمة")]
    CourtOrder = 7,

    /// <summary>
    /// Inheritance document (وثيقة ميراث)
    /// </summary>
    [ArabicLabel("وثيقة ميراث")]
    InheritanceDocument = 8,

    /// <summary>
    /// Tax receipt or property tax document
    /// (إيصال ضريبة)
    /// </summary>
    [ArabicLabel("إيصال ضريبة")]
    TaxReceipt = 9,

    /// <summary>
    /// Other document type
    /// </summary>
    [ArabicLabel("أخرى")]
    Other = 99
}