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
    /// Position on floor (e.g., "Left", "Right", "Center", "يمين", "يسار")
    /// Added for Day 2 - Survey workflow
    /// </summary>
    public string? PositionOnFloor { get; private set; }

    /// <summary>
    /// Property unit type (Apartment, Shop, Office, etc.)
    /// </summary>
    public PropertyUnitType UnitType { get; private set; }

    /// <summary>
    /// Property unit status (Occupied, Vacant, Damaged, etc.)
    /// </summary>
    public PropertyUnitStatus Status { get; private set; }

    /// <summary>
    /// Occupancy status as string (for flexible field survey data)
    /// Added for Day 2 - Survey workflow to complement Status enum
    /// </summary>
    public string? OccupancyStatus { get; private set; }

    /// <summary>
    /// Damage level for this unit
    /// </summary>
    public DamageLevel? DamageLevel { get; private set; }

    /// <summary>
    /// Area in square meters
    /// </summary>
    public decimal? AreaSquareMeters { get; private set; }

    /// <summary>
    /// Estimated area in square meters (for field survey estimates)
    /// Added for Day 2 - Survey workflow to complement AreaSquareMeters
    /// </summary>
    public decimal? EstimatedAreaSqm { get; private set; }

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

    // ==================== UTILITIES (Added for Day 2) ====================

    /// <summary>
    /// Has electricity connection
    /// Added for Day 2 - Survey workflow
    /// </summary>
    public bool HasElectricity { get; private set; }

    /// <summary>
    /// Has water connection
    /// Added for Day 2 - Survey workflow
    /// </summary>
    public bool HasWater { get; private set; }

    /// <summary>
    /// Has sewage connection
    /// Added for Day 2 - Survey workflow
    /// </summary>
    public bool HasSewage { get; private set; }

    /// <summary>
    /// Additional utilities notes
    /// Added for Day 2 - Survey workflow
    /// </summary>
    public string? UtilitiesNotes { get; private set; }

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
        // Initialize Day 2 fields
        HasElectricity = false;
        HasWater = false;
        HasSewage = false;
    }

    /// <summary>
    /// Create new property unit (original method for Building Management)
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
            Status = PropertyUnitStatus.Unknown,
            HasElectricity = false,
            HasWater = false,
            HasSewage = false
        };

        unit.MarkAsCreated(createdByUserId);

        return unit;
    }

    /// <summary>
    /// Create new property unit with string type (for Survey workflow - Day 2)
    /// Overload for field surveys where unit type is entered as text
    /// </summary>
    public static PropertyUnit Create(
        Guid buildingId,
        string unitIdentifier,
        string unitType,
        int? floorNumber,
        string? positionOnFloor,
        Guid createdByUserId)
    {
        // Parse unitType string to enum, default to Unknown if can't parse
        PropertyUnitType parsedUnitType = PropertyUnitType.Other;
        if (!string.IsNullOrWhiteSpace(unitType))
        {
            Enum.TryParse<PropertyUnitType>(unitType, ignoreCase: true, out parsedUnitType);
        }

        var unit = new PropertyUnit
        {
            BuildingId = buildingId,
            UnitIdentifier = unitIdentifier,
            UnitType = parsedUnitType,
            FloorNumber = floorNumber,
            PositionOnFloor = positionOnFloor,
            Status = PropertyUnitStatus.Unknown,
            HasElectricity = false,
            HasWater = false,
            HasSewage = false
        };

        unit.MarkAsCreated(createdByUserId);

        return unit;
    }

    // ==================== ORIGINAL DOMAIN METHODS ====================

    /// <summary>
    /// Re-parent this property unit to a different building (used during building merge).
    /// Preserves all unit details; only changes the parent building FK.
    /// </summary>
    public void ReParentToBuilding(Guid newBuildingId, Guid modifiedByUserId)
    {
        BuildingId = newBuildingId;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Update unit identifier
    /// </summary>
    public void UpdateUnitIdentifier(string unitIdentifier, Guid modifiedByUserId)
    {
        UnitIdentifier = unitIdentifier;
        MarkAsModified(modifiedByUserId);
    }

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

    // ==================== DAY 2 METHODS (new - for Survey workflow) ====================

    /// <summary>
    /// Update property unit location
    /// Used in field surveys to update floor and position information
    /// </summary>
    public void UpdateLocation(
        int? floorNumber,
        string? positionOnFloor,
        Guid modifiedByUserId)
    {
        if (floorNumber.HasValue)
            FloorNumber = floorNumber.Value;

        if (positionOnFloor != null)
            PositionOnFloor = positionOnFloor;

        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Update property unit details
    /// Used in field surveys to update occupancy, rooms, area, and description
    /// </summary>
    public void UpdateDetails(
        string? occupancyStatus,
        int? numberOfRooms,
        decimal? estimatedAreaSqm,
        string? description,
        Guid modifiedByUserId)
    {
        if (occupancyStatus != null)
            OccupancyStatus = occupancyStatus;

        if (numberOfRooms.HasValue)
            NumberOfRooms = numberOfRooms.Value;

        if (estimatedAreaSqm.HasValue)
            EstimatedAreaSqm = estimatedAreaSqm.Value;

        if (description != null)
            Description = description;

        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Update utilities information
    /// Used in field surveys to capture utility connections
    /// </summary>
    public void UpdateUtilities(
        bool hasElectricity,
        bool hasWater,
        bool hasSewage,
        string? utilitiesNotes,
        Guid modifiedByUserId)
    {
        HasElectricity = hasElectricity;
        HasWater = hasWater;
        HasSewage = hasSewage;
        UtilitiesNotes = utilitiesNotes;

        MarkAsModified(modifiedByUserId);
    }
}