using Microsoft.EntityFrameworkCore.Migrations;
using TRRCMS.Infrastructure.Persistence.SeedData;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <summary>
    /// Seeds sample buildings from the embedded GIS dataset
    /// (<c>Data/buildings_sample_v1.json</c>, sourced from a client-supplied
    /// shapefile via <c>tools/SeedBuildingsFromShapefile</c>).
    ///
    /// Behavior:
    ///   * INSERTs each row when its 17-digit BuildingId is missing.
    ///     <c>ON CONFLICT (BuildingId) DO NOTHING</c> makes re-runs a no-op.
    ///   * Each INSERT joins to <c>Communities / Governorates / Districts /
    ///     SubDistricts / Neighborhoods</c> to resolve the community local code
    ///     (from OCHA <c>ExternalPCode</c>) and the Arabic admin names. If the
    ///     hierarchy is missing for a row, the SELECT yields zero rows and the
    ///     row is silently skipped — safe.
    ///   * The data lives in an embedded resource, so this migration runs
    ///     identically on fresh local DBs, Docker images, CI, staging, and prod.
    ///
    /// To roll out a future shapefile update:
    ///   1. Drop the new shapefile under <c>tools/SeedBuildingsFromShapefile/shapefiles/&lt;batch&gt;/</c>.
    ///   2. Run <c>tools/SeedBuildingsFromShapefile</c> with <c>--output-json</c>
    ///      to regenerate the JSON. Bump the filename to <c>buildings_sample_v2.json</c>.
    ///   3. Either write a new migration that points at v2, or POST the JSON to
    ///      <c>/api/v1/Buildings/import-bulk</c> for a hot reload.
    /// </summary>
    public partial class SeedSampleBuildingsFromGIS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var dataset = BuildingsImporter.LoadEmbedded();
            foreach (var sql in BuildingsImporter.BuildSeedSqlStatements(dataset))
            {
                migrationBuilder.Sql(sql);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Soft-delete the buildings inserted by this migration: same admin
            // hierarchy + the BuildingNumbers from the embedded dataset.
            var dataset = BuildingsImporter.LoadEmbedded();
            if (dataset.Items is null || dataset.Items.Count == 0)
                return;

            var systemUser = "00000000-0000-0000-0000-000000000000";
            foreach (var item in dataset.Items)
            {
                if (string.IsNullOrWhiteSpace(item.BuildingNumber))
                    continue;
                var bnum = item.BuildingNumber.Replace("'", "''");
                migrationBuilder.Sql($@"
UPDATE ""Buildings""
   SET ""IsDeleted""    = true,
       ""DeletedAtUtc"" = NOW() AT TIME ZONE 'UTC',
       ""DeletedBy""    = '{systemUser}'
 WHERE ""BuildingNumber"" = '{bnum}'
   AND ""IsDeleted""      = false
   AND ""CreatedBy""      = '{systemUser}';");
            }
        }
    }
}
