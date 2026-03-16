using NetTopologySuite.Geometries;
using TRRCMS.Domain.Common;

namespace TRRCMS.Domain.Entities;

/// <summary>
/// Street reference entity — line features shown on the map
/// to help applicants recognize buildings and navigate neighborhoods.
/// Managed via QGIS plugin through the API.
/// شارع — خطوط مرجعية على الخريطة
/// </summary>
public class Street : BaseAuditableEntity
{
    /// <summary>
    /// Sequential integer identifier (managed by QGIS operator)
    /// </summary>
    public int Identifier { get; private set; }

    /// <summary>
    /// Street name (اسم الشارع)
    /// </summary>
    public string Name { get; private set; }
    /// <summary>
    /// Line geometry representing the street path (PostGIS LINESTRING, SRID 4326)
    /// </summary>
    public LineString? Geometry { get; private set; }

    /// <summary>
    /// Geometry as WKT string (computed, not stored)
    /// </summary>
    public string? GeometryWkt => Geometry?.AsText();
    /// <summary>
    /// EF Core constructor
    /// </summary>
    private Street() : base()
    {
        Name = string.Empty;
    }
    /// <summary>
    /// Create a new Street
    /// </summary>
    public static Street Create(
        int identifier,
        string name,
        LineString geometry,
        Guid createdByUserId)
    {
        geometry.SRID = 4326;

        var street = new Street
        {
            Identifier = identifier,
            Name = name,
            Geometry = geometry
        };

        street.MarkAsCreated(createdByUserId);
        return street;
    }
    /// <summary>
    /// Update street name
    /// </summary>
    public void UpdateName(string name, Guid modifiedByUserId)
    {
        Name = name;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Update street geometry
    /// </summary>
    public void UpdateGeometry(LineString geometry, Guid modifiedByUserId)
    {
        geometry.SRID = 4326;
        Geometry = geometry;
        MarkAsModified(modifiedByUserId);
    }
}
