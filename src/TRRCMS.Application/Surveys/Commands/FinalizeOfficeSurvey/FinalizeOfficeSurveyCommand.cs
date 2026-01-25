using MediatR;
using TRRCMS.Application.Surveys.Dtos;

namespace TRRCMS.Application.Surveys.Commands.FinalizeOfficeSurvey;

/// <summary>
/// Command to finalize an office survey
/// Corresponds to UC-004 S21: Mark as finalized
/// Transitions survey from Draft to Finalized status and optionally creates a claim
/// </summary>
public class FinalizeOfficeSurveyCommand : IRequest<OfficeSurveyFinalizationResultDto>
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
    /// Whether to automatically create a claim if ownership relations exist
    /// Default is true per FSD requirements
    /// </summary>
    public bool AutoCreateClaim { get; set; } = true;
}
