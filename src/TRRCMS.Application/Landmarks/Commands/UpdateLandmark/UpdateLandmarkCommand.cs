using MediatR;
using TRRCMS.Application.Landmarks.Dtos;

namespace TRRCMS.Application.Landmarks.Commands.UpdateLandmark;

/// <summary>
/// Update an existing landmark (name, type, location).
/// تعديل معلم موجود
/// </summary>
public record UpdateLandmarkCommand : IRequest<LandmarkDto>
{
    /// <summary>Landmark ID (GUID)</summary>
    public Guid Id { get; init; }

    /// <summary>Updated landmark name</summary>
    /// <example>جامع الأموي الكبير</example>
    public string Name { get; init; } = string.Empty;

    /// <summary>Updated landmark type</summary>
    /// <example>2</example>
    public int Type { get; init; }

    /// <summary>
    /// Updated point geometry in WKT format (optional — omit to keep current location).
    /// </summary>
    /// <example>POINT(37.1340 36.2018)</example>
    public string? LocationWkt { get; init; }
}
