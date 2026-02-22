using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Infrastructure.Persistence.Repositories;

namespace TRRCMS.Infrastructure.Persistence;

/// <summary>
/// Unit of Work implementation using Entity Framework Core.
/// Coordinates work of multiple repositories and manages transactions.
/// 
/// This class:
/// - Provides access to all repositories through a single instance
/// - Ensures all repositories share the same DbContext
/// - Manages transaction lifetime
/// - Provides atomic SaveChanges across all repositories
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private bool _disposed;

    // Lazy-initialized repositories
    private IBuildingRepository? _buildings;
    private IPropertyUnitRepository? _propertyUnits;
    private IPersonRepository? _persons;
    private IHouseholdRepository? _households;
    private IPersonPropertyRelationRepository? _personPropertyRelations;
    private IEvidenceRepository? _evidences;
    private IDocumentRepository? _documents;
    private IClaimRepository? _claims;
    private ISurveyRepository? _surveys;
    private IUserRepository? _users;
    private IBuildingAssignmentRepository? _buildingAssignments;
    private INeighborhoodRepository? _neighborhoods;
    private IVocabularyRepository? _vocabularies;
    private ISyncSessionRepository? _syncSessions;
    private IEvidenceRelationRepository? _evidenceRelations;
    private IImportPackageRepository? _importPackages;
    private IConflictResolutionRepository? _conflictResolutions;


    public UnitOfWork(ApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    // ==================== REPOSITORY PROPERTIES ====================
    // Lazy initialization ensures repositories are only created when needed
    // All repositories share the same DbContext instance

    public IBuildingRepository Buildings =>
        _buildings ??= new BuildingRepository(_context);

    public IPropertyUnitRepository PropertyUnits =>
        _propertyUnits ??= new PropertyUnitRepository(_context);

    public IPersonRepository Persons =>
        _persons ??= new PersonRepository(_context);

    public IHouseholdRepository Households =>
        _households ??= new HouseholdRepository(_context);

    public IPersonPropertyRelationRepository PersonPropertyRelations =>
        _personPropertyRelations ??= new PersonPropertyRelationRepository(_context, _currentUserService);

    public IEvidenceRepository Evidences =>
        _evidences ??= new EvidenceRepository(_context, _currentUserService);

    public IDocumentRepository Documents =>
        _documents ??= new DocumentRepository(_context);

    public IClaimRepository Claims =>
        _claims ??= new ClaimRepository(_context);

    public ISurveyRepository Surveys =>
        _surveys ??= new SurveyRepository(_context);

    public IUserRepository Users =>
        _users ??= new UserRepository(_context);

    public IBuildingAssignmentRepository BuildingAssignments =>
        _buildingAssignments ??= new BuildingAssignmentRepository(_context);

    public INeighborhoodRepository Neighborhoods =>
        _neighborhoods ??= new NeighborhoodRepository(_context);

    public IVocabularyRepository Vocabularies =>
        _vocabularies ??= new VocabularyRepository(_context);
    public ISyncSessionRepository SyncSessions =>
    _syncSessions ??= new SyncSessionRepository(_context);

    public IEvidenceRelationRepository EvidenceRelations =>
        _evidenceRelations ??= new EvidenceRelationRepository(_context);

    public IImportPackageRepository ImportPackages =>
        _importPackages ??= new ImportPackageRepository(_context);

    public IConflictResolutionRepository ConflictResolutions =>
        _conflictResolutions ??= new ConflictResolutionRepository(_context);


    // ==================== TRANSACTION OPERATIONS ====================

    /// <summary>
    /// Save all pending changes to the database.
    /// This is the single point where changes are committed.
    /// </summary>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Begin an explicit transaction for complex operations.
    /// </summary>
    public async Task<IUnitOfWorkTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        return new UnitOfWorkTransaction(transaction);
    }

    /// <summary>
    /// Execute operation within a transaction with automatic commit/rollback.
    /// </summary>
    public async Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<Task<TResult>> operation,
        CancellationToken cancellationToken = default)
    {
        // Use EF Core's execution strategy for retry logic (handles transient failures)
        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var result = await operation();
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return result;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }

    /// <summary>
    /// Execute action within a transaction with automatic commit/rollback.
    /// </summary>
    public async Task ExecuteInTransactionAsync(
        Func<Task> operation,
        CancellationToken cancellationToken = default)
    {
        var strategy = _context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                await operation();
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }

    // ==================== DISPOSAL ====================

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _context.Dispose();
        }
        _disposed = true;
    }
}

/// <summary>
/// Wrapper for EF Core transaction to implement IUnitOfWorkTransaction.
/// </summary>
internal class UnitOfWorkTransaction : IUnitOfWorkTransaction
{
    private readonly IDbContextTransaction _transaction;
    private bool _disposed;

    public UnitOfWorkTransaction(IDbContextTransaction transaction)
    {
        _transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await _transaction.CommitAsync(cancellationToken);
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        await _transaction.RollbackAsync(cancellationToken);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _transaction.Dispose();
            _disposed = true;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            await _transaction.DisposeAsync();
            _disposed = true;
        }
    }
}
