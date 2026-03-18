using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueNationalIdIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Surveys_RegistrationNumber",
                table: "Surveys");

            migrationBuilder.DropIndex(
                name: "IX_Person_NationalId",
                table: "Persons");

            migrationBuilder.AlterColumn<string>(
                name: "NationalId",
                table: "StagingPersons",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                comment: "Primary key for duplicate detection",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true,
                oldComment: "Primary key for duplicate detection (FR-D-5, §12.2.4)");

            migrationBuilder.AlterColumn<string>(
                name: "FileHash",
                table: "StagingEvidences",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true,
                comment: "SHA-256 hash for deduplication during commit",
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128,
                oldNullable: true,
                oldComment: "SHA-256 hash for deduplication during commit (FR-D-9)");

            migrationBuilder.AlterColumn<string>(
                name: "ClaimNumber",
                table: "StagingClaims",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true,
                comment: "Optional in staging — auto-generated during commit",
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30,
                oldNullable: true,
                oldComment: "Optional in staging — auto-generated during commit (FR-D-8)");

            migrationBuilder.AlterColumn<string>(
                name: "FileHash",
                table: "StagingBuildingDocuments",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true,
                comment: "SHA-256 hash for deduplication during commit",
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128,
                oldNullable: true,
                oldComment: "SHA-256 hash for deduplication during commit (FR-D-9)");

            migrationBuilder.CreateIndex(
                name: "IX_Surveys_RegistrationNumber",
                table: "Surveys",
                column: "RegistrationNumber",
                filter: "\"RegistrationNumber\" IS NOT NULL");

            // Resolve existing duplicate NationalIds before creating unique index.
            // For each duplicate group: keep the most recently created record, soft-delete the rest.
            migrationBuilder.Sql(@"
                UPDATE ""Persons"" SET
                    ""IsDeleted"" = true,
                    ""DeletedAtUtc"" = NOW(),
                    ""LastModifiedAtUtc"" = NOW()
                WHERE ""IsDeleted"" = false
                  AND ""NationalId"" IS NOT NULL
                  AND ""Id"" NOT IN (
                      SELECT DISTINCT ON (""NationalId"") ""Id""
                      FROM ""Persons""
                      WHERE ""IsDeleted"" = false AND ""NationalId"" IS NOT NULL
                      ORDER BY ""NationalId"", ""CreatedAtUtc"" DESC
                  );
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Person_NationalId",
                table: "Persons",
                column: "NationalId",
                unique: true,
                filter: "\"NationalId\" IS NOT NULL AND \"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Surveys_RegistrationNumber",
                table: "Surveys");

            migrationBuilder.DropIndex(
                name: "IX_Person_NationalId",
                table: "Persons");

            migrationBuilder.AlterColumn<string>(
                name: "NationalId",
                table: "StagingPersons",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                comment: "Primary key for duplicate detection (FR-D-5, §12.2.4)",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true,
                oldComment: "Primary key for duplicate detection");

            migrationBuilder.AlterColumn<string>(
                name: "FileHash",
                table: "StagingEvidences",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true,
                comment: "SHA-256 hash for deduplication during commit (FR-D-9)",
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128,
                oldNullable: true,
                oldComment: "SHA-256 hash for deduplication during commit");

            migrationBuilder.AlterColumn<string>(
                name: "ClaimNumber",
                table: "StagingClaims",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true,
                comment: "Optional in staging — auto-generated during commit (FR-D-8)",
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30,
                oldNullable: true,
                oldComment: "Optional in staging — auto-generated during commit");

            migrationBuilder.AlterColumn<string>(
                name: "FileHash",
                table: "StagingBuildingDocuments",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true,
                comment: "SHA-256 hash for deduplication during commit (FR-D-9)",
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128,
                oldNullable: true,
                oldComment: "SHA-256 hash for deduplication during commit");

            migrationBuilder.CreateIndex(
                name: "IX_Surveys_RegistrationNumber",
                table: "Surveys",
                column: "RegistrationNumber",
                filter: "[RegistrationNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Person_NationalId",
                table: "Persons",
                column: "NationalId");
        }
    }
}
