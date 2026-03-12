using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DropUnusedPersonPropertyRelationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PersonPropertyRelation_ContractType",
                table: "PersonPropertyRelations");

            migrationBuilder.DropColumn(
                name: "ContractType",
                table: "StagingPersonPropertyRelations");

            migrationBuilder.DropColumn(
                name: "ContractTypeOtherDesc",
                table: "StagingPersonPropertyRelations");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "StagingPersonPropertyRelations");

            migrationBuilder.DropColumn(
                name: "RelationTypeOtherDesc",
                table: "StagingPersonPropertyRelations");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "StagingPersonPropertyRelations");

            migrationBuilder.DropColumn(
                name: "ContractType",
                table: "PersonPropertyRelations");

            migrationBuilder.DropColumn(
                name: "ContractTypeOtherDesc",
                table: "PersonPropertyRelations");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "PersonPropertyRelations");

            migrationBuilder.DropColumn(
                name: "RelationTypeOtherDesc",
                table: "PersonPropertyRelations");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "PersonPropertyRelations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ContractType",
                table: "StagingPersonPropertyRelations",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContractTypeOtherDesc",
                table: "StagingPersonPropertyRelations",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "StagingPersonPropertyRelations",
                type: "timestamp with time zone",
                nullable: true,
                comment: "End date of the relation/contract");

            migrationBuilder.AddColumn<string>(
                name: "RelationTypeOtherDesc",
                table: "StagingPersonPropertyRelations",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "StagingPersonPropertyRelations",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Start date of the relation/contract");

            migrationBuilder.AddColumn<int>(
                name: "ContractType",
                table: "PersonPropertyRelations",
                type: "integer",
                nullable: true,
                comment: "نوع العقد - FullOwnership=1, SharedOwnership=2, etc. (deprecated for office survey)");

            migrationBuilder.AddColumn<string>(
                name: "ContractTypeOtherDesc",
                table: "PersonPropertyRelations",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                comment: "Description when contract type is 'Other' (deprecated for office survey)");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "PersonPropertyRelations",
                type: "timestamp with time zone",
                nullable: true,
                comment: "End date of the relation (deprecated for office survey)");

            migrationBuilder.AddColumn<string>(
                name: "RelationTypeOtherDesc",
                table: "PersonPropertyRelations",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                comment: "Description when relation type is 'Other' (deprecated for office survey)");

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "PersonPropertyRelations",
                type: "timestamp with time zone",
                nullable: true,
                comment: "تاريخ بدء العلاقة - Start date of the relation (deprecated for office survey)");

            migrationBuilder.CreateIndex(
                name: "IX_PersonPropertyRelation_ContractType",
                table: "PersonPropertyRelations",
                column: "ContractType");
        }
    }
}
