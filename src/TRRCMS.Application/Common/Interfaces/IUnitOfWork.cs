using TRRCMS.Domain.Entities;

namespace TRRCMS.Application.Common.Interfaces;

/// <summary>
/// Unit of Work interface for managing transactions across multiple repositories.
/// Ensures atomic operations - all changes succeed or all fail together.
/// 
/// Usage:
/// - Inject IUnitOfWork into command handlers
/// - Use repositories to make changes (no SaveChanges in repos)
/// - Call SaveChangesAsync once at the end to commit all changes
/// 
/// Benefits:
/// - Transaction management across multiple aggregates
/// - Single database round-trip for multiple operations
/// - Consistent state even when multiple entities are modified
/// </summary>
public interface IUnitOfWork : IDisposable
{
    // ==================== REPOSITORIES ====================

    /// <summary>
    /// Building repository - spatial data and building management
    /// </summary>
    IBuildingRepository Buildings { get; }

    /// <summary>
    /// Property unit repository - apartments, shops, offices within buildings
    /// </summary>
    IPropertyUnitRepository PropertyUnits { get; }

    /// <summary>
    /// Person repository - individuals linked to properties
    /// </summary>
    IPersonRepository Persons { get; }

    /// <summary>
    /// Household repository - household/occupancy profiles
    /// </summary>
    IHouseholdRepository Households { get; }

    /// <summary>
    /// Person-property relation repository - ownership, tenancy, etc.
    /// </summary>
    IPersonPropertyRelationRepository PersonPropertyRelations { get; }

    /// <summary>
    /// Evidence repository - documents supporting relations
    /// </summary>
    IEvidenceRepository Evidences { get; }

    /// <summary>
    /// Document repository - uploaded files and metadata
    /// </summary>
    IDocumentRepository Documents { get; }

    /// <summary>
    /// Claim repository - tenure rights claims
    /// </summary>
    IClaimRepository Claims { get; }

    /// <summary>
    /// Survey repository - field and office surveys
    /// </summary>
    ISurveyRepository Surveys { get; }

    /// <summary>
    /// User repository - system users and authentication
    /// </summary>
    IUserRepository Users { get; }

    /// <summary>
    /// Building assignment repository - field collector assignments
    /// UC-012: Assign Buildings to Field Collectors
    /// </summary>
    IBuildingAssignmentRepository BuildingAssignments { get; }

    /// <summary>
    /// Neighborhood reference data repository - spatial lookups for map navigation
    /// </summary>
    INeighborhoodRepository Neighborhoods { get; }

    // ==================== TRANSACTION OPERATIONS ====================

    /// <summary>
    /// Save all pending changes to the database in a single transaction.
    /// This is the only method that commits changes - repositories should NOT call SaveChanges.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of entities written to the database</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begin an explicit database transaction for complex operations.
    /// Use when you need to coordinate with external systems or need rollback points.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Transaction that must be committed or rolled back</returns>
    Task<IUnitOfWorkTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute a function within a transaction with automatic commit/rollback.
    /// Commits if the function succeeds, rolls back if an exception is thrown.
    /// </summary>
    /// <typeparam name="TResult">Return type</typeparam>
    /// <param name="operation">The operation to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of the operation</returns>
    Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<Task<TResult>> operation,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute an action within a transaction with automatic commit/rollback.
    /// </summary>
    /// <param name="operation">The operation to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ExecuteInTransactionAsync(
        Func<Task> operation,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents an active database transaction.
/// Must be committed or rolled back explicitly.
/// </summary>
public interface IUnitOfWorkTransaction : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Commit all changes made within this transaction.
    /// </summary>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rollback all changes made within this transaction.
    /// </summary>
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
