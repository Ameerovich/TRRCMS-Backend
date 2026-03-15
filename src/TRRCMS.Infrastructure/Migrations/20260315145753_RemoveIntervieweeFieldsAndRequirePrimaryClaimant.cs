using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIntervieweeFieldsAndRequirePrimaryClaimant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IntervieweeName",
                table: "Surveys");

            migrationBuilder.DropColumn(
                name: "IntervieweeRelationship",
                table: "Surveys");

            // Soft-delete any existing claims without a primary claimant before making the column required
            migrationBuilder.Sql(
                """
                UPDATE "Claims"
                SET "IsDeleted" = true,
                    "DeletedAtUtc" = NOW(),
                    "LastModifiedAtUtc" = NOW()
                WHERE "PrimaryClaimantId" IS NULL AND "IsDeleted" = false;
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "PrimaryClaimantId",
                table: "Claims",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                comment: "Foreign key to Person - Primary claimant (معرف المدعي الأساسي)",
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true,
                oldComment: "Foreign key to Person - Primary claimant (معرف المدعي الأساسي)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IntervieweeName",
                table: "Surveys",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IntervieweeRelationship",
                table: "Surveys",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "PrimaryClaimantId",
                table: "Claims",
                type: "uuid",
                nullable: true,
                comment: "Foreign key to Person - Primary claimant (معرف المدعي الأساسي)",
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldComment: "Foreign key to Person - Primary claimant (معرف المدعي الأساسي)");
        }
    }
}
