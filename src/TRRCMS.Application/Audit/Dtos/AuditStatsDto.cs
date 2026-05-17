namespace TRRCMS.Application.Audit.Dtos;

/// <summary>
/// Aggregated counts over a time window. Powers the dashboard overview cards
/// without forcing the frontend to fetch and tally page-sized windows client-side.
/// </summary>
public class AuditStatsDto
{
    public DateTime WindowStart { get; set; }
    public DateTime WindowEnd { get; set; }

    public int TotalActions { get; set; }

    /// <summary>Counts keyed by <c>ActionResult</c> string (e.g. "Success", "Failed").</summary>
    public Dictionary<string, int> ByActionResult { get; set; } = new();

    /// <summary>Counts keyed by <c>AuditActionType</c> integer value (as string).</summary>
    public Dictionary<string, int> ByActionType { get; set; } = new();

    /// <summary>Counts keyed by entity type name (e.g. "Building"). Excludes entries with no entity.</summary>
    public Dictionary<string, int> ByEntityType { get; set; } = new();

    /// <summary>Distinct user count (excludes the system pseudo-user).</summary>
    public int UniqueUsers { get; set; }

    /// <summary>Number of security-sensitive entries in the window.</summary>
    public int SecuritySensitive { get; set; }

    public List<AuditTopUserDto> TopUsers { get; set; } = new();
}

public class AuditTopUserDto
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string UserFullName { get; set; } = string.Empty;
    public int ActionCount { get; set; }
}
