using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.BuildingAssignments.Dtos;

/// <summary>
/// DTO for building assignment details
/// </summary>
public class BuildingAssignmentDto
{
    public Guid Id { get; set; }
    
    // Building Info
    public Guid BuildingId { get; set; }
    public string BuildingCode { get; set; } = string.Empty;
    public string? BuildingAddress { get; set; }
    public string? GovernorateCode { get; set; }
    public string? DistrictCode { get; set; }
    public string? SubDistrictCode { get; set; }
    public string? CommunityCode { get; set; }
    public string? NeighborhoodCode { get; set; }
    
    // Field Collector Info
    public Guid FieldCollectorId { get; set; }
    public string FieldCollectorName { get; set; } = string.Empty;
    public string? FieldCollectorDeviceId { get; set; }
    
    // Assignment Info
    public Guid? AssignedByUserId { get; set; }
    public string? AssignedByUserName { get; set; }
    public DateTime AssignedDate { get; set; }
    public DateTime? TargetCompletionDate { get; set; }
    public DateTime? ActualCompletionDate { get; set; }
    
    // Transfer Status
    public TransferStatus TransferStatus { get; set; }
    public string TransferStatusName { get; set; } = string.Empty;
    public DateTime? TransferredToTabletDate { get; set; }
    public DateTime? SynchronizedFromTabletDate { get; set; }
    public string? TransferErrorMessage { get; set; }
    public int TransferRetryCount { get; set; }
    
    // Progress
    public int TotalPropertyUnits { get; set; }
    public int CompletedPropertyUnits { get; set; }
    public decimal CompletionPercentage { get; set; }
    
    // Revisit Info
    public bool IsRevisit { get; set; }
    public Guid? OriginalAssignmentId { get; set; }
    public string? UnitsForRevisit { get; set; }
    public string? RevisitReason { get; set; }
    
    // Status
    public string Priority { get; set; } = "Normal";
    public string? AssignmentNotes { get; set; }
    public bool IsActive { get; set; }
    public bool IsOverdue { get; set; }
    
    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

/// <summary>
/// Summary DTO for listing assignments
/// </summary>
public class BuildingAssignmentSummaryDto
{
    public Guid Id { get; set; }
    public Guid BuildingId { get; set; }
    public string BuildingCode { get; set; } = string.Empty;
    public string? BuildingAddress { get; set; }
    public Guid FieldCollectorId { get; set; }
    public string FieldCollectorName { get; set; } = string.Empty;
    public DateTime AssignedDate { get; set; }
    public TransferStatus TransferStatus { get; set; }
    public string TransferStatusName { get; set; } = string.Empty;
    public int TotalPropertyUnits { get; set; }
    public int CompletedPropertyUnits { get; set; }
    public decimal CompletionPercentage { get; set; }
    public bool IsActive { get; set; }
    public bool IsOverdue { get; set; }
    public bool IsRevisit { get; set; }
    public string Priority { get; set; } = "Normal";
}

/// <summary>
/// DTO for available field collector (for assignment selection)
/// </summary>
public class AvailableFieldCollectorDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullNameArabic { get; set; } = string.Empty;
    public string? FullNameEnglish { get; set; }
    public string? AssignedTabletId { get; set; }
    public string? TeamName { get; set; }
    public bool IsAvailable { get; set; }
    
    // Current workload
    public int ActiveAssignments { get; set; }
    public int PendingTransferCount { get; set; }
    public int TotalPropertyUnitsAssigned { get; set; }
}

/// <summary>
/// DTO for building available for assignment
/// </summary>
public class BuildingForAssignmentDto
{
    public Guid Id { get; set; }
    public string BuildingCode { get; set; } = string.Empty;
    public string? Address { get; set; }
    
    // Administrative hierarchy
    public string GovernorateCode { get; set; } = string.Empty;
    public string? GovernorateName { get; set; }
    public string DistrictCode { get; set; } = string.Empty;
    public string? DistrictName { get; set; }
    public string SubDistrictCode { get; set; } = string.Empty;
    public string? SubDistrictName { get; set; }
    public string CommunityCode { get; set; } = string.Empty;
    public string? CommunityName { get; set; }
    public string NeighborhoodCode { get; set; } = string.Empty;
    public string? NeighborhoodName { get; set; }
    
    // Building details
    public int NumberOfPropertyUnits { get; set; }
    public string? BuildingType { get; set; }
    public string? BuildingStatus { get; set; }
    
    // Location
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    
    // Assignment status
    public bool HasActiveAssignment { get; set; }
    public Guid? CurrentAssignmentId { get; set; }
    public string? CurrentAssigneeId { get; set; }
    public string? CurrentAssigneeName { get; set; }
}

/// <summary>
/// DTO for property unit in revisit selection
/// </summary>
public class PropertyUnitForRevisitDto
{
    public Guid Id { get; set; }
    public string UnitCode { get; set; } = string.Empty;
    public string? UnitType { get; set; }
    public int? FloorNumber { get; set; }
    public string? Description { get; set; }
    
    // Survey status
    public bool HasCompletedSurvey { get; set; }
    public DateTime? LastSurveyDate { get; set; }
    
    // Associated data counts
    public int PersonCount { get; set; }
    public int HouseholdCount { get; set; }
    public int ClaimCount { get; set; }
}

/// <summary>
/// DTO for field collector's current tasks (assignments list for their tablet)
/// </summary>
public class FieldCollectorTasksDto
{
    public Guid FieldCollectorId { get; set; }
    public string FieldCollectorName { get; set; } = string.Empty;
    
    // Summary
    public int TotalAssignments { get; set; }
    public int PendingTransfer { get; set; }
    public int ReadyForSurvey { get; set; }
    public int InProgress { get; set; }
    public int Completed { get; set; }
    
    // Assignments list
    public List<BuildingAssignmentSummaryDto> Assignments { get; set; } = new();
}

/// <summary>
/// Paginated result for assignments
/// </summary>
public class BuildingAssignmentPagedResult
{
    public List<BuildingAssignmentSummaryDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}

/// <summary>
/// Paginated result for buildings available for assignment
/// Supports both regular search and polygon search responses
/// </summary>
public class BuildingsForAssignmentPagedResult
{
    /// <summary>
    /// List of buildings matching the search criteria
    /// </summary>
    public List<BuildingForAssignmentDto> Items { get; set; } = new();
    
    /// <summary>
    /// Total count of matching buildings (before pagination)
    /// </summary>
    public int TotalCount { get; set; }
    
    /// <summary>
    /// Current page number
    /// </summary>
    public int Page { get; set; }
    
    /// <summary>
    /// Page size used for this request
    /// </summary>
    public int PageSize { get; set; }
    
    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
    
    /// <summary>
    /// Indicates if there's a next page
    /// </summary>
    public bool HasNextPage => Page < TotalPages;
    
    /// <summary>
    /// Indicates if there's a previous page
    /// </summary>
    public bool HasPreviousPage => Page > 1;
    
    // ==================== POLYGON SEARCH RESPONSE FIELDS ====================
    
    /// <summary>
    /// The polygon WKT used for search (only present if polygon search was used)
    /// Returns null if regular/radius search was used
    /// </summary>
    public string? PolygonWkt { get; set; }
    
    /// <summary>
    /// Approximate polygon area in square meters (only present if polygon search was used)
    /// Returns null if regular/radius search was used or if area calculation failed
    /// </summary>
    public double? PolygonAreaSquareMeters { get; set; }
}
