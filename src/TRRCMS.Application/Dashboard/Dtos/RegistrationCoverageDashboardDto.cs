namespace TRRCMS.Application.Dashboard.Dtos;

/// <summary>
/// Registration coverage dashboard: demographics, relations, claims, and evidence.
/// Returned by GET /api/v1/dashboard/registration-coverage.
/// </summary>
public sealed class RegistrationCoverageDashboardDto
{
    // ==================== PERSON DEMOGRAPHICS ====================

    public int TotalPersons { get; set; }
    public int TotalHouseholds { get; set; }

    /// <summary>
    /// Count per Gender enum name (e.g. "Male": 120, "Female": 95).
    /// </summary>
    public Dictionary<string, int> PersonsByGender { get; set; } = new();

    public int PersonsWithNationalId { get; set; }
    public int PersonsWithIdentificationDocument { get; set; }

    // ==================== PERSON-PROPERTY RELATIONS ====================

    public int TotalPersonPropertyRelations { get; set; }

    /// <summary>
    /// Count per RelationType enum name (e.g. "Owner": 80, "Tenant": 40).
    /// </summary>
    public Dictionary<string, int> RelationsByType { get; set; } = new();

    public int RelationsWithEvidence { get; set; }

    // ==================== CLAIMS ====================

    public int ClaimsOpen { get; set; }
    public int ClaimsClosed { get; set; }

    /// <summary>
    /// Count per ClaimType enum name (e.g. "OwnershipClaim": 60, "OccupancyClaim": 30).
    /// </summary>
    public Dictionary<string, int> ClaimsByType { get; set; } = new();

    public int ClaimsWithAllDocuments { get; set; }
    public int ClaimsMissingDocuments { get; set; }

    // ==================== EVIDENCE ====================

    public int TotalEvidenceItems { get; set; }

    public DateTime GeneratedAtUtc { get; set; }
}
