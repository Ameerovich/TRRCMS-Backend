using TRRCMS.Domain.Common;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Domain.Entities.Staging;

/// <summary>
/// Staging entity for Survey records from .uhc packages.
/// Mirrors the <see cref="Survey"/> production entity in an isolated staging area.
/// Records are validated before commit to production (FSD FR-D-4).
/// 
/// Original UUIDs from .uhc (not production FKs):
/// - <see cref="OriginalBuildingId"/>: the surveyed building
/// - <see cref="OriginalPropertyUnitId"/>: the surveyed property unit (optional)
/// - <see cref="OriginalFieldCollectorId"/>: the field collector who conducted the survey
/// - <see cref="OriginalClaimId"/>: the claim this survey is associated with (optional)
/// 
/// Referenced in UC-003 Stage 2 (S13).
/// </summary>
public class StagingSurvey : BaseStagingEntity
{
    // ==================== RELATIONSHIPS (original UUIDs from .uhc) ====================

    /// <summary>Original Building UUID from .uhc — not a FK to production Buildings.</summary>
    public Guid OriginalBuildingId { get; private set; }

    /// <summary>Original PropertyUnit UUID from .uhc (optional — building-level surveys may not have one).</summary>
    public Guid? OriginalPropertyUnitId { get; private set; }

    /// <summary>Original field collector User UUID from .uhc. Optional — derived from user context during import.</summary>
    public Guid? OriginalFieldCollectorId { get; private set; }

    /// <summary>Original Claim UUID from .uhc (optional — survey may precede claim creation).</summary>
    public Guid? OriginalClaimId { get; private set; }

    // ==================== SURVEY IDENTIFICATION ====================

    /// <summary>Survey reference code — optional in staging, auto-generated during commit.</summary>
    public string? ReferenceCode { get; private set; }

    // ==================== SURVEY CLASSIFICATION ====================

    /// <summary>Survey type (Field or Office). Optional — auto-set during commit.</summary>
    public SurveyType? Type { get; private set; }

    /// <summary>How the survey data entered the system. Optional — auto-set during commit.</summary>
    public SurveySource? Source { get; private set; }

    /// <summary>Human-readable survey type name. Optional — auto-set during commit.</summary>
    public string? SurveyTypeName { get; private set; }

    // ==================== SURVEY DETAILS ====================

    /// <summary>Date when the survey was conducted.</summary>
    public DateTime SurveyDate { get; private set; }

    /// <summary>Current status of the survey. Optional — auto-set to Draft during commit.</summary>
    public SurveyStatus? Status { get; private set; }

    /// <summary>GPS coordinates as string (e.g. "35.123456,36.789012").</summary>
    public string? GpsCoordinates { get; private set; }

    /// <summary>Name of the person interviewed during the survey.</summary>
    public string? IntervieweeName { get; private set; }

    /// <summary>Interviewee's relationship to the property (e.g. "Owner", "Neighbor").</summary>
    public string? IntervieweeRelationship { get; private set; }

    /// <summary>Survey notes and observations.</summary>
    public string? Notes { get; private set; }

    // ==================== OFFICE SURVEY SPECIFIC ====================

    /// <summary>Office location where the survey was conducted (office surveys only).</summary>
    public string? OfficeLocation { get; private set; }

    /// <summary>Registration number (office surveys only).</summary>
    public string? RegistrationNumber { get; private set; }

    /// <summary>Appointment reference (office surveys only).</summary>
    public string? AppointmentReference { get; private set; }

    /// <summary>Contact phone number.</summary>
    public string? ContactPhone { get; private set; }

    /// <summary>Contact email address.</summary>
    public string? ContactEmail { get; private set; }

    // ==================== CONSTRUCTORS ====================

    /// <summary>EF Core constructor.</summary>
    private StagingSurvey() : base()
    {
    }

    // ==================== FACTORY METHOD ====================

    /// <summary>
    /// Create a new StagingSurvey record from .uhc package data.
    /// Required parameters match CreateFieldSurveyCommand/CreateOfficeSurveyCommand fields.
    /// </summary>
    public static StagingSurvey Create(
        Guid importPackageId,
        Guid originalEntityId,
        Guid originalBuildingId,
        DateTime surveyDate,
        // --- optional: from command ---
        Guid? originalPropertyUnitId = null,
        string? gpsCoordinates = null,
        string? intervieweeName = null,
        string? intervieweeRelationship = null,
        string? notes = null,
        string? officeLocation = null,
        string? registrationNumber = null,
        string? appointmentReference = null,
        string? contactPhone = null,
        string? contactEmail = null,
        // --- optional: auto-generated / commit-time ---
        Guid? originalFieldCollectorId = null,
        Guid? originalClaimId = null,
        string? referenceCode = null,
        SurveyType? type = null,
        SurveySource? source = null,
        string? surveyTypeName = null,
        SurveyStatus? status = null)
    {
        var entity = new StagingSurvey
        {
            OriginalBuildingId = originalBuildingId,
            OriginalPropertyUnitId = originalPropertyUnitId,
            OriginalFieldCollectorId = originalFieldCollectorId,
            OriginalClaimId = originalClaimId,
            ReferenceCode = referenceCode,
            Type = type,
            Source = source,
            SurveyTypeName = surveyTypeName,
            SurveyDate = surveyDate,
            Status = status,
            GpsCoordinates = gpsCoordinates,
            IntervieweeName = intervieweeName,
            IntervieweeRelationship = intervieweeRelationship,
            Notes = notes,
            OfficeLocation = officeLocation,
            RegistrationNumber = registrationNumber,
            AppointmentReference = appointmentReference,
            ContactPhone = contactPhone,
            ContactEmail = contactEmail
        };

        entity.InitializeStagingMetadata(importPackageId, originalEntityId);
        return entity;
    }
}
