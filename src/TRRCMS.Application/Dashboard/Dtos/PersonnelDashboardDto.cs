namespace TRRCMS.Application.Dashboard.Dtos;

/// <summary>
/// Personnel workload dashboard: field collectors and office clerks.
/// Returned by GET /api/v1/dashboard/personnel.
/// </summary>
public sealed class PersonnelDashboardDto
{
    public List<UserWorkloadDto> FieldCollectors { get; set; } = new();
    public List<UserWorkloadDto> OfficeClerks { get; set; } = new();

    /// <summary>
    /// Surveys outside the two lists above: owned by users of other roles (supervisor,
    /// data manager, admin, inactive users), or of the other survey type than the owner's
    /// role (e.g. a field survey attributed to an office clerk by an import). One row per
    /// user, so no draft is left uncounted.
    /// </summary>
    public List<UserWorkloadDto> Others { get; set; } = new();

    public DateTime GeneratedAtUtc { get; set; }
}

/// <summary>
/// Workload metrics for a single user.
/// </summary>
public sealed class UserWorkloadDto
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// User role name (e.g. "DataManager"); set on <see cref="PersonnelDashboardDto.Others"/> rows.
    /// </summary>
    public string? Role { get; set; }

    public int SurveysCompleted { get; set; }
    public int SurveysDraft { get; set; }
    public int TotalSurveys { get; set; }

    /// <summary>
    /// Number of buildings assigned (field collectors only).
    /// </summary>
    public int AssignedBuildings { get; set; }

    /// <summary>
    /// Number of buildings completed (field collectors only).
    /// </summary>
    public int CompletedBuildings { get; set; }
}
