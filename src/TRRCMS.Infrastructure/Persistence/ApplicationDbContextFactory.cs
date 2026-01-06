using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TRRCMS.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for EF Core tools
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        // Use the connection string for design-time tools
        optionsBuilder.UseNpgsql("Host=localhost;Database=TRRCMS_Dev;Username=postgres;Password=3123124");

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}