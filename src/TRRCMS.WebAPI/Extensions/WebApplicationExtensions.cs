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

            // Migrations must succeed — fail fast so schema mismatches are caught immediately.
            var context = services.GetRequiredService<ApplicationDbContext>();

            logger.LogInformation("Checking for pending database migrations.");
            await context.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully");

            // Step 1: Seed default users (independent — failure should not block permission sync)
            try
            {
                var userRepository = services.GetRequiredService<IUserRepository>();
                var passwordHasher = services.GetRequiredService<IPasswordHasher>();
                await SeedUsersIfNeeded(context, userRepository, passwordHasher, logger);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "User seeding failed (non-critical): {Message}", ex.Message);
                context.ChangeTracker.Clear(); // Clear failed state so sync can proceed
            }

            // Step 2: Sync permissions for ALL users (must run even if seeding fails)
            try
            {
                logger.LogWarning("========== STARTING USER PERMISSION SYNCHRONIZATION ==========");
                await SyncAllUserPermissions(context, logger);
                logger.LogWarning("========== USER PERMISSION SYNCHRONIZATION COMPLETED ==========");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "========== PERMISSION SYNC FAILED: {Message} ==========", ex.Message);
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

        // SCOPE 5: Seed default security policy (UC-011)
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            if (!await dbContext.SecurityPolicies.AnyAsync())
            {
                // Use the system admin user ID (the first admin created during seeding)
                var adminUser = await dbContext.Users
                    .Where(u => u.Role == UserRole.Administrator && !u.IsDeleted)
                    .FirstOrDefaultAsync();

                if (adminUser is not null)
                {
                    var defaultPolicy = SecurityPolicy.CreateDefault(adminUser.Id);
                    await dbContext.SecurityPolicies.AddAsync(defaultPolicy);
                    await dbContext.SaveChangesAsync();
                }
            }
        }

        // SCOPE 6: Seed landmark type icons
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<Program>>();

            try
            {
                var context = services.GetRequiredService<ApplicationDbContext>();
                await SeedLandmarkTypeIconsIfNeeded(context, logger);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding landmark type icons");
            }
        }

        // SCOPE 7: Warm vocabulary validation cache
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
        logger.LogWarning("Syncing permissions for {Count} users", users.Count);

        foreach (var user in users)
        {
            try
            {
                logger.LogWarning("Syncing permissions for user: {Username} (Role: {Role})",
                    user.Username, user.Role);

                await SyncUserPermissions(context, user.Id, user.Role, logger);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex,
                    "PERMISSION SYNC FAILED for user {Username}: {Message}",
                    user.Username, ex.Message);
            }
        }
    }

    private static async Task SyncUserPermissions(
        ApplicationDbContext context,
        Guid userId,
        UserRole role,
        ILogger logger)
    {
        var expectedPermissions = PermissionSeeder.GetDefaultPermissionsForRole(role).ToList();

        // Detect valid enum values for ghost permission detection
        var validEnumValues = Enum.GetValues<Permission>().ToHashSet();

        // Use IgnoreQueryFilters to see ALL permissions including soft-deleted ones.
        // The global soft-delete filter would hide IsDeleted=true records, but the
        // unique index (UserId, Permission, IsActive) WHERE IsActive=true does NOT
        // filter by IsDeleted — so we must see the full picture to avoid constraint violations.
        var allPermissions = await context.UserPermissions
            .IgnoreQueryFilters()
            .Where(up => up.UserId == userId)
            .ToListAsync();

        var activePermissions = allPermissions
            .Where(p => !p.IsDeleted && p.IsActive)
            .Select(p => p.Permission)
            .ToList();

        // PHASE 1: Detect and warn about ghost permissions (DB values not in current enum)
        var ghostPermissions = activePermissions
            .Where(p => !validEnumValues.Contains(p))
            .ToList();

        if (ghostPermissions.Any())
        {
            logger.LogWarning(
                "User {UserId} has {Count} ghost permissions (not in current Permission enum): {Codes}. These will be revoked.",
                userId, ghostPermissions.Count,
                string.Join(", ", ghostPermissions.Select(p => (int)p)));
        }

        // PHASE 2: Add missing permissions FIRST — save immediately so they persist
        //          even if removing extras fails later
        var missingPermissions = expectedPermissions
            .Except(activePermissions)
            .ToList();

        if (missingPermissions.Any())
        {
            logger.LogInformation("Adding {Count} missing permissions for user {UserId}: {Permissions}",
                missingPermissions.Count, userId,
                string.Join(", ", missingPermissions));

            foreach (var permission in missingPermissions)
            {
                var existing = allPermissions
                    .FirstOrDefault(p => p.Permission == permission);

                if (existing != null)
                {
                    if (existing.IsDeleted)
                        existing.Restore(Guid.Empty);
                    if (!existing.IsActive)
                        existing.Reactivate(Guid.Empty, "Auto-synced from PermissionSeeder");
                    logger.LogInformation("Reactivated existing permission {Permission} for user {UserId}",
                        permission, userId);
                }
                else
                {
                    var userPermission = UserPermission.Create(
                        userId: userId,
                        permission: permission,
                        grantedBy: Guid.Empty,
                        grantReason: "Auto-synced from PermissionSeeder");

                    context.UserPermissions.Add(userPermission);
                }
            }

            try
            {
                await context.SaveChangesAsync();
                logger.LogInformation("Successfully added {Count} permissions for user {UserId}",
                    missingPermissions.Count, userId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to save new permissions for user {UserId}", userId);
                return; // Don't proceed to removal if add failed
            }
        }

        // PHASE 3: Remove extra permissions (includes ghost permissions) — separate save
        var extraPermissions = activePermissions
            .Except(expectedPermissions)
            .ToList();

        if (extraPermissions.Any())
        {
            logger.LogInformation("Removing {Count} extra permissions for user {UserId}: {Permissions}",
                extraPermissions.Count, userId,
                string.Join(", ", extraPermissions.Select(p => $"{p} ({(int)p})")));

            foreach (var permission in extraPermissions)
            {
                var userPermission = allPermissions
                    .FirstOrDefault(p => p.Permission == permission && p.IsActive && !p.IsDeleted);

                if (userPermission != null)
                {
                    userPermission.Revoke(Guid.Empty, "Auto-removed by PermissionSeeder sync");
                }
            }

            try
            {
                await context.SaveChangesAsync();
                logger.LogInformation("Successfully removed {Count} extra permissions for user {UserId}",
                    extraPermissions.Count, userId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Failed to remove extra permissions for user {UserId}. New permissions were already saved successfully.",
                    userId);
            }
        }

        if (!missingPermissions.Any() && !extraPermissions.Any())
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

    private static async Task SeedLandmarkTypeIconsIfNeeded(ApplicationDbContext context, ILogger logger)
    {
        var existingTypes = await context.Set<LandmarkTypeIcon>()
            .Select(i => i.Type)
            .ToListAsync();

        logger.LogInformation("Checking landmark type icons ({Count} existing)...", existingTypes.Count);

        var systemUserId = Guid.Empty;
        var pinTop = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 32 42' width='32' height='42'>";
        var pinBot = "</svg>";

        string MakePin(string color, string symbol) =>
            $"{pinTop}<path d='M16 0C7.2 0 0 7.2 0 16c0 12 16 26 16 26s16-14 16-26C32 7.2 24.8 0 16 0z' fill='{color}'/><circle cx='16' cy='16' r='11' fill='#fff' opacity='0.3'/>{symbol}{pinBot}";

        var types = new (LandmarkType type, string nameAr, string nameEn, string svg)[]
        {
            (LandmarkType.PoliceStation, "مركز شرطة", "Police Station",
                MakePin("#3B82F6", "<path d='M16 8l-6 4v6h4v-4h4v4h4v-6z' fill='#fff'/>")),
            (LandmarkType.Mosque, "مسجد", "Mosque",
                MakePin("#10B981", "<path d='M16 8c-1 0-4 3-4 6h8c0-3-3-6-4-6z' fill='#fff'/><rect x='14' y='14' width='4' height='6' fill='#fff'/><rect x='10' y='20' width='12' height='2' fill='#fff'/>")),
            (LandmarkType.Square, "ساحة", "Square",
                MakePin("#8B5CF6", "<rect x='10' y='10' width='12' height='12' rx='1' fill='none' stroke='#fff' stroke-width='2'/><circle cx='16' cy='16' r='2' fill='#fff'/>")),
            (LandmarkType.Shop, "محل تجاري", "Shop",
                MakePin("#F59E0B", "<path d='M11 10h10l2 4H9l2-4zm-1 5h12v7H10v-7zm4 2v3h4v-3z' fill='#fff'/>")),
            (LandmarkType.School, "مدرسة", "School",
                MakePin("#EF4444", "<rect x='10' y='12' width='12' height='10' rx='1' fill='#fff'/><rect x='13' y='9' width='6' height='3' fill='#fff'/><line x1='13' y1='15' x2='19' y2='15' stroke='" + "#EF4444" + "' stroke-width='1'/><line x1='13' y1='17' x2='19' y2='17' stroke='" + "#EF4444" + "' stroke-width='1'/><line x1='13' y1='19' x2='17' y2='19' stroke='" + "#EF4444" + "' stroke-width='1'/>")),
            (LandmarkType.Clinic, "عيادة", "Clinic",
                MakePin("#EC4899", "<rect x='13' y='9' width='6' height='14' rx='1' fill='#fff'/><rect x='9' y='13' width='14' height='6' rx='1' fill='#fff'/>")),
            (LandmarkType.WaterTank, "خزان مياه", "Water Tank",
                MakePin("#06B6D4", "<path d='M16 9c-2 0-3 1-3 3v2c0 3 3 6 3 8 0-2 3-5 3-8v-2c0-2-1-3-3-3z' fill='#fff'/>")),
            (LandmarkType.FuelStation, "محطة وقود", "Fuel Station",
                MakePin("#F97316", "<rect x='10' y='10' width='8' height='12' rx='1' fill='#fff'/><path d='M18 12h2v4l2-1v6h-2v-4l-2 1z' fill='#fff'/><rect x='12' y='12' width='4' height='3' fill='" + "#F97316" + "'/>")),
            (LandmarkType.Hospital, "مستشفى", "Hospital",
                MakePin("#DC2626", "<rect x='13' y='9' width='6' height='14' rx='1' fill='#fff'/><rect x='9' y='13' width='14' height='6' rx='1' fill='#fff'/><text x='16' y='19' text-anchor='middle' fill='" + "#DC2626" + "' font-size='8' font-weight='bold' font-family='Arial'>H</text>")),
            (LandmarkType.Park, "حديقة", "Park",
                MakePin("#16A34A", "<circle cx='16' cy='12' r='4' fill='#fff'/><rect x='15' y='14' width='2' height='6' fill='#fff'/><path d='M12 20h8' stroke='#fff' stroke-width='2'/>")),
        };

        var added = 0;
        foreach (var (type, nameAr, nameEn, svg) in types)
        {
            if (existingTypes.Contains(type))
                continue;

            var icon = LandmarkTypeIcon.Create(type, svg, nameAr, nameEn, systemUserId);
            await context.Set<LandmarkTypeIcon>().AddAsync(icon);
            added++;
        }

        if (added > 0)
        {
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} new landmark type icons.", added);
        }
        else
        {
            logger.LogInformation("All landmark type icons already exist — skipping.");
        }
    }
}
