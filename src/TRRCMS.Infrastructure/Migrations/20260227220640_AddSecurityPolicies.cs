using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSecurityPolicies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Certificate",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CertificateNumber = table.Column<string>(type: "text", nullable: false),
                    QrCodeData = table.Column<string>(type: "text", nullable: true),
                    Barcode = table.Column<string>(type: "text", nullable: true),
                    ClaimId = table.Column<Guid>(type: "uuid", nullable: false),
                    BeneficiaryPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    PropertyUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    GeneratedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    GeneratedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IssuedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IssuedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CollectedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    HandedOverByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CollectedByName = table.Column<string>(type: "text", nullable: true),
                    CollectorRelationship = table.Column<string>(type: "text", nullable: true),
                    CollectorIdNumber = table.Column<string>(type: "text", nullable: true),
                    CollectorSignature = table.Column<string>(type: "text", nullable: true),
                    CertificateType = table.Column<string>(type: "text", nullable: false),
                    TitleArabic = table.Column<string>(type: "text", nullable: false),
                    TitleEnglish = table.Column<string>(type: "text", nullable: true),
                    RightsSummaryArabic = table.Column<string>(type: "text", nullable: false),
                    RightsSummaryEnglish = table.Column<string>(type: "text", nullable: true),
                    TermsAndConditions = table.Column<string>(type: "text", nullable: true),
                    Remarks = table.Column<string>(type: "text", nullable: true),
                    ValidityStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ValidityEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsPermanent = table.Column<bool>(type: "boolean", nullable: false),
                    PdfFilePath = table.Column<string>(type: "text", nullable: true),
                    PdfFileHash = table.Column<string>(type: "text", nullable: true),
                    PdfFileSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    VoidedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VoidedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    VoidReason = table.Column<string>(type: "text", nullable: true),
                    IsReissued = table.Column<bool>(type: "boolean", nullable: false),
                    OriginalCertificateId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReissueReason = table.Column<string>(type: "text", nullable: true),
                    ReissueNumber = table.Column<int>(type: "integer", nullable: true),
                    DigitalSignature = table.Column<string>(type: "text", nullable: true),
                    SigningAuthority = table.Column<string>(type: "text", nullable: true),
                    SignedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LegalBasis = table.Column<string>(type: "text", nullable: true),
                    LegalReference = table.Column<string>(type: "text", nullable: true),
                    IssuingOrganization = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_Certificate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Certificate_Certificate_OriginalCertificateId",
                        column: x => x.OriginalCertificateId,
                        principalTable: "Certificate",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Certificate_Claims_ClaimId",
                        column: x => x.ClaimId,
                        principalTable: "Claims",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Certificate_Persons_BeneficiaryPersonId",
                        column: x => x.BeneficiaryPersonId,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Certificate_PropertyUnits_PropertyUnitId",
                        column: x => x.PropertyUnitId,
                        principalTable: "PropertyUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SecurityPolicies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false, comment: "Policy version number, auto-incremented on each apply"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false, comment: "Whether this is the currently enforced policy"),
                    EffectiveFromUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "When this policy version became effective"),
                    EffectiveToUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "When this policy was superseded (null if still active)"),
                    Password_MinLength = table.Column<int>(type: "integer", nullable: false, defaultValue: 8, comment: "Minimum password length (8–128)"),
                    Password_RequireUppercase = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Password_RequireLowercase = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Password_RequireDigit = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Password_RequireSpecialCharacter = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Password_ExpiryDays = table.Column<int>(type: "integer", nullable: false, defaultValue: 90, comment: "Days until password expires (0 = never)"),
                    Password_ReuseHistory = table.Column<int>(type: "integer", nullable: false, defaultValue: 5, comment: "Number of previous passwords blocked (0 = none)"),
                    Session_TimeoutMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 30, comment: "Session inactivity timeout in minutes (5–1440)"),
                    Session_MaxFailedLoginAttempts = table.Column<int>(type: "integer", nullable: false, defaultValue: 5, comment: "Max failed logins before lockout (3–20)"),
                    Session_LockoutDurationMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 15, comment: "Lockout duration in minutes (1–1440)"),
                    Access_AllowPasswordAuth = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Access_AllowSsoAuth = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Access_AllowTokenAuth = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Access_EnforceIpAllowlist = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Access_IpAllowlist = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true, comment: "Comma-separated allowed IPs/CIDR"),
                    Access_IpDenylist = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true, comment: "Comma-separated denied IPs/CIDR"),
                    Access_RestrictByEnvironment = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Access_AllowedEnvironments = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "Comma-separated allowed environments"),
                    ChangeDescription = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true, comment: "Description of changes in this version"),
                    AppliedByUserId = table.Column<Guid>(type: "uuid", nullable: false, comment: "User who approved and applied this policy"),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: true),
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
                    table.PrimaryKey("PK_SecurityPolicies", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Certificate_BeneficiaryPersonId",
                table: "Certificate",
                column: "BeneficiaryPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificate_ClaimId",
                table: "Certificate",
                column: "ClaimId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificate_OriginalCertificateId",
                table: "Certificate",
                column: "OriginalCertificateId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificate_PropertyUnitId",
                table: "Certificate",
                column: "PropertyUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityPolicies_IsActive_Filtered",
                table: "SecurityPolicies",
                column: "IsActive",
                unique: true,
                filter: "\"IsActive\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityPolicies_Version",
                table: "SecurityPolicies",
                column: "Version",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Certificate");

            migrationBuilder.DropTable(
                name: "SecurityPolicies");
        }
    }
}
