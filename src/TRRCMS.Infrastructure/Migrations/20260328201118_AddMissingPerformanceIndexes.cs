using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <summary>
    /// Adds performance indexes on columns used in frequent queries but missing dedicated indexes.
    /// All indexes are filtered (WHERE IsDeleted = false or IsActive = true) to keep them small.
    /// Uses IF NOT EXISTS for idempotency.
    /// </summary>
    public partial class AddMissingPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Evidence.FileHash — used by EvidenceRepository.GetByFileHashAsync() for deduplication
            migrationBuilder.Sql("""
                CREATE INDEX IF NOT EXISTS "IX_Evidences_FileHash"
                ON "Evidences" ("FileHash")
                WHERE "IsDeleted" = false;
                """);

            // Surveys.PropertyUnitId — used by SurveyRepository.GetByPropertyUnitAsync()
            // Not an EF-managed FK, so no auto-index
            migrationBuilder.Sql("""
                CREATE INDEX IF NOT EXISTS "IX_Surveys_PropertyUnitId"
                ON "Surveys" ("PropertyUnitId")
                WHERE "IsDeleted" = false;
                """);

            // ImportPackages.ImportedByUserId — used by ImportPackageRepository.GetByImportedByUserAsync()
            // ExportedByUserId already has an index; ImportedByUserId does not
            migrationBuilder.Sql("""
                CREATE INDEX IF NOT EXISTS "IX_ImportPackages_ImportedByUserId"
                ON "ImportPackages" ("ImportedByUserId")
                WHERE "IsDeleted" = false;
                """);

            // Claims.CreatedBy — used in ClaimRepository.GetFilteredAsync() for "my claims" queries
            migrationBuilder.Sql("""
                CREATE INDEX IF NOT EXISTS "IX_Claims_CreatedBy"
                ON "Claims" ("CreatedBy")
                WHERE "IsDeleted" = false;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_Evidences_FileHash";""");
            migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_Surveys_PropertyUnitId";""");
            migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_ImportPackages_ImportedByUserId";""");
            migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_Claims_CreatedBy";""");
        }
    }
}
