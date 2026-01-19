using Microsoft.EntityFrameworkCore;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // ==================== BASIC CRUD ====================

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.Supervisor)
            .Include(u => u.Supervisees)
            .Include(u => u.Permissions) // IMPORTANT: load user permissions
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted, cancellationToken);
    }

    public async Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Where(u => !u.IsDeleted)
            .OrderBy(u => u.FullNameArabic)
            .ToListAsync(cancellationToken);
    }

    public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
        return user;
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        // IMPORTANT:
        // Do NOT call _context.Users.Update(user).
        // That forces a full update and can trigger concurrency exceptions.
        // If user was loaded via this DbContext (normal case), EF is already tracking changes.
        // If it's detached, attach without marking Modified.
        if (_context.Entry(user).State == EntityState.Detached)
        {
            _context.Users.Attach(user);
        }

        await Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    // ==================== AUTHENTICATION QUERIES ====================

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username && !u.IsDeleted, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted, cancellationToken);
    }

    public async Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken && !u.IsDeleted, cancellationToken);
    }

    public async Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AnyAsync(u => u.Username == username && !u.IsDeleted, cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AnyAsync(u => u.Email == email && !u.IsDeleted, cancellationToken);
    }

    // ==================== ROLE-BASED QUERIES ====================

    public async Task<List<User>> GetByRoleAsync(UserRole role, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Where(u => u.Role == role && !u.IsDeleted)
            .OrderBy(u => u.FullNameArabic)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Where(u => u.IsActive && !u.IsDeleted)
            .OrderBy(u => u.FullNameArabic)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<User>> GetUsersByRoleAsync(UserRole role, bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var query = _context.Users
            .Where(u => u.Role == role && !u.IsDeleted);

        if (activeOnly)
        {
            query = query.Where(u => u.IsActive);
        }

        return await query
            .OrderBy(u => u.FullNameArabic)
            .ToListAsync(cancellationToken);
    }

    // ==================== SUPERVISOR/TEAM QUERIES ====================

    public async Task<List<User>> GetSuperviseesByIdAsync(Guid supervisorId, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Where(u => u.SupervisorUserId == supervisorId && !u.IsDeleted)
            .OrderBy(u => u.FullNameArabic)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<User>> GetByTeamAsync(string teamName, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Where(u => u.TeamName == teamName && !u.IsDeleted)
            .OrderBy(u => u.FullNameArabic)
            .ToListAsync(cancellationToken);
    }

    // ==================== TABLET ASSIGNMENT QUERIES ====================

    public async Task<User?> GetByTabletIdAsync(string tabletId, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.AssignedTabletId == tabletId && !u.IsDeleted, cancellationToken);
    }

    public async Task<List<User>> GetFieldCollectorsAsync(bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var query = _context.Users
            .Where(u => u.Role == UserRole.FieldCollector && !u.IsDeleted);

        if (activeOnly)
        {
            query = query.Where(u => u.IsActive);
        }

        return await query
            .OrderBy(u => u.FullNameArabic)
            .ToListAsync(cancellationToken);
    }

    // ==================== PERMISSION MANAGEMENT ====================

    public async Task<UserPermission?> GetUserPermissionAsync(Guid userId, Permission permission, CancellationToken cancellationToken = default)
    {
        // Use DbSet directly (don't rely on user aggregate load)
        return await _context.Set<UserPermission>()
            .FirstOrDefaultAsync(up => up.UserId == userId && up.Permission == permission, cancellationToken);
    }

    public async Task AddUserPermissionAsync(UserPermission userPermission, CancellationToken cancellationToken = default)
    {
        await _context.Set<UserPermission>().AddAsync(userPermission, cancellationToken);
    }

    public async Task UpdateUserPermissionAsync(UserPermission userPermission, CancellationToken cancellationToken = default)
    {
        _context.Set<UserPermission>().Update(userPermission);
        await Task.CompletedTask;
    }
}
