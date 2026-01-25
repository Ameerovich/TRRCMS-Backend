using MediatR;
using TRRCMS.Application.Surveys.Dtos;

namespace TRRCMS.Application.Surveys.Queries.GetOfficeSurveys;

/// <summary>
/// Query to get office surveys with filtering and pagination
/// Corresponds to UC-004/UC-005: Office Survey listing
/// </summary>
public class GetOfficeSurveysQuery : IRequest<GetOfficeSurveysResponse>
{
    /// <summary>
    /// Filter by survey status (Draft, Completed, Finalized, etc.)
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Filter by building ID
    /// </summary>
    public Guid? BuildingId { get; set; }

    /// <summary>
    /// Filter by office clerk (user) ID
    /// </summary>
    public Guid? ClerkId { get; set; }

    /// <summary>
    /// Filter surveys from this date
    /// </summary>
    public DateTime? FromDate { get; set; }

    /// <summary>
    /// Filter surveys until this date
    /// </summary>
    public DateTime? ToDate { get; set; }

    /// <summary>
    /// Search by reference code
    /// </summary>
    public string? ReferenceCode { get; set; }

    /// <summary>
    /// Search by interviewee name
    /// </summary>
    public string? IntervieweeName { get; set; }

    /// <summary>
    /// Page number (1-based)
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Page size (default 20, max 100)
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Sort field (SurveyDate, ReferenceCode, Status, CreatedAtUtc)
    /// </summary>
    public string SortBy { get; set; } = "SurveyDate";

    /// <summary>
    /// Sort direction (asc or desc)
    /// </summary>
    public string SortDirection { get; set; } = "desc";
}

/// <summary>
/// Response for GetOfficeSurveysQuery with pagination info
/// </summary>
public class GetOfficeSurveysResponse
{
    /// <summary>
    /// List of office surveys
    /// </summary>
    public List<SurveyDto> Surveys { get; set; } = new();

    /// <summary>
    /// Total count of surveys matching filters
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

    /// <summary>
    /// Whether there are previous pages
    /// </summary>
    public bool HasPreviousPage => Page > 1;
}
