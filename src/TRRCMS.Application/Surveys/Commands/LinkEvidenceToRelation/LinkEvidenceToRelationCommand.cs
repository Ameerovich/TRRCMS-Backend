using MediatR;
using TRRCMS.Application.Evidences.Dtos;

namespace TRRCMS.Application.Surveys.Commands.LinkEvidenceToRelation;

/// <summary>
/// Command to link an existing Evidence to a PersonPropertyRelation (many-to-many).
/// Creates an EvidenceRelation join entity.
/// ربط دليل موجود بعلاقة شخص-عقار
/// </summary>
public class LinkEvidenceToRelationCommand : IRequest<EvidenceDto>
{
    /// <summary>
    /// Survey ID for authorization and context validation
    /// </summary>
    public Guid SurveyId { get; set; }

    /// <summary>
    /// Evidence ID to link
    /// </summary>
    public Guid EvidenceId { get; set; }

    /// <summary>
    /// Person-Property Relation ID to link the evidence to
    /// </summary>
    public Guid PersonPropertyRelationId { get; set; }

    /// <summary>
    /// Optional reason for linking (e.g., "Shared ownership deed", "Same rental contract")
    /// سبب الربط
    /// </summary>
    public string? LinkReason { get; set; }
}
