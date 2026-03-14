using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DropUnusedPersonAndHouseholdFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Household_IsDisplaced_IsDeleted",
                table: "Households");

            migrationBuilder.DropColumn(
                name: "FullNameEnglish",
                table: "StagingPersons");

            migrationBuilder.DropColumn(
                name: "YearOfBirth",
                table: "StagingPersons");

            migrationBuilder.DropColumn(
                name: "AdultCount",
                table: "StagingHouseholds");

            migrationBuilder.DropColumn(
                name: "DisplacementReason",
                table: "StagingHouseholds");

            migrationBuilder.DropColumn(
                name: "EmployedPersonsCount",
                table: "StagingHouseholds");

            migrationBuilder.DropColumn(
                name: "InfantCount",
                table: "StagingHouseholds");

            migrationBuilder.DropColumn(
                name: "IsDisplaced",
                table: "StagingHouseholds");

            migrationBuilder.DropColumn(
                name: "IsFemaleHeaded",
                table: "StagingHouseholds");

            migrationBuilder.DropColumn(
                name: "MinorCount",
                table: "StagingHouseholds");

            migrationBuilder.DropColumn(
                name: "MonthlyIncomeEstimate",
                table: "StagingHouseholds");

            migrationBuilder.DropColumn(
                name: "OriginLocation",
                table: "StagingHouseholds");

            migrationBuilder.DropColumn(
                name: "OrphanCount",
                table: "StagingHouseholds");

            migrationBuilder.DropColumn(
                name: "PrimaryIncomeSource",
                table: "StagingHouseholds");

            migrationBuilder.DropColumn(
                name: "SingleParentCount",
                table: "StagingHouseholds");

            migrationBuilder.DropColumn(
                name: "SpecialNeeds",
                table: "StagingHouseholds");

            migrationBuilder.DropColumn(
                name: "UnemployedPersonsCount",
                table: "StagingHouseholds");

            migrationBuilder.DropColumn(
                name: "WidowCount",
                table: "StagingHouseholds");

            migrationBuilder.DropColumn(
                name: "FullNameEnglish",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "HasIdentificationDocument",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "AdultCount",
                table: "Households");

            migrationBuilder.DropColumn(
                name: "ArrivalDate",
                table: "Households");

            migrationBuilder.DropColumn(
                name: "DisplacementReason",
                table: "Households");

            migrationBuilder.DropColumn(
                name: "EmployedPersonsCount",
                table: "Households");

            migrationBuilder.DropColumn(
                name: "InfantCount",
                table: "Households");

            migrationBuilder.DropColumn(
                name: "IsDisplaced",
                table: "Households");

            migrationBuilder.DropColumn(
                name: "IsFemaleHeaded",
                table: "Households");

            migrationBuilder.DropColumn(
                name: "MinorCount",
                table: "Households");

            migrationBuilder.DropColumn(
                name: "MonthlyIncomeEstimate",
                table: "Households");

            migrationBuilder.DropColumn(
                name: "OriginLocation",
                table: "Households");

            migrationBuilder.DropColumn(
                name: "OrphanCount",
                table: "Households");

            migrationBuilder.DropColumn(
                name: "PrimaryIncomeSource",
                table: "Households");

            migrationBuilder.DropColumn(
                name: "SingleParentCount",
                table: "Households");

            migrationBuilder.DropColumn(
                name: "SpecialNeeds",
                table: "Households");

            migrationBuilder.DropColumn(
                name: "UnemployedPersonsCount",
                table: "Households");

            migrationBuilder.DropColumn(
                name: "WidowCount",
                table: "Households");

            // Convert string columns to integer enums in StagingPersons (idempotent — skips columns already integer).
            // Uses USING NULL because staging data is transient and losing pending values is acceptable.
            migrationBuilder.Sql(
                """
                DO $$
                DECLARE
                    col_rec RECORD;
                BEGIN
                    FOR col_rec IN
                        SELECT column_name FROM information_schema.columns
                        WHERE table_name = 'StagingPersons'
                          AND column_name IN ('Gender', 'Nationality', 'RelationshipToHead')
                          AND data_type <> 'integer'
                    LOOP
                        EXECUTE format(
                            'ALTER TABLE "StagingPersons" ALTER COLUMN %I DROP DEFAULT, ALTER COLUMN %I TYPE integer USING NULL',
                            col_rec.column_name, col_rec.column_name);
                    END LOOP;
                END $$;

                COMMENT ON COLUMN "StagingPersons"."Gender"
                    IS 'الجنس - Gender enum stored as integer';
                COMMENT ON COLUMN "StagingPersons"."Nationality"
                    IS 'الجنسية - Nationality enum stored as integer';
                COMMENT ON COLUMN "StagingPersons"."RelationshipToHead"
                    IS 'صلة القرابة برب الأسرة - Relationship to head of household enum stored as integer';
                """);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "StagingPersons",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Date of birth — used in duplicate detection composite with name+gender");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "StagingPersons");

            migrationBuilder.AlterColumn<string>(
                name: "RelationshipToHead",
                table: "StagingPersons",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldComment: "صلة القرابة برب الأسرة - Relationship to head of household enum stored as integer");

            migrationBuilder.AlterColumn<string>(
                name: "Nationality",
                table: "StagingPersons",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldComment: "الجنسية - Nationality enum stored as integer");

            migrationBuilder.AlterColumn<string>(
                name: "Gender",
                table: "StagingPersons",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldComment: "الجنس - Gender enum stored as integer");

            migrationBuilder.AddColumn<string>(
                name: "FullNameEnglish",
                table: "StagingPersons",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "YearOfBirth",
                table: "StagingPersons",
                type: "integer",
                nullable: true,
                comment: "Year of birth — used in duplicate detection composite with name+gender");

            migrationBuilder.AddColumn<int>(
                name: "AdultCount",
                table: "StagingHouseholds",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DisplacementReason",
                table: "StagingHouseholds",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmployedPersonsCount",
                table: "StagingHouseholds",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "InfantCount",
                table: "StagingHouseholds",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsDisplaced",
                table: "StagingHouseholds",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFemaleHeaded",
                table: "StagingHouseholds",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MinorCount",
                table: "StagingHouseholds",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyIncomeEstimate",
                table: "StagingHouseholds",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OriginLocation",
                table: "StagingHouseholds",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrphanCount",
                table: "StagingHouseholds",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryIncomeSource",
                table: "StagingHouseholds",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SingleParentCount",
                table: "StagingHouseholds",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SpecialNeeds",
                table: "StagingHouseholds",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UnemployedPersonsCount",
                table: "StagingHouseholds",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WidowCount",
                table: "StagingHouseholds",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "FullNameEnglish",
                table: "Persons",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true,
                comment: "Full name in English (optional)");

            migrationBuilder.AddColumn<bool>(
                name: "HasIdentificationDocument",
                table: "Persons",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "Flag indicating if ID document was uploaded");

            migrationBuilder.AddColumn<int>(
                name: "AdultCount",
                table: "Households",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Number of adults (18-64 years)");

            migrationBuilder.AddColumn<DateTime>(
                name: "ArrivalDate",
                table: "Households",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Date of arrival at current location");

            migrationBuilder.AddColumn<string>(
                name: "DisplacementReason",
                table: "Households",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                comment: "Reason for displacement");

            migrationBuilder.AddColumn<int>(
                name: "EmployedPersonsCount",
                table: "Households",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Number of employed persons");

            migrationBuilder.AddColumn<int>(
                name: "InfantCount",
                table: "Households",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Number of infants (under 2 years)");

            migrationBuilder.AddColumn<bool>(
                name: "IsDisplaced",
                table: "Households",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "Indicates if household is displaced");

            migrationBuilder.AddColumn<bool>(
                name: "IsFemaleHeaded",
                table: "Households",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "Indicates if household is female-headed");

            migrationBuilder.AddColumn<int>(
                name: "MinorCount",
                table: "Households",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Number of minors/adolescents (13-17 years)");

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyIncomeEstimate",
                table: "Households",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                comment: "Estimated monthly income");

            migrationBuilder.AddColumn<string>(
                name: "OriginLocation",
                table: "Households",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                comment: "Origin location if displaced");

            migrationBuilder.AddColumn<int>(
                name: "OrphanCount",
                table: "Households",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Number of orphans");

            migrationBuilder.AddColumn<string>(
                name: "PrimaryIncomeSource",
                table: "Households",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                comment: "Primary income source");

            migrationBuilder.AddColumn<int>(
                name: "SingleParentCount",
                table: "Households",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Number of single parents");

            migrationBuilder.AddColumn<string>(
                name: "SpecialNeeds",
                table: "Households",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                comment: "Special needs or circumstances");

            migrationBuilder.AddColumn<int>(
                name: "UnemployedPersonsCount",
                table: "Households",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Number of unemployed persons");

            migrationBuilder.AddColumn<int>(
                name: "WidowCount",
                table: "Households",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Number of widows");

            migrationBuilder.CreateIndex(
                name: "IX_Household_IsDisplaced_IsDeleted",
                table: "Households",
                columns: new[] { "IsDisplaced", "IsDeleted" });
        }
    }
}
