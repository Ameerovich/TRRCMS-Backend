using TRRCMS.Domain.Common;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Domain.Entities;

/// <summary>
/// Survey entity - tracks field survey visits and generates reference codes
/// </summary>
public class Survey : BaseAuditableEntity
{
    // ==================== IDENTIFIERS ====================

    /// <summary>
    /// Survey reference code - unique code given to interviewee (رمز المرجع)
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
    /// Foreign key to field collector conducting the survey
    /// </summary>
    public Guid FieldCollectorId { get; private set; }

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
    /// Survey type (Field or Office)
    /// </summary>
    public string SurveyType { get; private set; }

    /// <summary>
    /// GPS coordinates where survey was conducted (if available)
    /// Format: "latitude,longitude"
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

    // ==================== NAVIGATION PROPERTIES ====================

    /// <summary>
    /// Building being surveyed
    /// </summary>
    public virtual Building Building { get; private set; } = null!;

    /// <summary>
    /// Property unit being surveyed (if specific unit survey)
    /// </summary>
    public virtual PropertyUnit? PropertyUnit { get; private set; }

    // Note: FieldCollector would be a User entity (to be created)
    // public virtual User FieldCollector { get; private set; } = null!;

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
    /// Create new survey
    /// </summary>
    public static Survey Create(
        Guid buildingId,
        Guid fieldCollectorId,
        string surveyType,
        DateTime surveyDate,
        Guid? propertyUnitId,
        Guid createdByUserId)
    {
        var survey = new Survey
        {
            BuildingId = buildingId,
            FieldCollectorId = fieldCollectorId,
            SurveyType = surveyType,
            SurveyDate = surveyDate,
            PropertyUnitId = propertyUnitId,
            Status = SurveyStatus.Draft
        };

        // Generate reference code (will be enhanced with actual business logic)
        survey.ReferenceCode = GenerateReferenceCode();

        survey.MarkAsCreated(createdByUserId);

        return survey;
    }

    // ==================== DOMAIN METHODS ====================

    /// <summary>
    /// Update survey details
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
    /// Mark survey as completed
    /// </summary>
    public void MarkAsCompleted(Guid modifiedByUserId)
    {
        Status = SurveyStatus.Completed;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Mark survey as finalized (ready for export)
    /// </summary>
    public void MarkAsFinalized(Guid modifiedByUserId)
    {
        Status = SurveyStatus.Finalized;
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

    // ==================== HELPER METHODS ====================

    /// <summary>
    /// Generate unique reference code for interviewee
    /// Format: ALG-YYYY-NNNNN (Aleppo-Year-Sequential)
    /// TODO: Implement proper sequential numbering logic
    /// </summary>
    private static string GenerateReferenceCode()
    {
        var year = DateTime.UtcNow.Year;
        var random = new Random();
        var sequence = random.Next(10000, 99999); // Temporary - should be sequential from DB
        return $"ALG-{year}-{sequence:D5}";
    }
}