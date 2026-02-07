using TRRCMS.Application.Conflicts.Dtos;

namespace TRRCMS.Application.Common.Interfaces;

/// <summary>
/// Service interface for merging duplicate entities during conflict resolution.
/// Implementations handle entity-specific merge logic:
///   - PersonMergeService: merges Person records, propagates FK changes across
///     PersonPropertyRelations, Households, Claims (UC-008 S06–S07).
///   - PropertyMergeService: merges Building/PropertyUnit records, propagates FK changes
///     across Surveys, Relations, Claims (UC-007 S06–S07).
/// 
/// Each implementation is registered in DI and distinguished by <see cref="EntityType"/>.
/// The ResolveConflictCommandHandler resolves the correct service at runtime.
/// </summary>
public interface IMergeService
{
    /// <summary>
    /// Discriminator for DI resolution.
    /// "Person" for PersonMergeService, "Building" for PropertyMergeService.
    /// </summary>
    string EntityType { get; }

    /// <summary>
    /// Merge the discarded entity into the master entity.
    /// 
    /// Steps:
    /// 1. Load both entities with navigation properties.
    /// 2. Choose field values (prefer master, fill gaps from discarded).
    /// 3. Re-point all FK references from discarded → master.
    /// 4. Soft-delete or deactivate the discarded entity.
    /// 5. Build merge mapping JSON for audit trail.
    /// </summary>
    /// <param name="masterEntityId">ID of the entity to keep (surviving record).</param>
    /// <param name="discardedEntityId">ID of the entity to merge away.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Merge result with mapping details and reference counts.</returns>
    Task<MergeResultDto> MergeAsync(
        Guid masterEntityId,
        Guid discardedEntityId,
        CancellationToken cancellationToken = default);
}
