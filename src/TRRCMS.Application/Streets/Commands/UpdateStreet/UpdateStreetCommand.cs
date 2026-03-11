using MediatR;
using TRRCMS.Application.Streets.Dtos;

namespace TRRCMS.Application.Streets.Commands.UpdateStreet;

/// <summary>
/// Update an existing street (name, geometry).
/// تعديل شارع موجود
/// </summary>
public record UpdateStreetCommand : IRequest<StreetDto>
{
    /// <summary>Street ID (GUID)</summary>
    public Guid Id { get; init; }

    /// <summary>Updated street name</summary>
    /// <example>شارع النصر الكبير</example>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Updated linestring geometry in WKT format (optional — omit to keep current geometry).
    /// </summary>
    /// <example>LINESTRING(37.1340 36.2018, 37.1350 36.2025)</example>
    public string? GeometryWkt { get; init; }
}
