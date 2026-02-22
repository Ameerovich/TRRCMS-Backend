using Microsoft.EntityFrameworkCore;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;
using TRRCMS.Infrastructure.Persistence;
using TRRCMS.Application.Common.Services;
using TRRCMS.Infrastructure.Persistence.SeedData;

namespace TRRCMS.WebAPI.Extensions;

public static class WebApplicationExtensions
{
    /// <summary>
    /// Applies pending migrations and seeds reference data
    /// (users, permissions, neighborhoods, vocabularies, admin hierarchy).
    /// </summary>
    public static async Task SeedDatabaseAsync(this WebApplication app)
    {
        // SCOPE 1: Migrations + Users + Permissions
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<Program>>();

            try
            {
                var context = services.GetRequiredService<ApplicationDbContext>();

                logger.LogInformation("Checking for pending database migrations.");
                await context.Database.MigrateAsync();
                logger.LogInformation("Database migrations applied successfully");

                var userRepository = services.GetRequiredService<IUserRepository>();
                var passwordHasher = services.GetRequiredService<IPasswordHasher>();

                logger.LogInformation("Starting user permission synchronization.");
                await SeedUsersIfNeeded(context, userRepository, passwordHasher, logger);
                await SyncAllUserPermissions(context, logger);
                logger.LogInformation("User permission synchronization completed successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding/syncing user permissions");
            }
        }

        // SCOPE 2: Neighborhood seeding
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<Program>>();

            try
            {
                var context = services.GetRequiredService<ApplicationDbContext>();
                await SeedNeighborhoodsIfNeeded(context, logger);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding neighborhoods");
            }
        }

        // SCOPE 3: Vocabulary seeding
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<Program>>();

            try
            {
                var context = services.GetRequiredService<ApplicationDbContext>();
                await VocabularySeedData.SeedAsync(context);
                logger.LogInformation("Vocabulary seed data applied successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding vocabularies");
            }
        }

        // SCOPE 4: Administrative hierarchy seeding
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<Program>>();

            try
            {
                var seeder = services.GetRequiredService<TRRCMS.Infrastructure.Data.AdministrativeHierarchySeeder>();
                await seeder.SeedAsync();
                logger.LogInformation("Administrative hierarchy seed data applied successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding administrative hierarchy");
            }
        }

        // SCOPE 5: Warm vocabulary validation cache
        {
            var vocabValidation = app.Services.GetRequiredService<IVocabularyValidationService>();
            await vocabValidation.WarmupAsync();
        }
    }

    // ── Seeding helpers ──────────────────────────────────────────────

    private static async Task SeedUsersIfNeeded(
        ApplicationDbContext context,
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ILogger logger)
    {
        var testUsers = new[]
        {
            new { Username = "admin",       Password = "Admin@123",   NameAr = "المسؤول الرئيسي",         NameEn = "System Administrator",  Email = "admin@trrcms.local",       Phone = (string?)null,            Role = UserRole.Administrator,   Mobile = false, Desktop = true,  Org = "UN-Habitat",          Job = "Administrator" },
            new { Username = "datamanager", Password = "Data@123",    NameAr = "مدير البيانات",            NameEn = "Data Manager",          Email = "datamanager@trrcms.local", Phone = (string?)"+963-11-2234567", Role = UserRole.DataManager,     Mobile = false, Desktop = true,  Org = "UN-Habitat",          Job = "Data Manager" },
            new { Username = "clerk",       Password = "Clerk@123",   NameAr = "موظف المكتب",             NameEn = "Office Clerk",          Email = "clerk@trrcms.local",       Phone = (string?)"+963-11-3234567", Role = UserRole.OfficeClerk,     Mobile = false, Desktop = true,  Org = "Aleppo Municipality", Job = "Office Clerk" },
            new { Username = "collector",   Password = "Field@123",   NameAr = "جامع البيانات الميداني",   NameEn = "Field Data Collector",  Email = "collector@trrcms.local",   Phone = (string?)"+963-11-4234567", Role = UserRole.FieldCollector,  Mobile = true,  Desktop = false, Org = "UN-Habitat",          Job = "Field Collector" },
            new { Username = "supervisor",  Password = "Super@123",   NameAr = "المشرف الميداني",          NameEn = "Field Supervisor",      Email = "supervisor@trrcms.local",  Phone = (string?)"+963-11-5234567", Role = UserRole.FieldSupervisor, Mobile = false, Desktop = true,  Org = "UN-Habitat",          Job = "Field Supervisor" },
            new { Username = "analyst",     Password = "Analyst@123", NameAr = "المحلل",                  NameEn = "Data Analyst",          Email = "analyst@trrcms.local",     Phone = (string?)"+963-11-6234567", Role = UserRole.Analyst,         Mobile = false, Desktop = true,  Org = "UN-Habitat",          Job = "Data Analyst" }
        };

        foreach (var u in testUsers)
        {
            var existing = await userRepository.GetByUsernameAsync(u.Username);
            if (existing != null)
            {
                logger.LogInformation("User '{Username}' already exists — skipping.", u.Username);
                continue;
            }

            logger.LogInformation("Creating user '{Username}' ({Role}).", u.Username, u.Role);

            string passwordHash = passwordHasher.HashPassword(u.Password, out string salt);

            var user = User.Create(
                username: u.Username,
                fullNameArabic: u.NameAr,
                passwordHash: passwordHash,
                passwordSalt: salt,
                role: u.Role,
                hasMobileAccess: u.Mobile,
                hasDesktopAccess: u.Desktop,
                email: u.Email,
                phoneNumber: u.Phone,
                createdByUserId: Guid.Empty
            );

            user.UpdateProfile(
                fullNameArabic: u.NameAr,
                fullNameEnglish: u.NameEn,
                email: u.Email,
                phoneNumber: u.Phone,
                organization: u.Org,
                jobTitle: u.Job,
                modifiedByUserId: Guid.Empty
            );

            context.Users.Add(user);
            await context.SaveChangesAsync();

            logger.LogInformation("User '{Username}' created successfully.", u.Username);
        }
    }

    private static async Task SyncAllUserPermissions(ApplicationDbContext context, ILogger logger)
    {
        var users = await context.Users.ToListAsync();

        foreach (var user in users)
        {
            logger.LogInformation("Syncing permissions for user: {Username} (Role: {Role})",
                user.Username, user.Role);

            await SyncUserPermissions(context, user.Id, user.Role, logger);
        }
    }

    private static async Task SyncUserPermissions(
        ApplicationDbContext context,
        Guid userId,
        UserRole role,
        ILogger logger)
    {
        var expectedPermissions = PermissionSeeder.GetDefaultPermissionsForRole(role);

        var currentPermissions = await context.UserPermissions
            .Where(up => up.UserId == userId)
            .Select(up => up.Permission)
            .ToListAsync();

        // Add missing permissions
        var missingPermissions = expectedPermissions
            .Except(currentPermissions)
            .ToList();

        if (missingPermissions.Any())
        {
            logger.LogInformation("Adding {Count} missing permissions for user {UserId}",
                missingPermissions.Count, userId);

            foreach (var permission in missingPermissions)
            {
                var userPermission = UserPermission.Create(
                    userId: userId,
                    permission: permission,
                    grantedBy: Guid.Empty,
                    grantReason: "Auto-synced from PermissionSeeder");

                context.UserPermissions.Add(userPermission);
            }
        }

        // Remove extra permissions
        var extraPermissions = currentPermissions
            .Except(expectedPermissions)
            .ToList();

        if (extraPermissions.Any())
        {
            logger.LogInformation("Removing {Count} extra permissions for user {UserId}",
                extraPermissions.Count, userId);

            foreach (var permission in extraPermissions)
            {
                var userPermission = await context.UserPermissions
                    .FirstOrDefaultAsync(up => up.UserId == userId && up.Permission == permission);

                if (userPermission != null)
                {
                    context.UserPermissions.Remove(userPermission);
                }
            }
        }

        if (missingPermissions.Any() || extraPermissions.Any())
        {
            await context.SaveChangesAsync();
            logger.LogInformation("Permission sync completed for user {UserId}", userId);
        }
        else
        {
            logger.LogInformation("No permission changes needed for user {UserId}", userId);
        }
    }

    private static async Task SeedNeighborhoodsIfNeeded(ApplicationDbContext context, ILogger logger)
    {
        var hasNeighborhoods = await context.Neighborhoods.AnyAsync();
        if (hasNeighborhoods)
        {
            logger.LogInformation("Neighborhoods already seeded — skipping.");
            return;
        }

        logger.LogInformation("Seeding Aleppo neighborhood reference data (20 neighborhoods)...");

        var systemUserId = Guid.Empty;
        var neighborhoods = NeighborhoodSeedData.GetAleppoNeighborhoods(systemUserId);

        foreach (var neighborhood in neighborhoods)
        {
            context.Neighborhoods.Add(neighborhood);
        }

        await context.SaveChangesAsync();
        logger.LogInformation("Successfully seeded {Count} neighborhoods.", neighborhoods.Count);
    }
}
