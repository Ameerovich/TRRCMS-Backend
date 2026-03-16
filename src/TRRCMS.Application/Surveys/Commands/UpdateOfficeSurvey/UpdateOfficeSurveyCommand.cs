using MediatR;
using TRRCMS.Application.Surveys.Dtos;

namespace TRRCMS.Application.Surveys.Commands.UpdateOfficeSurvey;

/// <summary>
/// Command to update an existing office survey
/// Supports partial updates while survey is in Draft status
/// </summary>
public class UpdateOfficeSurveyCommand : IRequest<SurveyDto>
{
    /// <summary>
    /// Survey ID to update (set from route parameter)
    /// </summary>
    public Guid SurveyId { get; set; }

    /// <summary>
    /// Property unit being surveyed (can be changed while in Draft)
    /// If provided, must belong to the survey's building
    /// </summary>
    public Guid? PropertyUnitId { get; set; }

    /// <summary>
    /// Date when survey is being conducted
    /// </summary>
    public DateTime? SurveyDate { get; set; }

    /// <summary>
    /// Survey notes and observations
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Duration of survey in minutes
    /// </summary>
    public int? DurationMinutes { get; set; }

    /// <summary>
    /// Office location where survey is being conducted
    /// </summary>
    public string? OfficeLocation { get; set; }

    /// <summary>
    /// Document registration number from registration desk
    /// </summary>
    public string? RegistrationNumber { get; set; }

    /// <summary>
    /// Appointment reference (if survey was scheduled)
    /// </summary>
    public string? AppointmentReference { get; set; }

    /// <summary>
    /// Contact phone for follow-up
    /// </summary>
    public string? ContactPhone { get; set; }

    /// <summary>
    /// Contact email for follow-up
    /// </summary>
    public string? ContactEmail { get; set; }

    /// <summary>
    /// Indicates if claimant visited in person
    /// </summary>
    public bool? InPersonVisit { get; set; }
}
