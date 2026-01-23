using FluentValidation;
using TRRCMS.Application.Common.Behaviors;
using System.Reflection;
using Microsoft.OpenApi.Models;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Services;
using TRRCMS.Application.Common.Mappings;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;
using TRRCMS.Infrastructure.Authorization;
using TRRCMS.Infrastructure.Persistence;
using TRRCMS.Infrastructure.Persistence.Repositories;
using TRRCMS.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// ============== DATABASE ==============
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));


// ============== REPOSITORIES ==============
builder.Services.AddScoped<IBuildingRepository, BuildingRepository>();
builder.Services.AddScoped<IPropertyUnitRepository, PropertyUnitRepository>();
builder.Services.AddScoped<IPersonRepository, PersonRepository>();
builder.Services.AddScoped<IHouseholdRepository, HouseholdRepository>();
builder.Services.AddScoped<IPersonPropertyRelationRepository, PersonPropertyRelationRepository>();
builder.Services.AddScoped<IEvidenceRepository, EvidenceRepository>();
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IClaimRepository, ClaimRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ISurveyRepository, SurveyRepository>();

// ============== AUTHENTICATION SERVICES ==============
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<ITokenService, TokenService>();

// ============== AUTHORIZATION SERVICES ==============
// Required for CurrentUserService to access HttpContext
builder.Services.AddHttpContextAccessor();

// Service to get current authenticated user from JWT claims
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Custom authorization handler for permission-based access control
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

// File Storage Service (add after other services)
builder.Services.AddScoped<IFileStorageService, FileStorageService>();

// ============== AUDIT SERVICE ==============
// Comprehensive audit logging for all system actions
// Supports 10+ year retention requirement (FSD Section 13)
builder.Services.AddScoped<IAuditService, AuditService>();

// ============== CLAIM NUMBER GENERATOR ==============
// Sequential claim number generation using PostgreSQL sequence
// Format: CLM-YYYY-NNNNNNNNN (e.g. CLM-2026-000000001)
// Thread-safe, no collisions, survives app restarts
builder.Services.AddScoped<IClaimNumberGenerator, ClaimNumberGenerator>();

// ============== MEDIATOR ==============
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(TRRCMS.Application.Buildings.Commands.CreateBuilding.CreateBuildingCommand).Assembly);
    // Validation behavior will be added separately via IPipelineBehavior
});

// ============== AUTOMAPPER ==============
builder.Services.AddAutoMapper(typeof(MappingProfile));

// ============== FLUENT VALIDATION ==============
// Register all validators from Application assembly
builder.Services.AddValidatorsFromAssembly(
    typeof(TRRCMS.Application.Buildings.Commands.CreateBuilding.CreateBuildingCommand).Assembly);

// Add validation pipeline behavior to MediatR
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// ============== JWT AUTHENTICATION ==============
var jwtSecret = builder.Configuration["JwtSettings:Secret"]
    ?? throw new InvalidOperationException("JWT Secret is not configured");
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "TRRCMS.API";
var jwtAudience = builder.Configuration["JwtSettings:Audience"] ?? "TRRCMS.Clients";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false; // Set to true in production
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ClockSkew = TimeSpan.Zero // No tolerance for expired tokens
    };
});

// ============== AUTHORIZATION POLICIES ==============
builder.Services.AddAuthorization(options =>
{
    // ==================== CLAIMS POLICIES ====================

    options.AddPolicy("CanViewAllClaims", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.Claims_ViewAll)));

    options.AddPolicy("CanViewOwnClaims", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.Claims_ViewAssigned)));

    options.AddPolicy("CanCreateClaims", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.Claims_Create)));

    options.AddPolicy("CanEditClaims", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.Claims_Update)));

    options.AddPolicy("CanDeleteClaims", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.Claims_Delete)));

    options.AddPolicy("CanSubmitClaims", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.Claims_Submit)));

    options.AddPolicy("CanAssignClaims", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.Claims_Assign)));

    options.AddPolicy("CanVerifyClaims", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.Claims_Verify)));

    options.AddPolicy("CanApproveClaims", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.Claims_Approve)));

    options.AddPolicy("CanRejectClaims", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.Claims_Reject)));

    options.AddPolicy("CanTransitionClaims", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.Claims_Transition)));

    options.AddPolicy("CanExportClaims", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.Claims_Export)));

    options.AddPolicy("CanViewClaimHistory", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.Claims_ViewHistory)));

    // ==================== EVIDENCE POLICIES ====================

    options.AddPolicy("CanViewEvidence", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.Evidence_View)));

    options.AddPolicy("CanUploadEvidence", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.Evidence_Upload)));

    options.AddPolicy("CanVerifyEvidence", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.Evidence_Verify)));

    options.AddPolicy("CanDeleteEvidence", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.Evidence_Delete)));

    // ==================== DOCUMENT POLICIES ======================

    options.AddPolicy("CanViewSensitiveDocuments", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.Documents_ViewSensitive)));

    options.AddPolicy("CanDownloadDocuments", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.Documents_Download)));

    options.AddPolicy("CanDeleteDocuments", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.Documents_Delete)));

    // ==================== BUILDING POLICIES ====================

    options.AddPolicy("CanViewAllBuildings", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.Buildings_View)));

    options.AddPolicy("CanCreateBuildings", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.Buildings_Create)));

    options.AddPolicy("CanEditBuildings", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.Buildings_Update)));

    options.AddPolicy("CanAssignBuildings", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.Buildings_Assign)));

    options.AddPolicy("CanDeleteBuildings", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.Buildings_Delete)));

    // ==================== PERSON POLICIES ====================

    options.AddPolicy("CanViewPersons", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.Persons_View)));

    options.AddPolicy("CanCreatePersons", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.Persons_Create)));

    options.AddPolicy("CanEditPersons", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.Persons_Update)));

    options.AddPolicy("CanMergePersons", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.Persons_Merge)));

    options.AddPolicy("CanDeletePersons", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.Persons_Delete)));

    // ==================== PROPERTY UNIT POLICIES ====================

    options.AddPolicy("CanViewPropertyUnits", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.PropertyUnits_View)));

    options.AddPolicy("CanCreatePropertyUnits", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.PropertyUnits_Create)));

    options.AddPolicy("CanEditPropertyUnits", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.PropertyUnits_Update)));

    options.AddPolicy("CanMergePropertyUnits", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.PropertyUnits_Merge)));

    options.AddPolicy("CanDeletePropertyUnits", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.PropertyUnits_Delete)));

    // ==================== SURVEY POLICIES ====================

    options.AddPolicy("CanCreateSurveys", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.Surveys_Create)));

    options.AddPolicy("CanViewSurveys", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.Surveys_View)));

    options.AddPolicy("CanExportSurveys", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.Surveys_Export)));

    // ==================== USER MANAGEMENT POLICIES ====================

    options.AddPolicy("CanViewAllUsers", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.Users_View)));

    options.AddPolicy("CanCreateUsers", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.Users_Create)));

    options.AddPolicy("CanEditUsers", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.Users_Update)));

    options.AddPolicy("CanDeactivateUsers", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.Users_Deactivate)));

    // Existing role/permission management policy name
    options.AddPolicy("CanManageRoles", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.Roles_Manage)));

    // ✅ ADDED: Alias policy used by UsersController for granting/revoking permissions
    // Keeps backward compatibility (some code may still use "CanManageRoles")
    options.AddPolicy("CanManageUserRoles", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.Roles_Manage)));

    // ==================== VOCABULARY POLICIES ====================

    options.AddPolicy("CanManageVocabularies", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.Vocabularies_Manage)));

    // ==================== SECURITY POLICIES ====================

    options.AddPolicy("CanManageSecuritySettings", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.Security_Settings)));

    // ==================== AUDIT POLICIES ====================

    options.AddPolicy("CanViewAuditLogs", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.Audit_ViewAll)));

    // ==================== SYSTEM POLICIES ====================

    options.AddPolicy("CanImportData", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.System_Import)));

    options.AddPolicy("CanExportData", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.System_Export)));

    options.AddPolicy("CanBackupSystem", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.System_Backup)));

    options.AddPolicy("CanRestoreSystem", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.System_Restore)));

    // ==================== SURVEY POLICIES ====================

    options.AddPolicy("CanCreateSurveys", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.Surveys_Create)));

    options.AddPolicy("CanViewOwnSurveys", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.Surveys_ViewOwn)));

    options.AddPolicy("CanViewAllSurveys", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.Surveys_ViewAll)));

    options.AddPolicy("CanEditOwnSurveys", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.Surveys_EditOwn)));

    options.AddPolicy("CanEditAllSurveys", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.Surveys_EditAll)));

    options.AddPolicy("CanDeleteSurveys", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.Surveys_Delete)));

    options.AddPolicy("CanFinalizeSurveys", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.Surveys_Finalize)));

    options.AddPolicy("CanExportSurveys", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.Surveys_Export)));

    options.AddPolicy("CanImportSurveys", policy =>
        policy.Requirements.Add(new PermissionRequirement(Permission.Surveys_Import)));
});

// ============== CONTROLLERS ==============
builder.Services.AddControllers();

// ============== SWAGGER WITH JWT ==============
builder.Services.AddEndpointsApiExplorer();
// ============== SWAGGER WITH JWT ==============
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TRRCMS API",
        Version = "v1",
        Description = "UN-Habitat Tenure Rights Registration & Claims Management System"
    });

    // Enable XML comments for Swagger documentation
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);  // Changed 'options' to 'c'

    // Add JWT Bearer authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid JWT token.\n\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.\""
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ============== CORS (for development) ==============
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ============== HEALTH CHECKS ==============
builder.Services.AddHealthChecks()
    .AddNpgSql(
        connectionString: builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Database connection string not configured"),
        name: "postgresql",
        tags: new[] { "db", "sql", "postgresql" });

var app = builder.Build();

// ============== DATABASE SEEDING & PERMISSION SYNC ==============
// Automatically sync user permissions on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();

        // Apply pending migrations
        logger.LogInformation("Checking for pending database migrations.");
        await context.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied successfully");

        var userRepository = services.GetRequiredService<IUserRepository>();
        var passwordHasher = services.GetRequiredService<IPasswordHasher>();

        logger.LogInformation("Starting user permission synchronization.");

        // Ensure database is created
        await context.Database.MigrateAsync();

        // Seed users if they don't exist
        await SeedUsersIfNeeded(context, userRepository, passwordHasher, logger);

        // Sync all user permissions based on PermissionSeeder
        await SyncAllUserPermissions(context, logger);

        logger.LogInformation("User permission synchronization completed successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while seeding/syncing user permissions");
    }
}

// ============== MIDDLEWARE ==============
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();

// ============== AUTHENTICATION & AUTHORIZATION (IMPORTANT ORDER!) ==============
// 1. Authentication MUST come first (identifies who the user is)
app.UseAuthentication();

// 2. Authorization comes second (checks what the user can do)
app.UseAuthorization();


// ============== FILES STORAGE ==============
// Enable serving files from wwwroot
app.UseStaticFiles(); 

// ============== HEALTH CHECK ENDPOINT ==============
app.MapHealthChecks("/health");

// 3. Map controllers last
app.MapControllers();

app.Run();

// ============== HELPER METHODS FOR SEEDING ==============

static async Task SeedUsersIfNeeded(
    ApplicationDbContext context,
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ILogger logger)
{
    // Check if admin exists
    var adminExists = await userRepository.GetByUsernameAsync("admin");
    if (adminExists == null)
    {
        logger.LogInformation("Creating admin user.");

        // Hash password with salt
        string salt;
        string passwordHash = passwordHasher.HashPassword("Admin@123", out salt);

        // Correct parameter order matching User.Create() signature
        var admin = User.Create(
            username: "admin",
            fullNameArabic: "ÇáãÓÄæá ÇáÑÆíÓí",
            passwordHash: passwordHash,
            passwordSalt: salt,  // Salt passed directly as parameter
            role: UserRole.Administrator,
            hasMobileAccess: false,
            hasDesktopAccess: true,
            email: "admin@trrcms.local",
            phoneNumber: null,
            createdByUserId: Guid.Empty
        );

        // Set English name using UpdateProfile (since Create doesn't accept it)
        admin.UpdateProfile(
            fullNameArabic: "ÇáãÓÄæá ÇáÑÆíÓí",
            fullNameEnglish: "System Administrator",
            email: "admin@trrcms.local",
            phoneNumber: null,
            organization: "UN-Habitat",
            jobTitle: "Administrator",
            modifiedByUserId: Guid.Empty
        );

        context.Users.Add(admin);
        await context.SaveChangesAsync();

        logger.LogInformation("Admin user created successfully");
    }

    // Check if analyst exists
    var analystExists = await userRepository.GetByUsernameAsync("analyst");
    if (analystExists == null)
    {
        logger.LogInformation("Creating analyst user.");

        // Hash password with salt
        string salt;
        string passwordHash = passwordHasher.HashPassword("Analyst@123", out salt);

        var analyst = User.Create(
            username: "analyst",
            fullNameArabic: "ÇáãÍáá ÇáäÙÇã",
            passwordHash: passwordHash,
            passwordSalt: salt,
            role: UserRole.Analyst,
            hasMobileAccess: false,
            hasDesktopAccess: true,
            email: "analyst@trrcms.local",
            phoneNumber: null,
            createdByUserId: Guid.Empty
        );

        analyst.UpdateProfile(
            fullNameArabic: "ÇáãÍáá ÇáäÙÇã",
            fullNameEnglish: "System Analyst",
            email: "analyst@trrcms.local",
            phoneNumber: null,
            organization: "UN-Habitat",
            jobTitle: "Analyst",
            modifiedByUserId: Guid.Empty
        );

        context.Users.Add(analyst);
        await context.SaveChangesAsync();

        logger.LogInformation("Analyst user created successfully");
    }
}

static async Task SyncAllUserPermissions(ApplicationDbContext context, ILogger logger)
{
    // Get all users
    var users = await context.Users.ToListAsync();

    foreach (var user in users)
    {
        logger.LogInformation("Syncing permissions for user: {Username} (Role: {Role})",
            user.Username, user.Role);

        await SyncUserPermissions(context, user.Id, user.Role, logger);
    }
}

static async Task SyncUserPermissions(
    ApplicationDbContext context,
    Guid userId,
    UserRole role,
    ILogger logger)
{
    // Get expected permissions for this role
    var expectedPermissions = PermissionSeeder.GetDefaultPermissionsForRole(role);

    // Get current permissions
    var currentPermissions = await context.UserPermissions
        .Where(up => up.UserId == userId)
        .Select(up => up.Permission)
        .ToListAsync();

    // Find missing permissions
    var missingPermissions = expectedPermissions
        .Except(currentPermissions)
        .ToList();

    // Add missing permissions
    if (missingPermissions.Any())
    {
        logger.LogInformation("Adding {Count} missing permissions for user {UserId}",
            missingPermissions.Count, userId);

        foreach (var permission in missingPermissions)
        {
            // Use factory method Create
            var userPermission = UserPermission.Create(
                userId: userId,
                permission: permission,
                grantedBy: Guid.Empty,
                grantReason: "Auto-synced from PermissionSeeder");

            context.UserPermissions.Add(userPermission);
        }
    }

    // Remove extra permissions (permissions that shouldn't be there)
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

    // Save changes
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
