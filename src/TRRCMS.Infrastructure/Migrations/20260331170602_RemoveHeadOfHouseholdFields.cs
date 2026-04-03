using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveHeadOfHouseholdFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Safe drop: FK and index may not exist on fresh databases that never had these fields
            migrationBuilder.Sql(@"
                ALTER TABLE ""Households"" DROP CONSTRAINT IF EXISTS ""FK_Households_Persons_HeadOfHouseholdPersonId"";
                DROP INDEX IF EXISTS ""IX_Household_HeadOfHouseholdPersonId"";
            ");

            // Safe drop: columns may not exist on fresh databases
            migrationBuilder.Sql(@"
                ALTER TABLE ""StagingHouseholds"" DROP COLUMN IF EXISTS ""HeadOfHouseholdName"";
                ALTER TABLE ""StagingHouseholds"" DROP COLUMN IF EXISTS ""OriginalHeadOfHouseholdPersonId"";
                ALTER TABLE ""Households"" DROP COLUMN IF EXISTS ""HeadOfHouseholdName"";
                ALTER TABLE ""Households"" DROP COLUMN IF EXISTS ""HeadOfHouseholdPersonId"";
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HeadOfHouseholdName",
                table: "StagingHouseholds",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "OriginalHeadOfHouseholdPersonId",
                table: "StagingHouseholds",
                type: "uuid",
                nullable: true,
                comment: "Original head-of-household Person UUID from .uhc");

            migrationBuilder.AddColumn<string>(
                name: "HeadOfHouseholdName",
                table: "Households",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                comment: "رب الأسرة/العميل - Name of head of household (nullable for office survey)");

            migrationBuilder.AddColumn<Guid>(
                name: "HeadOfHouseholdPersonId",
                table: "Households",
                type: "uuid",
                nullable: true,
                comment: "Foreign key to Person (head of household)");

            migrationBuilder.CreateIndex(
                name: "IX_Household_HeadOfHouseholdPersonId",
                table: "Households",
                column: "HeadOfHouseholdPersonId");

            migrationBuilder.AddForeignKey(
                name: "FK_Households_Persons_HeadOfHouseholdPersonId",
                table: "Households",
                column: "HeadOfHouseholdPersonId",
                principalTable: "Persons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
