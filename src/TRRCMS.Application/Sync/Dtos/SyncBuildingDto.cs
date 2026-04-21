using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Sync.DTOs;

/// <summary>
/// Compact representation of a building assignment sent to the tablet during a sync download.
/// Bundles the <see cref="BuildingAssignment"/> metadata together with its parent
/// <see cref="Building"/> attributes and the list of property units to be surveyed.
///
/// Designed to be bandwidth-efficient for LAN WiFi transfer; includes only the
/// fields the Flutter tablet application needs for offline field operation.
///
/// Sync Protocol Step 3 – GET /api/v1/sync/assignments.
/// </summary>
public sealed record SyncBuildingDto
{
    // ==================== ASSIGNMENT METADATA ====================

    /// <summary>
    /// Surrogate ID of the <c>BuildingAssignment</c> record.
    /// Required by the tablet to reference this assignment during acknowledgement (Step 4).
    /// </summary>
    public Guid AssignmentId { get; init; }

    /// <summary>
    /// Date when the supervisor assigned this building to the field collector.
    /// </summary>
    public DateTime AssignedDate { get; init; }

    /// <summary>
    /// Optional deadline for completing the field survey.
    /// Null when no deadline has been set.
    /// </summary>
    public DateTime? TargetCompletionDate { get; init; }

    /// <summary>
    /// Priority level of the assignment (Normal, High, Urgent).
    /// Used by the tablet to sort the work queue.
    /// </summary>
    public string Priority { get; init; } = "Normal";

    /// <summary>
    /// Supervisor notes or instructions accompanying the assignment.
    /// </summary>
    public string? AssignmentNotes { get; init; }

    /// <summary>
    /// Indicates whether this is a revisit assignment (data previously collected).
    /// When true, <see cref="UnitsForRevisit"/> lists the specific units to revisit.
    /// </summary>
    public bool IsRevisit { get; init; }

    /// <summary>
    /// JSON array of property unit IDs to revisit; null for full-building assignments.
    /// Stored as <c>["uuid1","uuid2",...]</c>.
    /// </summary>
    public string? UnitsForRevisit { get; init; }

    // ==================== BUILDING IDENTIFICATION ====================

    /// <summary>
    /// 17-digit building identifier code (رمز البناء).
    /// Storage format: <c>GGDDSSCCNCNNBBBBB</c> (no dashes).
    /// </summary>
    public string BuildingCode { get; init; } = string.Empty;

    /// <summary>
    /// Human-readable display format with dashes:
    /// <c>GG-DD-SS-CCC-NNN-BBBBB</c>.
    /// Computed from the stored code; included here for the tablet UI.
    /// </summary>
    public string BuildingCodeDisplay { get; init; } = string.Empty;

    // ==================== ADMINISTRATIVE LOCATION ====================

    /// <summary>Governorate code (محافظة) — 2 digits.</summary>
    public string GovernorateCode { get; init; } = string.Empty;

    /// <summary>District code (مدينة) — 2 digits.</summary>
    public string DistrictCode { get; init; } = string.Empty;

    /// <summary>Sub-district code (بلدة) — 2 digits.</summary>
    public string SubDistrictCode { get; init; } = string.Empty;

    /// <summary>Community code (قرية) — 3 digits.</summary>
    public string CommunityCode { get; init; } = string.Empty;

    /// <summary>Neighborhood code (حي) — 3 digits.</summary>
    public string NeighborhoodCode { get; init; } = string.Empty;

    /// <summary>Building number within the neighborhood — 5 digits.</summary>
    public string BuildingNumber { get; init; } = string.Empty;

    // ==================== LOCATION NAMES (ARABIC) ====================

    /// <summary>Governorate name in Arabic.</summary>
    public string GovernorateName { get; init; } = string.Empty;

    /// <summary>District name in Arabic.</summary>
    public string DistrictName { get; init; } = string.Empty;

    /// <summary>Sub-district name in Arabic.</summary>
    public string SubDistrictName { get; init; } = string.Empty;

    /// <summary>Community name in Arabic.</summary>
    public string CommunityName { get; init; } = string.Empty;

    /// <summary>Neighborhood name in Arabic.</summary>
    public string NeighborhoodName { get; init; } = string.Empty;

    // ==================== BUILDING ATTRIBUTES ====================

    /// <summary>
    /// Building type classification (نوع البناء).
    /// Matches the <c>building_type</c> column in the mobile SQLite DB.
    /// </summary>
    public BuildingType BuildingType { get; init; }

    /// <summary>
    /// Physical condition of the building (حالة البناء).
    /// Matches the <c>building_status</c> column in the mobile SQLite DB.
    /// </summary>
    public BuildingStatus BuildingStatus { get; init; }

    /// <summary>
    /// Total number of property units in the building (عدد الوحدات).
    /// Matches the <c>number_of_property_units</c> column in the mobile SQLite DB.
    /// </summary>
    public int NumberOfPropertyUnits { get; init; }

    /// <summary>
    /// Number of residential apartments (عدد الشقق).
    /// Matches the <c>number_of_apartments</c> column in the mobile SQLite DB.
    /// </summary>
    public int NumberOfApartments { get; init; }

    /// <summary>
    /// Number of commercial shops (عدد المحلات).
    /// Matches the <c>number_of_shops</c> column in the mobile SQLite DB.
    /// </summary>
    public int NumberOfShops { get; init; }

    /// <summary>Additional notes about the building.</summary>
    public string? Notes { get; init; }

    // ==================== SPATIAL DATA ====================

    /// <summary>
    /// Building polygon (or point) geometry in WKT format.
    /// Null when no geometry has been registered yet.
    /// </summary>
    public string? BuildingGeometryWkt { get; init; }

    /// <summary>
    /// GPS latitude of the building centroid (decimal degrees).
    /// For polygon buildings this is computed from the centroid; for
    /// point-only buildings it is the recorded GPS coordinate.
    /// Null when no location data exists.
    /// </summary>
    public decimal? Latitude { get; init; }

    /// <summary>
    /// GPS longitude of the building centroid (decimal degrees).
    /// Null when no location data exists.
    /// </summary>
    public decimal? Longitude { get; init; }

    // ==================== PROPERTY UNITS ====================

    /// <summary>
    /// Property units within this building that the field collector must survey.
    /// For full-building assignments this contains all active units;
    /// for revisit assignments only the units listed in <see cref="UnitsForRevisit"/>.
    /// </summary>
    public IReadOnlyList<SyncPropertyUnitDto> PropertyUnits { get; init; }
        = Array.Empty<SyncPropertyUnitDto>();
}
