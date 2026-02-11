using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Infrastructure.Persistence.SeedData;

/// <summary>
/// Seed data for Aleppo city neighborhoods.
/// Source: neighborhoods.json (20 neighborhoods for initial map coverage).
/// 
/// Usage in migration or startup:
///   var neighborhoods = NeighborhoodSeedData.GetAleppoNeighborhoods(systemUserId);
///   foreach (var n in neighborhoods)
///       await context.Neighborhoods.AddAsync(n);
///   await context.SaveChangesAsync();
/// </summary>
public static class NeighborhoodSeedData
{
    // Parent hierarchy for Aleppo city center
    private const string GovCode = "01";    // Aleppo Governorate
    private const string DistCode = "01";   // Aleppo District
    private const string SubDistCode = "01"; // Aleppo Sub-District
    private const string CommCode = "001";  // Aleppo City Community

    /// <summary>
    /// Returns 20 seeded Aleppo neighborhoods with boundary polygons.
    /// </summary>
    public static List<Neighborhood> GetAleppoNeighborhoods(Guid createdByUserId)
    {
        var reader = new WKTReader();
        var neighborhoods = new List<Neighborhood>();

        foreach (var (code, nameEn, nameAr, wkt, area) in RawData())
        {
            var boundary = reader.Read(wkt);
            boundary.SRID = 4326;

            // Compute centroid from polygon
            var centroid = boundary.Centroid;

            var neighborhood = Neighborhood.Create(
                governorateCode: GovCode,
                districtCode: DistCode,
                subDistrictCode: SubDistCode,
                communityCode: CommCode,
                neighborhoodCode: code,
                nameArabic: nameAr,
                nameEnglish: nameEn,
                centerLatitude: (decimal)centroid.Y,
                centerLongitude: (decimal)centroid.X,
                boundaryGeometry: boundary,
                areaSquareKm: area,
                zoomLevel: 15,
                createdByUserId: createdByUserId);

            neighborhoods.Add(neighborhood);
        }

        return neighborhoods;
    }

    /// <summary>
    /// Raw neighborhood data: (code, nameEn, nameAr, polygonWkt, areaKm²)
    /// </summary>
    private static IEnumerable<(string Code, string NameEn, string NameAr, string Wkt, double Area)> RawData()
    {
        yield return ("001", "Al-Jamiliyah", "الجميلية",
            "POLYGON((37.130 36.200, 37.135 36.200, 37.135 36.205, 37.130 36.205, 37.130 36.200))", 0.50);
        yield return ("002", "Al-Aziziyah", "العزيزية",
            "POLYGON((37.135 36.195, 37.145 36.195, 37.145 36.205, 37.135 36.205, 37.135 36.195))", 0.45);
        yield return ("003", "Al-Shahba", "الشهباء",
            "POLYGON((37.145 36.190, 37.155 36.190, 37.155 36.200, 37.145 36.200, 37.145 36.190))", 0.45);
        yield return ("004", "Al-Hamdaniyah", "الحمدانية",
            "POLYGON((37.120 36.210, 37.135 36.210, 37.135 36.220, 37.120 36.220, 37.120 36.210))", 3.50);
        yield return ("005", "Al-Midan", "الميدان",
            "POLYGON((37.155 36.200, 37.165 36.200, 37.165 36.210, 37.155 36.210, 37.155 36.200))", 0.60);
        yield return ("006", "Salah al-Din", "صلاح الدين",
            "POLYGON((37.135 36.185, 37.145 36.185, 37.145 36.195, 37.135 36.195, 37.135 36.185))", 0.80);
        yield return ("007", "Al-Firdaws", "الفردوس",
            "POLYGON((37.110 36.195, 37.120 36.195, 37.120 36.205, 37.110 36.205, 37.110 36.195))", 0.50);
        yield return ("008", "Al-Sabil", "السبيل",
            "POLYGON((37.120 36.195, 37.130 36.195, 37.130 36.205, 37.120 36.205, 37.120 36.195))", 0.30);
        yield return ("009", "Hanano", "هنانو",
            "POLYGON((37.165 36.210, 37.175 36.210, 37.175 36.220, 37.165 36.220, 37.165 36.210))", 1.20);
        yield return ("010", "Al-Sha'ar", "الشعار",
            "POLYGON((37.145 36.205, 37.155 36.205, 37.155 36.215, 37.145 36.215, 37.145 36.205))", 0.50);
        yield return ("011", "Al-Masri", "المصري",
            "POLYGON((37.100 36.190, 37.110 36.190, 37.110 36.200, 37.100 36.200, 37.100 36.190))", 0.55);
        yield return ("012", "Bab al-Nairab", "باب النيرب",
            "POLYGON((37.155 36.185, 37.165 36.185, 37.165 36.195, 37.155 36.195, 37.155 36.185))", 0.80);
        yield return ("013", "Al-Kalaseh", "الكلاسة",
            "POLYGON((37.130 36.205, 37.140 36.205, 37.140 36.215, 37.130 36.215, 37.130 36.205))", 0.35);
        yield return ("014", "Al-Farafra", "الفرافرة",
            "POLYGON((37.110 36.205, 37.120 36.205, 37.120 36.215, 37.110 36.215, 37.110 36.205))", 0.15);
        yield return ("015", "Al-Sukkari", "السكري",
            "POLYGON((37.140 36.180, 37.150 36.180, 37.150 36.190, 37.140 36.190, 37.140 36.180))", 0.65);
        yield return ("016", "Sheikh Maqsoud", "الشيخ مقصود",
            "POLYGON((37.100 36.215, 37.115 36.215, 37.115 36.230, 37.100 36.230, 37.100 36.215))", 1.50);
        yield return ("017", "Ashrafiyeh", "الأشرفية",
            "POLYGON((37.125 36.190, 37.135 36.190, 37.135 36.200, 37.125 36.200, 37.125 36.190))", 0.70);
        yield return ("018", "Al-Ansari", "الأنصاري",
            "POLYGON((37.140 36.195, 37.150 36.195, 37.150 36.205, 37.140 36.205, 37.140 36.195))", 0.55);
        yield return ("019", "Al-Shaar", "الشعار",
            "POLYGON((37.150 36.210, 37.160 36.210, 37.160 36.220, 37.150 36.220, 37.150 36.210))", 0.50);
        yield return ("020", "Bustan al-Qasr", "بستان القصر",
            "POLYGON((37.135 36.210, 37.145 36.210, 37.145 36.220, 37.135 36.220, 37.135 36.210))", 0.40);
    }
}
