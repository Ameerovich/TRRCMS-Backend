using MediatR;
using TRRCMS.Application.PersonPropertyRelations.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Surveys.Commands.UpdatePersonPropertyRelation;

/// <summary>
/// Partial update command - only provided fields are updated
/// Use Clear* flags to explicitly set nullable fields to null
/// </summary>
public class UpdatePersonPropertyRelationCommand : IRequest<PersonPropertyRelationDto>
{
    public Guid SurveyId { get; set; }
    public Guid RelationId { get; set; }

    /// <summary>
    /// نوع العلاقة - Owner=1, Occupant=2, Tenant=3, Guest=4, Heir=5, Other=99
    /// </summary>
    public RelationType? RelationType { get; set; }
    public string? RelationTypeOtherDesc { get; set; }
    public bool ClearRelationTypeOtherDesc { get; set; }

    /// <summary>
    /// نوع العقد - FullOwnership=1, SharedOwnership=2, LongTermRental=3, etc.
    /// </summary>
    public TenureContractType? ContractType { get; set; }
    public bool ClearContractType { get; set; }
    public string? ContractTypeOtherDesc { get; set; }
    public bool ClearContractTypeOtherDesc { get; set; }

    /// <summary>
    /// حصة الملكية - 0.0 to 1.0
    /// </summary>
    public decimal? OwnershipShare { get; set; }
    public bool ClearOwnershipShare { get; set; }

    public string? ContractDetails { get; set; }
    public bool ClearContractDetails { get; set; }

    /// <summary>
    /// تاريخ بدء العلاقة
    /// </summary>
    public DateTime? StartDate { get; set; }
    public bool ClearStartDate { get; set; }

    public DateTime? EndDate { get; set; }
    public bool ClearEndDate { get; set; }

    /// <summary>
    /// ملاحظاتك
    /// </summary>
    public string? Notes { get; set; }
    public bool ClearNotes { get; set; }
}
