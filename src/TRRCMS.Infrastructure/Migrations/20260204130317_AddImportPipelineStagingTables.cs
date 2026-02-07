using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddImportPipelineStagingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ImportPackages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PackageId = table.Column<Guid>(type: "uuid", nullable: false, comment: "Unique package identifier from .uhc manifest — enforces idempotent imports"),
                    PackageNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, comment: "Human-readable package number (PKG-YYYY-NNNN)"),
                    FileName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, comment: "Original filename of the .uhc container"),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    PackageCreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date when package was created on tablet"),
                    PackageExportedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date when package was exported from tablet"),
                    ExportedByUserId = table.Column<Guid>(type: "uuid", nullable: false, comment: "Field collector who created the package"),
                    DeviceId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "Tablet/device ID that created the package"),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, comment: "Current import workflow status"),
                    ImportedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Date when package was uploaded to desktop system"),
                    ImportedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ValidationStartedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValidationCompletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CommittedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Date when data was committed to production tables"),
                    CommittedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Checksum = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, comment: "SHA-256 checksum of the .uhc file"),
                    DigitalSignature = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true, comment: "Digital signature of the package (if signed)"),
                    IsSignatureValid = table.Column<bool>(type: "boolean", nullable: false),
                    IsChecksumValid = table.Column<bool>(type: "boolean", nullable: false),
                    SurveyCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    BuildingCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    PropertyUnitCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    PersonCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ClaimCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    DocumentCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    TotalAttachmentSizeBytes = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    VocabularyVersions = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true, comment: "JSON object of vocabulary versions: {\"ownership_type\": \"1.2.0\", ...}"),
                    IsVocabularyCompatible = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    VocabularyCompatibilityIssues = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true, comment: "Vocabulary compatibility issues (if any)"),
                    SchemaVersion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    IsSchemaValid = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ValidationErrors = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true, comment: "JSON array of validation error messages"),
                    ValidationWarnings = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true, comment: "JSON array of validation warning messages"),
                    ValidationErrorCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ValidationWarningCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    PersonDuplicateCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    PropertyDuplicateCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ConflictCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    AreConflictsResolved = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    SuccessfulImportCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    FailedImportCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    SkippedRecordCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ImportSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ErrorLog = table.Column<string>(type: "text", nullable: true, comment: "Detailed error log (stored as JSON)"),
                    ArchivePath = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true, comment: "File path: archives/YYYY/MM/[package_id].uhc"),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ArchivedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProcessingNotes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ImportMethod = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "Import method: Manual, NetworkSync, WatchedFolder"),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportPackages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConflictResolutions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConflictNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, comment: "Human-readable conflict number (CNF-YYYY-NNNN)"),
                    ConflictType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "PersonDuplicate, PropertyDuplicate, ClaimConflict"),
                    EntityType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "Type of entities in conflict: Person, PropertyUnit, Building"),
                    FirstEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    SecondEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstEntityIdentifier = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true, comment: "Human-readable identifier for first entity (e.g. name, building ID)"),
                    SecondEntityIdentifier = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true, comment: "Human-readable identifier for second entity"),
                    ImportPackageId = table.Column<Guid>(type: "uuid", nullable: true, comment: "Import package that triggered this conflict (null for manual detections)"),
                    SimilarityScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false, comment: "Similarity score 0-100% — higher means more likely duplicate"),
                    ConfidenceLevel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, comment: "Low, Medium, High"),
                    ConflictDescription = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    MatchingCriteria = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true, comment: "JSON: {\"national_id\": \"match\", \"name_similarity\": \"95%\", ...}"),
                    DataComparison = table.Column<string>(type: "text", nullable: true, comment: "JSON: side-by-side field comparison of conflicting entities"),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, comment: "PendingReview, Resolved, Ignored"),
                    ResolutionAction = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true, comment: "KeepBoth, Merge, KeepFirst, KeepSecond, Ignored, etc."),
                    DetectedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DetectedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssignedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AssignedToUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResolvedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResolvedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResolutionReason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ResolutionNotes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    MergedEntityId = table.Column<Guid>(type: "uuid", nullable: true, comment: "ID of the resulting merged entity (if Merge action)"),
                    DiscardedEntityId = table.Column<Guid>(type: "uuid", nullable: true, comment: "ID of the discarded entity (if Merge action)"),
                    MergeMapping = table.Column<string>(type: "text", nullable: true, comment: "JSON: which fields came from which entity — audit trail"),
                    Priority = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, comment: "Low, Normal, High, Critical"),
                    TargetResolutionHours = table.Column<int>(type: "integer", nullable: true, comment: "SLA target in hours for conflict resolution"),
                    IsOverdue = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsAutoDetected = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsAutoResolved = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    AutoResolutionRule = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "Name of the auto-resolution rule applied (if automated)"),
                    IsEscalated = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    EscalationReason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    EscalatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EscalatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewAttemptCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ReviewHistory = table.Column<string>(type: "text", nullable: true, comment: "JSON array: [{\"AttemptNumber\":1, \"Date\":\"...\", \"Notes\":\"...\"}]"),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConflictResolutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConflictResolutions_ImportPackages_ImportPackageId",
                        column: x => x.ImportPackageId,
                        principalTable: "ImportPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "StagingBuildings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BuildingId = table.Column<string>(type: "character varying(17)", maxLength: 17, nullable: true, comment: "Composite 17-digit ID — optional in staging, computed from admin codes during commit"),
                    GovernorateCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    DistrictCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    SubDistrictCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    CommunityCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    NeighborhoodCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    BuildingNumber = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    GovernorateName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "From lookup tables — not in mobile package"),
                    DistrictName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "From lookup tables — not in mobile package"),
                    SubDistrictName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "From lookup tables — not in mobile package"),
                    CommunityName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "From lookup tables — not in mobile package"),
                    NeighborhoodName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "From lookup tables — not in mobile package"),
                    BuildingType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DamageLevel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    NumberOfPropertyUnits = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    NumberOfApartments = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    NumberOfShops = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    NumberOfFloors = table.Column<int>(type: "integer", nullable: true, comment: "Future expansion — not in current mobile package"),
                    YearOfConstruction = table.Column<int>(type: "integer", nullable: true, comment: "Future expansion — not in current mobile package"),
                    BuildingGeometryWkt = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true, comment: "WKT representation — converted to PostGIS geometry on commit"),
                    Latitude = table.Column<decimal>(type: "numeric(10,7)", precision: 10, scale: 7, nullable: true),
                    Longitude = table.Column<decimal>(type: "numeric(10,7)", precision: 10, scale: 7, nullable: true),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Landmark = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LocationDescription = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    ImportPackageId = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    ValidationStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ValidationErrors = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true, comment: "JSON array of blocking validation error messages"),
                    ValidationWarnings = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true, comment: "JSON array of non-blocking validation warning messages"),
                    IsApprovedForCommit = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CommittedEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    StagedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StagingBuildings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StagingBuildings_ImportPackages_ImportPackageId",
                        column: x => x.ImportPackageId,
                        principalTable: "ImportPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StagingClaims",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalPropertyUnitId = table.Column<Guid>(type: "uuid", nullable: false, comment: "Original PropertyUnit UUID from .uhc — not a FK to production PropertyUnits"),
                    OriginalPrimaryClaimantId = table.Column<Guid>(type: "uuid", nullable: true, comment: "Original primary claimant Person UUID from .uhc"),
                    ClaimNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true, comment: "Optional in staging — auto-generated during commit (FR-D-8)"),
                    ClaimType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ClaimSource = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Priority = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Normal"),
                    LifecycleStage = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "Optional — auto-set to DraftPendingSubmission during commit"),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "Optional — auto-set to Draft during commit"),
                    TenureContractType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    OwnershipShare = table.Column<decimal>(type: "numeric", nullable: true, comment: "Ownership percentage (0-100)"),
                    TenureStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Date from which tenure/occupancy started"),
                    TenureEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Date when tenure/occupancy ended"),
                    TargetCompletionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Target completion date for claim processing"),
                    ClaimDescription = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    LegalBasis = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    SupportingNarrative = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    EvidenceCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    AllRequiredDocumentsSubmitted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    MissingDocuments = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true, comment: "JSON array of missing required document types"),
                    VerificationStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "Optional — auto-set to Pending during commit"),
                    VerificationNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ProcessingNotes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    PublicRemarks = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    ImportPackageId = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    ValidationStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ValidationErrors = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true, comment: "JSON array of blocking validation error messages"),
                    ValidationWarnings = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true, comment: "JSON array of non-blocking validation warning messages"),
                    IsApprovedForCommit = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CommittedEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    StagedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StagingClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StagingClaims_ImportPackages_ImportPackageId",
                        column: x => x.ImportPackageId,
                        principalTable: "ImportPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StagingEvidences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalPersonId = table.Column<Guid>(type: "uuid", nullable: true, comment: "Original Person UUID from .uhc — not a FK to production Persons"),
                    OriginalPersonPropertyRelationId = table.Column<Guid>(type: "uuid", nullable: true, comment: "Original PersonPropertyRelation UUID from .uhc"),
                    OriginalClaimId = table.Column<Guid>(type: "uuid", nullable: true, comment: "Original Claim UUID from .uhc"),
                    EvidenceType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FilePath = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false, comment: "File path within .uhc container or staging storage"),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    MimeType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FileHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true, comment: "SHA-256 hash for deduplication during commit (FR-D-9)"),
                    IssuingAuthority = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DocumentIssuedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Date when document was issued"),
                    DocumentExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Date when document expires"),
                    DocumentReferenceNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    OriginalPreviousVersionId = table.Column<Guid>(type: "uuid", nullable: true, comment: "Original previous version UUID from .uhc"),
                    IsCurrentVersion = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    ImportPackageId = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    ValidationStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ValidationErrors = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true, comment: "JSON array of blocking validation error messages"),
                    ValidationWarnings = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true, comment: "JSON array of non-blocking validation warning messages"),
                    IsApprovedForCommit = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CommittedEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    StagedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StagingEvidences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StagingEvidences_ImportPackages_ImportPackageId",
                        column: x => x.ImportPackageId,
                        principalTable: "ImportPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StagingHouseholds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalPropertyUnitId = table.Column<Guid>(type: "uuid", nullable: false, comment: "Original PropertyUnit UUID from .uhc — not a FK to production PropertyUnits"),
                    OriginalHeadOfHouseholdPersonId = table.Column<Guid>(type: "uuid", nullable: true, comment: "Original head-of-household Person UUID from .uhc"),
                    HeadOfHouseholdName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    HouseholdSize = table.Column<int>(type: "integer", nullable: false),
                    MaleCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    FemaleCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    MaleChildCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    FemaleChildCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    MaleElderlyCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    FemaleElderlyCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    MaleDisabledCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    FemaleDisabledCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    InfantCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ChildCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    MinorCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    AdultCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ElderlyCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    PersonsWithDisabilitiesCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsFemaleHeaded = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    WidowCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OrphanCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    SingleParentCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    EmployedPersonsCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    UnemployedPersonsCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    PrimaryIncomeSource = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    MonthlyIncomeEstimate = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    IsDisplaced = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    OriginLocation = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DisplacementReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SpecialNeeds = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    ImportPackageId = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    ValidationStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ValidationErrors = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true, comment: "JSON array of blocking validation error messages"),
                    ValidationWarnings = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true, comment: "JSON array of non-blocking validation warning messages"),
                    IsApprovedForCommit = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CommittedEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    StagedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StagingHouseholds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StagingHouseholds_ImportPackages_ImportPackageId",
                        column: x => x.ImportPackageId,
                        principalTable: "ImportPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StagingPersonPropertyRelations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalPersonId = table.Column<Guid>(type: "uuid", nullable: false, comment: "Original Person UUID from .uhc — not a FK to production Persons"),
                    OriginalPropertyUnitId = table.Column<Guid>(type: "uuid", nullable: false, comment: "Original PropertyUnit UUID from .uhc — not a FK to production PropertyUnits"),
                    RelationType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RelationTypeOtherDesc = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ContractType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ContractTypeOtherDesc = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    OwnershipShare = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    ContractDetails = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Start date of the relation/contract"),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "End date of the relation/contract"),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    ImportPackageId = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    ValidationStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ValidationErrors = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true, comment: "JSON array of blocking validation error messages"),
                    ValidationWarnings = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true, comment: "JSON array of non-blocking validation warning messages"),
                    IsApprovedForCommit = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CommittedEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    StagedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StagingPersonPropertyRelations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StagingPersonPropertyRelations_ImportPackages_ImportPackage~",
                        column: x => x.ImportPackageId,
                        principalTable: "ImportPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StagingPersons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FamilyNameArabic = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FirstNameArabic = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FatherNameArabic = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MotherNameArabic = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    NationalId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "Primary key for duplicate detection (FR-D-5, §12.2.4)"),
                    YearOfBirth = table.Column<int>(type: "integer", nullable: true, comment: "Year of birth — used in duplicate detection composite with name+gender"),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    MobileNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    FullNameEnglish = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Gender = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Nationality = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OriginalHouseholdId = table.Column<Guid>(type: "uuid", nullable: true, comment: "Original Household UUID from .uhc — not a FK to production Households"),
                    RelationshipToHead = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    ImportPackageId = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    ValidationStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ValidationErrors = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true, comment: "JSON array of blocking validation error messages"),
                    ValidationWarnings = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true, comment: "JSON array of non-blocking validation warning messages"),
                    IsApprovedForCommit = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CommittedEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    StagedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StagingPersons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StagingPersons_ImportPackages_ImportPackageId",
                        column: x => x.ImportPackageId,
                        principalTable: "ImportPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StagingPropertyUnits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalBuildingId = table.Column<Guid>(type: "uuid", nullable: false, comment: "Original Building UUID from .uhc — not a FK to production Buildings"),
                    UnitIdentifier = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UnitType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FloorNumber = table.Column<int>(type: "integer", nullable: true, comment: "Floor number (0=Ground, 1=First, -1=Basement)"),
                    NumberOfRooms = table.Column<int>(type: "integer", nullable: true, comment: "Number of rooms (عدد الغرف)"),
                    OccupancyStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DamageLevel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AreaSquareMeters = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    EstimatedAreaSqm = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    PositionOnFloor = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    OccupancyType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    OccupancyNature = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UtilitiesNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    SpecialFeatures = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    ImportPackageId = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    ValidationStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ValidationErrors = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true, comment: "JSON array of blocking validation error messages"),
                    ValidationWarnings = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true, comment: "JSON array of non-blocking validation warning messages"),
                    IsApprovedForCommit = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CommittedEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    StagedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StagingPropertyUnits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StagingPropertyUnits_ImportPackages_ImportPackageId",
                        column: x => x.ImportPackageId,
                        principalTable: "ImportPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StagingSurveys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalBuildingId = table.Column<Guid>(type: "uuid", nullable: false, comment: "Original Building UUID from .uhc — not a FK to production Buildings"),
                    OriginalPropertyUnitId = table.Column<Guid>(type: "uuid", nullable: true, comment: "Original PropertyUnit UUID from .uhc"),
                    OriginalFieldCollectorId = table.Column<Guid>(type: "uuid", nullable: true, comment: "Optional — derived from user context during import, not from package"),
                    OriginalClaimId = table.Column<Guid>(type: "uuid", nullable: true, comment: "Original Claim UUID from .uhc"),
                    ReferenceCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "Optional — auto-generated during commit"),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "Optional — auto-set (Field/Office) during commit"),
                    Source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "Optional — auto-set during commit"),
                    SurveyTypeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true, comment: "Optional — auto-set during commit"),
                    SurveyDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "Optional — auto-set to Draft during commit"),
                    GpsCoordinates = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IntervieweeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IntervieweeRelationship = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    OfficeLocation = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RegistrationNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AppointmentReference = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ContactPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ContactEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    ImportPackageId = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    ValidationStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ValidationErrors = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true, comment: "JSON array of blocking validation error messages"),
                    ValidationWarnings = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true, comment: "JSON array of non-blocking validation warning messages"),
                    IsApprovedForCommit = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CommittedEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    StagedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StagingSurveys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StagingSurveys_ImportPackages_ImportPackageId",
                        column: x => x.ImportPackageId,
                        principalTable: "ImportPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConflictResolutions_AssignedToUserId_Status",
                table: "ConflictResolutions",
                columns: new[] { "AssignedToUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ConflictResolutions_ConflictType_Status",
                table: "ConflictResolutions",
                columns: new[] { "ConflictType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ConflictResolutions_DetectedDate",
                table: "ConflictResolutions",
                column: "DetectedDate");

            migrationBuilder.CreateIndex(
                name: "IX_ConflictResolutions_FirstEntityId_SecondEntityId",
                table: "ConflictResolutions",
                columns: new[] { "FirstEntityId", "SecondEntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_ConflictResolutions_ImportPackageId",
                table: "ConflictResolutions",
                column: "ImportPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportPackages_ExportedByUserId",
                table: "ImportPackages",
                column: "ExportedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportPackages_ImportedDate",
                table: "ImportPackages",
                column: "ImportedDate");

            migrationBuilder.CreateIndex(
                name: "IX_ImportPackages_PackageId",
                table: "ImportPackages",
                column: "PackageId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImportPackages_Status",
                table: "ImportPackages",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_StagingBuildings_BuildingId",
                table: "StagingBuildings",
                column: "BuildingId");

            migrationBuilder.CreateIndex(
                name: "IX_StagingBuildings_ImportPackageId",
                table: "StagingBuildings",
                column: "ImportPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_StagingBuildings_ImportPackageId_OriginalEntityId",
                table: "StagingBuildings",
                columns: new[] { "ImportPackageId", "OriginalEntityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StagingBuildings_ImportPackageId_ValidationStatus",
                table: "StagingBuildings",
                columns: new[] { "ImportPackageId", "ValidationStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_StagingClaims_ClaimNumber",
                table: "StagingClaims",
                column: "ClaimNumber");

            migrationBuilder.CreateIndex(
                name: "IX_StagingClaims_ImportPackageId",
                table: "StagingClaims",
                column: "ImportPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_StagingClaims_ImportPackageId_OriginalEntityId",
                table: "StagingClaims",
                columns: new[] { "ImportPackageId", "OriginalEntityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StagingClaims_ImportPackageId_OriginalPropertyUnitId",
                table: "StagingClaims",
                columns: new[] { "ImportPackageId", "OriginalPropertyUnitId" });

            migrationBuilder.CreateIndex(
                name: "IX_StagingClaims_ImportPackageId_ValidationStatus",
                table: "StagingClaims",
                columns: new[] { "ImportPackageId", "ValidationStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_StagingEvidences_FileHash",
                table: "StagingEvidences",
                column: "FileHash");

            migrationBuilder.CreateIndex(
                name: "IX_StagingEvidences_ImportPackageId",
                table: "StagingEvidences",
                column: "ImportPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_StagingEvidences_ImportPackageId_OriginalClaimId",
                table: "StagingEvidences",
                columns: new[] { "ImportPackageId", "OriginalClaimId" });

            migrationBuilder.CreateIndex(
                name: "IX_StagingEvidences_ImportPackageId_OriginalEntityId",
                table: "StagingEvidences",
                columns: new[] { "ImportPackageId", "OriginalEntityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StagingEvidences_ImportPackageId_OriginalPersonId",
                table: "StagingEvidences",
                columns: new[] { "ImportPackageId", "OriginalPersonId" });

            migrationBuilder.CreateIndex(
                name: "IX_StagingEvidences_ImportPackageId_ValidationStatus",
                table: "StagingEvidences",
                columns: new[] { "ImportPackageId", "ValidationStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_StagingHouseholds_ImportPackageId",
                table: "StagingHouseholds",
                column: "ImportPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_StagingHouseholds_ImportPackageId_OriginalEntityId",
                table: "StagingHouseholds",
                columns: new[] { "ImportPackageId", "OriginalEntityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StagingHouseholds_ImportPackageId_OriginalPropertyUnitId",
                table: "StagingHouseholds",
                columns: new[] { "ImportPackageId", "OriginalPropertyUnitId" });

            migrationBuilder.CreateIndex(
                name: "IX_StagingHouseholds_ImportPackageId_ValidationStatus",
                table: "StagingHouseholds",
                columns: new[] { "ImportPackageId", "ValidationStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_StagingPersonPropertyRelations_ImportPackageId",
                table: "StagingPersonPropertyRelations",
                column: "ImportPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_StagingPersonPropertyRelations_ImportPackageId_OriginalEntityId",
                table: "StagingPersonPropertyRelations",
                columns: new[] { "ImportPackageId", "OriginalEntityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StagingPersonPropertyRelations_ImportPackageId_OriginalPersonId",
                table: "StagingPersonPropertyRelations",
                columns: new[] { "ImportPackageId", "OriginalPersonId" });

            migrationBuilder.CreateIndex(
                name: "IX_StagingPersonPropertyRelations_ImportPackageId_OriginalPropertyUnitId",
                table: "StagingPersonPropertyRelations",
                columns: new[] { "ImportPackageId", "OriginalPropertyUnitId" });

            migrationBuilder.CreateIndex(
                name: "IX_StagingPersonPropertyRelations_ImportPackageId_ValidationStatus",
                table: "StagingPersonPropertyRelations",
                columns: new[] { "ImportPackageId", "ValidationStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_StagingPersons_ImportPackageId",
                table: "StagingPersons",
                column: "ImportPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_StagingPersons_ImportPackageId_OriginalEntityId",
                table: "StagingPersons",
                columns: new[] { "ImportPackageId", "OriginalEntityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StagingPersons_ImportPackageId_OriginalHouseholdId",
                table: "StagingPersons",
                columns: new[] { "ImportPackageId", "OriginalHouseholdId" });

            migrationBuilder.CreateIndex(
                name: "IX_StagingPersons_ImportPackageId_ValidationStatus",
                table: "StagingPersons",
                columns: new[] { "ImportPackageId", "ValidationStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_StagingPersons_NationalId",
                table: "StagingPersons",
                column: "NationalId");

            migrationBuilder.CreateIndex(
                name: "IX_StagingPropertyUnits_ImportPackageId",
                table: "StagingPropertyUnits",
                column: "ImportPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_StagingPropertyUnits_ImportPackageId_OriginalBuildingId",
                table: "StagingPropertyUnits",
                columns: new[] { "ImportPackageId", "OriginalBuildingId" });

            migrationBuilder.CreateIndex(
                name: "IX_StagingPropertyUnits_ImportPackageId_OriginalEntityId",
                table: "StagingPropertyUnits",
                columns: new[] { "ImportPackageId", "OriginalEntityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StagingPropertyUnits_ImportPackageId_ValidationStatus",
                table: "StagingPropertyUnits",
                columns: new[] { "ImportPackageId", "ValidationStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_StagingSurveys_ImportPackageId",
                table: "StagingSurveys",
                column: "ImportPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_StagingSurveys_ImportPackageId_OriginalBuildingId",
                table: "StagingSurveys",
                columns: new[] { "ImportPackageId", "OriginalBuildingId" });

            migrationBuilder.CreateIndex(
                name: "IX_StagingSurveys_ImportPackageId_OriginalEntityId",
                table: "StagingSurveys",
                columns: new[] { "ImportPackageId", "OriginalEntityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StagingSurveys_ImportPackageId_ValidationStatus",
                table: "StagingSurveys",
                columns: new[] { "ImportPackageId", "ValidationStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_StagingSurveys_ReferenceCode",
                table: "StagingSurveys",
                column: "ReferenceCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConflictResolutions");

            migrationBuilder.DropTable(
                name: "StagingBuildings");

            migrationBuilder.DropTable(
                name: "StagingClaims");

            migrationBuilder.DropTable(
                name: "StagingEvidences");

            migrationBuilder.DropTable(
                name: "StagingHouseholds");

            migrationBuilder.DropTable(
                name: "StagingPersonPropertyRelations");

            migrationBuilder.DropTable(
                name: "StagingPersons");

            migrationBuilder.DropTable(
                name: "StagingPropertyUnits");

            migrationBuilder.DropTable(
                name: "StagingSurveys");

            migrationBuilder.DropTable(
                name: "ImportPackages");
        }
    }
}
