using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using TRRCMS.Application.Common.Interfaces;

namespace TRRCMS.Infrastructure.Services;

/// <summary>
/// Converts between WKT strings and PostGIS geometry objects.
/// Centralises WKTReader / GeometryFactory instantiation so domain entities
/// remain free of spatial-library infrastructure concerns.
/// </summary>
public class GeometryConverter : IGeometryConverter
{
    private static readonly GeometryFactory Factory =
        new(new PrecisionModel(), 4326);

    private static readonly WKTReader Reader = new(Factory);

    /// <inheritdoc />
    public Geometry? ParseWkt(string? wkt, int srid = 4326)
    {
        if (string.IsNullOrWhiteSpace(wkt))
            return null;

        var geometry = Reader.Read(wkt);
        geometry.SRID = srid;
        return geometry;
    }

    /// <inheritdoc />
    public Point CreatePoint(double longitude, double latitude, int srid = 4326)
    {
        var point = Factory.CreatePoint(new Coordinate(longitude, latitude));
        point.SRID = srid;
        return point;
    }
}
