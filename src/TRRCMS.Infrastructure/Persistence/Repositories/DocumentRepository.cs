using Microsoft.EntityFrameworkCore;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Document entity
/// </summary>
public class DocumentRepository : IDocumentRepository
{
    private readonly ApplicationDbContext _context;

    public DocumentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get document by ID
    /// </summary>
    public async Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Documents
            .Include(d => d.Evidence)
            .Include(d => d.Person)
            .Include(d => d.PropertyUnit)
            .Include(d => d.PersonPropertyRelation)
            // .Include(d => d.Claim) // Uncomment when Claim entity is implemented
            .Include(d => d.OriginalDocument)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    /// <summary>
    /// Get all documents
    /// </summary>
    public async Task<IEnumerable<Document>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Documents
            .Include(d => d.Evidence)
            .Include(d => d.Person)
            .Include(d => d.PropertyUnit)
            .Include(d => d.PersonPropertyRelation)
            // .Include(d => d.Claim) // Uncomment when Claim entity is implemented
            .OrderByDescending(d => d.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get documents by person ID
    /// </summary>
    public async Task<IEnumerable<Document>> GetByPersonIdAsync(Guid personId, CancellationToken cancellationToken = default)
    {
        return await _context.Documents
            .Include(d => d.Evidence)
            .Include(d => d.Person)
            .Where(d => d.PersonId == personId)
            .OrderByDescending(d => d.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get documents by property unit ID
    /// </summary>
    public async Task<IEnumerable<Document>> GetByPropertyUnitIdAsync(Guid propertyUnitId, CancellationToken cancellationToken = default)
    {
        return await _context.Documents
            .Include(d => d.Evidence)
            .Include(d => d.PropertyUnit)
            .Where(d => d.PropertyUnitId == propertyUnitId)
            .OrderByDescending(d => d.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get documents by person-property relation ID
    /// </summary>
    public async Task<IEnumerable<Document>> GetByRelationIdAsync(Guid relationId, CancellationToken cancellationToken = default)
    {
        return await _context.Documents
            .Include(d => d.Evidence)
            .Include(d => d.PersonPropertyRelation)
            .Where(d => d.PersonPropertyRelationId == relationId)
            .OrderByDescending(d => d.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get documents by claim ID
    /// </summary>
    public async Task<IEnumerable<Document>> GetByClaimIdAsync(Guid claimId, CancellationToken cancellationToken = default)
    {
        return await _context.Documents
            .Include(d => d.Evidence)
            // .Include(d => d.Claim) // Uncomment when Claim entity is implemented
            .Where(d => d.ClaimId == claimId)
            .OrderByDescending(d => d.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get documents by evidence ID
    /// </summary>
    public async Task<IEnumerable<Document>> GetByEvidenceIdAsync(Guid evidenceId, CancellationToken cancellationToken = default)
    {
        return await _context.Documents
            .Include(d => d.Evidence)
            .Where(d => d.EvidenceId == evidenceId)
            .OrderByDescending(d => d.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get documents by document type
    /// </summary>
    public async Task<IEnumerable<Document>> GetByDocumentTypeAsync(int documentType, CancellationToken cancellationToken = default)
    {
        return await _context.Documents
            .Include(d => d.Evidence)
            .Where(d => (int)d.DocumentType == documentType)
            .OrderByDescending(d => d.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get documents by verification status
    /// </summary>
    public async Task<IEnumerable<Document>> GetByVerificationStatusAsync(int verificationStatus, CancellationToken cancellationToken = default)
    {
        return await _context.Documents
            .Include(d => d.Evidence)
            .Where(d => (int)d.VerificationStatus == verificationStatus)
            .OrderByDescending(d => d.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get verified documents
    /// </summary>
    public async Task<IEnumerable<Document>> GetVerifiedDocumentsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Documents
            .Include(d => d.Evidence)
            .Where(d => d.IsVerified == true)
            .OrderByDescending(d => d.VerificationDate)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get pending verification documents
    /// </summary>
    public async Task<IEnumerable<Document>> GetPendingVerificationDocumentsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Documents
            .Include(d => d.Evidence)
            .Where(d => d.VerificationStatus == VerificationStatus.Pending)
            .OrderByDescending(d => d.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get expired documents
    /// </summary>
    public async Task<IEnumerable<Document>> GetExpiredDocumentsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.Documents
            .Include(d => d.Evidence)
            .Where(d => d.ExpiryDate.HasValue && d.ExpiryDate.Value < now)
            .OrderBy(d => d.ExpiryDate)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get expiring soon documents (within 30 days)
    /// </summary>
    public async Task<IEnumerable<Document>> GetExpiringSoonDocumentsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var thirtyDaysFromNow = now.AddDays(30);
        return await _context.Documents
            .Include(d => d.Evidence)
            .Where(d => d.ExpiryDate.HasValue 
                && d.ExpiryDate.Value > now 
                && d.ExpiryDate.Value <= thirtyDaysFromNow)
            .OrderBy(d => d.ExpiryDate)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Check if document exists
    /// </summary>
    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Documents
            .AnyAsync(d => d.Id == id, cancellationToken);
    }

    /// <summary>
    /// Add new document
    /// </summary>
    public async Task<Document> AddAsync(Document document, CancellationToken cancellationToken = default)
    {
        await _context.Documents.AddAsync(document, cancellationToken);
        return document;
    }

    /// <summary>
    /// Update document
    /// </summary>
    public async Task UpdateAsync(Document document, CancellationToken cancellationToken = default)
    {
        _context.Documents.Update(document);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Save changes to database
    /// </summary>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
