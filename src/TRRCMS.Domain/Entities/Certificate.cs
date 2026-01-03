using TRRCMS.Domain.Common;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Domain.Entities;

/// <summary>
/// Certificate entity - represents tenure rights certificates issued to beneficiaries
/// Tracks certificate generation, issuance, and collection
/// </summary>
public class Certificate : BaseAuditableEntity
{
    // ==================== CERTIFICATE IDENTIFICATION ====================

    /// <summary>
    /// Certificate number - unique identifier (رقم الشهادة)
    /// Format: CERT-YYYY-NNNNN
    /// </summary>
    public string CertificateNumber { get; private set; }

    /// <summary>
    /// QR code data for verification
    /// </summary>
    public string? QrCodeData { get; private set; }

    /// <summary>
    /// Barcode for quick scanning
    /// </summary>
    public string? Barcode { get; private set; }

    // ==================== CERTIFICATE RELATIONSHIPS ====================

    /// <summary>
    /// Foreign key to Claim this certificate is for
    /// </summary>
    public Guid ClaimId { get; private set; }

    /// <summary>
    /// Foreign key to primary beneficiary (Person)
    /// </summary>
    public Guid BeneficiaryPersonId { get; private set; }

    /// <summary>
    /// Foreign key to Property Unit
    /// </summary>
    public Guid PropertyUnitId { get; private set; }

    // ==================== CERTIFICATE STATUS ====================

    /// <summary>
    /// Current certificate status
    /// </summary>
    public CertificateStatus Status { get; private set; }

    /// <summary>
    /// Date when certificate was generated (تاريخ الإنشاء)
    /// </summary>
    public DateTime? GeneratedDate { get; private set; }

    /// <summary>
    /// User who generated the certificate
    /// </summary>
    public Guid? GeneratedByUserId { get; private set; }

    /// <summary>
    /// Date when certificate was approved for issuance
    /// </summary>
    public DateTime? ApprovedDate { get; private set; }

    /// <summary>
    /// User who approved certificate issuance
    /// </summary>
    public Guid? ApprovedByUserId { get; private set; }

    /// <summary>
    /// Date when certificate was issued to beneficiary (تاريخ الإصدار)
    /// </summary>
    public DateTime? IssuedDate { get; private set; }

    /// <summary>
    /// User who issued the certificate
    /// </summary>
    public Guid? IssuedByUserId { get; private set; }

    /// <summary>
    /// Date when beneficiary collected the certificate (تاريخ الاستلام)
    /// </summary>
    public DateTime? CollectedDate { get; private set; }

    /// <summary>
    /// User who handed over the certificate
    /// </summary>
    public Guid? HandedOverByUserId { get; private set; }

    // ==================== BENEFICIARY COLLECTION ====================

    /// <summary>
    /// Name of person who collected certificate (may differ from beneficiary)
    /// </summary>
    public string? CollectedByName { get; private set; }

    /// <summary>
    /// Relationship to beneficiary of person who collected
    /// </summary>
    public string? CollectorRelationship { get; private set; }

    /// <summary>
    /// ID document number of person who collected
    /// </summary>
    public string? CollectorIdNumber { get; private set; }

    /// <summary>
    /// Signature of person who collected (image path or data)
    /// </summary>
    public string? CollectorSignature { get; private set; }

    // ==================== CERTIFICATE CONTENT ====================

    /// <summary>
    /// Certificate type (e.g., "Ownership Certificate", "Occupancy Certificate")
    /// </summary>
    public string CertificateType { get; private set; }

    /// <summary>
    /// Certificate title in Arabic
    /// </summary>
    public string TitleArabic { get; private set; }

    /// <summary>
    /// Certificate title in English (optional)
    /// </summary>
    public string? TitleEnglish { get; private set; }

    /// <summary>
    /// Summary of rights conferred (Arabic)
    /// </summary>
    public string RightsSummaryArabic { get; private set; }

    /// <summary>
    /// Summary of rights conferred (English, optional)
    /// </summary>
    public string? RightsSummaryEnglish { get; private set; }

    /// <summary>
    /// Additional terms and conditions
    /// </summary>
    public string? TermsAndConditions { get; private set; }

    /// <summary>
    /// Certificate remarks or special notes
    /// </summary>
    public string? Remarks { get; private set; }

    // ==================== VALIDITY ====================

    /// <summary>
    /// Certificate validity start date (تاريخ السريان)
    /// </summary>
    public DateTime ValidityStartDate { get; private set; }

    /// <summary>
    /// Certificate expiry date (if applicable) (تاريخ الانتهاء)
    /// Null means no expiration
    /// </summary>
    public DateTime? ValidityEndDate { get; private set; }

    /// <summary>
    /// Indicates if certificate is permanent (no expiry)
    /// </summary>
    public bool IsPermanent { get; private set; }

    // ==================== FILE INFORMATION ====================

    /// <summary>
    /// File path to generated PDF certificate
    /// </summary>
    public string? PdfFilePath { get; private set; }

    /// <summary>
    /// SHA-256 hash of PDF file for integrity
    /// </summary>
    public string? PdfFileHash { get; private set; }

    /// <summary>
    /// File size of PDF in bytes
    /// </summary>
    public long? PdfFileSizeBytes { get; private set; }

    // ==================== VOIDING/CANCELLATION ====================

    /// <summary>
    /// Date when certificate was voided (if applicable)
    /// </summary>
    public DateTime? VoidedDate { get; private set; }

    /// <summary>
    /// User who voided the certificate
    /// </summary>
    public Guid? VoidedByUserId { get; private set; }

    /// <summary>
    /// Reason for voiding/cancellation
    /// </summary>
    public string? VoidReason { get; private set; }

    // ==================== REISSUANCE ====================

    /// <summary>
    /// Indicates if this is a reissued certificate
    /// </summary>
    public bool IsReissued { get; private set; }

    /// <summary>
    /// Reference to original certificate (if reissued)
    /// </summary>
    public Guid? OriginalCertificateId { get; private set; }

    /// <summary>
    /// Reason for reissuance (e.g., "Lost", "Damaged", "Error Correction")
    /// </summary>
    public string? ReissueReason { get; private set; }

    /// <summary>
    /// Reissue number (1st reissue, 2nd reissue, etc.)
    /// </summary>
    public int? ReissueNumber { get; private set; }

    // ==================== DIGITAL SIGNATURE ====================

    /// <summary>
    /// Digital signature of the certificate
    /// </summary>
    public string? DigitalSignature { get; private set; }

    /// <summary>
    /// Signing authority name
    /// </summary>
    public string? SigningAuthority { get; private set; }

    /// <summary>
    /// Date when certificate was digitally signed
    /// </summary>
    public DateTime? SignedDate { get; private set; }

    // ==================== LEGAL INFORMATION ====================

    /// <summary>
    /// Legal basis for certificate issuance
    /// </summary>
    public string? LegalBasis { get; private set; }

    /// <summary>
    /// Reference to law/regulation authorizing this certificate
    /// </summary>
    public string? LegalReference { get; private set; }

    /// <summary>
    /// Issuing organization (e.g., "UN-Habitat Aleppo Office")
    /// </summary>
    public string IssuingOrganization { get; private set; }

    // ==================== NAVIGATION PROPERTIES ====================

    /// <summary>
    /// Claim this certificate is based on
    /// </summary>
    public virtual Claim Claim { get; private set; } = null!;

    /// <summary>
    /// Primary beneficiary person
    /// </summary>
    public virtual Person BeneficiaryPerson { get; private set; } = null!;

    /// <summary>
    /// Property unit covered by certificate
    /// </summary>
    public virtual PropertyUnit PropertyUnit { get; private set; } = null!;

    /// <summary>
    /// Original certificate (if this is a reissue)
    /// </summary>
    public virtual Certificate? OriginalCertificate { get; private set; }

    // Note: User entities for GeneratedBy, ApprovedBy, IssuedBy, etc.
    // public virtual User? GeneratedByUser { get; private set; }
    // public virtual User? ApprovedByUser { get; private set; }
    // public virtual User? IssuedByUser { get; private set; }
    // public virtual User? HandedOverByUser { get; private set; }
    // public virtual User? VoidedByUser { get; private set; }

    // ==================== CONSTRUCTORS ====================

    /// <summary>
    /// EF Core constructor
    /// </summary>
    private Certificate() : base()
    {
        CertificateNumber = string.Empty;
        CertificateType = string.Empty;
        TitleArabic = string.Empty;
        RightsSummaryArabic = string.Empty;
        IssuingOrganization = string.Empty;
        Status = CertificateStatus.PendingGeneration;
        IsPermanent = true;
        IsReissued = false;
    }

    /// <summary>
    /// Create new certificate
    /// </summary>
    public static Certificate Create(
        Guid claimId,
        Guid beneficiaryPersonId,
        Guid propertyUnitId,
        string certificateType,
        string titleArabic,
        string rightsSummaryArabic,
        string issuingOrganization,
        bool isPermanent,
        DateTime? validityEndDate,
        Guid createdByUserId)
    {
        var certificate = new Certificate
        {
            ClaimId = claimId,
            BeneficiaryPersonId = beneficiaryPersonId,
            PropertyUnitId = propertyUnitId,
            CertificateType = certificateType,
            TitleArabic = titleArabic,
            RightsSummaryArabic = rightsSummaryArabic,
            IssuingOrganization = issuingOrganization,
            IsPermanent = isPermanent,
            ValidityStartDate = DateTime.UtcNow,
            ValidityEndDate = validityEndDate,
            Status = CertificateStatus.PendingGeneration,
            IsReissued = false
        };

        certificate.CertificateNumber = GenerateCertificateNumber();
        certificate.MarkAsCreated(createdByUserId);

        return certificate;
    }

    /// <summary>
    /// Create reissued certificate
    /// </summary>
    public static Certificate CreateReissue(
        Guid originalCertificateId,
        string reissueReason,
        Certificate originalCertificate,
        Guid createdByUserId)
    {
        var reissuedCert = new Certificate
        {
            ClaimId = originalCertificate.ClaimId,
            BeneficiaryPersonId = originalCertificate.BeneficiaryPersonId,
            PropertyUnitId = originalCertificate.PropertyUnitId,
            CertificateType = originalCertificate.CertificateType,
            TitleArabic = originalCertificate.TitleArabic,
            TitleEnglish = originalCertificate.TitleEnglish,
            RightsSummaryArabic = originalCertificate.RightsSummaryArabic,
            RightsSummaryEnglish = originalCertificate.RightsSummaryEnglish,
            IssuingOrganization = originalCertificate.IssuingOrganization,
            IsPermanent = originalCertificate.IsPermanent,
            ValidityStartDate = DateTime.UtcNow,
            ValidityEndDate = originalCertificate.ValidityEndDate,
            Status = CertificateStatus.PendingGeneration,
            IsReissued = true,
            OriginalCertificateId = originalCertificateId,
            ReissueReason = reissueReason,
            ReissueNumber = (originalCertificate.ReissueNumber ?? 0) + 1
        };

        reissuedCert.CertificateNumber = GenerateCertificateNumber();
        reissuedCert.MarkAsCreated(createdByUserId);

        return reissuedCert;
    }

    // ==================== DOMAIN METHODS ====================

    /// <summary>
    /// Mark certificate as generated
    /// </summary>
    public void MarkAsGenerated(
        Guid generatedByUserId,
        string pdfFilePath,
        string pdfFileHash,
        long pdfFileSizeBytes,
        string? qrCodeData,
        Guid modifiedByUserId)
    {
        Status = CertificateStatus.Generated;
        GeneratedDate = DateTime.UtcNow;
        GeneratedByUserId = generatedByUserId;
        PdfFilePath = pdfFilePath;
        PdfFileHash = pdfFileHash;
        PdfFileSizeBytes = pdfFileSizeBytes;
        QrCodeData = qrCodeData;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Approve certificate for issuance
    /// </summary>
    public void Approve(Guid approvedByUserId, Guid modifiedByUserId)
    {
        Status = CertificateStatus.ApprovedForIssuance;
        ApprovedDate = DateTime.UtcNow;
        ApprovedByUserId = approvedByUserId;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Issue certificate to beneficiary
    /// </summary>
    public void Issue(Guid issuedByUserId, Guid modifiedByUserId)
    {
        Status = CertificateStatus.Issued;
        IssuedDate = DateTime.UtcNow;
        IssuedByUserId = issuedByUserId;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Record certificate collection by beneficiary
    /// </summary>
    public void MarkAsCollected(
        string collectedByName,
        string? collectorRelationship,
        string? collectorIdNumber,
        string? collectorSignature,
        Guid handedOverByUserId,
        Guid modifiedByUserId)
    {
        Status = CertificateStatus.Collected;
        CollectedDate = DateTime.UtcNow;
        CollectedByName = collectedByName;
        CollectorRelationship = collectorRelationship;
        CollectorIdNumber = collectorIdNumber;
        CollectorSignature = collectorSignature;
        HandedOverByUserId = handedOverByUserId;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Void/cancel certificate
    /// </summary>
    public void Void(string voidReason, Guid voidedByUserId, Guid modifiedByUserId)
    {
        Status = CertificateStatus.Voided;
        VoidedDate = DateTime.UtcNow;
        VoidedByUserId = voidedByUserId;
        VoidReason = voidReason;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Put certificate issuance on hold
    /// </summary>
    public void PutOnHold(string reason, Guid modifiedByUserId)
    {
        Status = CertificateStatus.OnHold;
        Remarks = string.IsNullOrWhiteSpace(Remarks)
            ? $"[On Hold]: {reason}"
            : $"{Remarks}\n[On Hold]: {reason}";
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Add digital signature
    /// </summary>
    public void AddDigitalSignature(
        string digitalSignature,
        string signingAuthority,
        Guid modifiedByUserId)
    {
        DigitalSignature = digitalSignature;
        SigningAuthority = signingAuthority;
        SignedDate = DateTime.UtcNow;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Set legal information
    /// </summary>
    public void SetLegalInformation(
        string? legalBasis,
        string? legalReference,
        Guid modifiedByUserId)
    {
        LegalBasis = legalBasis;
        LegalReference = legalReference;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Check if certificate is expired
    /// </summary>
    public bool IsExpired()
    {
        if (IsPermanent || !ValidityEndDate.HasValue)
            return false;

        return DateTime.UtcNow > ValidityEndDate.Value;
    }

    /// <summary>
    /// Check if certificate is expiring soon (within 30 days)
    /// </summary>
    public bool IsExpiringSoon()
    {
        if (IsPermanent || !ValidityEndDate.HasValue)
            return false;

        return ValidityEndDate.Value > DateTime.UtcNow
            && ValidityEndDate.Value <= DateTime.UtcNow.AddDays(30);
    }

    /// <summary>
    /// Calculate days until expiry
    /// </summary>
    public int? DaysUntilExpiry()
    {
        if (IsPermanent || !ValidityEndDate.HasValue)
            return null;

        var days = (ValidityEndDate.Value - DateTime.UtcNow).Days;
        return days > 0 ? days : 0;
    }

    // ==================== HELPER METHODS ====================

    /// <summary>
    /// Generate certificate number
    /// Format: CERT-YYYY-NNNNN
    /// </summary>
    private static string GenerateCertificateNumber()
    {
        var year = DateTime.UtcNow.Year;
        var random = new Random();
        var sequence = random.Next(10000, 99999);
        return $"CERT-{year}-{sequence:D5}";
    }
}