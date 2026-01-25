using MediatR;
using TRRCMS.Application.Surveys.Dtos;

namespace TRRCMS.Application.Surveys.Commands.CreateOfficeSurvey;

/// <summary>
/// Command to create a new office survey
/// Corresponds to UC-004: Office Survey
/// </summary>
public class CreateOfficeSurveyCommand : IRequest<SurveyDto>
{
    // ==================== BUILDING CONTEXT ====================

    /// <summary>
    /// Building being surveyed (required)
    /// Must exist in the system
    /// </summary>
    public Guid BuildingId { get; set; }

    /// <summary>
    /// Property unit being surveyed (optional - can be selected/created later)
    /// If provided, must belong to the specified building
    /// </summary>
    public Guid? PropertyUnitId { get; set; }

    // ==================== SURVEY METADATA ====================

    /// <summary>
    /// Date when survey is being conducted
    /// Typically today's date for walk-in claimants
    /// </summary>
    public DateTime SurveyDate { get; set; }

    /// <summary>
    /// Name of person being interviewed (claimant or representative)
    /// </summary>
    public string? IntervieweeName { get; set; }

    /// <summary>
    /// Relationship of interviewee to the property
    /// e.g., "Owner", "Tenant", "Family Representative", "Legal Representative"
    /// </summary>
    public string? IntervieweeRelationship { get; set; }

    /// <summary>
    /// Initial survey notes
    /// </summary>
    public string? Notes { get; set; }

    // ==================== OFFICE SPECIFIC ====================

    /// <summary>
    /// Office location where survey is being conducted
    /// e.g., "UN-Habitat Aleppo Office", "Municipality Building"
    /// </summary>
    public string? OfficeLocation { get; set; }

    /// <summary>
    /// Document registration number from registration desk
    /// External reference for tracking walk-in visitors
    /// </summary>
    public string? RegistrationNumber { get; set; }

    /// <summary>
    /// Appointment reference (if survey was scheduled in advance)
    /// </summary>
    public string? AppointmentReference { get; set; }

    /// <summary>
    /// Contact phone for follow-up communication
    /// </summary>
    public string? ContactPhone { get; set; }

    /// <summary>
    /// Contact email for follow-up communication
    /// </summary>
    public string? ContactEmail { get; set; }

    /// <summary>
    /// Indicates if claimant visited in person (true) or submitted remotely (false)
    /// Default is true for office surveys
    /// </summary>
    public bool InPersonVisit { get; set; } = true;
}
