namespace TRRCMS.Application.Reporting.Dtos;

public sealed class SurveyActivityReportDto
{
    public DateTime? FromUtc { get; set; }
    public DateTime? ToUtc { get; set; }
    public DateTime GeneratedAtUtc { get; set; }

    public List<SurveyActivityRow> FieldCollectors { get; set; } = new();
    public List<SurveyActivityRow> OfficeClerks { get; set; } = new();

    public int TotalSurveysCompleted =>
        FieldCollectors.Sum(r => r.SurveysCompleted) + OfficeClerks.Sum(r => r.SurveysCompleted);

    public int TotalSurveysDraft =>
        FieldCollectors.Sum(r => r.SurveysDraft) + OfficeClerks.Sum(r => r.SurveysDraft);
}

public sealed class SurveyActivityRow
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int SurveysCompleted { get; set; }
    public int SurveysDraft { get; set; }
    public int TotalSurveys { get; set; }
    public int AssignedBuildings { get; set; }
    public int CompletedBuildings { get; set; }
}
