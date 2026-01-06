using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateHouseholdConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "WidowCount",
                table: "Households",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Number of widows",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "UnemployedPersonsCount",
                table: "Households",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Number of unemployed persons",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "SpecialNeeds",
                table: "Households",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                comment: "Special needs or circumstances",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SingleParentCount",
                table: "Households",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Number of single parents",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                table: "Households",
                type: "bytea",
                rowVersion: true,
                nullable: true,
                comment: "Concurrency token",
                oldClrType: typeof(byte[]),
                oldType: "bytea",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "PropertyUnitId",
                table: "Households",
                type: "uuid",
                nullable: false,
                comment: "Foreign key to PropertyUnit",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "PrimaryIncomeSource",
                table: "Households",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                comment: "Primary income source",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PersonsWithDisabilitiesCount",
                table: "Households",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Number of persons with disabilities",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "OrphanCount",
                table: "Households",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Number of orphans",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "OriginLocation",
                table: "Households",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                comment: "Origin location if displaced",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Households",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                comment: "Household notes",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "MonthlyIncomeEstimate",
                table: "Households",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true,
                comment: "Estimated monthly income",
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MinorCount",
                table: "Households",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Number of minors/adolescents (13-17 years)",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "MaleCount",
                table: "Households",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Number of male members",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<Guid>(
                name: "LastModifiedBy",
                table: "Households",
                type: "uuid",
                nullable: true,
                comment: "User who last modified this record",
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastModifiedAtUtc",
                table: "Households",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Last modification timestamp (UTC)",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsFemaleHeaded",
                table: "Households",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "Indicates if household is female-headed",
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDisplaced",
                table: "Households",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "Indicates if household is displaced",
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Households",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "Soft delete flag",
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<int>(
                name: "InfantCount",
                table: "Households",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Number of infants (under 2 years)",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "HouseholdSize",
                table: "Households",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Total household size",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<Guid>(
                name: "HeadOfHouseholdPersonId",
                table: "Households",
                type: "uuid",
                nullable: true,
                comment: "Foreign key to Person (head of household)",
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "HeadOfHouseholdName",
                table: "Households",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                comment: "Name of head of household",
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<int>(
                name: "FemaleCount",
                table: "Households",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Number of female members",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "EmployedPersonsCount",
                table: "Households",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Number of employed persons",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "ElderlyCount",
                table: "Households",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Number of elderly (65+ years)",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "DisplacementReason",
                table: "Households",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                comment: "Reason for displacement",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "DeletedBy",
                table: "Households",
                type: "uuid",
                nullable: true,
                comment: "User who deleted this record",
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAtUtc",
                table: "Households",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Deletion timestamp (UTC)",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedBy",
                table: "Households",
                type: "uuid",
                nullable: false,
                comment: "User who created this record",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "Households",
                type: "timestamp with time zone",
                nullable: false,
                comment: "Creation timestamp (UTC)",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<int>(
                name: "ChildCount",
                table: "Households",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Number of children (2-12 years)",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ArrivalDate",
                table: "Households",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Date of arrival at current location",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "AdultCount",
                table: "Households",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Number of adults (18-64 years)",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.CreateIndex(
                name: "IX_Household_IsDisplaced_IsDeleted",
                table: "Households",
                columns: new[] { "IsDisplaced", "IsDeleted" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Household_IsDisplaced_IsDeleted",
                table: "Households");

            migrationBuilder.AlterColumn<int>(
                name: "WidowCount",
                table: "Households",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0,
                oldComment: "Number of widows");

            migrationBuilder.AlterColumn<int>(
                name: "UnemployedPersonsCount",
                table: "Households",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0,
                oldComment: "Number of unemployed persons");

            migrationBuilder.AlterColumn<string>(
                name: "SpecialNeeds",
                table: "Households",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true,
                oldComment: "Special needs or circumstances");

            migrationBuilder.AlterColumn<int>(
                name: "SingleParentCount",
                table: "Households",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0,
                oldComment: "Number of single parents");

            migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                table: "Households",
                type: "bytea",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "bytea",
                oldRowVersion: true,
                oldNullable: true,
                oldComment: "Concurrency token");

            migrationBuilder.AlterColumn<Guid>(
                name: "PropertyUnitId",
                table: "Households",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldComment: "Foreign key to PropertyUnit");

            migrationBuilder.AlterColumn<string>(
                name: "PrimaryIncomeSource",
                table: "Households",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true,
                oldComment: "Primary income source");

            migrationBuilder.AlterColumn<int>(
                name: "PersonsWithDisabilitiesCount",
                table: "Households",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0,
                oldComment: "Number of persons with disabilities");

            migrationBuilder.AlterColumn<int>(
                name: "OrphanCount",
                table: "Households",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0,
                oldComment: "Number of orphans");

            migrationBuilder.AlterColumn<string>(
                name: "OriginLocation",
                table: "Households",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true,
                oldComment: "Origin location if displaced");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Households",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true,
                oldComment: "Household notes");

            migrationBuilder.AlterColumn<decimal>(
                name: "MonthlyIncomeEstimate",
                table: "Households",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true,
                oldComment: "Estimated monthly income");

            migrationBuilder.AlterColumn<int>(
                name: "MinorCount",
                table: "Households",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0,
                oldComment: "Number of minors/adolescents (13-17 years)");

            migrationBuilder.AlterColumn<int>(
                name: "MaleCount",
                table: "Households",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0,
                oldComment: "Number of male members");

            migrationBuilder.AlterColumn<Guid>(
                name: "LastModifiedBy",
                table: "Households",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true,
                oldComment: "User who last modified this record");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastModifiedAtUtc",
                table: "Households",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "Last modification timestamp (UTC)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsFemaleHeaded",
                table: "Households",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false,
                oldComment: "Indicates if household is female-headed");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDisplaced",
                table: "Households",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false,
                oldComment: "Indicates if household is displaced");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Households",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false,
                oldComment: "Soft delete flag");

            migrationBuilder.AlterColumn<int>(
                name: "InfantCount",
                table: "Households",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0,
                oldComment: "Number of infants (under 2 years)");

            migrationBuilder.AlterColumn<int>(
                name: "HouseholdSize",
                table: "Households",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0,
                oldComment: "Total household size");

            migrationBuilder.AlterColumn<Guid>(
                name: "HeadOfHouseholdPersonId",
                table: "Households",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true,
                oldComment: "Foreign key to Person (head of household)");

            migrationBuilder.AlterColumn<string>(
                name: "HeadOfHouseholdName",
                table: "Households",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldComment: "Name of head of household");

            migrationBuilder.AlterColumn<int>(
                name: "FemaleCount",
                table: "Households",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0,
                oldComment: "Number of female members");

            migrationBuilder.AlterColumn<int>(
                name: "EmployedPersonsCount",
                table: "Households",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0,
                oldComment: "Number of employed persons");

            migrationBuilder.AlterColumn<int>(
                name: "ElderlyCount",
                table: "Households",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0,
                oldComment: "Number of elderly (65+ years)");

            migrationBuilder.AlterColumn<string>(
                name: "DisplacementReason",
                table: "Households",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true,
                oldComment: "Reason for displacement");

            migrationBuilder.AlterColumn<Guid>(
                name: "DeletedBy",
                table: "Households",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true,
                oldComment: "User who deleted this record");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAtUtc",
                table: "Households",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "Deletion timestamp (UTC)");

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedBy",
                table: "Households",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldComment: "User who created this record");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "Households",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldComment: "Creation timestamp (UTC)");

            migrationBuilder.AlterColumn<int>(
                name: "ChildCount",
                table: "Households",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0,
                oldComment: "Number of children (2-12 years)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ArrivalDate",
                table: "Households",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "Date of arrival at current location");

            migrationBuilder.AlterColumn<int>(
                name: "AdultCount",
                table: "Households",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0,
                oldComment: "Number of adults (18-64 years)");
        }
    }
}
