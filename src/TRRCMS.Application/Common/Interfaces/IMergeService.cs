using TRRCMS.Application.Conflicts.Dtos;

namespace TRRCMS.Application.Common.Interfaces;

/// <summary>
/// Service interface for merging duplicate entities during conflict resolution.
/// Implementations handle entity-specific merge logic for both cross-batch
/// (staging vs production) and within-batch (staging vs staging) conflicts.
///
///   - PersonMergeService: merges Person records (UC-008 S06–S07).
///   - PropertyMergeService: merges PropertyUnit records (UC-007 S06–S07).
///
/// Each implementation is registered in DI and distinguished by <see cref="EntityType"/>.
/// The MergeConflictCommandHandler / ResolveConflictCommandHandler resolves the correct
/// service at runtime.
/// </summary>
public interface IMergeService
{
    /// <summary>
    /// Discriminator for DI resolution.
    /// "Person" for PersonMergeService, "PropertyUnit" for PropertyMergeService.
    /// </summary>
    string EntityType { get; }

    /// <summary>
    /// Merge the discarded entity into the master entity.
    ///
    /// The service determines whether each entity lives in staging or production
    /// by attempting to load from production first, then falling back to staging
    /// (scoped by <paramref name="importPackageId"/>).
    ///
    /// Cross-batch (staging vs production):
    ///   - Updates production entity with merged field values.
    ///   - Marks staging entity as Skipped (won't be committed as new record).
    ///   - Sets staging.CommittedEntityId = production.Id for ID map resolution.
    ///
    /// Within-batch (staging vs staging):
    ///   - Marks discarded staging entity as Skipped.
    ///   - Master staging entity proceeds to commit normally.
    ///
    /// </summary>
    /// <param name="masterEntityId">ID of the entity to keep (surviving record).</param>
    /// <param name="discardedEntityId">ID of the entity to merge away.</param>
    /// <param name="importPackageId">Import package ID for staging entity resolution.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Merge result with mapping details and reference counts.</returns>
    Task<MergeResultDto> MergeAsync(
        Guid masterEntityId,
        Guid discardedEntityId,
        Guid? importPackageId,
        CancellationToken cancellationToken = default);
}
