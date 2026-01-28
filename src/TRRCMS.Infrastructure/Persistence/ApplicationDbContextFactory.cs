using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace TRRCMS.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for EF Core tools (migrations)
/// Reads connection string from appsettings.Development.json or environment variable
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Build configuration from appsettings files
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../TRRCMS.WebAPI"))
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        // Get connection string from configuration
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // Fallback to environment variable if not found in config
        if (string.IsNullOrEmpty(connectionString))
        {
            connectionString = Environment.GetEnvironmentVariable("TRRCMS_CONNECTION_STRING");
        }

        // Throw helpful error if no connection string found
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string not found. Please ensure:\n" +
                "1. appsettings.Development.json exists in TRRCMS.WebAPI folder with 'DefaultConnection', OR\n" +
                "2. Environment variable 'TRRCMS_CONNECTION_STRING' is set.\n\n" +
                "See SETUP_GUIDE.md for instructions.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.UseNetTopologySuite(); // Enable PostGIS support
        });

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}