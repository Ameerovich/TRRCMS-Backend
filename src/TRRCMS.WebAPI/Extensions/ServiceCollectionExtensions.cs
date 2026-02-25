using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TRRCMS.Application.Common.Behaviors;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Mappings;
using TRRCMS.Application.Common.Services;
using TRRCMS.Application.Import.Models;
using TRRCMS.Domain.Enums;
using TRRCMS.Infrastructure.Authorization;
using TRRCMS.Infrastructure.Persistence;
using TRRCMS.Infrastructure.Persistence.Repositories;
using TRRCMS.Infrastructure.Services;
using TRRCMS.Infrastructure.Services.Validators;

namespace TRRCMS.WebAPI.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers infrastructure services: DbContext, repositories, file storage,
    /// audit, import pipeline, sync, vocabulary, and admin hierarchy seeding.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        // ── Database (PostgreSQL + PostGIS) ──────────────────────────
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                    npgsqlOptions.UseNetTopologySuite();
                }));

        // ── Request body size limits ─────────────────────────────────
        services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
        {
            options.MultipartBodyLengthLimit = 500L * 1024 * 1024; // 500 MB
        });

        // ── Unit of Work ─────────────────────────────────────────────
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // ── Core repositories ────────────────────────────────────────
        services.AddScoped<IBuildingRepository, BuildingRepository>();
        services.AddScoped<IPropertyUnitRepository, PropertyUnitRepository>();
        services.AddScoped<IPersonRepository, PersonRepository>();
        services.AddScoped<IHouseholdRepository, HouseholdRepository>();
        services.AddScoped<IPersonPropertyRelationRepository, PersonPropertyRelationRepository>();
        services.AddScoped<IEvidenceRepository, EvidenceRepository>();
        services.AddScoped<IEvidenceRelationRepository, EvidenceRelationRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IClaimRepository, ClaimRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ISurveyRepository, SurveyRepository>();
        services.AddScoped<INeighborhoodRepository, NeighborhoodRepository>();
        services.AddScoped<IVocabularyRepository, VocabularyRepository>();
        services.AddScoped<IBuildingAssignmentRepository, BuildingAssignmentRepository>();

        // ── Administrative hierarchy repositories ────────────────────
        services.AddScoped<IGovernorateRepository, GovernorateRepository>();
        services.AddScoped<IDistrictRepository, DistrictRepository>();
        services.AddScoped<ISubDistrictRepository, SubDistrictRepository>();
        services.AddScoped<ICommunityRepository, CommunityRepository>();

        // ── Vocabulary services ──────────────────────────────────────
        services.AddScoped<IVocabularyVersionProvider, DatabaseVocabularyVersionProvider>();
        services.AddSingleton<IVocabularyValidationService, CachedVocabularyValidationService>();

        // ── Data seeding ─────────────────────────────────────────────
        services.AddScoped<TRRCMS.Infrastructure.Data.AdministrativeHierarchySeeder>();

        // ── Geometry conversion ─────────────────────────────────────
        services.AddSingleton<IGeometryConverter, GeometryConverter>();

        // ── File storage & audit ─────────────────────────────────────
        services.AddScoped<IFileStorageService, FileStorageService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IClaimNumberGenerator, ClaimNumberGenerator>();
        services.AddScoped<ISurveyReferenceCodeGenerator, SurveyReferenceCodeGenerator>();

        // ── Import pipeline ──────────────────────────────────────────
        services.AddScoped(typeof(IStagingRepository<>), typeof(StagingRepository<>));
        services.AddScoped<IImportPackageRepository, ImportPackageRepository>();
        services.AddScoped<IConflictResolutionRepository, ConflictResolutionRepository>();
        services.Configure<ImportPipelineSettings>(
            configuration.GetSection(ImportPipelineSettings.SectionName));
        services.AddScoped<IImportService, ImportService>();
        services.AddScoped<IStagingService, StagingService>();
        services.AddScoped<IValidationPipeline, ValidationPipeline>();

        // ── Staging validators ───────────────────────────────────────
        services.AddScoped<IStagingValidator, DataConsistencyValidator>();
        services.AddScoped<IStagingValidator, CrossEntityRelationValidator>();
        services.AddScoped<IStagingValidator, OwnershipEvidenceValidator>();
        services.AddScoped<IStagingValidator, HouseholdStructureValidator>();
        services.AddScoped<IStagingValidator, SpatialGeometryValidator>();
        services.AddScoped<IStagingValidator, ClaimLifecycleValidator>();
        services.AddScoped<IStagingValidator, VocabularyVersionValidator>();
        services.AddScoped<IStagingValidator, BuildingUnitCodeValidator>();

        // ── Matching & duplicate detection ───────────────────────────
        services.AddScoped<TRRCMS.Infrastructure.Services.Matching.PersonMatchingService>();
        services.AddScoped<TRRCMS.Infrastructure.Services.Matching.PropertyMatchingService>();
        services.AddScoped<IDuplicateDetectionService, DuplicateDetectionService>();
        services.AddScoped<ICommitService, CommitService>();

        // ── Merge services (Property,Person) ─────────
        services.AddScoped<IMergeService, TRRCMS.Infrastructure.Services.Merge.PersonMergeService>();
        services.AddScoped<IMergeService, TRRCMS.Infrastructure.Services.Merge.PropertyMergeService>();

        // ── Sync ─────────────────────────────────────────────────────
        services.AddScoped<ISyncSessionRepository, SyncSessionRepository>();
        services.AddScoped<ISyncPackageStore, LocalSyncPackageStore>();

        return services;
    }

    /// <summary>
    /// Registers application-layer services: MediatR, AutoMapper,
    /// FluentValidation, and the validation pipeline behavior.
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var applicationAssembly = typeof(TRRCMS.Application.Buildings.Commands.CreateBuilding.CreateBuildingCommand).Assembly;

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(applicationAssembly));

        services.AddAutoMapper(typeof(MappingProfile));

        services.AddValidatorsFromAssembly(applicationAssembly);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }

    /// <summary>
    /// Registers authentication (JWT Bearer) and authorization
    /// (permission-based policies). Also registers the current-user service.
    /// </summary>
    public static IServiceCollection AddAuthenticationAndAuthorization(
        this IServiceCollection services, IConfiguration configuration)
    {
        // ── Auth support services ────────────────────────────────────
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        // ── JWT Bearer authentication ────────────────────────────────
        var jwtSecret = configuration["JwtSettings:Secret"]
            ?? throw new InvalidOperationException("JWT Secret is not configured");
        var jwtIssuer = configuration["JwtSettings:Issuer"] ?? "TRRCMS.API";
        var jwtAudience = configuration["JwtSettings:Audience"] ?? "TRRCMS.Clients";

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                ClockSkew = TimeSpan.Zero
            };
        });

        // ── Authorization policies ───────────────────────────────────
        services.AddAuthorization(options =>
        {
            // Claims
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

            // Evidence
            options.AddPolicy("CanViewEvidence", policy =>
                policy.Requirements.Add(new PermissionRequirement(Permission.Evidence_View)));
            options.AddPolicy("CanUploadEvidence", policy =>
                policy.Requirements.Add(new PermissionRequirement(Permission.Evidence_Upload)));
            options.AddPolicy("CanVerifyEvidence", policy =>
                policy.Requirements.Add(new PermissionRequirement(Permission.Evidence_Verify)));
            options.AddPolicy("CanDeleteEvidence", policy =>
                policy.Requirements.Add(new PermissionRequirement(Permission.Evidence_Delete)));

            // Documents
            options.AddPolicy("CanViewSensitiveDocuments", policy =>
                policy.Requirements.Add(new PermissionRequirement(Permission.Documents_ViewSensitive)));
            options.AddPolicy("CanDownloadDocuments", policy =>
                policy.Requirements.Add(new PermissionRequirement(Permission.Documents_Download)));
            options.AddPolicy("CanDeleteDocuments", policy =>
                policy.Requirements.Add(new PermissionRequirement(Permission.Documents_Delete)));

            // Buildings
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

            // Persons
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

            // Property Units
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

            // Surveys
            options.AddPolicy("CanCreateSurveys", policy =>
                policy.Requirements.Add(new PermissionRequirement(Permission.Surveys_Create)));
            options.AddPolicy("CanViewSurveys", policy =>
                policy.Requirements.Add(new PermissionRequirement(Permission.Surveys_View)));
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

            // User Management
            options.AddPolicy("CanViewAllUsers", policy =>
                policy.Requirements.Add(new PermissionRequirement(Permission.Users_View)));
            options.AddPolicy("CanCreateUsers", policy =>
                policy.Requirements.Add(new PermissionRequirement(Permission.Users_Create)));
            options.AddPolicy("CanEditUsers", policy =>
                policy.Requirements.Add(new PermissionRequirement(Permission.Users_Update)));
            options.AddPolicy("CanDeactivateUsers", policy =>
                policy.Requirements.Add(new PermissionRequirement(Permission.Users_Deactivate)));
            options.AddPolicy("CanManageRoles", policy =>
                policy.Requirements.Add(new PermissionRequirement(Permission.Roles_Manage)));
            options.AddPolicy("CanManageUserRoles", policy =>
                policy.Requirements.Add(new PermissionRequirement(Permission.Roles_Manage)));

            // Vocabularies
            options.AddPolicy("CanManageVocabularies", policy =>
                policy.Requirements.Add(new PermissionRequirement(Permission.Vocabularies_Manage)));

            // Security
            options.AddPolicy("CanManageSecuritySettings", policy =>
                policy.Requirements.Add(new PermissionRequirement(Permission.Security_Settings)));

            // Audit
            options.AddPolicy("CanViewAuditLogs", policy =>
                policy.Requirements.Add(new PermissionRequirement(Permission.Audit_ViewAll)));

            // System
            options.AddPolicy("CanImportData", policy =>
                policy.Requirements.Add(new PermissionRequirement(Permission.System_Import)));
            options.AddPolicy("CanExportData", policy =>
                policy.Requirements.Add(new PermissionRequirement(Permission.System_Export)));
            options.AddPolicy("CanBackupSystem", policy =>
                policy.Requirements.Add(new PermissionRequirement(Permission.System_Backup)));
            options.AddPolicy("CanRestoreSystem", policy =>
                policy.Requirements.Add(new PermissionRequirement(Permission.System_Restore)));

            // Sync
            options.AddPolicy("CanSyncData", policy =>
                policy.Requirements.Add(new PermissionRequirement(Permission.System_Sync)));
        });

        return services;
    }
}
