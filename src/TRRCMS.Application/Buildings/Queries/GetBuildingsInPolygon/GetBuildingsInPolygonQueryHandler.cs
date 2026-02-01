using AutoMapper;
using MediatR;
using TRRCMS.Application.Buildings.Dtos;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;

namespace TRRCMS.Application.Buildings.Queries.GetBuildingsInPolygon;

/// <summary>
/// Handler for GetBuildingsInPolygonQuery
/// Uses repository for PostGIS spatial queries (Clean Architecture)
/// </summary>
public class GetBuildingsInPolygonQueryHandler 
    : IRequestHandler<GetBuildingsInPolygonQuery, GetBuildingsInPolygonResponse>
{
    private readonly IBuildingRepository _buildingRepository;
    private readonly IMapper _mapper;

    public GetBuildingsInPolygonQueryHandler(
        IBuildingRepository buildingRepository,
        IMapper mapper)
    {
        _buildingRepository = buildingRepository;
        _mapper = mapper;
    }

    public async Task<GetBuildingsInPolygonResponse> Handle(
        GetBuildingsInPolygonQuery request,
        CancellationToken cancellationToken)
    {
        // ============================================================
        // PARSE/VALIDATE POLYGON INPUT
        // ============================================================
        
        string polygonWkt;

        // Option 1: WKT string provided
        if (!string.IsNullOrWhiteSpace(request.PolygonWkt))
        {
            polygonWkt = request.PolygonWkt;
            
            if (!polygonWkt.Trim().ToUpper().StartsWith("POLYGON"))
            {
                throw new ValidationException("Invalid WKT format. Must start with POLYGON.");
            }
        }
        // Option 2: Coordinates array provided
        else if (request.Coordinates != null && request.Coordinates.Length >= 3)
        {
            try
            {
                var coords = request.Coordinates.ToList();
                
                // Ensure polygon is closed (first point = last point)
                if (coords[0][0] != coords[^1][0] || coords[0][1] != coords[^1][1])
                {
                    coords.Add(coords[0]);
                }

                var coordStrings = coords.Select(c => $"{c[0]} {c[1]}");
                polygonWkt = $"POLYGON(({string.Join(", ", coordStrings)}))";
            }
            catch (Exception ex)
            {
                throw new ValidationException($"Invalid coordinates format: {ex.Message}");
            }
        }
        else
        {
            throw new ValidationException("Either PolygonWkt or Coordinates (minimum 3 points) must be provided.");
        }

        // ============================================================
        // QUERY VIA REPOSITORY
        // ============================================================

        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 100 : (request.PageSize > 1000 ? 1000 : request.PageSize);

        try
        {
            var (buildings, totalCount) = await _buildingRepository.SearchBuildingsInPolygonAsync(
                polygonWkt: polygonWkt,
                buildingType: request.BuildingType,
                status: request.Status,
                damageLevel: request.DamageLevel,
                page: page,
                pageSize: pageSize,
                cancellationToken: cancellationToken);

            // ============================================================
            // MAP RESULTS
            // ============================================================

            var buildingDtos = buildings.Select(b => new BuildingInPolygonDto
            {
                Id = b.Id,
                BuildingId = b.BuildingId,
                BuildingIdFormatted = FormatBuildingId(b.BuildingId),
                Latitude = b.Latitude,
                Longitude = b.Longitude,
                BuildingType = b.BuildingType.ToString(),
                Status = b.Status.ToString(),
                DamageLevel = b.DamageLevel?.ToString(),
                NumberOfPropertyUnits = b.NumberOfPropertyUnits,
                NeighborhoodName = b.NeighborhoodName,
                CommunityName = b.CommunityName,
                BuildingGeometryWkt = request.IncludeFullDetails ? b.BuildingGeometryWkt : null,
                FullDetails = request.IncludeFullDetails ? _mapper.Map<BuildingDto>(b) : null
            }).ToList();

            double? polygonArea = CalculateApproximateArea(polygonWkt);

            return new GetBuildingsInPolygonResponse
            {
                Buildings = buildingDtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                PolygonWkt = polygonWkt,
                PolygonAreaSquareMeters = polygonArea
            };
        }
        catch (ValidationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ValidationException($"Invalid polygon geometry: {ex.Message}");
        }
    }

    private static string FormatBuildingId(string buildingId)
    {
        if (string.IsNullOrEmpty(buildingId) || buildingId.Length != 17)
            return buildingId;

        return $"{buildingId[..2]}-{buildingId[2..4]}-{buildingId[4..6]}-" +
               $"{buildingId[6..9]}-{buildingId[9..12]}-{buildingId[12..17]}";
    }

    private static double? CalculateApproximateArea(string polygonWkt)
    {
        try
        {
            var coordsStr = polygonWkt
                .Replace("POLYGON((", "")
                .Replace("POLYGON ((", "")
                .Replace("))", "")
                .Trim();

            var points = coordsStr.Split(',')
                .Select(p => p.Trim().Split(' '))
                .Select(p => (Lng: double.Parse(p[0]), Lat: double.Parse(p[1])))
                .ToList();

            if (points.Count < 3) return null;

            var minLat = points.Min(p => p.Lat);
            var maxLat = points.Max(p => p.Lat);
            var avgLat = (minLat + maxLat) / 2;

            var metersPerDegreeLat = 111139.0;
            var metersPerDegreeLng = 111139.0 * Math.Cos(avgLat * Math.PI / 180);

            double areaInDegrees = 0;
            for (int i = 0; i < points.Count - 1; i++)
            {
                areaInDegrees += points[i].Lng * points[i + 1].Lat;
                areaInDegrees -= points[i + 1].Lng * points[i].Lat;
            }
            areaInDegrees = Math.Abs(areaInDegrees) / 2;

            return areaInDegrees * metersPerDegreeLat * metersPerDegreeLng;
        }
        catch
        {
            return null;
        }
    }
}
