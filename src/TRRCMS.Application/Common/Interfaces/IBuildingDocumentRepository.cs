using TRRCMS.Domain.Entities;

namespace TRRCMS.Application.Common.Interfaces;

/// <summary>
/// Repository interface for BuildingDocument entities.
/// Provides basic CRUD and deduplication support for building documents
/// (photos/PDFs) populated by field survey via .uhc import.
/// </summary>
public interface IBuildingDocumentRepository
{
    /// <summary>
    /// Get a building document by its ID.
    /// </summary>
    Task<BuildingDocument?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Find an existing building document by SHA-256 file hash (for deduplication).
    /// Returns the first non-deleted document with the matching hash.
    /// </summary>
    Task<BuildingDocument?> GetByFileHashAsync(string fileHash, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all building documents linked to a specific building.
    /// </summary>
    Task<List<BuildingDocument>> GetByBuildingIdAsync(Guid buildingId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add a new building document.
    /// </summary>
    Task AddAsync(BuildingDocument document, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing building document.
    /// </summary>
    Task UpdateAsync(BuildingDocument document, CancellationToken cancellationToken = default);

    /// <summary>
    /// Save pending changes.
    /// </summary>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
