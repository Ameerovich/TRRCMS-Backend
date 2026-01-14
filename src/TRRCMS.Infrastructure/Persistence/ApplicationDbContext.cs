using Microsoft.EntityFrameworkCore;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets for entities
    public DbSet<Building> Buildings => Set<Building>();
    public DbSet<PropertyUnit> PropertyUnits { get; set; }
    public DbSet<PersonPropertyRelation> PersonPropertyRelations => Set<PersonPropertyRelation>();
    public DbSet<Household> Households => Set<Household>();
    public DbSet<Person> Persons => Set<Person>();
    public DbSet<Evidence> Evidences => Set<Evidence>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<Claim> Claims => Set<Claim>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserPermission> UserPermissions => Set<UserPermission>();

    // ==================== NEW: AUDIT LOG ====================
    /// <summary>
    /// Audit logs for comprehensive system action tracking
    /// Supports 10+ year retention requirement per FSD Section 13
    /// </summary>
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // TODO: Add other entities later as we implement them
    // public DbSet<Certificate> Certificates => Set<Certificate>();
    // etc.

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
