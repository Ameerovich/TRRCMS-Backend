using MediatR;
using TRRCMS.Application.Surveys.Dtos;

namespace TRRCMS.Application.Surveys.Commands.SaveDraftSurvey;

/// <summary>
/// Command to save survey progress as draft
/// Corresponds to UC-002: Save draft and resume later
/// </summary>
public class SaveDraftSurveyCommand : IRequest<SurveyDto>
{
    /// <summary>
    /// Survey ID to update
    /// </summary>
    public Guid SurveyId { get; set; }

    // ==================== OPTIONAL UPDATES ====================

    /// <summary>
    /// Update property unit selection (optional)
    /// </summary>
    public Guid? PropertyUnitId { get; set; }

    /// <summary>
    /// Update GPS coordinates (optional)
    /// Format: "latitude,longitude"
    /// </summary>
    public string? GpsCoordinates { get; set; }

    /// <summary>
    /// Update interviewee name (optional)
    /// </summary>
    public string? IntervieweeName { get; set; }

    /// <summary>
    /// Update interviewee relationship (optional)
    /// </summary>
    public string? IntervieweeRelationship { get; set; }

    /// <summary>
    /// Update or add notes (optional)
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Update survey duration (optional)
    /// </summary>
    public int? DurationMinutes { get; set; }
}