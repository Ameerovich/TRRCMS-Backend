using MediatR;
using TRRCMS.Application.Landmarks.Dtos;

namespace TRRCMS.Application.Landmarks.Commands.RegisterLandmark;

/// <summary>
/// Register a new landmark from QGIS plugin.
/// تسجيل معلم جديد من خلال QGIS
/// </summary>
public record RegisterLandmarkCommand : IRequest<LandmarkDto>
{
    /// <summary>Sequential identifier (managed by QGIS operator)</summary>
    /// <example>1</example>
    public int Identifier { get; init; }

    /// <summary>Landmark name (اسم المعلم)</summary>
    /// <example>جامع الأموي</example>
    public string Name { get; init; } = string.Empty;

    /// <summary>Landmark type (1=PoliceStation, 2=Mosque, 3=PublicBuilding, 4=Shop, 5=School, 6=Clinic, 7=WaterTank, 8=FuelStation)</summary>
    /// <example>2</example>
    public int Type { get; init; }

    /// <summary>
    /// Point geometry in WKT format (from QGIS digitization).
    /// Coordinates in longitude latitude (X Y) order, SRID 4326 (WGS84).
    /// </summary>
    /// <example>POINT(37.1340 36.2018)</example>
    public string LocationWkt { get; init; } = string.Empty;
}
