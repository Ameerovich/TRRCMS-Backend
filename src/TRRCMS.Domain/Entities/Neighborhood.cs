using NetTopologySuite.Geometries;
using TRRCMS.Domain.Common;

namespace TRRCMS.Domain.Entities;

/// <summary>
/// Neighborhood reference entity — links neighborhood codes to spatial data.
/// Used for map navigation ("fly to"), boundary rendering, and building location validation.
/// حي — بيانات مرجعية للأحياء
/// </summary>
public class Neighborhood : BaseAuditableEntity
{
    // ==================== ADMINISTRATIVE HIERARCHY ====================

    /// <summary>
    /// Governorate code (محافظة) — 2 digits
    /// </summary>
    public string GovernorateCode { get; private set; }

    /// <summary>
    /// District code (مدينة) — 2 digits
    /// </summary>
    public string DistrictCode { get; private set; }

    /// <summary>
    /// Sub-district code (بلدة) — 2 digits
    /// </summary>
    public string SubDistrictCode { get; private set; }

    /// <summary>
    /// Community code (قرية) — 3 digits
    /// </summary>
    public string CommunityCode { get; private set; }

    /// <summary>
    /// Neighborhood code (حي) — 3 digits. Unique within parent hierarchy.
    /// </summary>
    public string NeighborhoodCode { get; private set; }

    /// <summary>
    /// Full composite code: GG+DD+SS+CCC+NNN (12 digits)
    /// Used for fast lookups matching the Building.BuildingId prefix.
    /// </summary>
    public string FullCode { get; private set; }

    // ==================== NAMES ====================

    /// <summary>
    /// Neighborhood name in Arabic (الاسم بالعربية)
    /// </summary>
    public string NameArabic { get; private set; }

    /// <summary>
    /// Neighborhood name in English
    /// </summary>
    public string? NameEnglish { get; private set; }

    // ==================== SPATIAL DATA (PostGIS) ====================

    /// <summary>
    /// Center point for "fly-to" map navigation (PostGIS POINT, SRID 4326)
    /// </summary>
    public Point? CenterPoint { get; private set; }

    /// <summary>
    /// Center latitude (convenience — matches CenterPoint.Y)
    /// </summary>
    public decimal CenterLatitude { get; private set; }

    /// <summary>
    /// Center longitude (convenience — matches CenterPoint.X)
    /// </summary>
    public decimal CenterLongitude { get; private set; }

    /// <summary>
    /// Boundary polygon for rendering on map (PostGIS POLYGON/MULTIPOLYGON, SRID 4326)
    /// </summary>
    public Geometry? BoundaryGeometry { get; private set; }

    /// <summary>
    /// Boundary as WKT string (computed, not stored)
    /// </summary>
    public string? BoundaryWkt => BoundaryGeometry?.AsText();

    /// <summary>
    /// Approximate area in square kilometers
    /// </summary>
    public double? AreaSquareKm { get; private set; }

    /// <summary>
    /// Suggested map zoom level when navigating to this neighborhood
    /// </summary>
    public int ZoomLevel { get; private set; }

    // ==================== STATUS ====================

    /// <summary>
    /// Whether this neighborhood is active in the system
    /// </summary>
    public bool IsActive { get; private set; }

    // ==================== CONSTRUCTORS ====================

    /// <summary>
    /// EF Core constructor
    /// </summary>
    private Neighborhood() : base()
    {
        GovernorateCode = string.Empty;
        DistrictCode = string.Empty;
        SubDistrictCode = string.Empty;
        CommunityCode = string.Empty;
        NeighborhoodCode = string.Empty;
        FullCode = string.Empty;
        NameArabic = string.Empty;
    }

    // ==================== FACTORY METHOD ====================

    /// <summary>
    /// Create a new Neighborhood reference entry
    /// </summary>
    public static Neighborhood Create(
        string governorateCode,
        string districtCode,
        string subDistrictCode,
        string communityCode,
        string neighborhoodCode,
        string nameArabic,
        string? nameEnglish,
        decimal centerLatitude,
        decimal centerLongitude,
        Geometry? boundaryGeometry,
        double? areaSquareKm,
        int zoomLevel,
        Guid createdByUserId)
    {
        var neighborhood = new Neighborhood
        {
            GovernorateCode = governorateCode,
            DistrictCode = districtCode,
            SubDistrictCode = subDistrictCode,
            CommunityCode = communityCode,
            NeighborhoodCode = neighborhoodCode,
            FullCode = $"{governorateCode}{districtCode}{subDistrictCode}{communityCode}{neighborhoodCode}",
            NameArabic = nameArabic,
            NameEnglish = nameEnglish,
            CenterLatitude = centerLatitude,
            CenterLongitude = centerLongitude,
            AreaSquareKm = areaSquareKm,
            ZoomLevel = zoomLevel,
            IsActive = true
        };

        // Create center point geometry
        var geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
        neighborhood.CenterPoint = geometryFactory.CreatePoint(
            new Coordinate((double)centerLongitude, (double)centerLatitude));

        // Set boundary if provided
        if (boundaryGeometry != null)
        {
            boundaryGeometry.SRID = 4326;
            neighborhood.BoundaryGeometry = boundaryGeometry;
        }

        neighborhood.MarkAsCreated(createdByUserId);
        return neighborhood;
    }

    // ==================== DOMAIN METHODS ====================

    /// <summary>
    /// Update boundary polygon
    /// </summary>
    public void UpdateBoundary(Geometry boundaryGeometry, Guid modifiedByUserId)
    {
        boundaryGeometry.SRID = 4326;
        BoundaryGeometry = boundaryGeometry;

        // Recompute center from centroid
        var centroid = boundaryGeometry.Centroid;
        CenterLatitude = (decimal)centroid.Y;
        CenterLongitude = (decimal)centroid.X;

        var geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
        CenterPoint = geometryFactory.CreatePoint(centroid.Coordinate);

        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Update names
    /// </summary>
    public void UpdateNames(string nameArabic, string? nameEnglish, Guid modifiedByUserId)
    {
        NameArabic = nameArabic;
        NameEnglish = nameEnglish;
        MarkAsModified(modifiedByUserId);
    }

    /// <summary>
    /// Activate/deactivate
    /// </summary>
    public void SetActive(bool isActive, Guid modifiedByUserId)
    {
        IsActive = isActive;
        MarkAsModified(modifiedByUserId);
    }
}
