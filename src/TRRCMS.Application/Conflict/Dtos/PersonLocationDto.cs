namespace TRRCMS.Application.Conflicts.Dtos;

/// <summary>
/// Where a person in a person-duplicate conflict lives / is linked to: the property unit
/// and its building. Lets the reviewer see whether the two records belong to the same place
/// before deciding to merge or keep separate.
///
/// The unit is resolved from the person's household first; when the person has no household
/// (absent owner, heir, or a mobile package that did not set <c>persons.household_id</c>),
/// it falls back to the person's property relations.
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

    /// <summary>
    /// "Household" or "Relation" — how the unit was resolved.
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// Number of other distinct units the person is linked to (via relations) besides this one.
    /// </summary>
    public int OtherUnitsCount { get; set; }
}
