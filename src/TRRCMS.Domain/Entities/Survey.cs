using TRRCMS.Domain.Common;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Domain.Entities;

/// <summary>
/// Survey entity - tracks field and office survey visits and generates reference codes
/// Supports both UC-001 (Field Survey) and UC-004 (Office Survey) workflows
/// </summary>
public class Survey : BaseAuditableEntity
{
    // ==================== IDENTIFIERS ====================

    /// <summary>
    /// Survey reference code - unique code given to interviewee (رمز المرجع)
    /// Format: ALG-YYYY-NNNNN for Field, OFC-YYYY-NNNNN for Office
    /// </summary>
    public string ReferenceCode { get; private set; }

    // ==================== RELATIONSHIPS ====================

    /// <summary>
    /// Foreign key to Building being surveyed
    /// </summary>
    public Guid BuildingId { get; private set; }

    /// <summary>
    /// Foreign key to PropertyUnit being surveyed (optional if surveying entire building)
    /// </summary>
    public Guid? PropertyUnitId { get; private set; }

    /// <summary>
    /// Foreign key to user conducting the survey (field collector or office clerk)
    /// Named FieldCollectorId for backward compatibility but applies to office clerks too
    /// </summary>
    public Guid FieldCollectorId { get; private set; }

    // ==================== SURVEY CLASSIFICATION ====================

    /// <summary>
    /// Survey type enum (Field or Office)
    /// </summary>
    public SurveyType Type { get; private set; }

    /// <summary>
    /// Survey source - how the survey data entered the system
    /// </summary>
    public SurveySource Source { get; private set; }

    /// <summary>
    /// Survey type as string (kept for backward compatibility with existing queries)
    /// </summary>
    public string SurveyType { get; private set; }

    // ==================== SURVEY DETAILS ====================

    /// <summary>
    /// Date when survey was conducted (تاريخ الاستطلاع)
    /// </summary>
    public DateTime SurveyDate { get; private set; }

    /// <summary>
    /// Survey status (Draft, Completed, Finalized, Exported, etc.)
    /// </summary>
    public SurveyStatus Status { get; private set; }

    /// <summary>
    /// GPS coordinates where survey was conducted (if available)
    /// Format: "latitude,longitude"
    /// Primarily used for field surveys
    /// </summary>
    public string? GpsCoordinates { get; private set; }

    /// <summary>
    /// Name of person interviewed (if different from property owner)
    /// </summary>
    public string? IntervieweeName { get; private set; }

    /// <summary>
    /// Relationship of interviewee to property (if applicable)
    /// </summary>
    public string? IntervieweeRelationship { get; private set; }

    /// <summary>
    /// Survey notes and observations
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// Duration of survey in minutes
    /// </summary>
    public int? DurationMinutes { get; private set; }

    // ==================== OFFICE SURVEY SPECIFIC ====================

    /// <summary>
    /// Office location where survey was conducted (for office surveys)
    /// e.g., "UN-Habitat Aleppo Office", "Municipality Building"
    /// </summary>
    public string? OfficeLocation { get; private set; }

    /// <summary>
    /// Document registration number (for office surveys with walk-in claimants)
    /// External reference from registration desk
    /// </summary>
    public string? RegistrationNumber { get; private set; }

    /// <summary>
    /// Appointment reference (if survey was scheduled)
    /// </summary>
    public string? AppointmentReference { get; private set; }

    /// <summary>
    /// Contact phone for follow-up (office surveys)
    /// </summary>
    public string? ContactPhone { get; private set; }

    /// <summary>
    /// Contact email for follow-up (office surveys)
    /// </summary>
    public string? ContactEmail { get; private set; }

    /// <summary>
    /// Indicates if claimant visited in person (true) or submitted remotely (false)
    /// </summary>
    public bool? InPersonVisit { get; private set; }

    // ==================== EXPORT TRACKING ====================

    /// <summary>
    /// Date when survey was exported to .uhc container
    /// </summary>
    public DateTime? ExportedDate { get; private set; }

    /// <summary>
    /// Package ID of the .uhc container this survey was exported in
    /// </summary>
    public Guid? ExportPackageId { get; private set; }

    /// <summary>
    /// Date when survey was imported to desktop system
    /// </summary>
    public DateTime? ImportedDate { get; private set; }

    // ==================== CLAIM LINKING ====================

    /// <summary>
    /// Foreign key to Claim created from this survey (if any)
    /// Set when survey is finalized and claim is auto-generated
    /// </summary>
    public Guid? ClaimId { get; private set; }

    /// <summary>
    /// Date when claim was created from this survey
    /// </summary>
    public DateTime? ClaimCreatedDate { get; private set; }

    // ==================== NAVIGATION PROPERTIES ====================

    /// <summary>
    /// Building being surveyed
    /// </summary>
    public virtual Building Building { get; private set; } = null!;

    /// <summary>
    /// Property unit being surveyed (if specific unit survey)
    /// </summary>
    public virtual PropertyUnit? PropertyUnit { get; private set; }

    /// <summary>
    /// User conducting the survey (field collector or office clerk)
    /// </summary>
    public virtual User? Collector { get; private set; }

    /// <summary>
    /// Claim created from this survey
    /// </summary>
    public virtual Claim? Claim { get; private set; }

    // ==================== CONSTRUCTORS ====================

    /// <summary>
    /// EF Core constructor
    /// </summary>
    private Survey() : base()
    {
        ReferenceCode = string.Empty;
        SurveyType = string.Empty;
    }

    /// <summary>
    /// Create new field survey (UC-001)
    /// </summary>
    public static Survey CreateFieldSurvey(
        Guid buildingId,
        Guid fieldCollectorId,
        DateTime surveyDate,
        Guid? propertyUnitId,
        Guid createdByUserId)
    {
        var survey = new Survey
        {
            BuildingId = buildingId,
            FieldCollectorId = fieldCollectorId,
            Type = Enums.SurveyType.Field,
            Source = SurveySource.FieldCollection,
            SurveyType = "Field", // Backward compatibility
            SurveyDate = surveyDate,
            PropertyUnitId = propertyUnitId,
            Status = SurveyStatus.Draft
        };

        survey.MarkAsCreated(createdByUserId);

        return survey;
    }

    /// <summary>
    /// Create new office survey (UC-004)
    /// </summary>
    public static Survey CreateOfficeSurvey(
        Guid buildingId,
        Guid officeClerkId,
        DateTime surveyDate,
        Guid? propertyUnitId,
        string? officeLocation,
        string? registrationNumber,
        bool? inPersonVisit,
        Guid createdByUserId)
    {
        var survey = new Survey
        {
            BuildingId = buildingId,
            FieldCollectorId = officeClerkId, // Using same field for compatibility
            Type = Enums.SurveyType.Office,
            Source = SurveySource.OfficeSubmission,
            SurveyType = "Office", // Backward compatibility
            SurveyDate = surveyDate,
            PropertyUnitId = propertyUnitId,
            OfficeLocation = officeLocation,
            RegistrationNumber = registrationNumber,
            InPersonVisit = inPersonVisit,
            Status = SurveyStatus.Draft
        };

        survey.MarkAsCreated(createdByUserId);

        return survey;
    }

    /// <summary>
    /// Create new survey (backward compatible - kept for existing code).
    /// Reference code must be supplied by the caller (generated via ISurveyReferenceCodeGenerator).
    /// </summary>
    public static Survey Create(
        Guid buildingId,
        Guid fieldCollectorId,
        string surveyType,
        DateTime surveyDate,
        Guid? propertyUnitId,
        Guid createdByUserId,
        string referenceCode)
    {
        if (string.IsNullOrWhiteSpace(referenceCode))
            throw new ArgumentException("Reference code is required.", nameof(referenceCode));

        var isOffice = surveyType.Equals("Office", StringComparison.OrdinalIgnoreCase);

        var survey = new Survey
        {
            BuildingId = buildingId,
            FieldCollectorId = fieldCollectorId,
            Type = isOffice ? Enums.SurveyType.Office : Enums.SurveyType.Field,
            Source = isOffice ? SurveySource.OfficeSubmission : SurveySource.FieldCollection,
            SurveyType = surveyType,
            SurveyDate = surveyDate,
            PropertyUnitId = propertyUnitId,
            Status = SurveyStatus.Draft,
            ReferenceCode = referenceCode
        };

        survey.MarkAsCreated(createdByUserId);

        return survey;
    }

    // ==================== DOMAIN METHODS ====================
    /// <summary>
    /// Re-point this survey to a different building (used during building merge).
    /// Preserves all survey details; only changes the building FK.
    /// </summary>
    public void UpdateBuildingId(Guid newBuildingId, Guid modifiedByUserId)
    {
        BuildingId = newBuildingId;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Set the reference code (used by handlers after generating from repository)
    /// </summary>
    public void SetReferenceCode(string referenceCode)
    {
        if (string.IsNullOrWhiteSpace(referenceCode))
            throw new ArgumentException("Reference code cannot be empty", nameof(referenceCode));

        ReferenceCode = referenceCode;
    }

    /// <summary>
    /// Update survey details (common fields for both field and office surveys)
    /// </summary>
    public void UpdateSurveyDetails(
        string? gpsCoordinates,
        string? intervieweeName,
        string? intervieweeRelationship,
        string? notes,
        int? durationMinutes,
        Guid modifiedByUserId)
    {
        GpsCoordinates = gpsCoordinates;
        IntervieweeName = intervieweeName;
        IntervieweeRelationship = intervieweeRelationship;
        Notes = notes;
        DurationMinutes = durationMinutes;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Update office survey specific details (UC-004)
    /// </summary>
    public void UpdateOfficeDetails(
        string? officeLocation,
        string? registrationNumber,
        string? appointmentReference,
        string? contactPhone,
        string? contactEmail,
        bool? inPersonVisit,
        Guid modifiedByUserId)
    {
        if (Type != Enums.SurveyType.Office)
            throw new InvalidOperationException("Office details can only be updated on Office surveys");

        OfficeLocation = officeLocation;
        RegistrationNumber = registrationNumber;
        AppointmentReference = appointmentReference;
        ContactPhone = contactPhone;
        ContactEmail = contactEmail;
        InPersonVisit = inPersonVisit;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Mark survey as completed
    /// </summary>
    public void MarkAsCompleted(Guid modifiedByUserId)
    {
        Status = SurveyStatus.Completed;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Mark survey as finalized (ready for export/claim creation)
    /// </summary>
    public void MarkAsFinalized(Guid modifiedByUserId)
    {
        Status = SurveyStatus.Finalized;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Link claim to survey (called when claim is auto-generated)
    /// </summary>
    public void LinkClaim(Guid claimId, Guid modifiedByUserId)
    {
        ClaimId = claimId;
        ClaimCreatedDate = DateTime.UtcNow;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Mark survey as exported
    /// </summary>
    public void MarkAsExported(Guid exportPackageId, Guid modifiedByUserId)
    {
        Status = SurveyStatus.Exported;
        ExportedDate = DateTime.UtcNow;
        ExportPackageId = exportPackageId;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Mark survey as imported to desktop system
    /// </summary>
    public void MarkAsImported(Guid modifiedByUserId)
    {
        Status = SurveyStatus.Imported;
        ImportedDate = DateTime.UtcNow;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Mark survey as validated by data manager
    /// </summary>
    public void MarkAsValidated(Guid modifiedByUserId)
    {
        Status = SurveyStatus.Validated;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Mark survey as requiring revision
    /// </summary>
    public void MarkAsRequiringRevision(string revisionNotes, Guid modifiedByUserId)
    {
        Status = SurveyStatus.RequiresRevision;
        Notes = string.IsNullOrWhiteSpace(Notes)
            ? revisionNotes
            : $"{Notes}\n[Revision Required]: {revisionNotes}";
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Cancel survey
    /// </summary>
    public void Cancel(string cancellationReason, Guid modifiedByUserId)
    {
        Status = SurveyStatus.Cancelled;
        Notes = string.IsNullOrWhiteSpace(Notes)
            ? $"[Cancelled]: {cancellationReason}"
            : $"{Notes}\n[Cancelled]: {cancellationReason}";
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Update survey date
    /// </summary>
    public void UpdateSurveyDate(DateTime surveyDate, Guid modifiedByUserId)
    {
        SurveyDate = surveyDate;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Verify survey can be modified (must be Draft)
    /// </summary>
    public void EnsureCanModify()
    {
        if (Status != SurveyStatus.Draft)
            throw new InvalidOperationException($"Cannot modify survey in {Status} status. Only Draft surveys can be modified.");
    }

    /// <summary>
    /// Check if this is a field survey
    /// </summary>
    public bool IsFieldSurvey => Type == Enums.SurveyType.Field;

    /// <summary>
    /// Check if this is an office survey
    /// </summary>
    public bool IsOfficeSurvey => Type == Enums.SurveyType.Office;

    /// <summary>
    /// Link to property unit (UC-001 Stage 2, UC-004)
    /// </summary>
    public void LinkToPropertyUnit(Guid propertyUnitId, Guid modifiedByUserId)
    {
        EnsureCanModify();
        PropertyUnitId = propertyUnitId;
        MarkAsModified(modifiedByUserId);
    }

}