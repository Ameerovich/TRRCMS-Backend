namespace TRRCMS.Application.Conflicts.Dtos;

/// <summary>
/// Where a person in a person-duplicate conflict lives / is linked to: the property unit
/// and its building. Lets the reviewer see whether the two records belong to the same place
/// before deciding to merge or keep separate.
///
/// When both persons are linked to the same unit (building code + unit identifier), that
/// unit is shown on both sides. Otherwise each side shows its own unit: the household unit
/// first, falling back to the person's property relations (absent owner, heir, or a mobile
/// package that did not set <c>persons.household_id</c>).
/// </summary>
public class PersonLocationDto
{
    /// <summary>
    /// Building code (Building.BuildingId).
    /// </summary>
    public string? BuildingCode { get; set; }

    /// <summary>
    /// Community (city/town) name, or its code when the name is not available.
    /// </summary>
    public string? CommunityName { get; set; }

    /// <summary>
    /// Neighborhood name, or its code when the name is not available.
    /// </summary>
    public string? NeighborhoodName { get; set; }

    public string? UnitIdentifier { get; set; }

    /// <summary>
    /// PropertyUnitType enum value.
    /// </summary>
    public int? UnitType { get; set; }

    public int? FloorNumber { get; set; }
}
