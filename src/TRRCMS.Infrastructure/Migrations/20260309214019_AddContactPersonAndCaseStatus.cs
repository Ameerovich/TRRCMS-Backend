using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContactPersonAndCaseStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Claims_Status",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "StagingClaims");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Claims");

            migrationBuilder.AddColumn<string>(
                name: "ContactPersonFullName",
                table: "Surveys",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                comment: "Denormalized contact person full name: firstname fathername familyname (mothername)");

            migrationBuilder.AddColumn<Guid>(
                name: "ContactPersonId",
                table: "Surveys",
                type: "uuid",
                nullable: true,
                comment: "FK to Person who is the contact person for this survey");

            migrationBuilder.AddColumn<Guid>(
                name: "OriginalContactPersonId",
                table: "StagingSurveys",
                type: "uuid",
                nullable: true,
                comment: "Original Contact Person UUID from .uhc — not a FK to production Persons");

            migrationBuilder.AddColumn<bool>(
                name: "IsContactPerson",
                table: "StagingPersons",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "CaseStatus",
                table: "StagingClaims",
                type: "integer",
                nullable: true,
                comment: "Optional — auto-set to Open during commit");

            migrationBuilder.AddColumn<int>(
                name: "CaseStatus",
                table: "Claims",
                type: "integer",
                nullable: false,
                defaultValue: 1,
                comment: "Case status: 1=Open (حالة مفتوحة), 2=Closed (حالة مغلقة)");

            migrationBuilder.CreateIndex(
                name: "IX_Surveys_ContactPersonId",
                table: "Surveys",
                column: "ContactPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_StagingSurveys_ImportPackageId_OriginalContactPersonId",
                table: "StagingSurveys",
                columns: new[] { "ImportPackageId", "OriginalContactPersonId" });

            migrationBuilder.CreateIndex(
                name: "IX_Claims_CaseStatus",
                table: "Claims",
                column: "CaseStatus");

            migrationBuilder.AddForeignKey(
                name: "FK_Surveys_Persons_ContactPersonId",
                table: "Surveys",
                column: "ContactPersonId",
                principalTable: "Persons",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Surveys_Persons_ContactPersonId",
                table: "Surveys");

            migrationBuilder.DropIndex(
                name: "IX_Surveys_ContactPersonId",
                table: "Surveys");

            migrationBuilder.DropIndex(
                name: "IX_StagingSurveys_ImportPackageId_OriginalContactPersonId",
                table: "StagingSurveys");

            migrationBuilder.DropIndex(
                name: "IX_Claims_CaseStatus",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "ContactPersonFullName",
                table: "Surveys");

            migrationBuilder.DropColumn(
                name: "ContactPersonId",
                table: "Surveys");

            migrationBuilder.DropColumn(
                name: "OriginalContactPersonId",
                table: "StagingSurveys");

            migrationBuilder.DropColumn(
                name: "IsContactPerson",
                table: "StagingPersons");

            migrationBuilder.DropColumn(
                name: "CaseStatus",
                table: "StagingClaims");

            migrationBuilder.DropColumn(
                name: "CaseStatus",
                table: "Claims");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "StagingClaims",
                type: "integer",
                nullable: true,
                comment: "Optional — auto-set to Draft during commit");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Claims",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Legacy status: 1=Draft, 2=Finalized, 3=UnderReview, 4=Approved, 5=Rejected, 6=PendingEvidence, 7=Disputed, 99=Archived (الحالة)");

            migrationBuilder.CreateIndex(
                name: "IX_Claims_Status",
                table: "Claims",
                column: "Status");
        }
    }
}
