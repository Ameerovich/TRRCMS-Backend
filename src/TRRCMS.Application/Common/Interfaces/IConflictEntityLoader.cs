using TRRCMS.Application.Conflicts.Dtos;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Application.Common.Interfaces;

/// <summary>
/// Loads the two conflicting entities for a <see cref="ConflictResolution"/> as
/// fully populated DTO snapshots so the data manager can compare them side-by-side
/// in a single API response.
///
/// Per the conflict-detection contract:
/// - The first entity is always the staging row (FirstEntityId is the staging
///   <c>OriginalEntityId</c> of the row from the .uhc package).
/// - The second entity is the production row for cross-batch conflicts, or another
///   staging row when <c>ConflictType</c> ends with <c>_WithinBatch</c>.
/// </summary>
public interface IConflictEntityLoader
{
    /// <summary>
    /// Load a full snapshot of the first conflicting entity (always staging).
    /// Returns a wrapper with <c>Source</c> and <c>EntityType</c> populated even if
    /// the row could not be found, so callers don't need null-checks at the top level.
    /// </summary>
    Task<ConflictEntitySnapshotDto> LoadFirstEntityAsync(
        ConflictResolution conflict,
        CancellationToken cancellationToken);

    /// <summary>
    /// Load a full snapshot of the second conflicting entity. Resolves to staging or
    /// production based on <c>ConflictType.EndsWith("_WithinBatch")</c>.
    /// </summary>
    Task<ConflictEntitySnapshotDto> LoadSecondEntityAsync(
        ConflictResolution conflict,
        CancellationToken cancellationToken);
}
