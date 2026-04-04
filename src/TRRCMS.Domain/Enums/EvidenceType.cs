namespace TRRCMS.Domain.Enums;

/// <summary>
/// Evidence type classification for tenure documents only.
/// Identification documents are now handled by the separate IdentificationDocument entity
/// with its own DocumentType enum.
/// </summary>
public enum EvidenceType
{
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
