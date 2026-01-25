namespace TRRCMS.Application.Surveys.Dtos;

/// <summary>
/// Result DTO for office survey finalization
/// Contains survey details and optionally created claim information
/// </summary>
public class OfficeSurveyFinalizationResultDto
{
    /// <summary>
    /// Finalized survey details
    /// </summary>
    public SurveyDto Survey { get; set; } = null!;

    /// <summary>
    /// Indicates if a claim was created from this survey
    /// </summary>
    public bool ClaimCreated { get; set; }

    /// <summary>
    /// Created claim ID (if any)
    /// </summary>
    public Guid? ClaimId { get; set; }

    /// <summary>
    /// Created claim number (if any)
    /// Format: CL-YYYY-NNNNNN
    /// </summary>
    public string? ClaimNumber { get; set; }

    /// <summary>
    /// Reason why claim was not created (if applicable)
    /// e.g., "No ownership relations found", "AutoCreateClaim disabled"
    /// </summary>
    public string? ClaimNotCreatedReason { get; set; }

    /// <summary>
    /// Validation warnings (non-blocking issues found during finalization)
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// Summary of data captured in the survey
    /// </summary>
    public SurveyDataSummaryDto DataSummary { get; set; } = null!;
}

/// <summary>
/// Summary of data captured in the survey
/// </summary>
public class SurveyDataSummaryDto
{
    /// <summary>
    /// Number of property units captured
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
    /// Number of person-property relations captured
    /// </summary>
    public int RelationsCount { get; set; }

    /// <summary>
    /// Number of ownership relations (basis for claim creation)
    /// </summary>
    public int OwnershipRelationsCount { get; set; }

    /// <summary>
    /// Number of evidence items uploaded
    /// </summary>
    public int EvidenceCount { get; set; }

    /// <summary>
    /// Total evidence file size in bytes
    /// </summary>
    public long TotalEvidenceSizeBytes { get; set; }
}
