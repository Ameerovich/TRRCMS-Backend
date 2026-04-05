using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeparateIdentificationDocumentEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsEditable",
                table: "Cases",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.CreateTable(
                name: "IdentificationDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentType = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    MimeType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FileHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    DocumentIssuedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DocumentExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IssuingAuthority = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DocumentReferenceNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("PK_IdentificationDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IdentificationDocuments_Persons_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IdentificationDocuments_DocumentType",
                table: "IdentificationDocuments",
                column: "DocumentType");

            migrationBuilder.CreateIndex(
                name: "IX_IdentificationDocuments_FileHash",
                table: "IdentificationDocuments",
                column: "FileHash");

            migrationBuilder.CreateIndex(
                name: "IX_IdentificationDocuments_PersonId",
                table: "IdentificationDocuments",
                column: "PersonId");

            // Data migration: copy existing identification document Evidence records to the new table
            migrationBuilder.Sql(@"
                INSERT INTO ""IdentificationDocuments"" (
                    ""Id"", ""PersonId"", ""DocumentType"", ""Description"",
                    ""OriginalFileName"", ""FilePath"", ""FileSizeBytes"", ""MimeType"", ""FileHash"",
                    ""DocumentIssuedDate"", ""DocumentExpiryDate"", ""IssuingAuthority"",
                    ""DocumentReferenceNumber"", ""Notes"",
                    ""CreatedAtUtc"", ""CreatedBy"", ""LastModifiedAtUtc"", ""LastModifiedBy"",
                    ""IsDeleted"", ""DeletedAtUtc"", ""DeletedBy""
                )
                SELECT
                    gen_random_uuid(), ""PersonId"", 1, COALESCE(""Description"", ''),
                    ""OriginalFileName"", ""FilePath"", ""FileSizeBytes"", ""MimeType"", ""FileHash"",
                    ""DocumentIssuedDate"", ""DocumentExpiryDate"", ""IssuingAuthority"",
                    ""DocumentReferenceNumber"", ""Notes"",
                    ""CreatedAtUtc"", ""CreatedBy"", ""LastModifiedAtUtc"", ""LastModifiedBy"",
                    ""IsDeleted"", ""DeletedAtUtc"", ""DeletedBy""
                FROM ""Evidences""
                WHERE ""EvidenceType"" = 1 AND ""PersonId"" IS NOT NULL;
            ");

            // Soft-delete migrated identification evidence records from the old table
            migrationBuilder.Sql(@"
                UPDATE ""Evidences"" SET ""IsDeleted"" = true, ""DeletedAtUtc"" = NOW()
                WHERE ""EvidenceType"" = 1 AND ""PersonId"" IS NOT NULL AND ""IsDeleted"" = false;
            ");

            // Now safe to drop PersonId from Evidences (data already migrated)
            migrationBuilder.DropForeignKey(
                name: "FK_Evidences_Persons_PersonId",
                table: "Evidences");

            migrationBuilder.DropIndex(
                name: "IX_Evidences_PersonId",
                table: "Evidences");

            migrationBuilder.DropColumn(
                name: "PersonId",
                table: "Evidences");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IdentificationDocuments");

            migrationBuilder.AddColumn<Guid>(
                name: "PersonId",
                table: "Evidences",
                type: "uuid",
                nullable: true,
                comment: "Foreign key to Person");

            migrationBuilder.AlterColumn<bool>(
                name: "IsEditable",
                table: "Cases",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_Evidences_PersonId",
                table: "Evidences",
                column: "PersonId");

            migrationBuilder.AddForeignKey(
                name: "FK_Evidences_Persons_PersonId",
                table: "Evidences",
                column: "PersonId",
                principalTable: "Persons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
