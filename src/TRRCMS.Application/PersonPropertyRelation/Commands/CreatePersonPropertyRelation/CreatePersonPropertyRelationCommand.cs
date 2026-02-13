using MediatR;
using TRRCMS.Application.PersonPropertyRelations.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.PersonPropertyRelations.Commands.CreatePersonPropertyRelation;

/// <summary>
/// Command to create a person-property relation
/// </summary>
public class CreatePersonPropertyRelationCommand : IRequest<PersonPropertyRelationDto>
{
    public Guid PersonId { get; set; }
    public Guid PropertyUnitId { get; set; }

    /// <summary>
    /// نوع العلاقة - Owner=1, Occupant=2, Tenant=3, Guest=4, Heir=5, Other=99
    /// </summary>
    public RelationType RelationType { get; set; }

    // ==================== NEW FIELDS FOR OFFICE SURVEY ====================

    /// <summary>
    /// نوع الإشغال - OwnerOccupied=1, TenantOccupied=2, FamilyOccupied=3, etc.
    /// </summary>
    public OccupancyType? OccupancyType { get; set; }

    /// <summary>
    /// هل يوجد دليل؟ - Indicates if evidence documents are available
    /// </summary>
    public bool HasEvidence { get; set; }

    // ==================== OTHER FIELDS ====================

    /// <summary>
    /// حصة الملكية - 0.0 to 1.0
    /// </summary>
    public decimal? OwnershipShare { get; set; }
    public string? ContractDetails { get; set; }
    public string? Notes { get; set; }
}
