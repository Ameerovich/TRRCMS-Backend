using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOccupancyFieldsToHousehold : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDate",
                table: "PersonPropertyRelations",
                type: "timestamp with time zone",
                nullable: true,
                comment: "تاريخ بدء العلاقة - Start date of the relation (deprecated for office survey)",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "تاريخ بدء العلاقة - Start date of the relation");

            migrationBuilder.AlterColumn<string>(
                name: "RelationTypeOtherDesc",
                table: "PersonPropertyRelations",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                comment: "Description when relation type is 'Other' (deprecated for office survey)",
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true,
                oldComment: "Description when relation type is 'Other'");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndDate",
                table: "PersonPropertyRelations",
                type: "timestamp with time zone",
                nullable: true,
                comment: "End date of the relation (deprecated for office survey)",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "End date of the relation");

            migrationBuilder.AlterColumn<string>(
                name: "ContractTypeOtherDesc",
                table: "PersonPropertyRelations",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                comment: "Description when contract type is 'Other' (deprecated for office survey)",
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true,
                oldComment: "Description when contract type is 'Other'");

            migrationBuilder.AlterColumn<int>(
                name: "ContractType",
                table: "PersonPropertyRelations",
                type: "integer",
                nullable: true,
                comment: "نوع العقد - FullOwnership=1, SharedOwnership=2, etc. (deprecated for office survey)",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldComment: "نوع العقد - FullOwnership=1, SharedOwnership=2, LongTermRental=3, etc.");

            migrationBuilder.AddColumn<bool>(
                name: "HasEvidence",
                table: "PersonPropertyRelations",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "هل يوجد دليل؟ - Indicates if evidence documents are available/attached");

            migrationBuilder.AddColumn<string>(
                name: "OccupancyType",
                table: "PersonPropertyRelations",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                comment: "نوع الإشغال - Occupancy type (enum): OwnerOccupied=1, TenantOccupied=2, etc.");

            migrationBuilder.AlterColumn<string>(
                name: "HeadOfHouseholdName",
                table: "Households",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                comment: "رب الأسرة/العميل - Name of head of household (nullable for office survey)",
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldComment: "رب الأسرة/العميل - Name of head of household");

            migrationBuilder.AddColumn<string>(
                name: "OccupancyNature",
                table: "Households",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                comment: "طبيعة الإشغال - Occupancy nature (enum converted to string): LegalFormal, Informal, Customary, etc.");

            migrationBuilder.AddColumn<string>(
                name: "OccupancyType",
                table: "Households",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                comment: "نوع الإشغال - Occupancy type (enum converted to string): OwnerOccupied, TenantOccupied, etc.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasEvidence",
                table: "PersonPropertyRelations");

            migrationBuilder.DropColumn(
                name: "OccupancyType",
                table: "PersonPropertyRelations");

            migrationBuilder.DropColumn(
                name: "OccupancyNature",
                table: "Households");

            migrationBuilder.DropColumn(
                name: "OccupancyType",
                table: "Households");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDate",
                table: "PersonPropertyRelations",
                type: "timestamp with time zone",
                nullable: true,
                comment: "تاريخ بدء العلاقة - Start date of the relation",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "تاريخ بدء العلاقة - Start date of the relation (deprecated for office survey)");

            migrationBuilder.AlterColumn<string>(
                name: "RelationTypeOtherDesc",
                table: "PersonPropertyRelations",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                comment: "Description when relation type is 'Other'",
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true,
                oldComment: "Description when relation type is 'Other' (deprecated for office survey)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndDate",
                table: "PersonPropertyRelations",
                type: "timestamp with time zone",
                nullable: true,
                comment: "End date of the relation",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "End date of the relation (deprecated for office survey)");

            migrationBuilder.AlterColumn<string>(
                name: "ContractTypeOtherDesc",
                table: "PersonPropertyRelations",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                comment: "Description when contract type is 'Other'",
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true,
                oldComment: "Description when contract type is 'Other' (deprecated for office survey)");

            migrationBuilder.AlterColumn<int>(
                name: "ContractType",
                table: "PersonPropertyRelations",
                type: "integer",
                nullable: true,
                comment: "نوع العقد - FullOwnership=1, SharedOwnership=2, LongTermRental=3, etc.",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldComment: "نوع العقد - FullOwnership=1, SharedOwnership=2, etc. (deprecated for office survey)");

            migrationBuilder.AlterColumn<string>(
                name: "HeadOfHouseholdName",
                table: "Households",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                comment: "رب الأسرة/العميل - Name of head of household",
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true,
                oldComment: "رب الأسرة/العميل - Name of head of household (nullable for office survey)");
        }
    }
}
