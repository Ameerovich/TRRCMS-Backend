namespace TRRCMS.Application.Surveys.Dtos;

/// <summary>
/// Data transfer object for Survey entity
/// Used for survey operations (field and office)
/// </summary>
public class SurveyDto
{
    public Guid Id { get; set; }

    /// <summary>
    /// Unique reference code given to interviewee
    /// Format: ALG-YYYY-NNNNN
    /// </summary>
    public string ReferenceCode { get; set; } = string.Empty;

    // ==================== RELATIONSHIPS ====================

    public Guid BuildingId { get; set; }
    public string? BuildingNumber { get; set; }
    public string? BuildingAddress { get; set; }

    public Guid? PropertyUnitId { get; set; }
    public string? UnitIdentifier { get; set; }

    public Guid FieldCollectorId { get; set; }
    public string? FieldCollectorName { get; set; }

    // ==================== SURVEY DETAILS ====================

    /// <summary>
    /// Date when survey was conducted
    /// </summary>
    public DateTime SurveyDate { get; set; }

    /// <summary>
    /// Survey status (Draft, Completed, Finalized, etc.)
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Survey type (Field or Office)
    /// </summary>
    public string SurveyType { get; set; } = string.Empty;

    /// <summary>
    /// GPS coordinates where survey was conducted
    /// Format: "latitude,longitude"
    /// </summary>
    public string? GpsCoordinates { get; set; }

    /// <summary>
    /// Name of person interviewed
    /// </summary>
    public string? IntervieweeName { get; set; }

    /// <summary>
    /// Relationship of interviewee to property
    /// </summary>
    public string? IntervieweeRelationship { get; set; }

    /// <summary>
    /// Survey notes and observations
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Duration of survey in minutes
    /// </summary>
    public int? DurationMinutes { get; set; }

    // ==================== EXPORT TRACKING ====================

    /// <summary>
    /// Date when survey was exported to .uhc container
    /// </summary>
    public DateTime? ExportedDate { get; set; }

    /// <summary>
    /// Package ID of the .uhc container this survey was exported in
    /// </summary>
    public Guid? ExportPackageId { get; set; }

    /// <summary>
    /// Date when survey was imported to desktop system
    /// </summary>
    public DateTime? ImportedDate { get; set; }

    // ==================== AUDIT ====================

    public DateTime CreatedAtUtc { get; set; }
    public Guid CreatedBy { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime? LastModifiedAtUtc { get; set; }
    public Guid? LastModifiedBy { get; set; }
}