using MediatR;
using TRRCMS.Application.Surveys.Dtos;

namespace TRRCMS.Application.Surveys.Queries.GetFieldDraftSurveys;

/// <summary>
/// Query to get draft field surveys for the current field collector
/// Corresponds to UC-002: Resume draft field survey
/// Returns only Draft status field surveys belonging to the authenticated user
/// </summary>
public class GetFieldDraftSurveysQuery : IRequest<GetFieldDraftSurveysResponse>
{
    /// <summary>
    /// Optional: Filter by building ID
    /// </summary>
    public Guid? BuildingId { get; set; }

    /// <summary>
    /// Page number (1-based)
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Page size (default 20, max 50)
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Sort field (SurveyDate, ReferenceCode, LastModifiedAtUtc)
    /// </summary>
    public string SortBy { get; set; } = "LastModifiedAtUtc";

    /// <summary>
    /// Sort direction (asc or desc)
    /// </summary>
    public string SortDirection { get; set; } = "desc";
}

/// <summary>
/// Response for GetFieldDraftSurveysQuery
/// </summary>
public class GetFieldDraftSurveysResponse
{
    /// <summary>
    /// List of draft field surveys
    /// </summary>
    public List<SurveyDto> Surveys { get; set; } = new();

    /// <summary>
    /// Total count of draft surveys
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Current page number
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Page size
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    /// <summary>
    /// Whether there are more pages
    /// </summary>
    public bool HasNextPage => Page < TotalPages;
}