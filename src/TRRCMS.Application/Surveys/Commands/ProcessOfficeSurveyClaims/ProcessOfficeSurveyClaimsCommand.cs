using MediatR;
using TRRCMS.Application.Surveys.Dtos;

namespace TRRCMS.Application.Surveys.Commands.ProcessOfficeSurveyClaims;

/// <summary>
/// Command to process an office survey: validate data, collect summary, and create claims
/// from ownership/heir relations — WITHOUT changing the survey status.
/// </summary>
public class ProcessOfficeSurveyClaimsCommand : IRequest<OfficeSurveyFinalizationResultDto>
{
    /// <summary>
    /// Survey ID to process (set from route parameter)
    /// </summary>
    public Guid SurveyId { get; set; }

    /// <summary>
    /// Final notes to add before processing
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
