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
    IdentificationDocument = 1,

    /// <summary>
    /// Property ownership deed (سند ملكية)
    /// </summary>
    OwnershipDeed = 2,

    /// <summary>
    /// Rental/Lease contract (عقد إيجار)
    /// </summary>
    RentalContract = 3,

    /// <summary>
    /// Utility bill (electricity, water)
    /// (فاتورة مرافق)
    /// </summary>
    UtilityBill = 4,

    /// <summary>
    /// Building or property photo (صورة)
    /// </summary>
    Photo = 5,

    /// <summary>
    /// Official letter or government document
    /// (رسالة رسمية)
    /// </summary>
    OfficialLetter = 6,

    /// <summary>
    /// Court order or legal document
    /// (أمر محكمة)
    /// </summary>
    CourtOrder = 7,

    /// <summary>
    /// Inheritance document (وثيقة ميراث)
    /// </summary>
    InheritanceDocument = 8,

    /// <summary>
    /// Tax receipt or property tax document
    /// (إيصال ضريبة)
    /// </summary>
    TaxReceipt = 9,

    /// <summary>
    /// Other document type
    /// </summary>
    Other = 99
}