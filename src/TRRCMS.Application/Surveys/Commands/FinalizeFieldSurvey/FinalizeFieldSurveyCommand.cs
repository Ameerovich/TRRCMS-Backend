using MediatR;
using TRRCMS.Application.Surveys.Dtos;

namespace TRRCMS.Application.Surveys.Commands.FinalizeFieldSurvey;

/// <summary>
/// Command to finalize a field survey
/// Corresponds to UC-001 Final Stage: Finalize field survey for export
/// Transitions survey from Draft to Finalized status
/// Unlike office surveys, field surveys are typically exported to .uhc containers
/// before claims are created during the import process (UC-003)
/// </summary>
public class FinalizeFieldSurveyCommand : IRequest<FieldSurveyFinalizationResultDto>
{
    /// <summary>
    /// Survey ID to finalize (set from route parameter)
    /// </summary>
    public Guid SurveyId { get; set; }

    /// <summary>
    /// Final notes to add before finalizing
    /// </summary>
    public string? FinalNotes { get; set; }

    /// <summary>
    /// Duration of survey in minutes (if not set during survey)
    /// </summary>
    public int? DurationMinutes { get; set; }

    /// <summary>
    /// Final GPS coordinates (if updated at end of survey)
    /// </summary>
    public string? FinalGpsCoordinates { get; set; }

    /// <summary>
    /// Whether to validate completeness before finalizing
    /// If true, will return warnings for missing data but still allow finalization
    /// Default is true
    /// </summary>
    public bool ValidateCompleteness { get; set; } = true;
}