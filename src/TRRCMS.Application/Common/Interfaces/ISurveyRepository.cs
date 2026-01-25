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
}