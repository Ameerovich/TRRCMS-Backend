using MediatR;
using TRRCMS.Application.Evidences.Dtos;

namespace TRRCMS.Application.Surveys.Commands.UnlinkEvidenceFromRelation;

/// <summary>
/// Command to unlink an existing Evidence from a single PersonPropertyRelation.
/// Deactivates the EvidenceRelation join row without deleting the Evidence itself, so
/// an evidence shared across several relations can be detached from just one of them.
/// إلغاء ربط دليل عن علاقة شخص-عقار واحدة
/// </summary>
public class UnlinkEvidenceFromRelationCommand : IRequest<EvidenceDto>
{
    /// <summary>
    /// Survey ID for authorization and context validation.
    /// </summary>
    public Guid SurveyId { get; set; }

    /// <summary>
    /// Evidence ID to unlink.
    /// </summary>
    public Guid EvidenceId { get; set; }

    /// <summary>
    /// Person-Property Relation ID to detach the evidence from.
    /// </summary>
    public Guid PersonPropertyRelationId { get; set; }

    /// <summary>
    /// Optional reason for unlinking (recorded on the link for audit).
    /// سبب إلغاء الربط
    /// </summary>
    public string? Reason { get; set; }
}
