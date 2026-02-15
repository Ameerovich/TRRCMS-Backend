using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSurveyIdToPersonPropertyRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SurveyId",
                table: "PersonPropertyRelations",
                type: "uuid",
                nullable: true,
                comment: "Foreign key to the originating Survey — scopes claim creation to this survey's relations only");

            migrationBuilder.CreateIndex(
                name: "IX_PersonPropertyRelation_SurveyId",
                table: "PersonPropertyRelations",
                column: "SurveyId");

            migrationBuilder.AddForeignKey(
                name: "FK_PersonPropertyRelations_Surveys_SurveyId",
                table: "PersonPropertyRelations",
                column: "SurveyId",
                principalTable: "Surveys",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PersonPropertyRelations_Surveys_SurveyId",
                table: "PersonPropertyRelations");

            migrationBuilder.DropIndex(
                name: "IX_PersonPropertyRelation_SurveyId",
                table: "PersonPropertyRelations");

            migrationBuilder.DropColumn(
                name: "SurveyId",
                table: "PersonPropertyRelations");
        }
    }
}
