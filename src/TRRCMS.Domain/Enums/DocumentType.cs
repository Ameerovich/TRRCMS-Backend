namespace TRRCMS.Domain.Enums;

/// <summary>
/// Document type classification - comprehensive list of all document types
/// Referenced in FSD section 6.2.1 and throughout the system
/// </summary>
public enum DocumentType
{
    // ==================== PROPERTY OWNERSHIP DOCUMENTS ====================

    /// <summary>
    /// Green Tabu (ownership deed) - Official property deed (طابو أخضر)
    /// </summary>
    [ArabicLabel("طابو أخضر")]
    TabuGreen = 1,

    /// <summary>
    /// Red Tabu - Temporary or conditional property deed (طابو أحمر)
    /// </summary>
    [ArabicLabel("طابو أحمر")]
    TabuRed = 2,

    /// <summary>
    /// Agricultural property deed (سجل زراعي)
    /// </summary>
    [ArabicLabel("سجل زراعي")]
    AgriculturalDeed = 3,

    /// <summary>
    /// Real estate registry extract (كشف عقاري)
    /// </summary>
    [ArabicLabel("كشف عقاري")]
    RealEstateRegistryExtract = 4,

    /// <summary>
    /// Property ownership certificate (شهادة ملكية)
    /// </summary>
    [ArabicLabel("شهادة ملكية")]
    OwnershipCertificate = 5,

    // ==================== RENTAL & TENANCY DOCUMENTS ====================

    /// <summary>
    /// Rental/Lease contract (عقد إيجار)
    /// </summary>
    [ArabicLabel("عقد إيجار")]
    RentalContract = 10,

    /// <summary>
    /// Tenancy agreement (اتفاقية إيجار)
    /// </summary>
    [ArabicLabel("اتفاقية إيجار")]
    TenancyAgreement = 11,

    /// <summary>
    /// Rent receipt (إيصال إيجار)
    /// </summary>
    [ArabicLabel("إيصال إيجار")]
    RentReceipt = 12,

    // ==================== PERSONAL IDENTIFICATION ====================

    /// <summary>
    /// National ID card (بطاقة هوية وطنية)
    /// </summary>
    [ArabicLabel("بطاقة هوية وطنية")]
    NationalIdCard = 20,

    /// <summary>
    /// Passport (جواز سفر)
    /// </summary>
    [ArabicLabel("جواز سفر")]
    Passport = 21,

    /// <summary>
    /// Family registry (قيد عائلي)
    /// </summary>
    [ArabicLabel("قيد عائلي")]
    FamilyRegistry = 22,

    /// <summary>
    /// Birth certificate (شهادة ميلاد)
    /// </summary>
    [ArabicLabel("شهادة ميلاد")]
    BirthCertificate = 23,

    /// <summary>
    /// Marriage certificate (عقد زواج)
    /// </summary>
    [ArabicLabel("عقد زواج")]
    MarriageCertificate = 24,

    // ==================== UTILITY & SERVICE DOCUMENTS ====================

    /// <summary>
    /// Electricity bill (فاتورة كهرباء)
    /// </summary>
    [ArabicLabel("فاتورة كهرباء")]
    ElectricityBill = 30,

    /// <summary>
    /// Water bill (فاتورة مياه)
    /// </summary>
    [ArabicLabel("فاتورة مياه")]
    WaterBill = 31,

    /// <summary>
    /// Gas bill (فاتورة غاز)
    /// </summary>
    [ArabicLabel("فاتورة غاز")]
    GasBill = 32,

    /// <summary>
    /// Telephone/Internet bill (فاتورة هاتف/إنترنت)
    /// </summary>
    [ArabicLabel("فاتورة هاتف/إنترنت")]
    TelephoneBill = 33,

    // ==================== LEGAL & OFFICIAL DOCUMENTS ====================

    /// <summary>
    /// Court order/judgment (حكم محكمة)
    /// </summary>
    [ArabicLabel("حكم محكمة")]
    CourtOrder = 40,

    /// <summary>
    /// Legal notification (إشعار قانوني)
    /// </summary>
    [ArabicLabel("إشعار قانوني")]
    LegalNotification = 41,

    /// <summary>
    /// Power of attorney (وكالة)
    /// </summary>
    [ArabicLabel("وكالة")]
    PowerOfAttorney = 42,

    /// <summary>
    /// Inheritance document (وثيقة ميراث)
    /// </summary>
    [ArabicLabel("وثيقة ميراث")]
    InheritanceDocument = 43,

    /// <summary>
    /// Death certificate (شهادة وفاة)
    /// </summary>
    [ArabicLabel("شهادة وفاة")]
    DeathCertificate = 44,

    /// <summary>
    /// Divorce certificate (وثيقة طلاق)
    /// </summary>
    [ArabicLabel("وثيقة طلاق")]
    DivorceCertificate = 45,

    // ==================== MUNICIPAL & GOVERNMENT ====================

    /// <summary>
    /// Municipal building permit (رخصة بناء بلدية)
    /// </summary>
    [ArabicLabel("رخصة بناء بلدية")]
    BuildingPermit = 50,

    /// <summary>
    /// Occupancy permit (شهادة إشغال)
    /// </summary>
    [ArabicLabel("شهادة إشغال")]
    OccupancyPermit = 51,

    /// <summary>
    /// Property tax receipt (إيصال ضريبة عقارية)
    /// </summary>
    [ArabicLabel("إيصال ضريبة عقارية")]
    PropertyTaxReceipt = 52,

    /// <summary>
    /// Municipality certificate (شهادة بلدية)
    /// </summary>
    [ArabicLabel("شهادة بلدية")]
    MunicipalityCertificate = 53,

    /// <summary>
    /// Planning certificate (شهادة تخطيط)
    /// </summary>
    [ArabicLabel("شهادة تخطيط")]
    PlanningCertificate = 54,

    // ==================== SUPPORTING DOCUMENTS ====================

    /// <summary>
    /// Building or property photograph (صورة فوتوغرافية)
    /// </summary>
    [ArabicLabel("صورة فوتوغرافية")]
    Photograph = 60,

    /// <summary>
    /// Sketch/Drawing of property (مخطط)
    /// </summary>
    [ArabicLabel("مخطط")]
    PropertySketch = 61,

    /// <summary>
    /// Survey map (خريطة مساحية)
    /// </summary>
    [ArabicLabel("خريطة مساحية")]
    SurveyMap = 62,

    /// <summary>
    /// Witness statement/affidavit (شهادة شهود)
    /// </summary>
    [ArabicLabel("شهادة شهود")]
    WitnessStatement = 63,

    /// <summary>
    /// Statutory declaration (إقرار قانوني)
    /// </summary>
    [ArabicLabel("إقرار قانوني")]
    StatutoryDeclaration = 64,

    // ==================== SALE & PURCHASE DOCUMENTS ====================

    /// <summary>
    /// Sale contract/agreement (عقد بيع)
    /// </summary>
    [ArabicLabel("عقد بيع")]
    SaleContract = 70,

    /// <summary>
    /// Purchase receipt (إيصال شراء)
    /// </summary>
    [ArabicLabel("إيصال شراء")]
    PurchaseReceipt = 71,

    /// <summary>
    /// Preliminary sale agreement (عقد بيع ابتدائي)
    /// </summary>
    [ArabicLabel("عقد بيع ابتدائي")]
    PreliminarySaleAgreement = 72,

    // ==================== MISCELLANEOUS ====================

    /// <summary>
    /// Bank statement (كشف حساب بنكي)
    /// </summary>
    [ArabicLabel("كشف حساب بنكي")]
    BankStatement = 80,

    /// <summary>
    /// Official letter from government entity (رسالة رسمية)
    /// </summary>
    [ArabicLabel("رسالة رسمية")]
    OfficialLetter = 81,

    /// <summary>
    /// Notarized document (وثيقة موثقة)
    /// </summary>
    [ArabicLabel("وثيقة موثقة")]
    NotarizedDocument = 82,

    /// <summary>
    /// Reference letter (رسالة تزكية)
    /// </summary>
    [ArabicLabel("رسالة تزكية")]
    ReferenceLetter = 83,

    /// <summary>
    /// Other document type not listed
    /// </summary>
    [ArabicLabel("أخرى")]
    Other = 999
}