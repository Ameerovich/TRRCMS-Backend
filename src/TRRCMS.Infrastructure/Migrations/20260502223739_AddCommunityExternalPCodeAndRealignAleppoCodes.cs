using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCommunityExternalPCodeAndRealignAleppoCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalPCode",
                table: "Communities",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true,
                comment: "OCHA community P-Code, e.g. C1007. Cannot be derived from numeric Code.");

            migrationBuilder.CreateIndex(
                name: "IX_Communities_ExternalPCode",
                table: "Communities",
                column: "ExternalPCode",
                filter: "\"ExternalPCode\" IS NOT NULL AND \"IsDeleted\" = false");

            // ── Drop the three composite admin-hierarchy foreign keys before rebasing ──
            // Children reference parent codes that we are about to change. EF's FKs are
            // NOT DEFERRABLE, so we drop and recreate them around the data block.
            migrationBuilder.DropForeignKey(
                name: "FK_Districts_Governorates_GovernorateCode",
                table: "Districts");

            migrationBuilder.DropForeignKey(
                name: "FK_SubDistricts_Districts_GovernorateCode_DistrictCode",
                table: "SubDistricts");

            migrationBuilder.DropForeignKey(
                name: "FK_Communities_SubDistricts_GovernorateCode_DistrictCode_SubDi~",
                table: "Communities");

            // ── Realign Aleppo seed/test data to OCHA codes ───────────────────────────
            // Existing seed used internal codes 01/01/01 for Aleppo Governorate /
            // Mount Simeon District / Markaz Jebel Saman SubDistrict. OCHA assigns
            // SY02 / SY0200 / SY020000 respectively, so we now standardize on the
            // numeric portions 02 / 00 / 00. Each statement is guarded by a WHERE
            // clause that matches only the pre-realignment state, making the whole
            // block idempotent (re-running is a no-op).
            migrationBuilder.Sql(@"
DO $$
BEGIN
    -- 1. Aleppo governorate row: code 01 -> 02
    UPDATE ""Governorates""
       SET ""Code"" = '02'
     WHERE ""Code"" = '01' AND ""NameEnglish"" = 'Aleppo';

    -- 2. Mount Simeon district under Aleppo: code 01 -> 00, parent 01 -> 02
    UPDATE ""Districts""
       SET ""Code"" = '00', ""GovernorateCode"" = '02'
     WHERE ""GovernorateCode"" = '01' AND ""Code"" = '01';

    -- 3. Other districts under former Aleppo: just rebase the parent
    UPDATE ""Districts""
       SET ""GovernorateCode"" = '02'
     WHERE ""GovernorateCode"" = '01' AND ""Code"" <> '01';

    -- 4. Markaz Jebel Saman sub-district: code 01 -> 00 + rebase parents
    UPDATE ""SubDistricts""
       SET ""Code"" = '00', ""DistrictCode"" = '00', ""GovernorateCode"" = '02'
     WHERE ""GovernorateCode"" = '01' AND ""DistrictCode"" = '01' AND ""Code"" = '01';

    -- 5. Other sub-districts under Mount Simeon: rebase parents, keep own code
    UPDATE ""SubDistricts""
       SET ""DistrictCode"" = '00', ""GovernorateCode"" = '02'
     WHERE ""GovernorateCode"" = '01' AND ""DistrictCode"" = '01' AND ""Code"" <> '01';

    -- 6. Sub-districts under other Aleppo districts: only rebase governorate
    UPDATE ""SubDistricts""
       SET ""GovernorateCode"" = '02'
     WHERE ""GovernorateCode"" = '01' AND ""DistrictCode"" <> '01';

    -- 7. Aleppo-city community: stamp ExternalPCode = C1007 + rebase parents
    UPDATE ""Communities""
       SET ""GovernorateCode"" = '02', ""DistrictCode"" = '00', ""SubDistrictCode"" = '00',
           ""ExternalPCode"" = 'C1007'
     WHERE ""GovernorateCode"" = '01' AND ""DistrictCode"" = '01'
       AND ""SubDistrictCode"" = '01' AND ""Code"" = '001'
       AND (""ExternalPCode"" IS NULL OR ""ExternalPCode"" = '');

    -- 8. Sibling communities under the same parent: rebase parents only
    UPDATE ""Communities""
       SET ""GovernorateCode"" = '02', ""DistrictCode"" = '00', ""SubDistrictCode"" = '00'
     WHERE ""GovernorateCode"" = '01' AND ""DistrictCode"" = '01'
       AND ""SubDistrictCode"" = '01' AND ""Code"" <> '001';

    -- 9. Communities under Mount Simeon's other sub-districts
    UPDATE ""Communities""
       SET ""GovernorateCode"" = '02', ""DistrictCode"" = '00'
     WHERE ""GovernorateCode"" = '01' AND ""DistrictCode"" = '01' AND ""SubDistrictCode"" <> '01';

    -- 10. Communities under other Aleppo districts
    UPDATE ""Communities""
       SET ""GovernorateCode"" = '02'
     WHERE ""GovernorateCode"" = '01' AND ""DistrictCode"" <> '01';

    -- 11. Neighborhoods under Markaz Jebel Saman: full rebase
    UPDATE ""Neighborhoods""
       SET ""GovernorateCode"" = '02', ""DistrictCode"" = '00', ""SubDistrictCode"" = '00'
     WHERE ""GovernorateCode"" = '01' AND ""DistrictCode"" = '01' AND ""SubDistrictCode"" = '01';

    -- 12. Neighborhoods under Mount Simeon's other sub-districts
    UPDATE ""Neighborhoods""
       SET ""GovernorateCode"" = '02', ""DistrictCode"" = '00'
     WHERE ""GovernorateCode"" = '01' AND ""DistrictCode"" = '01' AND ""SubDistrictCode"" <> '01';

    -- 13. Neighborhoods under other Aleppo districts
    UPDATE ""Neighborhoods""
       SET ""GovernorateCode"" = '02'
     WHERE ""GovernorateCode"" = '01' AND ""DistrictCode"" <> '01';

    -- 14. Recompute Neighborhoods.FullCode wherever the cached value is stale
    UPDATE ""Neighborhoods""
       SET ""FullCode"" = ""GovernorateCode"" || ""DistrictCode"" || ""SubDistrictCode""
                        || ""CommunityCode""  || ""NeighborhoodCode""
     WHERE ""FullCode"" <> ""GovernorateCode"" || ""DistrictCode"" || ""SubDistrictCode""
                        || ""CommunityCode""  || ""NeighborhoodCode"";

    -- 15. Buildings: rebase admin codes (rows under former gov 01)
    UPDATE ""Buildings""
       SET ""GovernorateCode"" = '02', ""DistrictCode"" = '00', ""SubDistrictCode"" = '00'
     WHERE ""GovernorateCode"" = '01' AND ""DistrictCode"" = '01' AND ""SubDistrictCode"" = '01';
    UPDATE ""Buildings""
       SET ""GovernorateCode"" = '02', ""DistrictCode"" = '00'
     WHERE ""GovernorateCode"" = '01' AND ""DistrictCode"" = '01' AND ""SubDistrictCode"" <> '01';
    UPDATE ""Buildings""
       SET ""GovernorateCode"" = '02'
     WHERE ""GovernorateCode"" = '01' AND ""DistrictCode"" <> '01';

    -- 16. Recompute BuildingId (first 12 chars) wherever it is stale; preserve last 5
    UPDATE ""Buildings""
       SET ""BuildingId"" = ""GovernorateCode"" || ""DistrictCode"" || ""SubDistrictCode""
                          || ""CommunityCode""  || ""NeighborhoodCode""
                          || SUBSTRING(""BuildingId"" FROM 13)
     WHERE LENGTH(""BuildingId"") = 17
       AND SUBSTRING(""BuildingId"" FROM 1 FOR 12) <>
           ""GovernorateCode"" || ""DistrictCode"" || ""SubDistrictCode""
        || ""CommunityCode""  || ""NeighborhoodCode"";

    -- 17. StagingBuildings: same admin-code rebase
    UPDATE ""StagingBuildings""
       SET ""GovernorateCode"" = '02', ""DistrictCode"" = '00', ""SubDistrictCode"" = '00'
     WHERE ""GovernorateCode"" = '01' AND ""DistrictCode"" = '01' AND ""SubDistrictCode"" = '01';
    UPDATE ""StagingBuildings""
       SET ""GovernorateCode"" = '02', ""DistrictCode"" = '00'
     WHERE ""GovernorateCode"" = '01' AND ""DistrictCode"" = '01' AND ""SubDistrictCode"" <> '01';
    UPDATE ""StagingBuildings""
       SET ""GovernorateCode"" = '02'
     WHERE ""GovernorateCode"" = '01' AND ""DistrictCode"" <> '01';

    UPDATE ""StagingBuildings""
       SET ""BuildingId"" = ""GovernorateCode"" || ""DistrictCode"" || ""SubDistrictCode""
                          || ""CommunityCode""  || ""NeighborhoodCode""
                          || SUBSTRING(""BuildingId"" FROM 13)
     WHERE ""BuildingId"" IS NOT NULL
       AND LENGTH(""BuildingId"") = 17
       AND SUBSTRING(""BuildingId"" FROM 1 FOR 12) <>
           ""GovernorateCode"" || ""DistrictCode"" || ""SubDistrictCode""
        || ""CommunityCode""  || ""NeighborhoodCode"";
END $$;
");

            // ── Recreate the three composite FKs exactly as before ────────────────────
            migrationBuilder.AddForeignKey(
                name: "FK_Districts_Governorates_GovernorateCode",
                table: "Districts",
                column: "GovernorateCode",
                principalTable: "Governorates",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SubDistricts_Districts_GovernorateCode_DistrictCode",
                table: "SubDistricts",
                columns: new[] { "GovernorateCode", "DistrictCode" },
                principalTable: "Districts",
                principalColumns: new[] { "GovernorateCode", "Code" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Communities_SubDistricts_GovernorateCode_DistrictCode_SubDi~",
                table: "Communities",
                columns: new[] { "GovernorateCode", "DistrictCode", "SubDistrictCode" },
                principalTable: "SubDistricts",
                principalColumns: new[] { "GovernorateCode", "DistrictCode", "Code" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Communities_ExternalPCode",
                table: "Communities");

            migrationBuilder.DropColumn(
                name: "ExternalPCode",
                table: "Communities");
        }
    }
}
