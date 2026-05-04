using MediatR;
using TRRCMS.Application.BuildingAssignments.Dtos;
using TRRCMS.Application.Common;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Models;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Application.BuildingAssignments.Queries.GetBuildingsForAssignment;

/// <summary>
/// Handler for GetBuildingsForAssignmentQuery.
/// Supports text search, administrative hierarchy, radius search, and polygon search.
/// </summary>
public class GetBuildingsForAssignmentQueryHandler 
    : IRequestHandler<GetBuildingsForAssignmentQuery, BuildingsForAssignmentPagedResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICommunityRepository _communityRepository;

    public GetBuildingsForAssignmentQueryHandler(
        IUnitOfWork unitOfWork,
        ICommunityRepository communityRepository)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _communityRepository = communityRepository ?? throw new ArgumentNullException(nameof(communityRepository));
    }

    public async Task<BuildingsForAssignmentPagedResult> Handle(
        GetBuildingsForAssignmentQuery request,
        CancellationToken cancellationToken)
    {
        // Normalize OCHA pCode filters → raw numeric codes.
        var (govCode, distCode, subDistCode) = OchaCommandNormalizer.ResolveAdmCodes(
            request.GovernorateCode, request.DistrictCode, request.SubDistrictCode,
            request.GovernoratePCode, request.DistrictPCode, request.SubDistrictPCode);
        var neighCode = OchaCommandNormalizer.ResolveNeighborhoodCode(
            request.NeighborhoodCode, request.NeighborhoodPCode);
        var commCode = request.CommunityCode;
        var commPCodeNorm = OchaCommandNormalizer.NormalizeCommunityPCode(request.CommunityPCode);
        if (commPCodeNorm != null)
        {
            var matched = await _communityRepository.GetByExternalPCodeAsync(
                commPCodeNorm,
                string.IsNullOrWhiteSpace(govCode) ? null : govCode,
                string.IsNullOrWhiteSpace(distCode) ? null : distCode,
                string.IsNullOrWhiteSpace(subDistCode) ? null : subDistCode,
                cancellationToken);
            if (matched != null)
            {
                commCode = matched.Code;
                govCode = matched.GovernorateCode;
                distCode = matched.DistrictCode;
                subDistCode = matched.SubDistrictCode;
            }
            else
            {
                return new BuildingsForAssignmentPagedResult
                {
                    Items = new List<BuildingForAssignmentDto>(),
                    TotalCount = 0,
                    Page = request.Page,
                    PageSize = request.PageSize
                };
            }
        }

        List<Building> buildings;
        int totalCount;
        string? polygonWkt = null;
        double? polygonArea = null;

        // Determine search mode: Polygon vs Regular (with optional radius)
        bool usePolygonSearch = !string.IsNullOrWhiteSpace(request.PolygonWkt) || 
                                (request.Coordinates != null && request.Coordinates.Length >= 3);

        if (usePolygonSearch)
        {
            polygonWkt = ParsePolygonInput(request.PolygonWkt, request.Coordinates);
            polygonArea = CalculateApproximateArea(polygonWkt);

            // Validate and cap page size for polygon search
            var pageSize = PagedQuery.ClampPageSize(request.PageSize);

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
            if (!string.IsNullOrWhiteSpace(govCode))
                buildings = buildings.Where(b => b.GovernorateCode == govCode).ToList();

            if (!string.IsNullOrWhiteSpace(distCode))
                buildings = buildings.Where(b => b.DistrictCode == distCode).ToList();

            if (!string.IsNullOrWhiteSpace(subDistCode))
                buildings = buildings.Where(b => b.SubDistrictCode == subDistCode).ToList();

            if (!string.IsNullOrWhiteSpace(commCode))
                buildings = buildings.Where(b => b.CommunityCode == commCode).ToList();

            if (!string.IsNullOrWhiteSpace(neighCode))
                buildings = buildings.Where(b => b.NeighborhoodCode == neighCode).ToList();

            if (!string.IsNullOrWhiteSpace(request.BuildingCode))
            {
                var normalizedCode = request.BuildingCode.Replace("-", "");
                buildings = buildings.Where(b => b.BuildingId.Contains(normalizedCode)).ToList();
            }

            // Update total count after additional filtering
            totalCount = buildings.Count;
        }
        else
        {
            var (searchBuildings, searchTotalCount) = await _unitOfWork.Buildings.SearchBuildingsAsync(
                governorateCode: string.IsNullOrWhiteSpace(govCode) ? null : govCode,
                districtCode: string.IsNullOrWhiteSpace(distCode) ? null : distCode,
                subDistrictCode: string.IsNullOrWhiteSpace(subDistCode) ? null : subDistCode,
                communityCode: string.IsNullOrWhiteSpace(commCode) ? null : commCode,
                neighborhoodCode: string.IsNullOrWhiteSpace(neighCode) ? null : neighCode,
                buildingId: request.BuildingCode,
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

        // Batch-resolve real OCHA Community.ExternalPCode for the result set so each
        // DTO can carry the canonical "Cxxxx" value (not just the synthetic "C{Code}").
        var distinctCommunityKeys = buildings
            .Select(b => (b.GovernorateCode, b.DistrictCode, b.SubDistrictCode, b.CommunityCode))
            .Distinct()
            .ToList();
        var communityPCodeMap = new Dictionary<(string, string, string, string), string?>();
        foreach (var key in distinctCommunityKeys)
        {
            var c = await _communityRepository.GetByCodeAsync(
                key.GovernorateCode, key.DistrictCode, key.SubDistrictCode, key.CommunityCode,
                cancellationToken);
            communityPCodeMap[key] = c?.ExternalPCode;
        }

        var items = buildings.Select(b =>
        {
            var hasAssignment = activeAssignments.TryGetValue(b.Id, out var assignmentInfo);
            var commKey = (b.GovernorateCode, b.DistrictCode, b.SubDistrictCode, b.CommunityCode);
            communityPCodeMap.TryGetValue(commKey, out var commExternalPCode);

            return new BuildingForAssignmentDto
            {
                Id = b.Id,
                BuildingCode = b.BuildingId,
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
                // OCHA P-Codes
                GovernoratePCode = OchaPCodeConverter.ToGovPCode(b.GovernorateCode),
                DistrictPCode = OchaPCodeConverter.ToDistrictPCode(b.GovernorateCode, b.DistrictCode),
                SubDistrictPCode = OchaPCodeConverter.ToSubDistrictPCode(b.GovernorateCode, b.DistrictCode, b.SubDistrictCode),
                CommunityPCode = OchaPCodeConverter.ToCommunityPCode(commExternalPCode, b.CommunityCode),
                NeighborhoodPCode = OchaPCodeConverter.ToNeighborhoodPCode(b.NeighborhoodCode),
                NumberOfPropertyUnits = b.NumberOfPropertyUnits,
                BuildingType = b.BuildingType.ToString(),
                BuildingStatus = b.Status.ToString(),
                Latitude = b.Latitude,
                Longitude = b.Longitude,
                BuildingGeometryWkt = b.BuildingGeometryWkt,
                HasActiveAssignment = hasAssignment,
                CurrentAssignmentId = hasAssignment ? assignmentInfo.AssignmentId : null,
                CurrentAssigneeId = hasAssignment ? assignmentInfo.CollectorId.ToString() : null,
                CurrentAssigneeName = hasAssignment ? assignmentInfo.CollectorName : null,
                IsAssigned = b.IsAssigned,
                IsLocked = b.IsLocked
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
