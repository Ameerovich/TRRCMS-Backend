using MediatR;
using TRRCMS.Application.PersonPropertyRelations.Dtos;

namespace TRRCMS.Application.PersonPropertyRelations.Commands.CreatePersonPropertyRelation;

/// <summary>
/// Command to create a new person-property relation
/// </summary>
public class CreatePersonPropertyRelationCommand : IRequest<PersonPropertyRelationDto>
{
    // ==================== REQUIRED FIELDS ====================

    /// <summary>
    /// Person ID (required)
    /// </summary>
    public Guid PersonId { get; set; }

    /// <summary>
    /// Property unit ID (required)
    /// </summary>
    public Guid PropertyUnitId { get; set; }

    /// <summary>
    /// Relation type (required) - e.g., owner, tenant, occupant, guest, heir, other
    /// </summary>
    public string RelationType { get; set; } = string.Empty;

    /// <summary>
    /// Description in case the chosen type is "Other" (optional)
    /// </summary>
    public string? RelationTypeOtherDesc { get; set; }

    /// <summary>
    /// User creating this relation (required)
    /// </summary>
    public Guid CreatedByUserId { get; set; }

    // ==================== OPTIONAL FIELDS ====================

    /// <summary>
    /// Ownership or occupancy share (optional) - percentage (e.g., 0.5 for 50%)
    /// </summary>
    public decimal? OwnershipShare { get; set; }

    /// <summary>
    /// Contract or agreement details (optional)
    /// </summary>
    public string? ContractDetails { get; set; }

    /// <summary>
    /// Start date of the relation (optional)
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// End date of the relation (optional)
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Additional notes (optional)
    /// </summary>
    public string? Notes { get; set; }
}