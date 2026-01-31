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
    public string? RelationTypeOtherDesc { get; set; }

    /// <summary>
    /// نوع العقد - FullOwnership=1, SharedOwnership=2, LongTermRental=3, etc.
    /// </summary>
    public TenureContractType? ContractType { get; set; }
    public string? ContractTypeOtherDesc { get; set; }

    /// <summary>
    /// حصة الملكية - 0.0 to 1.0
    /// </summary>
    public decimal? OwnershipShare { get; set; }
    public string? ContractDetails { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Notes { get; set; }
}
