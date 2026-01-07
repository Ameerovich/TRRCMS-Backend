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

    // TODO: Add other entities later as we implement them
    // public DbSet<Claim> Claims => Set<Claim>();
    // etc.

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}