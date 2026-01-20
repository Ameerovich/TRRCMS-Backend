using MediatR;
using TRRCMS.Application.Surveys.Dtos;

namespace TRRCMS.Application.Surveys.Commands.CreateFieldSurvey;

/// <summary>
/// Command to create a new field survey
/// Corresponds to UC-001 Stage 1: Building Identification
/// </summary>
public class CreateFieldSurveyCommand : IRequest<SurveyDto>
{
    // ==================== BUILDING CONTEXT ====================

    /// <summary>
    /// Building being surveyed (required)
    /// </summary>
    public Guid BuildingId { get; set; }

    /// <summary>
    /// Property unit being surveyed (optional - can be selected later)
    /// </summary>
    public Guid? PropertyUnitId { get; set; }

    // ==================== SURVEY METADATA ====================

    /// <summary>
    /// Date when survey is being conducted
    /// </summary>
    public DateTime SurveyDate { get; set; }

    /// <summary>
    /// GPS coordinates where survey is conducted (if available)
    /// Format: "latitude,longitude"
    /// </summary>
    public string? GpsCoordinates { get; set; }

    /// <summary>
    /// Name of person being interviewed
    /// </summary>
    public string? IntervieweeName { get; set; }

    /// <summary>
    /// Relationship of interviewee to the property
    /// </summary>
    public string? IntervieweeRelationship { get; set; }

    /// <summary>
    /// Initial survey notes
    /// </summary>
    public string? Notes { get; set; }
}