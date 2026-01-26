using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Common.Interfaces;

/// <summary>
/// Filter criteria for field surveys query
/// This class is in Common.Interfaces namespace so all handlers can use it
/// </summary>
public class FieldSurveyFilterCriteria
{
    /// <summary>
    /// Filter by survey status
    /// </summary>
    public SurveyStatus? Status { get; set; }
    
    /// <summary>
    /// Filter by building ID
    /// </summary>
    public Guid? BuildingId { get; set; }
    
    /// <summary>
    /// Filter by field collector ID
    /// </summary>
    public Guid? FieldCollectorId { get; set; }
    
    /// <summary>
    /// Filter by property unit ID
    /// </summary>
    public Guid? PropertyUnitId { get; set; }
    
    /// <summary>
    /// Filter surveys from this date
    /// </summary>
    public DateTime? FromDate { get; set; }
    
    /// <summary>
    /// Filter surveys until this date
    /// </summary>
    public DateTime? ToDate { get; set; }
    
    /// <summary>
    /// Search by reference code (partial match)
    /// </summary>
    public string? ReferenceCode { get; set; }
    
    /// <summary>
    /// Search by interviewee name (partial match)
    /// </summary>
    public string? IntervieweeName { get; set; }
    
    /// <summary>
    /// Sort field (SurveyDate, ReferenceCode, Status, CreatedAtUtc)
    /// </summary>
    public string SortBy { get; set; } = "SurveyDate";
    
    /// <summary>
    /// Sort direction (asc or desc)
    /// </summary>
    public string SortDirection { get; set; } = "desc";
}
