namespace TRRCMS.Application.Surveys.Dtos;

/// <summary>
/// Result DTO for office survey finalization
/// Contains survey details and all claims created from ownership/heir relations
/// </summary>
public class OfficeSurveyFinalizationResultDto
{
    /// <summary>
    /// Finalized survey details
    /// </summary>
    public SurveyDto Survey { get; set; } = null!;

    /// <summary>
    /// Indicates if any claims were created from this survey
    /// </summary>
    public bool ClaimCreated { get; set; }

    /// <summary>
    /// First created claim ID (backward compatibility - use CreatedClaims for full list)
    /// </summary>
    public Guid? ClaimId { get; set; }

    /// <summary>
    /// First created claim number (backward compatibility - use CreatedClaims for full list)
    /// Format: CL-YYYY-NNNNNN
    /// </summary>
    public string? ClaimNumber { get; set; }

    /// <summary>
    /// Total number of claims created from this survey
    /// </summary>
    public int ClaimsCreatedCount { get; set; }

    /// <summary>
    /// Detailed list of all claims created from ownership/heir relations in this survey.
    /// Each relation that qualifies (Owner, Heir) generates one claim.
    /// Contains all fields required by the "تسجيل الحالة" UI panel.
    /// </summary>
    public List<CreatedClaimSummaryDto> CreatedClaims { get; set; } = new();

    /// <summary>
    /// Reason why claims were not created (if applicable)
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
