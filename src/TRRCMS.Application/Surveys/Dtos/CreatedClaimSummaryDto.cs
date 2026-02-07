using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Surveys.Dtos;

/// <summary>
/// Summary DTO for each claim created during office survey finalization.
/// Each ownership/heir relation in the survey generates one claim.
/// Maps to the "تسجيل الحالة" (Case Registration) UI panel.
/// </summary>
public class CreatedClaimSummaryDto
{
    // ==================== CLAIM IDENTIFIERS ====================

    /// <summary>
    /// Created claim ID
    /// </summary>
    public Guid ClaimId { get; set; }

    /// <summary>
    /// Claim number (Format: CL-YYYY-NNNNNN)
    /// Maps to UI: معرف المطالب
    /// </summary>
    public string ClaimNumber { get; set; } = string.Empty;

    // ==================== UI-REQUIRED FIELDS ====================

    /// <summary>
    /// The unit identifier (number) of the property unit whose relationship generated the claim.
    /// Maps to UI: معرف الوحدة المطالب بها / رقم الوحدة
    /// </summary>
    public string PropertyUnitIdNumber { get; set; } = string.Empty;

    /// <summary>
    /// Full Arabic name of the person whose relationship generated the claim.
    /// Maps to UI: معرف المطالب / اسم الشخص
    /// </summary>
    public string FullNameArabic { get; set; } = string.Empty;

    /// <summary>
    /// Claim source - how the claim was created.
    /// Default: OfficeSubmission (2) for office survey finalization.
    /// Maps to UI: المصدر
    /// </summary>
    public ClaimSource ClaimSource { get; set; } = ClaimSource.OfficeSubmission;

    /// <summary>
    /// Case priority level.
    /// Default: Normal (2).
    /// Maps to UI: الأولوية
    /// </summary>
    public CasePriority CasePriority { get; set; } = CasePriority.Normal;

    /// <summary>
    /// Claim status.
    /// Default: Draft (1).
    /// Maps to UI: حالة الحالة
    /// </summary>
    public ClaimStatus ClaimStatus { get; set; } = ClaimStatus.Draft;

    /// <summary>
    /// The UTC date when the related survey was created.
    /// Maps to UI: تاريخ المسح
    /// </summary>
    public DateTime SurveyDate { get; set; }

    /// <summary>
    /// Type of works based on the related property unit type.
    /// Values: "Residential", "Commercial", "Factorial"
    /// Maps to UI: طبيعة الأعمال
    /// Derived from PropertyUnitType enum:
    ///   Apartment → Residential
    ///   Shop, Office → Commercial
    ///   Warehouse → Factorial
    ///   Other → Other
    /// </summary>
    public string TypeOfWorks { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether a tenure document (evidence) was added to the
    /// person-property relation that created this claim.
    /// Maps to UI: الأدلة متوفرة (green indicator)
    /// </summary>
    public bool HasEvidence { get; set; }

    // ==================== ADDITIONAL CONTEXT ====================

    /// <summary>
    /// The person-property relation ID that generated this claim
    /// </summary>
    public Guid SourceRelationId { get; set; }

    /// <summary>
    /// The relation type that triggered claim creation (Owner, Heir)
    /// </summary>
    public string RelationType { get; set; } = string.Empty;

    /// <summary>
    /// Person ID of the claimant
    /// </summary>
    public Guid PersonId { get; set; }

    /// <summary>
    /// Property Unit ID
    /// </summary>
    public Guid PropertyUnitId { get; set; }
}
