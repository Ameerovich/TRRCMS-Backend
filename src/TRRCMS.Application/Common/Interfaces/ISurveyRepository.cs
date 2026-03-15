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
    /// Get surveys linked to the given claim IDs (via Survey.ClaimId).
    /// Returns one survey per claim (the first match).
    /// </summary>
    Task<Dictionary<Guid, Survey>> GetByClaimIdsAsync(IEnumerable<Guid> claimIds, CancellationToken cancellationToken = default);

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
        string? contactPersonName = null,
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
    // ==================== AGGREGATE QUERIES (Dashboard) ====================

    /// <summary>
    /// Get count of surveys grouped by status.
    /// Used for dashboard summary tiles.
    /// </summary>
    Task<Dictionary<SurveyStatus, int>> GetStatusCountsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get total count of all surveys (excluding soft-deleted).
    /// </summary>
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get count of surveys finalized (status = Finalized or later) since a given date.
    /// Used for "completed last 7/30 days" dashboard counters.
    /// </summary>
    Task<int> GetFinalizedCountSinceAsync(DateTime sinceUtc, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get count of surveys grouped by type (Field vs Office).
    /// Used for dashboard summary tiles.
    /// </summary>
    Task<Dictionary<SurveyType, int>> GetTypeCountsAsync(CancellationToken cancellationToken = default);

    // ==================== DASHBOARD TREND QUERIES ====================

    /// <summary>
    /// Get monthly creation counts for time-series trends.
    /// </summary>
    Task<List<(int Year, int Month, int Count)>> GetMonthlyCreationCountsAsync(
        DateTime? from = null, DateTime? to = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get survey counts grouped by collector (FieldCollectorId).
    /// Returns completed, draft, and total counts per user.
    /// </summary>
    Task<List<(Guid UserId, int Completed, int Draft, int Total)>> GetCountsByCollectorAsync(
        DateTime? from = null, DateTime? to = null,
        CancellationToken cancellationToken = default);
}
