using TRRCMS.Application.Evidences.Dtos;
using TRRCMS.Application.PersonPropertyRelations.Dtos;
using TRRCMS.Application.Persons.Dtos;
using TRRCMS.Application.PropertyUnits.Dtos;

namespace TRRCMS.Application.Surveys.Dtos;

/// <summary>
/// Detailed DTO for field survey with all related data
/// Used for GetFieldSurveyById response
/// </summary>
public class FieldSurveyDetailDto
{
    // ==================== SURVEY CORE ====================

    public Guid Id { get; set; }
    public string ReferenceCode { get; set; } = string.Empty;

    // ==================== RELATIONSHIPS ====================

    public Guid BuildingId { get; set; }
    public string? BuildingNumber { get; set; }

    public Guid? PropertyUnitId { get; set; }
    public string? UnitIdentifier { get; set; }
    public PropertyUnitDto? PropertyUnit { get; set; }

    public Guid FieldCollectorId { get; set; }
    public string? FieldCollectorName { get; set; }

    // ==================== SURVEY DETAILS ====================

    public DateTime SurveyDate { get; set; }
    public int Status { get; set; }
    public int SurveyType { get; set; }
    public string? GpsCoordinates { get; set; }
    public string? IntervieweeName { get; set; }
    public string? IntervieweeRelationship { get; set; }
    public string? Notes { get; set; }
    public int? DurationMinutes { get; set; }

    // ==================== EXPORT TRACKING ====================

    public DateTime? ExportedDate { get; set; }
    public Guid? ExportPackageId { get; set; }
    public DateTime? ImportedDate { get; set; }

    // ==================== CLAIM INFO ====================

    public Guid? ClaimId { get; set; }
    public string? ClaimNumber { get; set; }
    public int? CaseStatus { get; set; }

    // ==================== RELATED DATA ====================

    /// <summary>
    /// Households captured in this survey
    /// </summary>
    public List<HouseholdWithPersonsDto>? Households { get; set; }

    /// <summary>
    /// Person-property relations captured in this survey
    /// </summary>
    public List<PersonPropertyRelationDto>? Relations { get; set; }

    /// <summary>
    /// Evidence uploaded for this survey
    /// </summary>
    public List<EvidenceDto>? Evidence { get; set; }

    /// <summary>
    /// Summary of collected data
    /// </summary>
    public SurveyDataSummaryDto DataSummary { get; set; } = new();

    // ==================== AUDIT ====================

    public DateTime CreatedAtUtc { get; set; }
    public Guid CreatedBy { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime? LastModifiedAtUtc { get; set; }
    public Guid? LastModifiedBy { get; set; }
}

/// <summary>
/// Household DTO with nested persons
/// Property names EXACTLY match the Household entity
/// </summary>
public class HouseholdWithPersonsDto
{
    // ==================== IDENTIFIERS ====================

    public Guid Id { get; set; }
    public Guid PropertyUnitId { get; set; }

    // ==================== BASIC INFORMATION ====================

    /// <summary>
    /// Head of household name
    /// </summary>
    public string HeadOfHouseholdName { get; set; } = string.Empty;

    /// <summary>
    /// Foreign key to Person who is head of household (if registered)
    /// </summary>
    public Guid? HeadOfHouseholdPersonId { get; set; }

    /// <summary>
    /// Household size (total number of members)
    /// </summary>
    public int HouseholdSize { get; set; }

    // ==================== GENDER COMPOSITION ====================

    /// <summary>
    /// Number of male members
    /// </summary>
    public int MaleCount { get; set; }

    /// <summary>
    /// Number of female members
    /// </summary>
    public int FemaleCount { get; set; }

    // ==================== COMPUTED TOTALS ====================

    /// <summary>
    /// Number of children
    /// </summary>
    public int ChildCount { get; set; }

    /// <summary>
    /// Number of elderly (65+ years)
    /// </summary>
    public int ElderlyCount { get; set; }

    /// <summary>
    /// Number of persons with disabilities
    /// </summary>
    public int PersonsWithDisabilitiesCount { get; set; }

    // ==================== ADDITIONAL ====================

    /// <summary>
    /// Household notes
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Persons in this household
    /// </summary>
    public List<PersonDto> Persons { get; set; } = new();
}

/// <summary>
/// Result DTO for field survey finalization
/// </summary>
public class FieldSurveyFinalizationResultDto
{
    /// <summary>
    /// Finalized survey details
    /// </summary>
    public SurveyDto Survey { get; set; } = null!;

    /// <summary>
    /// Whether survey is ready for export to .uhc container
    /// </summary>
    public bool IsReadyForExport { get; set; }

    /// <summary>
    /// Warnings about incomplete or missing data
    /// Survey is still finalized but these issues should be addressed
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// Summary of data collected in this survey
    /// </summary>
    public SurveyDataSummaryDto DataSummary { get; set; } = new();
}