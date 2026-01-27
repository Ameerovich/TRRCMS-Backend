using MediatR;
using TRRCMS.Application.PersonPropertyRelations.Dtos;

namespace TRRCMS.Application.Surveys.Commands.LinkPersonToPropertyUnit;

/// <summary>
/// Command to link a person to a property unit
/// Creates PersonPropertyRelation for ownership/tenancy tracking
/// Returns the created relation data
/// </summary>
public class LinkPersonToPropertyUnitCommand : IRequest<PersonPropertyRelationDto>
{
    // ==================== REQUIRED FIELDS ====================

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
    /// Relationship type (Owner, Tenant, Occupant, Heir, Guest, Other)
    /// </summary>
    public string RelationType { get; set; } = string.Empty;

    // ==================== OPTIONAL FIELDS ====================

    /// <summary>
    /// Description when RelationType is "Other"
    /// </summary>
    public string? RelationTypeOtherDesc { get; set; }

    /// <summary>
    /// Ownership share as decimal (e.g., 0.5 for 50%, 1.0 for 100%)
    /// Required for Owner relation type
    /// </summary>
    public decimal? OwnershipShare { get; set; }

    /// <summary>
    /// Contract or agreement details (lease number, deed reference, etc.)
    /// </summary>
    public string? ContractDetails { get; set; }

    /// <summary>
    /// Start date of the relation (when ownership/tenancy began)
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// End date of the relation (for terminated relations or fixed-term leases)
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Additional notes about the relation
    /// </summary>
    public string? Notes { get; set; }
}