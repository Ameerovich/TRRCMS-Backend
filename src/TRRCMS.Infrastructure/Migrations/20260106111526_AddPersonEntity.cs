using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Claims",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimNumber = table.Column<string>(type: "text", nullable: false),
                    PropertyUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    PrimaryClaimantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClaimType = table.Column<string>(type: "text", nullable: false),
                    ClaimSource = table.Column<int>(type: "integer", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    LifecycleStage = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    SubmittedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SubmittedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DecisionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DecisionByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssignedToUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssignedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TargetCompletionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenureContractType = table.Column<int>(type: "integer", nullable: true),
                    OwnershipShare = table.Column<int>(type: "integer", nullable: true),
                    TenureStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenureEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClaimDescription = table.Column<string>(type: "text", nullable: true),
                    LegalBasis = table.Column<string>(type: "text", nullable: true),
                    SupportingNarrative = table.Column<string>(type: "text", nullable: true),
                    HasConflicts = table.Column<bool>(type: "boolean", nullable: false),
                    ConflictCount = table.Column<int>(type: "integer", nullable: false),
                    ConflictResolutionStatus = table.Column<string>(type: "text", nullable: true),
                    EvidenceCount = table.Column<int>(type: "integer", nullable: false),
                    AllRequiredDocumentsSubmitted = table.Column<bool>(type: "boolean", nullable: false),
                    MissingDocuments = table.Column<string>(type: "text", nullable: true),
                    VerificationStatus = table.Column<int>(type: "integer", nullable: false),
                    VerificationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    VerificationNotes = table.Column<string>(type: "text", nullable: true),
                    FinalDecision = table.Column<string>(type: "text", nullable: true),
                    DecisionReason = table.Column<string>(type: "text", nullable: true),
                    DecisionNotes = table.Column<string>(type: "text", nullable: true),
                    CertificateStatus = table.Column<int>(type: "integer", nullable: false),
                    ProcessingNotes = table.Column<string>(type: "text", nullable: true),
                    PublicRemarks = table.Column<string>(type: "text", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Claims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Claims_PropertyUnits_PropertyUnitId",
                        column: x => x.PropertyUnitId,
                        principalTable: "PropertyUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Referral",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferralNumber = table.Column<string>(type: "text", nullable: false),
                    ClaimId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromRole = table.Column<int>(type: "integer", nullable: false),
                    FromUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ToRole = table.Column<int>(type: "integer", nullable: false),
                    ToUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReferralType = table.Column<string>(type: "text", nullable: false),
                    ReferralReason = table.Column<string>(type: "text", nullable: false),
                    ReferralNotes = table.Column<string>(type: "text", nullable: true),
                    Priority = table.Column<string>(type: "text", nullable: false),
                    ReferralDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpectedResponseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    AcknowledgedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AcknowledgedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    StartedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResponseNotes = table.Column<string>(type: "text", nullable: true),
                    Outcome = table.Column<string>(type: "text", nullable: true),
                    IsEscalation = table.Column<bool>(type: "boolean", nullable: false),
                    PreviousReferralId = table.Column<Guid>(type: "uuid", nullable: true),
                    EscalationLevel = table.Column<int>(type: "integer", nullable: true),
                    ActionsRequired = table.Column<string>(type: "text", nullable: true),
                    DocumentsRequired = table.Column<string>(type: "text", nullable: true),
                    TargetResolutionHours = table.Column<int>(type: "integer", nullable: true),
                    IsOverdue = table.Column<bool>(type: "boolean", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Referral", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Referral_Claims_ClaimId",
                        column: x => x.ClaimId,
                        principalTable: "Claims",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Referral_Referral_PreviousReferralId",
                        column: x => x.PreviousReferralId,
                        principalTable: "Referral",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Document",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentType = table.Column<int>(type: "integer", nullable: false),
                    DocumentNumber = table.Column<string>(type: "text", nullable: true),
                    DocumentTitle = table.Column<string>(type: "text", nullable: true),
                    IssueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IssuingAuthority = table.Column<string>(type: "text", nullable: true),
                    IssuingPlace = table.Column<string>(type: "text", nullable: true),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false),
                    VerificationStatus = table.Column<int>(type: "integer", nullable: false),
                    VerificationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    VerificationNotes = table.Column<string>(type: "text", nullable: true),
                    EvidenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    DocumentHash = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    PropertyUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    PersonPropertyRelationId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClaimId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsLegallyValid = table.Column<bool>(type: "boolean", nullable: false),
                    LegalValidityNotes = table.Column<string>(type: "text", nullable: true),
                    IsOriginal = table.Column<bool>(type: "boolean", nullable: false),
                    OriginalDocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsNotarized = table.Column<bool>(type: "boolean", nullable: false),
                    NotaryOffice = table.Column<string>(type: "text", nullable: true),
                    NotarizationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NotarizationNumber = table.Column<string>(type: "text", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Document", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Document_Claims_ClaimId",
                        column: x => x.ClaimId,
                        principalTable: "Claims",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Document_Document_OriginalDocumentId",
                        column: x => x.OriginalDocumentId,
                        principalTable: "Document",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Document_PropertyUnits_PropertyUnitId",
                        column: x => x.PropertyUnitId,
                        principalTable: "PropertyUnits",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Evidence",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EvidenceType = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    OriginalFileName = table.Column<string>(type: "text", nullable: false),
                    FilePath = table.Column<string>(type: "text", nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    MimeType = table.Column<string>(type: "text", nullable: false),
                    FileHash = table.Column<string>(type: "text", nullable: true),
                    DocumentIssuedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DocumentExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IssuingAuthority = table.Column<string>(type: "text", nullable: true),
                    DocumentReferenceNumber = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    PreviousVersionId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsCurrentVersion = table.Column<bool>(type: "boolean", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    PersonPropertyRelationId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClaimId = table.Column<Guid>(type: "uuid", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Evidence", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Evidence_Claims_ClaimId",
                        column: x => x.ClaimId,
                        principalTable: "Claims",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Evidence_Evidence_PreviousVersionId",
                        column: x => x.PreviousVersionId,
                        principalTable: "Evidence",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Households",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PropertyUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    HouseholdSize = table.Column<int>(type: "integer", nullable: false),
                    HeadOfHouseholdName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    HeadOfHouseholdPersonId = table.Column<Guid>(type: "uuid", nullable: true),
                    MaleCount = table.Column<int>(type: "integer", nullable: false),
                    FemaleCount = table.Column<int>(type: "integer", nullable: false),
                    InfantCount = table.Column<int>(type: "integer", nullable: false),
                    ChildCount = table.Column<int>(type: "integer", nullable: false),
                    MinorCount = table.Column<int>(type: "integer", nullable: false),
                    AdultCount = table.Column<int>(type: "integer", nullable: false),
                    ElderlyCount = table.Column<int>(type: "integer", nullable: false),
                    PersonsWithDisabilitiesCount = table.Column<int>(type: "integer", nullable: false),
                    IsFemaleHeaded = table.Column<bool>(type: "boolean", nullable: false),
                    WidowCount = table.Column<int>(type: "integer", nullable: false),
                    OrphanCount = table.Column<int>(type: "integer", nullable: false),
                    SingleParentCount = table.Column<int>(type: "integer", nullable: false),
                    EmployedPersonsCount = table.Column<int>(type: "integer", nullable: false),
                    UnemployedPersonsCount = table.Column<int>(type: "integer", nullable: false),
                    PrimaryIncomeSource = table.Column<string>(type: "text", nullable: true),
                    MonthlyIncomeEstimate = table.Column<decimal>(type: "numeric", nullable: true),
                    IsDisplaced = table.Column<bool>(type: "boolean", nullable: false),
                    OriginLocation = table.Column<string>(type: "text", nullable: true),
                    ArrivalDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DisplacementReason = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    SpecialNeeds = table.Column<string>(type: "text", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Households", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Households_PropertyUnits_PropertyUnitId",
                        column: x => x.PropertyUnitId,
                        principalTable: "PropertyUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Persons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstNameArabic = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "First name in Arabic (الاسم الأول)"),
                    FatherNameArabic = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Father's name in Arabic (اسم الأب)"),
                    FamilyNameArabic = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Family/Last name in Arabic (اسم العائلة)"),
                    MotherNameArabic = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "Mother's name in Arabic (اسم الأم)"),
                    FullNameEnglish = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true, comment: "Full name in English (optional)"),
                    NationalId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "National ID or identification number"),
                    YearOfBirth = table.Column<int>(type: "integer", nullable: true, comment: "Year of birth (integer)"),
                    Gender = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true, comment: "Gender (controlled vocabulary: M/F)"),
                    Nationality = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "Nationality (controlled vocabulary)"),
                    PrimaryPhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true, comment: "Primary phone number"),
                    SecondaryPhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true, comment: "Secondary phone number"),
                    IsContactPerson = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false, comment: "Indicates if this person is the main contact"),
                    HouseholdId = table.Column<Guid>(type: "uuid", nullable: true, comment: "Foreign key to household (nullable)"),
                    RelationshipToHead = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "Relationship to head of household"),
                    HasIdentificationDocument = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false, comment: "Flag indicating if ID document was uploaded"),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true, comment: "Concurrency token"),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Creation timestamp (UTC)"),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false, comment: "User who created this record"),
                    LastModifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Last modification timestamp (UTC)"),
                    LastModifiedBy = table.Column<Guid>(type: "uuid", nullable: true, comment: "User who last modified this record"),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false, comment: "Soft delete flag"),
                    DeletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Deletion timestamp (UTC)"),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true, comment: "User who deleted this record")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Persons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Persons_Households_HouseholdId",
                        column: x => x.HouseholdId,
                        principalTable: "Households",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PersonPropertyRelations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    PropertyUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    RelationType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OwnershipShare = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    ContractDetails = table.Column<string>(type: "text", nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonPropertyRelations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonPropertyRelations_Persons_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PersonPropertyRelations_PropertyUnits_PropertyUnitId",
                        column: x => x.PropertyUnitId,
                        principalTable: "PropertyUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Claims_PrimaryClaimantId",
                table: "Claims",
                column: "PrimaryClaimantId");

            migrationBuilder.CreateIndex(
                name: "IX_Claims_PropertyUnitId",
                table: "Claims",
                column: "PropertyUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Document_ClaimId",
                table: "Document",
                column: "ClaimId");

            migrationBuilder.CreateIndex(
                name: "IX_Document_EvidenceId",
                table: "Document",
                column: "EvidenceId");

            migrationBuilder.CreateIndex(
                name: "IX_Document_OriginalDocumentId",
                table: "Document",
                column: "OriginalDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Document_PersonId",
                table: "Document",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Document_PersonPropertyRelationId",
                table: "Document",
                column: "PersonPropertyRelationId");

            migrationBuilder.CreateIndex(
                name: "IX_Document_PropertyUnitId",
                table: "Document",
                column: "PropertyUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Evidence_ClaimId",
                table: "Evidence",
                column: "ClaimId");

            migrationBuilder.CreateIndex(
                name: "IX_Evidence_PersonId",
                table: "Evidence",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Evidence_PersonPropertyRelationId",
                table: "Evidence",
                column: "PersonPropertyRelationId");

            migrationBuilder.CreateIndex(
                name: "IX_Evidence_PreviousVersionId",
                table: "Evidence",
                column: "PreviousVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_Household_HeadOfHouseholdPersonId",
                table: "Households",
                column: "HeadOfHouseholdPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Household_IsDeleted",
                table: "Households",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Household_PropertyUnitId",
                table: "Households",
                column: "PropertyUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonPropertyRelation_IsActive",
                table: "PersonPropertyRelations",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_PersonPropertyRelation_IsDeleted",
                table: "PersonPropertyRelations",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_PersonPropertyRelation_PersonId",
                table: "PersonPropertyRelations",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonPropertyRelation_PersonId_PropertyUnitId",
                table: "PersonPropertyRelations",
                columns: new[] { "PersonId", "PropertyUnitId" });

            migrationBuilder.CreateIndex(
                name: "IX_PersonPropertyRelation_PropertyUnitId",
                table: "PersonPropertyRelations",
                column: "PropertyUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Person_FullNameArabic",
                table: "Persons",
                columns: new[] { "FirstNameArabic", "FatherNameArabic", "FamilyNameArabic" });

            migrationBuilder.CreateIndex(
                name: "IX_Person_HouseholdId",
                table: "Persons",
                column: "HouseholdId");

            migrationBuilder.CreateIndex(
                name: "IX_Person_IsDeleted",
                table: "Persons",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Person_NationalId",
                table: "Persons",
                column: "NationalId");

            migrationBuilder.CreateIndex(
                name: "IX_Person_PrimaryPhoneNumber",
                table: "Persons",
                column: "PrimaryPhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Referral_ClaimId",
                table: "Referral",
                column: "ClaimId");

            migrationBuilder.CreateIndex(
                name: "IX_Referral_PreviousReferralId",
                table: "Referral",
                column: "PreviousReferralId");

            migrationBuilder.AddForeignKey(
                name: "FK_Claims_Persons_PrimaryClaimantId",
                table: "Claims",
                column: "PrimaryClaimantId",
                principalTable: "Persons",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Document_Evidence_EvidenceId",
                table: "Document",
                column: "EvidenceId",
                principalTable: "Evidence",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Document_PersonPropertyRelations_PersonPropertyRelationId",
                table: "Document",
                column: "PersonPropertyRelationId",
                principalTable: "PersonPropertyRelations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Document_Persons_PersonId",
                table: "Document",
                column: "PersonId",
                principalTable: "Persons",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Evidence_PersonPropertyRelations_PersonPropertyRelationId",
                table: "Evidence",
                column: "PersonPropertyRelationId",
                principalTable: "PersonPropertyRelations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Evidence_Persons_PersonId",
                table: "Evidence",
                column: "PersonId",
                principalTable: "Persons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Households_Persons_HeadOfHouseholdPersonId",
                table: "Households",
                column: "HeadOfHouseholdPersonId",
                principalTable: "Persons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Households_Persons_HeadOfHouseholdPersonId",
                table: "Households");

            migrationBuilder.DropTable(
                name: "Document");

            migrationBuilder.DropTable(
                name: "Referral");

            migrationBuilder.DropTable(
                name: "Evidence");

            migrationBuilder.DropTable(
                name: "Claims");

            migrationBuilder.DropTable(
                name: "PersonPropertyRelations");

            migrationBuilder.DropTable(
                name: "Persons");

            migrationBuilder.DropTable(
                name: "Households");
        }
    }
}
