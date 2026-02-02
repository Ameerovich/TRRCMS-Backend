using MediatR;
using TRRCMS.Application.Surveys.Dtos;

namespace TRRCMS.Application.Surveys.Queries.GetFieldSurveyById;

/// <summary>
/// Query to get a field survey by ID with full details
/// Corresponds to UC-001/UC-002: Field Survey view/resume
/// Returns survey with all related data (households, persons, relations, evidence)
/// </summary>
public class GetFieldSurveyByIdQuery : IRequest<FieldSurveyDetailDto>
{
    /// <summary>
    /// Survey ID to retrieve
    /// </summary>
    public Guid SurveyId { get; set; }

    /// <summary>
    /// Whether to include related households
    /// </summary>
    public bool IncludeHouseholds { get; set; } = true;

    /// <summary>
    /// Whether to include related persons
    /// </summary>
    public bool IncludePersons { get; set; } = true;

    /// <summary>
    /// Whether to include person-property relations
    /// </summary>
    public bool IncludeRelations { get; set; } = true;

    /// <summary>
    /// Whether to include evidence
    /// </summary>
    public bool IncludeEvidence { get; set; } = true;
}