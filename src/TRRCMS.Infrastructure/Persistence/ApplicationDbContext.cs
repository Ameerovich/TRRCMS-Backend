using Microsoft.EntityFrameworkCore;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Entities.Staging;

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
    public DbSet<Survey> Surveys => Set<Survey>();

    // ==================== BUILDING ASSIGNMENTS ====================
    /// <summary>
    /// Building assignments for field collectors
    /// UC-012: Assign Buildings to Field Collectors
    /// </summary>
    public DbSet<BuildingAssignment> BuildingAssignments => Set<BuildingAssignment>();

    // ==================== NEIGHBORHOODS (Reference Data) ====================
    /// <summary>
    /// Neighborhood reference data with PostGIS boundary polygons.
    /// Used for map navigation and building location validation.
    /// </summary>
    public DbSet<Neighborhood> Neighborhoods => Set<Neighborhood>();

    // ==================== ADMINISTRATIVE HIERARCHY (Reference Data) ====================
    /// <summary>
    /// Governorate (محافظة) - Top-level administrative division
    /// </summary>
    public DbSet<Governorate> Governorates => Set<Governorate>();

    /// <summary>
    /// District (منطقة/قضاء) - Second-level administrative division under Governorate
    /// </summary>
    public DbSet<District> Districts => Set<District>();

    /// <summary>
    /// SubDistrict (ناحية) - Third-level administrative division under District
    /// </summary>
    public DbSet<SubDistrict> SubDistricts => Set<SubDistrict>();

    /// <summary>
    /// Community (قرية/مجتمع) - Fourth-level administrative division under SubDistrict
    /// </summary>
    public DbSet<Community> Communities => Set<Community>();

    // ==================== NEW: AUDIT LOG ====================
    /// <summary>
    /// Audit logs for comprehensive system action tracking
    /// Supports 10+ year retention requirement per FSD Section 13
    /// </summary>
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // ==================== VOCABULARIES ====================
    /// <summary>
    /// Controlled vocabularies with semantic versioning.
    /// Provides bilingual labels for enum-backed dropdowns.
    /// </summary>
    public DbSet<Vocabulary> Vocabularies => Set<Vocabulary>();

    // TODO: Add other entities later as we implement them
    // public DbSet<Certificate> Certificates => Set<Certificate>();
    // etc.

    // ==================== IMPORT PIPELINE ====================

    /// <summary>
    /// Import packages tracking the full .uhc import lifecycle.
    /// Referenced in UC-003 and FSD FR-D-2 through FR-D-4.
    /// </summary>
    public DbSet<ImportPackage> ImportPackages => Set<ImportPackage>();

    /// <summary>
    /// Conflict resolution records for duplicate detection and merge decisions.
    /// Referenced in UC-007, UC-008, and FSD FR-D-7.
    /// </summary>
    public DbSet<ConflictResolution> ConflictResolutions => Set<ConflictResolution>();

    // ==================== STAGING ENTITIES (Import Pipeline) ====================

    /// <summary>
    /// Staging area for Building records from .uhc packages.
    /// Isolated from production until validation and commit.
    /// </summary>
    public DbSet<StagingBuilding> StagingBuildings => Set<StagingBuilding>();

    /// <summary>
    /// Staging area for PropertyUnit records from .uhc packages.
    /// </summary>
    public DbSet<StagingPropertyUnit> StagingPropertyUnits => Set<StagingPropertyUnit>();

    /// <summary>
    /// Staging area for Person records from .uhc packages.
    /// Central to duplicate detection per FSD FR-D-5.
    /// </summary>
    public DbSet<StagingPerson> StagingPersons => Set<StagingPerson>();

    /// <summary>
    /// Staging area for Household records from .uhc packages.
    /// Subject to household structure validation (FR-D-4 Level 4).
    /// </summary>
    public DbSet<StagingHousehold> StagingHouseholds => Set<StagingHousehold>();

    /// <summary>
    /// Staging area for PersonPropertyRelation records from .uhc packages.
    /// Subject to cross-entity relation and ownership evidence validation.
    /// </summary>
    public DbSet<StagingPersonPropertyRelation> StagingPersonPropertyRelations => Set<StagingPersonPropertyRelation>();

    /// <summary>
    /// Staging area for Evidence records from .uhc packages.
    /// Subject to attachment deduplication by SHA-256 hash (FR-D-9).
    /// </summary>
    public DbSet<StagingEvidence> StagingEvidences => Set<StagingEvidence>();

    /// <summary>
    /// Staging area for Claim records from .uhc packages.
    /// Subject to claim lifecycle validation (FR-D-4 Level 6).
    /// </summary>
    public DbSet<StagingClaim> StagingClaims => Set<StagingClaim>();

    /// <summary>
    /// Staging area for Survey records from .uhc packages.
    /// </summary>
    public DbSet<StagingSurvey> StagingSurveys => Set<StagingSurvey>();

    /// <summary>
    /// LAN Sync sessions (telemetry + auditability)
    /// </summary>
    public DbSet<SyncSession> SyncSessions => Set<SyncSession>();


    // TODO: Add other entities later as we implement them


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
