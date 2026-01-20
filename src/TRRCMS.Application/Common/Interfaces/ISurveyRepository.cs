using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Common.Interfaces;

/// <summary>
/// Repository interface for Survey entity operations
/// Handles field and office survey data access
/// </summary>
public interface ISurveyRepository
{
    /// <summary>
    /// Get survey by ID
    /// </summary>
    Task<Survey?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get survey by reference code
    /// </summary>
    Task<Survey?> GetByReferenceCodeAsync(string referenceCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all surveys for a field collector
    /// </summary>
    Task<List<Survey>> GetByFieldCollectorAsync(Guid fieldCollectorId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get draft surveys for a field collector (for resuming)
    /// </summary>
    Task<List<Survey>> GetDraftsByCollectorAsync(Guid fieldCollectorId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get finalized surveys ready for export
    /// </summary>
    Task<List<Survey>> GetFinalizedSurveysAsync(DateTime? fromDate = null, CancellationToken cancellationToken = default);

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
}