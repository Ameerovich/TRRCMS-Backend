namespace TRRCMS.Application.Import.Dtos;

/// <summary>
/// DTO representing a staged entity in the import pipeline.
/// Used by the GET staged-entities endpoint to show all entities
/// in a package with their IDs, types, and validation status.
/// </summary>
public class StagedEntityDto
{
    /// <summary>Surrogate ID (BaseStagingEntity.Id — database PK).</summary>
    public Guid StagingId { get; init; }

    /// <summary>Original entity ID from the .uhc package (used for FK resolution).</summary>
    public Guid OriginalEntityId { get; init; }

    /// <summary>Entity type: Building, PropertyUnit, Person, Household, etc.</summary>
    public string EntityType { get; init; } = string.Empty;

    /// <summary>Human-readable identifier (BuildingId, UnitIdentifier, NationalId, name, etc.).</summary>
    public string Identifier { get; init; } = string.Empty;

    /// <summary>Validation status: Pending, Valid, Invalid, Warning, Skipped.</summary>
    public string ValidationStatus { get; init; } = string.Empty;

    /// <summary>Whether the record is approved for commit.</summary>
    public bool IsApprovedForCommit { get; init; }

    /// <summary>Production entity ID (set after commit or merge).</summary>
    public Guid? CommittedEntityId { get; init; }

    /// <summary>Optional secondary display info (e.g. building type, unit type, person name).</summary>
    public string? DisplayInfo { get; init; }

    /// <summary>For child entities: the OriginalEntityId of the parent staging entity.</summary>
    public Guid? ParentOriginalEntityId { get; init; }
}

/// <summary>
/// Response containing all staged entities for an import package, grouped by entity type.
/// </summary>
public class GetStagedEntitiesResponse
{
    public Guid ImportPackageId { get; set; }
    public List<StagedEntityDto> Buildings { get; set; } = new();
    public List<StagedEntityDto> PropertyUnits { get; set; } = new();
    public List<StagedEntityDto> Persons { get; set; } = new();
    public List<StagedEntityDto> Households { get; set; } = new();
    public List<StagedEntityDto> PersonPropertyRelations { get; set; } = new();
    public List<StagedEntityDto> Claims { get; set; } = new();
    public List<StagedEntityDto> Surveys { get; set; } = new();
    public List<StagedEntityDto> Evidences { get; set; } = new();
    public int TotalCount => Buildings.Count + PropertyUnits.Count + Persons.Count +
        Households.Count + PersonPropertyRelations.Count + Claims.Count +
        Surveys.Count + Evidences.Count;
}
