using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContactPersonAndCaseStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Idempotent: handle databases where the frontend team already applied changes manually

            // Drop old Claims.Status index and column (may already be gone)
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_Claims_Status"";");
            migrationBuilder.Sql(@"ALTER TABLE ""StagingClaims"" DROP COLUMN IF EXISTS ""Status"";");
            migrationBuilder.Sql(@"ALTER TABLE ""Claims"" DROP COLUMN IF EXISTS ""Status"";");

            // Surveys: ContactPersonFullName, ContactPersonId
            migrationBuilder.Sql(@"
                ALTER TABLE ""Surveys"" ADD COLUMN IF NOT EXISTS ""ContactPersonFullName"" character varying(500);
                COMMENT ON COLUMN ""Surveys"".""ContactPersonFullName"" IS 'Denormalized contact person full name: firstname fathername familyname (mothername)';
            ");
            migrationBuilder.Sql(@"
                ALTER TABLE ""Surveys"" ADD COLUMN IF NOT EXISTS ""ContactPersonId"" uuid;
                COMMENT ON COLUMN ""Surveys"".""ContactPersonId"" IS 'FK to Person who is the contact person for this survey';
            ");

            // StagingSurveys: OriginalContactPersonId
            migrationBuilder.Sql(@"
                ALTER TABLE ""StagingSurveys"" ADD COLUMN IF NOT EXISTS ""OriginalContactPersonId"" uuid;
                COMMENT ON COLUMN ""StagingSurveys"".""OriginalContactPersonId"" IS 'Original Contact Person UUID from .uhc — not a FK to production Persons';
            ");

            // StagingPersons: IsContactPerson
            migrationBuilder.Sql(@"
                ALTER TABLE ""StagingPersons"" ADD COLUMN IF NOT EXISTS ""IsContactPerson"" boolean NOT NULL DEFAULT false;
            ");

            // StagingClaims: CaseStatus
            migrationBuilder.Sql(@"
                ALTER TABLE ""StagingClaims"" ADD COLUMN IF NOT EXISTS ""CaseStatus"" integer;
                COMMENT ON COLUMN ""StagingClaims"".""CaseStatus"" IS 'Optional — auto-set to Open during commit';
            ");

            // Claims: CaseStatus (default 1 = Open for existing rows)
            migrationBuilder.Sql(@"
                ALTER TABLE ""Claims"" ADD COLUMN IF NOT EXISTS ""CaseStatus"" integer NOT NULL DEFAULT 1;
                COMMENT ON COLUMN ""Claims"".""CaseStatus"" IS 'Case status: 1=Open (حالة مفتوحة), 2=Closed (حالة مغلقة)';
            ");

            // Indexes (IF NOT EXISTS)
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ""IX_Surveys_ContactPersonId"" ON ""Surveys"" (""ContactPersonId"");");
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ""IX_StagingSurveys_ImportPackageId_OriginalContactPersonId"" ON ""StagingSurveys"" (""ImportPackageId"", ""OriginalContactPersonId"");");
            migrationBuilder.Sql(@"CREATE INDEX IF NOT EXISTS ""IX_Claims_CaseStatus"" ON ""Claims"" (""CaseStatus"");");

            // Foreign key (skip if already exists)
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'FK_Surveys_Persons_ContactPersonId') THEN
                        ALTER TABLE ""Surveys"" ADD CONSTRAINT ""FK_Surveys_Persons_ContactPersonId""
                            FOREIGN KEY (""ContactPersonId"") REFERENCES ""Persons"" (""Id"") ON DELETE SET NULL;
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Surveys_Persons_ContactPersonId",
                table: "Surveys");

            migrationBuilder.DropIndex(
                name: "IX_Surveys_ContactPersonId",
                table: "Surveys");

            migrationBuilder.DropIndex(
                name: "IX_StagingSurveys_ImportPackageId_OriginalContactPersonId",
                table: "StagingSurveys");

            migrationBuilder.DropIndex(
                name: "IX_Claims_CaseStatus",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "ContactPersonFullName",
                table: "Surveys");

            migrationBuilder.DropColumn(
                name: "ContactPersonId",
                table: "Surveys");

            migrationBuilder.DropColumn(
                name: "OriginalContactPersonId",
                table: "StagingSurveys");

            migrationBuilder.DropColumn(
                name: "IsContactPerson",
                table: "StagingPersons");

            migrationBuilder.DropColumn(
                name: "CaseStatus",
                table: "StagingClaims");

            migrationBuilder.DropColumn(
                name: "CaseStatus",
                table: "Claims");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "StagingClaims",
                type: "integer",
                nullable: true,
                comment: "Optional — auto-set to Draft during commit");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Claims",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Legacy status: 1=Draft, 2=Finalized, 3=UnderReview, 4=Approved, 5=Rejected, 6=PendingEvidence, 7=Disputed, 99=Archived (الحالة)");

            migrationBuilder.CreateIndex(
                name: "IX_Claims_Status",
                table: "Claims",
                column: "Status");
        }
    }
}
