using MediatR;
using TRRCMS.Application.Surveys.Dtos;

namespace TRRCMS.Application.Surveys.Commands.SaveDraftSurvey;

/// <summary>
/// Command to save survey progress as draft
/// Persists current survey state for later resumption
/// </summary>
public class SaveDraftSurveyCommand : IRequest<SurveyDto>
{
    /// <summary>
    /// Survey ID to update
    /// </summary>
    public Guid SurveyId { get; set; }

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
    /// Update or add notes (optional)
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Update survey duration (optional)
    /// </summary>
    public int? DurationMinutes { get; set; }
}