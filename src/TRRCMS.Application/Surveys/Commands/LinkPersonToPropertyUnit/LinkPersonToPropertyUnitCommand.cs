using MediatR;

namespace TRRCMS.Application.Surveys.Commands.LinkPersonToPropertyUnit;

/// <summary>
/// Command to link a person to a property unit
/// Creates PersonPropertyRelation for ownership/tenancy tracking
/// </summary>
public class LinkPersonToPropertyUnitCommand : IRequest<Unit>
{
    /// <summary>
    /// Survey ID for authorization
    /// </summary>
    public Guid SurveyId { get; set; }

    /// <summary>
    /// Person ID to link
    /// </summary>
    public Guid PersonId { get; set; }

    /// <summary>
    /// Property unit ID to link to
    /// </summary>
    public Guid PropertyUnitId { get; set; }

    /// <summary>
    /// Relationship type (Owner, Tenant, Occupant, etc.)
    /// </summary>
    public string RelationType { get; set; } = string.Empty;

}