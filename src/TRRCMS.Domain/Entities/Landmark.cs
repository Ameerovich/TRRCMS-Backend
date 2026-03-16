using NetTopologySuite.Geometries;
using TRRCMS.Domain.Common;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Domain.Entities;

/// <summary>
/// Landmark reference entity — characteristic point features shown on the map
/// to help applicants recognize buildings (mosques, schools, shops, etc.).
/// Managed via QGIS plugin through the API.
/// معلم — نقاط مرجعية على الخريطة
/// </summary>
public class Landmark : BaseAuditableEntity
{
    /// <summary>
    /// Sequential integer identifier (managed by QGIS operator)
    /// </summary>
    public int Identifier { get; private set; }

    /// <summary>
    /// Landmark name (اسم المعلم)
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Landmark type classification (نوع المعلم)
    /// </summary>
    public LandmarkType Type { get; private set; }
    /// <summary>
    /// Point location (PostGIS POINT, SRID 4326)
    /// </summary>
    public Point? Location { get; private set; }

    /// <summary>
    /// Latitude (convenience — matches Location.Y)
    /// </summary>
    public decimal Latitude { get; private set; }

    /// <summary>
    /// Longitude (convenience — matches Location.X)
    /// </summary>
    public decimal Longitude { get; private set; }
    /// <summary>
    /// EF Core constructor
    /// </summary>
    private Landmark() : base()
    {
        Name = string.Empty;
    }
    /// <summary>
    /// Create a new Landmark
    /// </summary>
    public static Landmark Create(
        int identifier,
        string name,
        LandmarkType type,
        Point location,
        Guid createdByUserId)
    {
        location.SRID = 4326;

        var landmark = new Landmark
        {
            Identifier = identifier,
            Name = name,
            Type = type,
            Location = location,
            Latitude = (decimal)location.Y,
            Longitude = (decimal)location.X
        };

        landmark.MarkAsCreated(createdByUserId);
        return landmark;
    }
    /// <summary>
    /// Update landmark details
    /// </summary>
    public void Update(
        string name,
        LandmarkType type,
        Guid modifiedByUserId)
    {
        Name = name;
        Type = type;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Update landmark location
    /// </summary>
    public void UpdateLocation(Point location, Guid modifiedByUserId)
    {
        location.SRID = 4326;
        Location = location;
        Latitude = (decimal)location.Y;
        Longitude = (decimal)location.X;
        MarkAsModified(modifiedByUserId);
    }
}
