using Microsoft.EntityFrameworkCore;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of <see cref="ISecurityPolicyRepository"/>.
/// Uses the versioned singleton pattern: only one active policy at any time.
/// </summary>
public class SecurityPolicyRepository : ISecurityPolicyRepository
{
    private readonly ApplicationDbContext _context;

    public SecurityPolicyRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<SecurityPolicy?> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SecurityPolicies
            .Where(sp => !sp.IsDeleted && sp.IsActive)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<SecurityPolicy?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.SecurityPolicies
            .Where(sp => !sp.IsDeleted && sp.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<int> GetLatestVersionNumberAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SecurityPolicies
            .Where(sp => !sp.IsDeleted)
            .OrderByDescending(sp => sp.Version)
            .Select(sp => sp.Version)
            .FirstOrDefaultAsync(cancellationToken); // Returns 0 if no rows
    }

    public async Task<List<SecurityPolicy>> GetVersionHistoryAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SecurityPolicies
            .Where(sp => !sp.IsDeleted)
            .OrderByDescending(sp => sp.Version)
            .ToListAsync(cancellationToken);
    }

    public async Task<SecurityPolicy> AddAsync(SecurityPolicy policy, CancellationToken cancellationToken = default)
    {
        var entry = await _context.SecurityPolicies.AddAsync(policy, cancellationToken);
        return entry.Entity;
    }

    public Task UpdateAsync(SecurityPolicy policy, CancellationToken cancellationToken = default)
    {
        _context.SecurityPolicies.Update(policy);
        return Task.CompletedTask;
    }
}
