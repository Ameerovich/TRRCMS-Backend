using TRRCMS.Application.Surveys.Queries.GetFieldSurveys;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Common.Interfaces;

/// <summary>
/// Repository interface for Survey entity operations
/// Handles field and office survey data access
/// </summary>
public interface ISurveyRepository
{
    // ==================== COMMON METHODS ====================

    /// <summary>
    /// Get survey by ID
    /// </summary>
    Task<Survey?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get survey by reference code
    /// </summary>
    Task<Survey?> GetByReferenceCodeAsync(string referenceCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get surveys by building
    /// </summary>
    Task<List<Survey>> GetByBuildingAsync(Guid buildingId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get surveys by property unit
    /// </summary>
    Task<List<Survey>> GetByPropertyUnitAsync(Guid propertyUnitId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add new survey
    /// </summary>
    Task<Survey> AddAsync(Survey survey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update existing survey
    /// </summary>
    Task UpdateAsync(Survey survey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Save all changes
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get next sequence number for reference code generation
    /// Uses PostgreSQL sequence for thread-safe sequential numbering
    /// </summary>
    Task<int> GetNextReferenceSequenceAsync(CancellationToken cancellationToken = default);

    // ==================== FIELD SURVEY METHODS ====================

    /// <summary>
    /// Get all field surveys for a field collector
    /// </summary>
    Task<List<Survey>> GetByFieldCollectorAsync(Guid fieldCollectorId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get draft field surveys for a field collector (for resuming)
    /// </summary>
    Task<List<Survey>> GetDraftsByCollectorAsync(Guid fieldCollectorId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get finalized field surveys ready for export
    /// </summary>
    Task<List<Survey>> GetFinalizedSurveysAsync(DateTime? fromDate = null, CancellationToken cancellationToken = default);

    // ==================== OFFICE SURVEY METHODS ====================

    /// <summary>
    /// Get office surveys with filtering and pagination
    /// UC-004/UC-005: Office survey listing
    /// </summary>
    Task<(List<Survey> Surveys, int TotalCount)> GetOfficeSurveysAsync(
        string? status = null,
        Guid? buildingId = null,
        Guid? clerkId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? referenceCode = null,
        string? intervieweeName = null,
        int page = 1,
        int pageSize = 20,
        string sortBy = "SurveyDate",
        string sortDirection = "desc",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get draft office surveys for an office clerk (for resuming)
    /// UC-005: Resume draft office survey
    /// </summary>
    Task<List<Survey>> GetOfficeDraftsByClerkAsync(Guid clerkId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all office surveys for an office clerk
    /// </summary>
    Task<List<Survey>> GetByOfficeClerkAsync(Guid clerkId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get finalized office surveys (for reporting)
    /// </summary>
    Task<List<Survey>> GetFinalizedOfficeSurveysAsync(DateTime? fromDate = null, CancellationToken cancellationToken = default);
    /// <summary>
    /// Get field surveys with filtering and pagination
    /// Used by GetFieldSurveysQuery
    /// </summary>
    /// <param name="criteria">Filter criteria</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of field surveys</returns>
    Task<List<Survey>> GetFieldSurveysAsync(
        FieldSurveyFilterCriteria criteria,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get total count of field surveys matching criteria
    /// Used for pagination
    /// </summary>
    /// <param name="criteria">Filter criteria</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Total count</returns>
    Task<int> GetFieldSurveysCountAsync(
        FieldSurveyFilterCriteria criteria,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get draft field surveys for a specific collector with pagination
    /// Used by GetFieldDraftSurveysQuery
    /// </summary>
    /// <param name="fieldCollectorId">Field collector user ID</param>
    /// <param name="buildingId">Optional building filter</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="sortBy">Sort field</param>
    /// <param name="sortDirection">Sort direction (asc/desc)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Tuple of (surveys, totalCount)</returns>
    Task<(List<Survey> Surveys, int TotalCount)> GetFieldDraftSurveysByCollectorAsync(
        Guid fieldCollectorId,
        Guid? buildingId,
        int page,
        int pageSize,
        string sortBy,
        string sortDirection,
        CancellationToken cancellationToken = default);
}
