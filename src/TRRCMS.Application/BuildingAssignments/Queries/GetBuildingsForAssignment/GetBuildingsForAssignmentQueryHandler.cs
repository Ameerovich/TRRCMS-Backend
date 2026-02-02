using MediatR;
using TRRCMS.Application.BuildingAssignments.Dtos;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Application.BuildingAssignments.Queries.GetBuildingsForAssignment;

/// <summary>
/// Handler for GetBuildingsForAssignmentQuery
/// UC-012: S01-S03 - Search and select buildings for assignment
/// Supports: Text search, administrative hierarchy, radius search, AND polygon search
/// </summary>
public class GetBuildingsForAssignmentQueryHandler 
    : IRequestHandler<GetBuildingsForAssignmentQuery, BuildingsForAssignmentPagedResult>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetBuildingsForAssignmentQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<BuildingsForAssignmentPagedResult> Handle(
        GetBuildingsForAssignmentQuery request, 
        CancellationToken cancellationToken)
    {
        List<Building> buildings;
        int totalCount;
        string? polygonWkt = null;
        double? polygonArea = null;

        // Determine search mode: Polygon vs Regular (with optional radius)
        bool usePolygonSearch = !string.IsNullOrWhiteSpace(request.PolygonWkt) || 
                                (request.Coordinates != null && request.Coordinates.Length >= 3);

        if (usePolygonSearch)
        {
            // ============================================================
            // POLYGON SEARCH MODE
            // ============================================================
            polygonWkt = ParsePolygonInput(request.PolygonWkt, request.Coordinates);
            polygonArea = CalculateApproximateArea(polygonWkt);

            // Validate and cap page size for polygon search
            var pageSize = Math.Min(request.PageSize, 1000); // Cap at 1000 for polygon

            var (polygonBuildings, polygonTotalCount) = await _unitOfWork.Buildings.SearchBuildingsInPolygonAsync(
                polygonWkt: polygonWkt,
                buildingType: request.BuildingType,
                status: request.BuildingStatus,
                page: request.Page,
                pageSize: pageSize,
                cancellationToken: cancellationToken
            );

            buildings = polygonBuildings;
            totalCount = polygonTotalCount;

            // Apply additional filters that aren't supported by SearchBuildingsInPolygonAsync
            if (!string.IsNullOrWhiteSpace(request.GovernorateCode))
                buildings = buildings.Where(b => b.GovernorateCode == request.GovernorateCode).ToList();
            
            if (!string.IsNullOrWhiteSpace(request.DistrictCode))
                buildings = buildings.Where(b => b.DistrictCode == request.DistrictCode).ToList();
            
            if (!string.IsNullOrWhiteSpace(request.SubDistrictCode))
                buildings = buildings.Where(b => b.SubDistrictCode == request.SubDistrictCode).ToList();
            
            if (!string.IsNullOrWhiteSpace(request.CommunityCode))
                buildings = buildings.Where(b => b.CommunityCode == request.CommunityCode).ToList();
            
            if (!string.IsNullOrWhiteSpace(request.NeighborhoodCode))
                buildings = buildings.Where(b => b.NeighborhoodCode == request.NeighborhoodCode).ToList();

            if (!string.IsNullOrWhiteSpace(request.BuildingCode))
            {
                var normalizedCode = request.BuildingCode.Replace("-", "");
                buildings = buildings.Where(b => b.BuildingId.Contains(normalizedCode)).ToList();
            }

            if (!string.IsNullOrWhiteSpace(request.Address))
                buildings = buildings.Where(b => b.Address != null && b.Address.Contains(request.Address)).ToList();

            // Update total count after additional filtering
            totalCount = buildings.Count;
        }
        else
        {
            // ============================================================
            // REGULAR SEARCH MODE (with optional radius)
            // ============================================================
            var (searchBuildings, searchTotalCount) = await _unitOfWork.Buildings.SearchBuildingsAsync(
                governorateCode: request.GovernorateCode,
                districtCode: request.DistrictCode,
                subDistrictCode: request.SubDistrictCode,
                communityCode: request.CommunityCode,
                neighborhoodCode: request.NeighborhoodCode,
                buildingId: request.BuildingCode,
                address: request.Address,
                latitude: request.Latitude,
                longitude: request.Longitude,
                radiusMeters: request.RadiusMeters,
                status: request.BuildingStatus,
                buildingType: request.BuildingType,
                page: request.Page,
                pageSize: request.PageSize,
                sortBy: request.SortBy,
                sortDescending: request.SortDescending,
                cancellationToken: cancellationToken
            );

            buildings = searchBuildings;
            totalCount = searchTotalCount;
        }

        // ============================================================
        // GET ASSIGNMENT STATUS FOR BUILDINGS
        // ============================================================
        var buildingIds = buildings.Select(b => b.Id).ToList();
        var activeAssignments = new Dictionary<Guid, (Guid AssignmentId, Guid CollectorId, string CollectorName)>();
        
        foreach (var buildingId in buildingIds)
        {
            var assignment = await _unitOfWork.BuildingAssignments
                .GetActiveAssignmentForBuildingAsync(buildingId, cancellationToken);
            
            if (assignment != null)
            {
                var collector = await _unitOfWork.Users.GetByIdAsync(assignment.FieldCollectorId, cancellationToken);
                activeAssignments[buildingId] = (
                    assignment.Id, 
                    assignment.FieldCollectorId, 
                    collector?.FullNameArabic ?? "Unknown"
                );
            }
        }

        // ============================================================
        // FILTER BY ASSIGNMENT STATUS (if requested)
        // ============================================================
        if (request.HasActiveAssignment.HasValue)
        {
            if (request.HasActiveAssignment.Value)
            {
                // Only buildings with active assignments
                buildings = buildings.Where(b => activeAssignments.ContainsKey(b.Id)).ToList();
            }
            else
            {
                // Only buildings without active assignments
                buildings = buildings.Where(b => !activeAssignments.ContainsKey(b.Id)).ToList();
            }
            
            totalCount = buildings.Count;
        }

        // ============================================================
        // MAP TO DTOs
        // ============================================================
        var items = buildings.Select(b =>
        {
            var hasAssignment = activeAssignments.TryGetValue(b.Id, out var assignmentInfo);
            
            return new BuildingForAssignmentDto
            {
                Id = b.Id,
                BuildingCode = b.BuildingId,
                Address = b.Address,
                GovernorateCode = b.GovernorateCode,
                GovernorateName = b.GovernorateName,
                DistrictCode = b.DistrictCode,
                DistrictName = b.DistrictName,
                SubDistrictCode = b.SubDistrictCode,
                SubDistrictName = b.SubDistrictName,
                CommunityCode = b.CommunityCode,
                CommunityName = b.CommunityName,
                NeighborhoodCode = b.NeighborhoodCode,
                NeighborhoodName = b.NeighborhoodName,
                NumberOfPropertyUnits = b.NumberOfPropertyUnits,
                BuildingType = b.BuildingType.ToString(),
                BuildingStatus = b.Status.ToString(),
                Latitude = b.Latitude,
                Longitude = b.Longitude,
                HasActiveAssignment = hasAssignment,
                CurrentAssignmentId = hasAssignment ? assignmentInfo.AssignmentId : null,
                CurrentAssigneeId = hasAssignment ? assignmentInfo.CollectorId.ToString() : null,
                CurrentAssigneeName = hasAssignment ? assignmentInfo.CollectorName : null
            };
        }).ToList();

        return new BuildingsForAssignmentPagedResult
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            PolygonWkt = polygonWkt,
            PolygonAreaSquareMeters = polygonArea
        };
    }

    /// <summary>
    /// Parse polygon input from WKT or coordinates array
    /// </summary>
    private static string ParsePolygonInput(string? polygonWkt, double[][]? coordinates)
    {
        // Option 1: WKT string provided
        if (!string.IsNullOrWhiteSpace(polygonWkt))
        {
            if (!polygonWkt.Trim().ToUpper().StartsWith("POLYGON"))
            {
                throw new ValidationException("Invalid WKT format. Must start with POLYGON.");
            }
            return polygonWkt;
        }

        // Option 2: Coordinates array provided
        if (coordinates != null && coordinates.Length >= 3)
        {
            try
            {
                var coords = coordinates.ToList();
                
                // Validate each coordinate has at least 2 values (lng, lat)
                foreach (var coord in coords)
                {
                    if (coord == null || coord.Length < 2)
                    {
                        throw new ValidationException("Each coordinate must have at least 2 values [longitude, latitude].");
                    }
                }

                // Ensure polygon is closed (first point = last point)
                if (coords[0][0] != coords[^1][0] || coords[0][1] != coords[^1][1])
                {
                    coords.Add(coords[0]);
                }

                // Build WKT string - format: "lng lat, lng lat, ..."
                var coordStrings = coords.Select(c => $"{c[0]} {c[1]}");
                return $"POLYGON(({string.Join(", ", coordStrings)}))";
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ValidationException($"Invalid coordinates format: {ex.Message}");
            }
        }

        throw new ValidationException("Either PolygonWkt or Coordinates (minimum 3 points) must be provided for polygon search.");
    }

    /// <summary>
    /// Calculate approximate polygon area in square meters using Shoelace formula
    /// </summary>
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

            // Get center latitude for coordinate conversion
            var minLat = points.Min(p => p.Lat);
            var maxLat = points.Max(p => p.Lat);
            var avgLat = (minLat + maxLat) / 2;

            // Meters per degree at this latitude
            var metersPerDegreeLat = 111139.0;
            var metersPerDegreeLng = 111139.0 * Math.Cos(avgLat * Math.PI / 180);

            // Shoelace formula for area calculation
            double areaInDegrees = 0;
            for (int i = 0; i < points.Count - 1; i++)
            {
                areaInDegrees += points[i].Lng * points[i + 1].Lat;
                areaInDegrees -= points[i + 1].Lng * points[i].Lat;
            }
            areaInDegrees = Math.Abs(areaInDegrees) / 2;

            // Convert to square meters
            return areaInDegrees * metersPerDegreeLat * metersPerDegreeLng;
        }
        catch
        {
            return null;
        }
    }
}
