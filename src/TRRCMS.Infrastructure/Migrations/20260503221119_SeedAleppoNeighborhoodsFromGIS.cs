using Microsoft.EntityFrameworkCore.Migrations;
using TRRCMS.Infrastructure.Persistence.SeedData;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <summary>
    /// Seeds the 109 official Aleppo neighborhoods (codes N0119–N0227) from the
    /// embedded GIS dataset (Data/aleppo_neighborhoods_v1.json, sourced from
    /// city_neighbourhoods.shp delivered by the GIS team on 2026-03-29).
    ///
    /// Behavior:
    ///   * INSERTs all 109 rows on a fresh DB (codes 119..227 don't collide with
    ///     the legacy placeholder seed codes 001..020).
    ///   * On a DB that already has these 109 rows, the ON CONFLICT (FullCode)
    ///     branch runs and updates names / geometry in place — idempotent.
    ///   * Soft-deletes the legacy placeholder seed (codes 001..020 under
    ///     02/00/00/001) so it stops appearing in dropdowns/maps. Buildings that
    ///     stored those codes keep their stored values; only the reference rows
    ///     are hidden.
    ///   * The data lives in an embedded resource, so this migration works
    ///     identically on fresh local DBs, Docker images, CI, staging, and prod.
    ///
    /// To roll out a future GIS update:
    ///   1. Run tools/<i>UpdateAleppoNeighborhoodsJson</i> to regenerate the JSON
    ///      from the new shapefile. Bump the filename to <c>_v2.json</c>.
    ///   2. Either write a new migration that points at the v2 file, or POST the
    ///      JSON to <c>/api/v1/Neighborhoods/import-bulk</c> for a hot reload.
    /// </summary>
    public partial class SeedAleppoNeighborhoodsFromGIS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var dataset = AleppoNeighborhoodsImporter.LoadEmbedded();
            foreach (var sql in AleppoNeighborhoodsImporter.BuildSeedSqlStatements(dataset))
            {
                migrationBuilder.Sql(sql);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Soft-delete the 109 GIS-supplied rows (codes 119..227 under 02/00/00/001).
            // Do not restore the legacy placeholders — that would resurrect stale data.
            migrationBuilder.Sql(@"
UPDATE ""Neighborhoods""
   SET ""IsDeleted""    = true,
       ""DeletedAtUtc"" = NOW() AT TIME ZONE 'UTC',
       ""DeletedBy""    = '00000000-0000-0000-0000-000000000000'
 WHERE ""GovernorateCode""  = '02'
   AND ""DistrictCode""     = '00'
   AND ""SubDistrictCode""  = '00'
   AND ""CommunityCode""    = '001'
   AND ""NeighborhoodCode"" >= '119'
   AND ""NeighborhoodCode"" <= '227'
   AND ""IsDeleted""        = false;");
        }
    }
}
