namespace TRRCMS.Domain.Enums;

/// <summary>
/// Type of document describing a building (وثيقة البناء).
/// Populated by field survey via .uhc import pipeline.
/// </summary>
public enum BuildingDocumentType
{
    /// <summary>
    /// Photograph of the building
    /// </summary>
    Photo = 0,

    /// <summary>
    /// PDF document describing the building
    /// </summary>
    PDF = 1
}
