using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDeadSurveyFieldsAndEnumValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExportPackageId",
                table: "Surveys");

            migrationBuilder.DropColumn(
                name: "ExportedDate",
                table: "Surveys");

            migrationBuilder.DropColumn(
                name: "ImportedDate",
                table: "Surveys");

            migrationBuilder.DropColumn(
                name: "SurveyType",
                table: "Surveys");

            migrationBuilder.DropColumn(
                name: "SurveyTypeName",
                table: "StagingSurveys");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ExportPackageId",
                table: "Surveys",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExportedDate",
                table: "Surveys",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ImportedDate",
                table: "Surveys",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SurveyType",
                table: "Surveys",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Field");

            migrationBuilder.AddColumn<string>(
                name: "SurveyTypeName",
                table: "StagingSurveys",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                comment: "Optional — auto-set during commit");
        }
    }
}
