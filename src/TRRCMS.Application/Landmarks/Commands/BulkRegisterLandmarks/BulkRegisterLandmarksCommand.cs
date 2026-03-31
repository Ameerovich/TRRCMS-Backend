using MediatR;
using TRRCMS.Application.Common.Models;
using TRRCMS.Application.Landmarks.Dtos;

namespace TRRCMS.Application.Landmarks.Commands.BulkRegisterLandmarks;

/// <summary>
/// Bulk register landmarks (from QGIS plugin or desktop).
/// Accepts an array of landmarks and processes them in a single transaction.
/// Duplicates by Identifier are skipped.
/// </summary>
public record BulkRegisterLandmarksCommand : IRequest<BulkOperationResult<LandmarkDto>>
{
    public List<LandmarkItem> Landmarks { get; init; } = new();
}

public record LandmarkItem
{
    /// <summary>Sequential identifier (managed by QGIS operator)</summary>
    public int Identifier { get; init; }

    /// <summary>Landmark name</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Landmark type (1=PoliceStation, 2=Mosque, etc.)</summary>
    public int Type { get; init; }

    /// <summary>Point geometry in WKT format, SRID 4326</summary>
    public string LocationWkt { get; init; } = string.Empty;
}
