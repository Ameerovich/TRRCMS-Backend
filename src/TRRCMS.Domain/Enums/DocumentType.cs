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
    TabuGreen = 1,

    /// <summary>
    /// Red Tabu - Temporary or conditional property deed (طابو أحمر)
    /// </summary>
    TabuRed = 2,

    /// <summary>
    /// Agricultural property deed (سجل زراعي)
    /// </summary>
    AgriculturalDeed = 3,

    /// <summary>
    /// Real estate registry extract (كشف عقاري)
    /// </summary>
    RealEstateRegistryExtract = 4,

    /// <summary>
    /// Property ownership certificate (شهادة ملكية)
    /// </summary>
    OwnershipCertificate = 5,

    // ==================== RENTAL & TENANCY DOCUMENTS ====================

    /// <summary>
    /// Rental/Lease contract (عقد إيجار)
    /// </summary>
    RentalContract = 10,

    /// <summary>
    /// Tenancy agreement (اتفاقية إيجار)
    /// </summary>
    TenancyAgreement = 11,

    /// <summary>
    /// Rent receipt (إيصال إيجار)
    /// </summary>
    RentReceipt = 12,

    // ==================== PERSONAL IDENTIFICATION ====================

    /// <summary>
    /// National ID card (بطاقة هوية وطنية)
    /// </summary>
    NationalIdCard = 20,

    /// <summary>
    /// Passport (جواز سفر)
    /// </summary>
    Passport = 21,

    /// <summary>
    /// Family registry (قيد عائلي)
    /// </summary>
    FamilyRegistry = 22,

    /// <summary>
    /// Birth certificate (شهادة ميلاد)
    /// </summary>
    BirthCertificate = 23,

    /// <summary>
    /// Marriage certificate (عقد زواج)
    /// </summary>
    MarriageCertificate = 24,

    // ==================== UTILITY & SERVICE DOCUMENTS ====================

    /// <summary>
    /// Electricity bill (فاتورة كهرباء)
    /// </summary>
    ElectricityBill = 30,

    /// <summary>
    /// Water bill (فاتورة مياه)
    /// </summary>
    WaterBill = 31,

    /// <summary>
    /// Gas bill (فاتورة غاز)
    /// </summary>
    GasBill = 32,

    /// <summary>
    /// Telephone/Internet bill (فاتورة هاتف/إنترنت)
    /// </summary>
    TelephoneBill = 33,

    // ==================== LEGAL & OFFICIAL DOCUMENTS ====================

    /// <summary>
    /// Court order/judgment (حكم محكمة)
    /// </summary>
    CourtOrder = 40,

    /// <summary>
    /// Legal notification (إشعار قانوني)
    /// </summary>
    LegalNotification = 41,

    /// <summary>
    /// Power of attorney (وكالة)
    /// </summary>
    PowerOfAttorney = 42,

    /// <summary>
    /// Inheritance document (وثيقة ميراث)
    /// </summary>
    InheritanceDocument = 43,

    /// <summary>
    /// Death certificate (شهادة وفاة)
    /// </summary>
    DeathCertificate = 44,

    /// <summary>
    /// Divorce certificate (وثيقة طلاق)
    /// </summary>
    DivorceCertificate = 45,

    // ==================== MUNICIPAL & GOVERNMENT ====================

    /// <summary>
    /// Municipal building permit (رخصة بناء بلدية)
    /// </summary>
    BuildingPermit = 50,

    /// <summary>
    /// Occupancy permit (شهادة إشغال)
    /// </summary>
    OccupancyPermit = 51,

    /// <summary>
    /// Property tax receipt (إيصال ضريبة عقارية)
    /// </summary>
    PropertyTaxReceipt = 52,

    /// <summary>
    /// Municipality certificate (شهادة بلدية)
    /// </summary>
    MunicipalityCertificate = 53,

    /// <summary>
    /// Planning certificate (شهادة تخطيط)
    /// </summary>
    PlanningCertificate = 54,

    // ==================== SUPPORTING DOCUMENTS ====================

    /// <summary>
    /// Building or property photograph (صورة فوتوغرافية)
    /// </summary>
    Photograph = 60,

    /// <summary>
    /// Sketch/Drawing of property (مخطط)
    /// </summary>
    PropertySketch = 61,

    /// <summary>
    /// Survey map (خريطة مساحية)
    /// </summary>
    SurveyMap = 62,

    /// <summary>
    /// Witness statement/affidavit (شهادة شهود)
    /// </summary>
    WitnessStatement = 63,

    /// <summary>
    /// Statutory declaration (إقرار قانوني)
    /// </summary>
    StatutoryDeclaration = 64,

    // ==================== SALE & PURCHASE DOCUMENTS ====================

    /// <summary>
    /// Sale contract/agreement (عقد بيع)
    /// </summary>
    SaleContract = 70,

    /// <summary>
    /// Purchase receipt (إيصال شراء)
    /// </summary>
    PurchaseReceipt = 71,

    /// <summary>
    /// Preliminary sale agreement (عقد بيع ابتدائي)
    /// </summary>
    PreliminarySaleAgreement = 72,

    // ==================== MISCELLANEOUS ====================

    /// <summary>
    /// Bank statement (كشف حساب بنكي)
    /// </summary>
    BankStatement = 80,

    /// <summary>
    /// Official letter from government entity (رسالة رسمية)
    /// </summary>
    OfficialLetter = 81,

    /// <summary>
    /// Notarized document (وثيقة موثقة)
    /// </summary>
    NotarizedDocument = 82,

    /// <summary>
    /// Reference letter (رسالة تزكية)
    /// </summary>
    ReferenceLetter = 83,

    /// <summary>
    /// Other document type not listed
    /// </summary>
    Other = 999
}