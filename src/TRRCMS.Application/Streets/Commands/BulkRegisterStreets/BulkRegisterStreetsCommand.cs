using MediatR;
using TRRCMS.Application.Common.Models;
using TRRCMS.Application.Streets.Dtos;

namespace TRRCMS.Application.Streets.Commands.BulkRegisterStreets;

/// <summary>
/// Bulk register streets (from QGIS plugin or desktop).
/// Accepts an array of streets and processes them in a single transaction.
/// Duplicates by Identifier are skipped.
/// </summary>
public record BulkRegisterStreetsCommand : IRequest<BulkOperationResult<StreetDto>>
{
    public List<StreetItem> Streets { get; init; } = new();
}

public record StreetItem
{
    /// <summary>Sequential identifier (managed by QGIS operator)</summary>
    public int Identifier { get; init; }

    /// <summary>Street name</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>LineString geometry in WKT format, SRID 4326</summary>
    public string GeometryWkt { get; init; } = string.Empty;
}
