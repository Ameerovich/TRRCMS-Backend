using TRRCMS.Domain.Common;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Domain.Entities;

/// <summary>
/// Property Unit entity - individual units within a building
/// </summary>
public class PropertyUnit : BaseAuditableEntity
{
    // ==================== IDENTIFIERS ====================

    /// <summary>
    /// Foreign key to parent building
    /// </summary>
    public Guid BuildingId { get; private set; }

    /// <summary>
    /// Unit identifier within building (e.g., "Apt 1", "Shop 2")
    /// </summary>
    public string UnitIdentifier { get; private set; }

    // ==================== UNIT ATTRIBUTES ====================

    /// <summary>
    /// Floor number where unit is located
    /// </summary>
    public int? FloorNumber { get; private set; }

    /// <summary>
    /// Property unit type (Apartment, Shop, Office, etc.)
    /// </summary>
    public PropertyUnitType UnitType { get; private set; }

    /// <summary>
    /// Property unit status (Occupied, Vacant, Damaged, etc.)
    /// </summary>
    public PropertyUnitStatus Status { get; private set; }

    /// <summary>
    /// Damage level for this unit
    /// </summary>
    public DamageLevel? DamageLevel { get; private set; }

    /// <summary>
    /// Area in square meters
    /// </summary>
    public decimal? AreaSquareMeters { get; private set; }

    /// <summary>
    /// Number of rooms (for residential units)
    /// </summary>
    public int? NumberOfRooms { get; private set; }

    /// <summary>
    /// Number of bathrooms
    /// </summary>
    public int? NumberOfBathrooms { get; private set; }

    /// <summary>
    /// Has balcony
    /// </summary>
    public bool? HasBalcony { get; private set; }

    // ==================== OCCUPANCY INFORMATION ====================

    /// <summary>
    /// Occupancy type (Owner-occupied, Tenant-occupied, etc.)
    /// </summary>
    public OccupancyType? OccupancyType { get; private set; }

    /// <summary>
    /// Occupancy nature (Legal, Informal, Customary, etc.)
    /// </summary>
    public OccupancyNature? OccupancyNature { get; private set; }

    /// <summary>
    /// Number of households in this unit
    /// </summary>
    public int? NumberOfHouseholds { get; private set; }

    /// <summary>
    /// Total occupants count
    /// </summary>
    public int? TotalOccupants { get; private set; }

    // ==================== ADDITIONAL INFORMATION ====================

    /// <summary>
    /// Unit description or notes
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Special features or characteristics
    /// </summary>
    public string? SpecialFeatures { get; private set; }

    // ==================== NAVIGATION PROPERTIES ====================

    /// <summary>
    /// Parent building
    /// </summary>
    public virtual Building Building { get; private set; } = null!;

    /// <summary>
    /// Households occupying this unit
    /// </summary>
    public virtual ICollection<Household> Households { get; private set; }

    /// <summary>
    /// Relations between persons and this property unit
    /// </summary>
    public virtual ICollection<PersonPropertyRelation> PersonRelations { get; private set; }

    /// <summary>
    /// Claims for this property unit
    /// </summary>
    public virtual ICollection<Claim> Claims { get; private set; }

    /// <summary>
    /// Documents related to this property unit
    /// </summary>
    public virtual ICollection<Document> Documents { get; private set; }

    /// <summary>
    /// Surveys for this specific unit
    /// </summary>
    public virtual ICollection<Survey> Surveys { get; private set; }

    /// <summary>
    /// Certificates issued for this unit
    /// </summary>
    public virtual ICollection<Certificate> Certificates { get; private set; }

    // ==================== CONSTRUCTORS ====================

    /// <summary>
    /// EF Core constructor
    /// </summary>
    private PropertyUnit() : base()
    {
        UnitIdentifier = string.Empty;
        Households = new List<Household>();
        PersonRelations = new List<PersonPropertyRelation>();
        Claims = new List<Claim>();
        Documents = new List<Document>();
        Surveys = new List<Survey>();
        Certificates = new List<Certificate>();
        Status = PropertyUnitStatus.Unknown;
    }

    /// <summary>
    /// Create new property unit
    /// </summary>
    public static PropertyUnit Create(
        Guid buildingId,
        string unitIdentifier,
        PropertyUnitType unitType,
        int? floorNumber,
        Guid createdByUserId)
    {
        var unit = new PropertyUnit
        {
            BuildingId = buildingId,
            UnitIdentifier = unitIdentifier,
            UnitType = unitType,
            FloorNumber = floorNumber,
            Status = PropertyUnitStatus.Unknown
        };

        unit.MarkAsCreated(createdByUserId);

        return unit;
    }

    // ==================== DOMAIN METHODS ====================

    /// <summary>
    /// Update unit status and damage level
    /// </summary>
    public void UpdateStatus(PropertyUnitStatus status, DamageLevel? damageLevel, Guid modifiedByUserId)
    {
        Status = status;
        DamageLevel = damageLevel;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Update unit area
    /// </summary>
    public void UpdateArea(decimal areaSquareMeters, Guid modifiedByUserId)
    {
        AreaSquareMeters = areaSquareMeters;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Update number of rooms
    /// </summary>
    public void UpdateRoomCount(int numberOfRooms, Guid modifiedByUserId)
    {
        NumberOfRooms = numberOfRooms;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Update unit physical details
    /// </summary>
    public void UpdatePhysicalDetails(
        int? numberOfRooms,
        int? numberOfBathrooms,
        bool? hasBalcony,
        decimal? areaSquareMeters,
        string? specialFeatures,
        Guid modifiedByUserId)
    {
        NumberOfRooms = numberOfRooms;
        NumberOfBathrooms = numberOfBathrooms;
        HasBalcony = hasBalcony;
        AreaSquareMeters = areaSquareMeters;
        SpecialFeatures = specialFeatures;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Update occupancy information
    /// </summary>
    public void UpdateOccupancyInfo(
        OccupancyType? occupancyType,
        OccupancyNature? occupancyNature,
        int? numberOfHouseholds,
        int? totalOccupants,
        Guid modifiedByUserId)
    {
        OccupancyType = occupancyType;
        OccupancyNature = occupancyNature;
        NumberOfHouseholds = numberOfHouseholds;
        TotalOccupants = totalOccupants;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Update unit description
    /// </summary>
    public void UpdateDescription(string? description, Guid modifiedByUserId)
    {
        Description = description;
        MarkAsModified(modifiedByUserId);
    }
}