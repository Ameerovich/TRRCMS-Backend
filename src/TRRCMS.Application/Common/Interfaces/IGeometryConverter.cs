using NetTopologySuite.Geometries;

namespace TRRCMS.Application.Common.Interfaces;

/// <summary>
/// Domain service for converting between WKT strings and PostGIS geometry objects.
/// Keeps spatial library instantiation (WKTReader, GeometryFactory) out of domain entities.
/// </summary>
public interface IGeometryConverter
{
    /// <summary>
    /// Parse a WKT (Well-Known Text) string into a Geometry with the given SRID.
    /// Returns null for null/empty input.
    /// </summary>
    Geometry? ParseWkt(string? wkt, int srid = 4326);

    /// <summary>
    /// Create a Point geometry from longitude/latitude coordinates.
    /// </summary>
    Point CreatePoint(double longitude, double latitude, int srid = 4326);
}
