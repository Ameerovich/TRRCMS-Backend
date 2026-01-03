namespace TRRCMS.Domain.Enums;

/// <summary>
/// Export format for data extraction and reporting
/// </summary>
public enum ExportFormat
{
    /// <summary>
    /// Comma-Separated Values (CSV) for data analysis
    /// </summary>
    CSV = 1,

    /// <summary>
    /// Microsoft Excel format (.xlsx) for management review
    /// </summary>
    Excel = 2,

    /// <summary>
    /// Portable Document Format (PDF) for official documents
    /// </summary>
    PDF = 3,

    /// <summary>
    /// GeoJSON format for GIS analysis
    /// </summary>
    GeoJSON = 4,

    /// <summary>
    /// XML format for system integration
    /// </summary>
    XML = 5,

    /// <summary>
    /// JSON format for API integrations
    /// </summary>
    JSON = 6,

    /// <summary>
    /// UN-Habitat Container (.uhc) format for tablet exports
    /// </summary>
    UHC = 7,

    /// <summary>
    /// Shapefile for GIS systems
    /// </summary>
    Shapefile = 8,

    /// <summary>
    /// KML/KMZ for Google Earth
    /// </summary>
    KML = 9,

    /// <summary>
    /// HTML format for web viewing
    /// </summary>
    HTML = 10
}