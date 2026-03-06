using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBuildingDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BuildingDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BuildingId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "Optional description of the document"),
                    OriginalFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false, comment: "Original filename as uploaded"),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, comment: "File path in storage system"),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false, comment: "File size in bytes"),
                    MimeType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "MIME type (e.g., image/jpeg, application/pdf)"),
                    FileHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true, comment: "SHA-256 hash of the file for deduplication"),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true, comment: "Additional notes"),
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
                    table.PrimaryKey("PK_BuildingDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BuildingDocuments_Buildings_BuildingId",
                        column: x => x.BuildingId,
                        principalTable: "Buildings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StagingBuildingDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalBuildingId = table.Column<Guid>(type: "uuid", nullable: false, comment: "Original Building UUID from .uhc — not a FK to production Buildings"),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    OriginalFileName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FilePath = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false, comment: "File path within .uhc container or staging storage"),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    MimeType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FileHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true, comment: "SHA-256 hash for deduplication during commit (FR-D-9)"),
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
                    table.PrimaryKey("PK_StagingBuildingDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StagingBuildingDocuments_ImportPackages_ImportPackageId",
                        column: x => x.ImportPackageId,
                        principalTable: "ImportPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BuildingDocuments_BuildingId",
                table: "BuildingDocuments",
                column: "BuildingId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingDocuments_FileHash",
                table: "BuildingDocuments",
                column: "FileHash");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingDocuments_IsDeleted",
                table: "BuildingDocuments",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_StagingBuildingDocuments_FileHash",
                table: "StagingBuildingDocuments",
                column: "FileHash");

            migrationBuilder.CreateIndex(
                name: "IX_StagingBuildingDocuments_ImportPackageId",
                table: "StagingBuildingDocuments",
                column: "ImportPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_StagingBuildingDocuments_ImportPackageId_OriginalBuildingId",
                table: "StagingBuildingDocuments",
                columns: new[] { "ImportPackageId", "OriginalBuildingId" });

            migrationBuilder.CreateIndex(
                name: "IX_StagingBuildingDocuments_ImportPackageId_OriginalEntityId",
                table: "StagingBuildingDocuments",
                columns: new[] { "ImportPackageId", "OriginalEntityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StagingBuildingDocuments_ImportPackageId_ValidationStatus",
                table: "StagingBuildingDocuments",
                columns: new[] { "ImportPackageId", "ValidationStatus" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BuildingDocuments");

            migrationBuilder.DropTable(
                name: "StagingBuildingDocuments");
        }
    }
}
