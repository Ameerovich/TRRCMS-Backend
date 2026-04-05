using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStagingIdentificationDocumentTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StagingIdentificationDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalPersonId = table.Column<Guid>(type: "uuid", nullable: false, comment: "Original Person UUID from .uhc"),
                    DocumentType = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    MimeType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FileHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    DocumentIssuedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DocumentExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IssuingAuthority = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DocumentReferenceNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    ImportPackageId = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    ValidationStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ValidationErrors = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    ValidationWarnings = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    IsApprovedForCommit = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CommittedEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    StagedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StagingIdentificationDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StagingIdentificationDocuments_ImportPackages_ImportPackage~",
                        column: x => x.ImportPackageId,
                        principalTable: "ImportPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StagingIdentificationDocuments_FileHash",
                table: "StagingIdentificationDocuments",
                column: "FileHash");

            migrationBuilder.CreateIndex(
                name: "IX_StagingIdentificationDocuments_ImportPackageId",
                table: "StagingIdentificationDocuments",
                column: "ImportPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_StagingIdentificationDocuments_ImportPackageId_OriginalEntityId",
                table: "StagingIdentificationDocuments",
                columns: new[] { "ImportPackageId", "OriginalEntityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StagingIdentificationDocuments_ImportPackageId_OriginalPersonId",
                table: "StagingIdentificationDocuments",
                columns: new[] { "ImportPackageId", "OriginalPersonId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StagingIdentificationDocuments");
        }
    }
}
