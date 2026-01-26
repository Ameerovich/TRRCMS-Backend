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
