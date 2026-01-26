namespace TRRCMS.Application.Surveys.Dtos;

/// <summary>
/// Summary of survey data for finalization results
/// Shared between Field and Office surveys
/// </summary>
public class SurveyDataSummaryDto
{
    /// <summary>
    /// Number of property units linked to survey
    /// </summary>
    public int PropertyUnitsCount { get; set; }

    /// <summary>
    /// Number of households captured
    /// </summary>
    public int HouseholdsCount { get; set; }

    /// <summary>
    /// Number of persons captured
    /// </summary>
    public int PersonsCount { get; set; }

    /// <summary>
    /// Number of person-property relations
    /// </summary>
    public int RelationsCount { get; set; }

    /// <summary>
    /// Number of ownership relations (Owner, Heir, Co-owner)
    /// These are the basis for claim creation
    /// </summary>
    public int OwnershipRelationsCount { get; set; }

    /// <summary>
    /// Number of evidence files uploaded
    /// </summary>
    public int EvidenceCount { get; set; }

    /// <summary>
    /// Total size of all evidence files in bytes
    /// </summary>
    public long TotalEvidenceSizeBytes { get; set; }
}