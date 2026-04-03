using TRRCMS.Domain.Common;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Domain.Entities;

/// <summary>
/// Property Unit entity - individual units within a building
/// </summary>
public class PropertyUnit : BaseAuditableEntity
{
    /// <summary>
    /// Foreign key to parent building
    /// </summary>
    public Guid BuildingId { get; private set; }

    /// <summary>
    /// Unit identifier within building (e.g., "Apt 1", "Shop 2")
    /// </summary>
    public string UnitIdentifier { get; private set; }
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
    /// Area in square meters
    /// </summary>
    public decimal? AreaSquareMeters { get; private set; }

    /// <summary>
    /// Number of rooms (for residential units)
    /// </summary>
    public int? NumberOfRooms { get; private set; }
    /// <summary>
    /// Unit description or notes
    /// </summary>
    public string? Description { get; private set; }
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
    /// Surveys for this specific unit
    /// </summary>
    public virtual ICollection<Survey> Surveys { get; private set; }
    /// <summary>
    /// EF Core constructor
    /// </summary>
    private PropertyUnit() : base()
    {
        UnitIdentifier = string.Empty;
        Households = new List<Household>();
        PersonRelations = new List<PersonPropertyRelation>();
        Claims = new List<Claim>();
        Surveys = new List<Survey>();
        Status = PropertyUnitStatus.Unknown;
    }

    /// <summary>
    /// Create new property unit (original method for Building Management)
    /// </summary>
    public static PropertyUnit Create(
        Guid buildingId,
        string unitIdentifier,
        PropertyUnitType unitType,
        int? floorNumber,
        Guid createdByUserId,
        PropertyUnitStatus? status = null,
        int? numberOfRooms = null,
        decimal? areaSquareMeters = null,
        string? description = null)
    {
        var unit = new PropertyUnit
        {
            BuildingId = buildingId,
            UnitIdentifier = unitIdentifier,
            UnitType = unitType,
            FloorNumber = floorNumber,
            Status = status ?? PropertyUnitStatus.Unknown,
            NumberOfRooms = numberOfRooms,
            AreaSquareMeters = areaSquareMeters,
            Description = description
        };

        unit.MarkAsCreated(createdByUserId);

        return unit;
    }

    /// <summary>
    /// Create new property unit with string type.
    /// Overload for field surveys where unit type is entered as text.
    /// </summary>
    public static PropertyUnit Create(
        Guid buildingId,
        string unitIdentifier,
        string unitType,
        int? floorNumber,
        Guid createdByUserId)
    {
        if (string.IsNullOrWhiteSpace(unitType))
            throw new ArgumentException("Unit type is required.", nameof(unitType));

        if (!Enum.TryParse<PropertyUnitType>(unitType, ignoreCase: true, out var parsedUnitType))
            throw new ArgumentException($"Invalid unit type '{unitType}'. Valid values: {string.Join(", ", Enum.GetNames<PropertyUnitType>())}", nameof(unitType));

        var unit = new PropertyUnit
        {
            BuildingId = buildingId,
            UnitIdentifier = unitIdentifier,
            UnitType = parsedUnitType,
            FloorNumber = floorNumber,
            Status = PropertyUnitStatus.Unknown
        };

        unit.MarkAsCreated(createdByUserId);

        return unit;
    }
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
    /// Update unit type (نوع الوحدة)
    /// </summary>
    public void UpdateUnitType(PropertyUnitType unitType, Guid modifiedByUserId)
    {
        UnitType = unitType;
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
    /// Update unit status
    /// </summary>
    public void UpdateStatus(PropertyUnitStatus status, Guid modifiedByUserId)
    {
        Status = status;
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
        decimal? areaSquareMeters,
        Guid modifiedByUserId)
    {
        NumberOfRooms = numberOfRooms;
        AreaSquareMeters = areaSquareMeters;
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
    /// <summary>
    /// Update property unit location (floor number)
    /// </summary>
    public void UpdateLocation(
        int? floorNumber,
        Guid modifiedByUserId)
    {
        if (floorNumber.HasValue)
            FloorNumber = floorNumber.Value;

        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Update property unit details (rooms, description)
    /// </summary>
    public void UpdateDetails(
        int? numberOfRooms,
        string? description,
        Guid modifiedByUserId)
    {
        if (numberOfRooms.HasValue)
            NumberOfRooms = numberOfRooms.Value;

        if (description != null)
            Description = description;

        MarkAsModified(modifiedByUserId);
    }
}