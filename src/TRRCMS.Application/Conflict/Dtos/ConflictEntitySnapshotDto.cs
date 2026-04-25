using TRRCMS.Application.Buildings.Dtos;
using TRRCMS.Application.Persons.Dtos;
using TRRCMS.Application.PropertyUnits.Dtos;

namespace TRRCMS.Application.Conflicts.Dtos;

/// <summary>
/// Full snapshot of one side of a conflict (either the staging row or the production row).
/// Reuses the production DTOs (<see cref="PersonDto"/>, <see cref="PropertyUnitDto"/>,
/// <see cref="BuildingDto"/>) on both sides so the review UI can render the comparison
/// with a single per-entity-type renderer and diff fields directly.
///
/// Exactly one of <see cref="Person"/>, <see cref="PropertyUnit"/>, <see cref="Building"/>
/// is non-null, selected by <see cref="EntityType"/>. All three are null when the entity
/// could not be loaded (deleted, missing, or the conflict references inconsistent data) —
/// the wrapper itself is still returned so the frontend can render a "missing" placeholder.
/// </summary>
public class ConflictEntitySnapshotDto
{
    /// <summary>
    /// Side of the comparison: "Staging" (incoming row from .uhc) or "Production"
    /// (existing record already in the database).
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// Entity type discriminator: "Person", "PropertyUnit", or "Building".
    /// Mirrors <c>ConflictResolution.EntityType</c>.
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// Populated when <see cref="EntityType"/> is "Person".
    /// </summary>
    public PersonDto? Person { get; set; }

    /// <summary>
    /// Populated when <see cref="EntityType"/> is "PropertyUnit".
    /// </summary>
    public PropertyUnitDto? PropertyUnit { get; set; }

    /// <summary>
    /// Populated when <see cref="EntityType"/> is "Building".
    /// </summary>
    public BuildingDto? Building { get; set; }

    /// <summary>
    /// Surrogate ID of the staging row (BaseStagingEntity.Id). Null for production rows.
    /// Useful for the UI to link directly to the staging record.
    /// </summary>
    public Guid? StagingRowId { get; set; }

    /// <summary>
    /// ImportPackage that owns this staging row. Null for production rows.
    /// </summary>
    public Guid? ImportPackageId { get; set; }

    /// <summary>
    /// Validation status of the staging row (Pending, Valid, Warning, Invalid, Skipped).
    /// Null for production rows.
    /// </summary>
    public string? StagingValidationStatus { get; set; }

    /// <summary>
    /// Whether the staging row has been approved for commit. Null for production rows.
    /// </summary>
    public bool? IsApprovedForCommit { get; set; }
}
