namespace TRRCMS.Application.Dashboard.Dtos;

/// <summary>
/// Registration coverage dashboard: demographics, relations, claims, and evidence.
/// Returned by GET /api/v1/dashboard/registration-coverage.
/// </summary>
public sealed class RegistrationCoverageDashboardDto
{
    /// <summary>
    /// Individually registered person records (contact points, claimants, members entered by name).
    /// </summary>
    public int TotalPersons { get; set; }
    public int TotalHouseholds { get; set; }

    /// <summary>
    /// Population reported by the households: sum of HouseholdSize. Households can be recorded
    /// with member counts only, so this can exceed TotalPersons.
    /// </summary>
    public int TotalHouseholdMembers { get; set; }

    /// <summary>
    /// Count per Gender enum name (e.g. "Male": 120, "Female": 95).
    /// </summary>
    public Dictionary<string, int> PersonsByGender { get; set; } = new();

    public int PersonsWithNationalId { get; set; }

    public int TotalPersonPropertyRelations { get; set; }

    /// <summary>
    /// Count per RelationType enum name (e.g. "Owner": 80, "Tenant": 40).
    /// </summary>
    public Dictionary<string, int> RelationsByType { get; set; } = new();

    public int RelationsWithEvidence { get; set; }

    public int TotalCases { get; set; }
    public int CasesOpen { get; set; }
    public int CasesClosed { get; set; }

    public int ClaimsOpen { get; set; }
    public int ClaimsClosed { get; set; }

    /// <summary>
    /// Count per ClaimType enum name (e.g. "OwnershipClaim": 60, "OccupancyClaim": 30).
    /// </summary>
    public Dictionary<string, int> ClaimsByType { get; set; } = new();

    /// <summary>
    /// Claims backed by at least one evidence document with a file (on the claim's source
    /// relation or the claim itself). Drives the dashboard "documents completion" rate.
    /// </summary>
    public int ClaimsWithAllDocuments { get; set; }

    /// <summary>
    /// Claims with no evidence document file.
    /// </summary>
    public int ClaimsMissingDocuments { get; set; }

    public int TotalEvidenceItems { get; set; }

    public DateTime GeneratedAtUtc { get; set; }
}
