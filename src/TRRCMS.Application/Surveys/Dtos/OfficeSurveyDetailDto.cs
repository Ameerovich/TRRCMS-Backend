using TRRCMS.Application.Households.Dtos;
using TRRCMS.Application.PersonPropertyRelations.Dtos;
using TRRCMS.Application.Evidences.Dtos;

namespace TRRCMS.Application.Surveys.Dtos;

/// <summary>
/// Detailed DTO for office survey with all related data
/// Used for UC-004/UC-005: View full office survey details
/// </summary>
public class OfficeSurveyDetailDto : SurveyDto
{
    // ==================== OFFICE SPECIFIC FIELDS ====================

    /// <summary>
    /// Office location where survey was conducted
    /// </summary>
    public string? OfficeLocation { get; set; }

    /// <summary>
    /// Document registration number
    /// </summary>
    public string? RegistrationNumber { get; set; }

    /// <summary>
    /// Appointment reference
    /// </summary>
    public string? AppointmentReference { get; set; }

    /// <summary>
    /// Contact phone
    /// </summary>
    public string? ContactPhone { get; set; }

    /// <summary>
    /// Contact email
    /// </summary>
    public string? ContactEmail { get; set; }

    /// <summary>
    /// Whether claimant visited in person
    /// </summary>
    public bool? InPersonVisit { get; set; }

    // ==================== CLAIM LINKING ====================

    /// <summary>
    /// Linked claim ID (if any)
    /// </summary>
    public Guid? ClaimId { get; set; }

    /// <summary>
    /// Linked claim number (if any)
    /// </summary>
    public string? ClaimNumber { get; set; }

    /// <summary>
    /// Date when claim was created from this survey
    /// </summary>
    public DateTime? ClaimCreatedDate { get; set; }

    // ==================== RELATED DATA ====================

    /// <summary>
    /// Households captured in this survey context
    /// </summary>
    public List<HouseholdDto> Households { get; set; } = new();

    /// <summary>
    /// Person-property relations captured
    /// </summary>
    public List<PersonPropertyRelationDto> Relations { get; set; } = new();

    /// <summary>
    /// Evidence uploaded for this survey
    /// </summary>
    public List<EvidenceDto> Evidence { get; set; } = new();

    // ==================== SUMMARY ====================

    /// <summary>
    /// Summary of captured data
    /// </summary>
    public SurveyDataSummaryDto DataSummary { get; set; } = new();
}
