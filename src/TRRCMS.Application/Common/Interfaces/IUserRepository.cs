using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Common.Interfaces;

public interface IUserRepository
{
    // ==================== BASIC CRUD ====================
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<User> AddAsync(User user, CancellationToken cancellationToken = default);
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    // ==================== AUTHENTICATION QUERIES ====================
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

    // ==================== ROLE-BASED QUERIES ====================
    Task<List<User>> GetByRoleAsync(UserRole role, CancellationToken cancellationToken = default);
    Task<List<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default);
    Task<List<User>> GetUsersByRoleAsync(UserRole role, bool activeOnly = true, CancellationToken cancellationToken = default);

    // ==================== SUPERVISOR/TEAM QUERIES ====================
    Task<List<User>> GetSuperviseesByIdAsync(Guid supervisorId, CancellationToken cancellationToken = default);
    Task<List<User>> GetByTeamAsync(string teamName, CancellationToken cancellationToken = default);

    // ==================== TABLET ASSIGNMENT QUERIES ====================
    Task<User?> GetByTabletIdAsync(string tabletId, CancellationToken cancellationToken = default);
    Task<List<User>> GetFieldCollectorsAsync(bool activeOnly = true, CancellationToken cancellationToken = default);
}