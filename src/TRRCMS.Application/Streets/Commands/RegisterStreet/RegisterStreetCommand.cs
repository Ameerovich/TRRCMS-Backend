using MediatR;
using TRRCMS.Application.Streets.Dtos;

namespace TRRCMS.Application.Streets.Commands.RegisterStreet;

/// <summary>
/// Register a new street from QGIS plugin.
/// تسجيل شارع جديد من خلال QGIS
/// </summary>
public record RegisterStreetCommand : IRequest<StreetDto>
{
    /// <summary>Sequential identifier (managed by QGIS operator)</summary>
    /// <example>1</example>
    public int Identifier { get; init; }

    /// <summary>Street name (اسم الشارع)</summary>
    /// <example>شارع النصر</example>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// LineString geometry in WKT format (from QGIS digitization).
    /// Coordinates in longitude latitude (X Y) order, SRID 4326 (WGS84).
    /// </summary>
    /// <example>LINESTRING(37.1340 36.2018, 37.1350 36.2025, 37.1360 36.2030)</example>
    public string GeometryWkt { get; init; } = string.Empty;
}
