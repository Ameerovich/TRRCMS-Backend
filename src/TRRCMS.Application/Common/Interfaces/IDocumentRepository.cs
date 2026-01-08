using TRRCMS.Domain.Entities;

namespace TRRCMS.Application.Common.Interfaces;

/// <summary>
/// Repository interface for Document entity operations
/// </summary>
public interface IDocumentRepository
{
    /// <summary>
    /// Get document by ID
    /// </summary>
    Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all documents
    /// </summary>
    Task<IEnumerable<Document>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get documents by person ID
    /// </summary>
    Task<IEnumerable<Document>> GetByPersonIdAsync(Guid personId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get documents by property unit ID
    /// </summary>
    Task<IEnumerable<Document>> GetByPropertyUnitIdAsync(Guid propertyUnitId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get documents by person-property relation ID
    /// </summary>
    Task<IEnumerable<Document>> GetByRelationIdAsync(Guid relationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get documents by claim ID
    /// </summary>
    Task<IEnumerable<Document>> GetByClaimIdAsync(Guid claimId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get documents by evidence ID
    /// </summary>
    Task<IEnumerable<Document>> GetByEvidenceIdAsync(Guid evidenceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get documents by document type
    /// </summary>
    Task<IEnumerable<Document>> GetByDocumentTypeAsync(int documentType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get documents by verification status
    /// </summary>
    Task<IEnumerable<Document>> GetByVerificationStatusAsync(int verificationStatus, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get verified documents
    /// </summary>
    Task<IEnumerable<Document>> GetVerifiedDocumentsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get pending verification documents
    /// </summary>
    Task<IEnumerable<Document>> GetPendingVerificationDocumentsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get expired documents
    /// </summary>
    Task<IEnumerable<Document>> GetExpiredDocumentsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get expiring soon documents (within 30 days)
    /// </summary>
    Task<IEnumerable<Document>> GetExpiringSoonDocumentsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if document exists
    /// </summary>
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add new document
    /// </summary>
    Task<Document> AddAsync(Document document, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update document
    /// </summary>
    Task UpdateAsync(Document document, CancellationToken cancellationToken = default);

    /// <summary>
    /// Save changes to database
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
