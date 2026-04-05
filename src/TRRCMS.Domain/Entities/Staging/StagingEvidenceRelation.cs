using TRRCMS.Domain.Common;

namespace TRRCMS.Domain.Entities.Staging;

/// <summary>
/// Staging entity for evidence-to-relation links from the .uhc evidence_relations table.
/// Supports many-to-many linking of evidence to person-property relations.
/// During commit, each row becomes a production EvidenceRelation record.
/// </summary>
public class StagingEvidenceRelation : BaseStagingEntity
{
    /// <summary>Original Evidence UUID from .uhc.</summary>
    public Guid OriginalEvidenceId { get; private set; }

    /// <summary>Original PersonPropertyRelation UUID from .uhc.</summary>
    public Guid OriginalPersonPropertyRelationId { get; private set; }

    /// <summary>EF Core constructor.</summary>
    private StagingEvidenceRelation() : base() { }

    /// <summary>
    /// Create a new StagingEvidenceRelation from .uhc package data.
    /// </summary>
    public static StagingEvidenceRelation Create(
        Guid importPackageId,
        Guid originalEntityId,
        Guid originalEvidenceId,
        Guid originalPersonPropertyRelationId)
    {
        var entity = new StagingEvidenceRelation
        {
            OriginalEvidenceId = originalEvidenceId,
            OriginalPersonPropertyRelationId = originalPersonPropertyRelationId
        };

        entity.InitializeStagingMetadata(importPackageId, originalEntityId);
        return entity;
    }
}
