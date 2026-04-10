using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RealignHouseholdFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // =========================================================================
            // Household field realignment (aligned with .uhc v1.8).
            //
            // Order matters — we must:
            //   1. Add new columns (nullable, so no default needed on existing rows)
            //   2. Backfill new columns from the old columns — production table only
            //      (StagingHouseholds is ephemeral; no backfill needed)
            //   3. Drop the old columns
            //   4. Relax the nullability / default on MaleCount, FemaleCount,
            //      ChildCount, ElderlyCount (they become fully nullable)
            //
            // Semantic change:
            //   - OLD MaleCount = adult males only
            //   - NEW MaleCount = total males across all ages
            //   - OLD ChildCount / ElderlyCount were stored "legacy computed totals"
            //     that already hold the correct sums, so no update is needed for them.
            //   - OLD PersonsWithDisabilitiesCount → NEW DisabledCount
            // =========================================================================

            // ---- Step 1: add the three new columns to both tables ----

            migrationBuilder.AddColumn<int>(
                name: "AdultCount",
                table: "Households",
                type: "integer",
                nullable: true,
                comment: "عدد البالغين - Number of adults");

            migrationBuilder.AddColumn<int>(
                name: "DisabledCount",
                table: "Households",
                type: "integer",
                nullable: true,
                comment: "عدد ذوي الإعاقة - Number of persons with disabilities");

            migrationBuilder.AddColumn<DateTime>(
                name: "OccupancyStartDate",
                table: "Households",
                type: "timestamp with time zone",
                nullable: true,
                comment: "تاريخ بداية الإشغال - Date the household started occupying this unit (UTC)");

            migrationBuilder.AddColumn<int>(
                name: "AdultCount",
                table: "StagingHouseholds",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DisabledCount",
                table: "StagingHouseholds",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OccupancyStartDate",
                table: "StagingHouseholds",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Date the household started occupying this unit (UTC)");

            // ---- Step 2: backfill Households from the soon-to-be-dropped columns ----
            // Value-preserving: compute new AdultCount/MaleCount/FemaleCount/DisabledCount
            // from the existing per-gender/per-age columns. ChildCount and ElderlyCount
            // already hold the summed totals (they were "legacy computed total" columns),
            // so no update is needed for them.
            migrationBuilder.Sql(@"
                UPDATE ""Households"" SET
                    ""AdultCount""    = COALESCE(""MaleCount"", 0) + COALESCE(""FemaleCount"", 0),
                    ""DisabledCount"" = ""PersonsWithDisabilitiesCount"",
                    ""MaleCount""     = COALESCE(""MaleCount"", 0)
                                      + COALESCE(""MaleChildCount"", 0)
                                      + COALESCE(""MaleElderlyCount"", 0),
                    ""FemaleCount""   = COALESCE(""FemaleCount"", 0)
                                      + COALESCE(""FemaleChildCount"", 0)
                                      + COALESCE(""FemaleElderlyCount"", 0);
            ");

            // ---- Step 3: drop obsolete columns from both tables ----

            migrationBuilder.DropColumn(name: "FemaleChildCount", table: "StagingHouseholds");
            migrationBuilder.DropColumn(name: "FemaleDisabledCount", table: "StagingHouseholds");
            migrationBuilder.DropColumn(name: "FemaleElderlyCount", table: "StagingHouseholds");
            migrationBuilder.DropColumn(name: "MaleChildCount", table: "StagingHouseholds");
            migrationBuilder.DropColumn(name: "MaleDisabledCount", table: "StagingHouseholds");
            migrationBuilder.DropColumn(name: "MaleElderlyCount", table: "StagingHouseholds");
            migrationBuilder.DropColumn(name: "OccupancyType", table: "StagingHouseholds");
            migrationBuilder.DropColumn(name: "PersonsWithDisabilitiesCount", table: "StagingHouseholds");

            migrationBuilder.DropColumn(name: "FemaleChildCount", table: "Households");
            migrationBuilder.DropColumn(name: "FemaleDisabledCount", table: "Households");
            migrationBuilder.DropColumn(name: "FemaleElderlyCount", table: "Households");
            migrationBuilder.DropColumn(name: "MaleChildCount", table: "Households");
            migrationBuilder.DropColumn(name: "MaleDisabledCount", table: "Households");
            migrationBuilder.DropColumn(name: "MaleElderlyCount", table: "Households");
            migrationBuilder.DropColumn(name: "OccupancyType", table: "Households");
            migrationBuilder.DropColumn(name: "PersonsWithDisabilitiesCount", table: "Households");

            // ---- Step 4: relax nullability / drop defaults on the surviving count columns ----

            migrationBuilder.AlterColumn<int>(
                name: "MaleCount",
                table: "StagingHouseholds",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "FemaleCount",
                table: "StagingHouseholds",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "ElderlyCount",
                table: "StagingHouseholds",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "ChildCount",
                table: "StagingHouseholds",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "MaleCount",
                table: "Households",
                type: "integer",
                nullable: true,
                comment: "عدد الذكور - Total males (all ages)",
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0,
                oldComment: "عدد البالغين الذكور - Number of adult males");

            migrationBuilder.AlterColumn<int>(
                name: "FemaleCount",
                table: "Households",
                type: "integer",
                nullable: true,
                comment: "عدد الإناث - Total females (all ages)",
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0,
                oldComment: "عدد البالغين الإناث - Number of adult females");

            migrationBuilder.AlterColumn<int>(
                name: "ElderlyCount",
                table: "Households",
                type: "integer",
                nullable: true,
                comment: "عدد كبار السن - Number of elderly",
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0,
                oldComment: "Number of elderly (65+ years) - legacy total");

            migrationBuilder.AlterColumn<int>(
                name: "ChildCount",
                table: "Households",
                type: "integer",
                nullable: true,
                comment: "عدد الأطفال - Number of children",
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0,
                oldComment: "Number of children (2-12 years) - legacy total");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdultCount",
                table: "StagingHouseholds");

            migrationBuilder.DropColumn(
                name: "DisabledCount",
                table: "StagingHouseholds");

            migrationBuilder.DropColumn(
                name: "OccupancyStartDate",
                table: "StagingHouseholds");

            migrationBuilder.DropColumn(
                name: "AdultCount",
                table: "Households");

            migrationBuilder.DropColumn(
                name: "DisabledCount",
                table: "Households");

            migrationBuilder.DropColumn(
                name: "OccupancyStartDate",
                table: "Households");

            migrationBuilder.AlterColumn<int>(
                name: "MaleCount",
                table: "StagingHouseholds",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "FemaleCount",
                table: "StagingHouseholds",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ElderlyCount",
                table: "StagingHouseholds",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ChildCount",
                table: "StagingHouseholds",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FemaleChildCount",
                table: "StagingHouseholds",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FemaleDisabledCount",
                table: "StagingHouseholds",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FemaleElderlyCount",
                table: "StagingHouseholds",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaleChildCount",
                table: "StagingHouseholds",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaleDisabledCount",
                table: "StagingHouseholds",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaleElderlyCount",
                table: "StagingHouseholds",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OccupancyType",
                table: "StagingHouseholds",
                type: "integer",
                nullable: true,
                comment: "OccupancyType enum");

            migrationBuilder.AddColumn<int>(
                name: "PersonsWithDisabilitiesCount",
                table: "StagingHouseholds",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "MaleCount",
                table: "Households",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "عدد البالغين الذكور - Number of adult males",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldComment: "عدد الذكور - Total males (all ages)");

            migrationBuilder.AlterColumn<int>(
                name: "FemaleCount",
                table: "Households",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "عدد البالغين الإناث - Number of adult females",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldComment: "عدد الإناث - Total females (all ages)");

            migrationBuilder.AlterColumn<int>(
                name: "ElderlyCount",
                table: "Households",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Number of elderly (65+ years) - legacy total",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldComment: "عدد كبار السن - Number of elderly");

            migrationBuilder.AlterColumn<int>(
                name: "ChildCount",
                table: "Households",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Number of children (2-12 years) - legacy total",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldComment: "عدد الأطفال - Number of children");

            migrationBuilder.AddColumn<int>(
                name: "FemaleChildCount",
                table: "Households",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "عدد الأطفال الإناث (أقل من 18) - Number of female children under 18");

            migrationBuilder.AddColumn<int>(
                name: "FemaleDisabledCount",
                table: "Households",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "عدد المعاقين الإناث - Number of female persons with disabilities");

            migrationBuilder.AddColumn<int>(
                name: "FemaleElderlyCount",
                table: "Households",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "عدد كبار السن الإناث (أكثر من 65) - Number of female elderly over 65");

            migrationBuilder.AddColumn<int>(
                name: "MaleChildCount",
                table: "Households",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "عدد الأطفال الذكور (أقل من 18) - Number of male children under 18");

            migrationBuilder.AddColumn<int>(
                name: "MaleDisabledCount",
                table: "Households",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "عدد المعاقين الذكور - Number of male persons with disabilities");

            migrationBuilder.AddColumn<int>(
                name: "MaleElderlyCount",
                table: "Households",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "عدد كبار السن الذكور (أكثر من 65) - Number of male elderly over 65");

            migrationBuilder.AddColumn<int>(
                name: "OccupancyType",
                table: "Households",
                type: "integer",
                nullable: true,
                comment: "نوع الإشغال - Occupancy type enum stored as integer");

            migrationBuilder.AddColumn<int>(
                name: "PersonsWithDisabilitiesCount",
                table: "Households",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Total persons with disabilities - legacy total");
        }
    }
}
