using MediatR;
using TRRCMS.Application.PersonPropertyRelations.Dtos;

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
    /// Person ID to re-link this relation to a different person
    /// </summary>
    public Guid? PersonId { get; set; }

    /// <summary>
    /// Property unit ID to re-link this relation to a different property unit
    /// Must belong to the survey's building
    /// </summary>
    public Guid? PropertyUnitId { get; set; }

    /// <summary>
    /// نوع العلاقة - Owner=1, Occupant=2, Tenant=3, Guest=4, Heir=5, Other=99
    /// </summary>
    public int? RelationType { get; set; }

    // ==================== NEW FIELDS FOR OFFICE SURVEY ====================

    /// <summary>
    /// نوع الإشغال - OwnerOccupied=1, TenantOccupied=2, FamilyOccupied=3, etc.
    /// </summary>
    public int? OccupancyType { get; set; }
    public bool ClearOccupancyType { get; set; }

    /// <summary>
    /// هل يوجد دليل؟ - Indicates if evidence documents are available
    /// </summary>
    public bool? HasEvidence { get; set; }

    // ==================== OTHER FIELDS ====================

    /// <summary>
    /// حصة الملكية - 0.0 to 1.0
    /// </summary>
    public decimal? OwnershipShare { get; set; }
    public bool ClearOwnershipShare { get; set; }

    public string? ContractDetails { get; set; }
    public bool ClearContractDetails { get; set; }

    /// <summary>
    /// ملاحظاتك
    /// </summary>
    public string? Notes { get; set; }
    public bool ClearNotes { get; set; }
}
