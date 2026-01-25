using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOfficeSurveyFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Surveys_Buildings_BuildingId",
                table: "Surveys");

            migrationBuilder.DropForeignKey(
                name: "FK_Surveys_PropertyUnits_PropertyUnitId",
                table: "Surveys");

            migrationBuilder.AlterColumn<string>(
                name: "SurveyType",
                table: "Surveys",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Field",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Surveys",
                type: "integer",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "ReferenceCode",
                table: "Surveys",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Surveys",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Surveys",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<string>(
                name: "IntervieweeRelationship",
                table: "Surveys",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "IntervieweeName",
                table: "Surveys",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "GpsCoordinates",
                table: "Surveys",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AppointmentReference",
                table: "Surveys",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ClaimCreatedDate",
                table: "Surveys",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ClaimId",
                table: "Surveys",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactEmail",
                table: "Surveys",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactPhone",
                table: "Surveys",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "InPersonVisit",
                table: "Surveys",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OfficeLocation",
                table: "Surveys",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegistrationNumber",
                table: "Surveys",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Source",
                table: "Surveys",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Surveys",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_Surveys_ClaimId",
                table: "Surveys",
                column: "ClaimId");

            migrationBuilder.CreateIndex(
                name: "IX_Surveys_FieldCollectorId",
                table: "Surveys",
                column: "FieldCollectorId");

            migrationBuilder.CreateIndex(
                name: "IX_Surveys_ReferenceCode",
                table: "Surveys",
                column: "ReferenceCode",
                unique: true);

            // FIXED: Changed from SQL Server syntax [RegistrationNumber] to PostgreSQL syntax "RegistrationNumber"
            migrationBuilder.CreateIndex(
                name: "IX_Surveys_RegistrationNumber",
                table: "Surveys",
                column: "RegistrationNumber",
                filter: "\"RegistrationNumber\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Surveys_Status",
                table: "Surveys",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Surveys_SurveyDate",
                table: "Surveys",
                column: "SurveyDate");

            migrationBuilder.CreateIndex(
                name: "IX_Surveys_Type",
                table: "Surveys",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Surveys_Type_Status_Collector",
                table: "Surveys",
                columns: new[] { "Type", "Status", "FieldCollectorId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Surveys_Buildings_BuildingId",
                table: "Surveys",
                column: "BuildingId",
                principalTable: "Buildings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Surveys_Claims_ClaimId",
                table: "Surveys",
                column: "ClaimId",
                principalTable: "Claims",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Surveys_PropertyUnits_PropertyUnitId",
                table: "Surveys",
                column: "PropertyUnitId",
                principalTable: "PropertyUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Surveys_Users_FieldCollectorId",
                table: "Surveys",
                column: "FieldCollectorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Surveys_Buildings_BuildingId",
                table: "Surveys");

            migrationBuilder.DropForeignKey(
                name: "FK_Surveys_Claims_ClaimId",
                table: "Surveys");

            migrationBuilder.DropForeignKey(
                name: "FK_Surveys_PropertyUnits_PropertyUnitId",
                table: "Surveys");

            migrationBuilder.DropForeignKey(
                name: "FK_Surveys_Users_FieldCollectorId",
                table: "Surveys");

            migrationBuilder.DropIndex(
                name: "IX_Surveys_ClaimId",
                table: "Surveys");

            migrationBuilder.DropIndex(
                name: "IX_Surveys_FieldCollectorId",
                table: "Surveys");

            migrationBuilder.DropIndex(
                name: "IX_Surveys_ReferenceCode",
                table: "Surveys");

            migrationBuilder.DropIndex(
                name: "IX_Surveys_RegistrationNumber",
                table: "Surveys");

            migrationBuilder.DropIndex(
                name: "IX_Surveys_Status",
                table: "Surveys");

            migrationBuilder.DropIndex(
                name: "IX_Surveys_SurveyDate",
                table: "Surveys");

            migrationBuilder.DropIndex(
                name: "IX_Surveys_Type",
                table: "Surveys");

            migrationBuilder.DropIndex(
                name: "IX_Surveys_Type_Status_Collector",
                table: "Surveys");

            migrationBuilder.DropColumn(
                name: "AppointmentReference",
                table: "Surveys");

            migrationBuilder.DropColumn(
                name: "ClaimCreatedDate",
                table: "Surveys");

            migrationBuilder.DropColumn(
                name: "ClaimId",
                table: "Surveys");

            migrationBuilder.DropColumn(
                name: "ContactEmail",
                table: "Surveys");

            migrationBuilder.DropColumn(
                name: "ContactPhone",
                table: "Surveys");

            migrationBuilder.DropColumn(
                name: "InPersonVisit",
                table: "Surveys");

            migrationBuilder.DropColumn(
                name: "OfficeLocation",
                table: "Surveys");

            migrationBuilder.DropColumn(
                name: "RegistrationNumber",
                table: "Surveys");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "Surveys");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Surveys");

            migrationBuilder.AlterColumn<string>(
                name: "SurveyType",
                table: "Surveys",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldDefaultValue: "Field");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Surveys",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<string>(
                name: "ReferenceCode",
                table: "Surveys",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Surveys",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Surveys",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "IntervieweeRelationship",
                table: "Surveys",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "IntervieweeName",
                table: "Surveys",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "GpsCoordinates",
                table: "Surveys",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Surveys_Buildings_BuildingId",
                table: "Surveys",
                column: "BuildingId",
                principalTable: "Buildings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Surveys_PropertyUnits_PropertyUnitId",
                table: "Surveys",
                column: "PropertyUnitId",
                principalTable: "PropertyUnits",
                principalColumn: "Id");
        }
    }
}