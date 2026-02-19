namespace TRRCMS.Application.Sync.DTOs;

/// <summary>
/// Full payload returned by GET /api/v1/sync/assignments.
/// This is the central data bundle the tablet downloads during Sync Step 3.
///
/// The payload contains:
/// <list type="bullet">
///   <item>All building assignments (with nested property units) whose transfer
///         status is <c>Pending</c> or <c>Failed</c> for the authenticated
///         field collector.</item>
///   <item>A snapshot of all current controlled vocabularies so the tablet can
///         operate fully offline during field collection.</item>
///   <item>A compact version map (<see cref="VocabularyVersionsSentJson"/>) that
///         the server persists on the <c>SyncSession</c> record for audit and
///         compatibility checks during the subsequent upload (Step 4).</item>
/// </list>
///
/// Sync Protocol Step 3 – GET /api/v1/sync/assignments.
/// FSD: FR-D-5 (Sync Package Contents), FR-V-1 (Vocabulary Delivery).
/// UC-012: Assign Buildings to Field Collectors.
/// </summary>
public sealed record SyncAssignmentPayloadDto
{
    // ==================== SYNC SESSION CONTEXT ====================

    /// <summary>
    /// The active <c>SyncSession</c> ID this payload was generated for.
    /// The tablet must reference this in the acknowledgement request (Step 4).
    /// </summary>
    public Guid SyncSessionId { get; init; }

    /// <summary>
    /// ID of the field collector who requested the download.
    /// Included for client-side validation; matches the JWT sub claim.
    /// </summary>
    public Guid FieldCollectorId { get; init; }

    /// <summary>
    /// UTC timestamp when this payload was assembled on the server.
    /// Used by the tablet as the "last synced at" reference point.
    /// </summary>
    public DateTime GeneratedAtUtc { get; init; }

    // ==================== ASSIGNMENTS ====================

    /// <summary>
    /// Building assignments that must be transferred to the tablet.
    /// Each item bundles the assignment metadata, building details, and
    /// the list of property units to survey.
    /// </summary>
    public IReadOnlyList<SyncBuildingDto> Assignments { get; init; }
        = Array.Empty<SyncBuildingDto>();

    /// <summary>
    /// Total number of building assignments included in this payload.
    /// Convenience property for the tablet — avoids iterating <see cref="Assignments"/>.
    /// </summary>
    public int TotalAssignments => Assignments.Count;

    // ==================== VOCABULARIES ====================

    /// <summary>
    /// Snapshot of all active controlled vocabularies at the time of the sync.
    /// The tablet must replace its local cache with these values to ensure
    /// consistent field data entry.
    /// </summary>
    public IReadOnlyList<SyncVocabularyDto> Vocabularies { get; init; }
        = Array.Empty<SyncVocabularyDto>();

    /// <summary>
    /// Compact JSON map of vocabulary name → version string persisted on the
    /// <c>SyncSession</c> record.
    /// Format: <c>{"ownership_type":"2.1.0","document_type":"1.0.3",...}</c>.
    /// </summary>
    public string VocabularyVersionsSentJson { get; init; } = "{}";
}
