using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncStagingWithProductionEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Safe drop: columns may not exist on fresh databases
            migrationBuilder.Sql(@"
                ALTER TABLE ""StagingSurveys"" DROP COLUMN IF EXISTS ""IntervieweeName"";
                ALTER TABLE ""StagingSurveys"" DROP COLUMN IF EXISTS ""IntervieweeRelationship"";
            ");

            migrationBuilder.AddColumn<int>(
                name: "DurationMinutes",
                table: "StagingSurveys",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasEvidence",
                table: "StagingPersonPropertyRelations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "OccupancyType",
                table: "StagingPersonPropertyRelations",
                type: "integer",
                nullable: true,
                comment: "OccupancyType enum");

            migrationBuilder.AddColumn<int>(
                name: "OccupancyNature",
                table: "StagingHouseholds",
                type: "integer",
                nullable: true,
                comment: "OccupancyNature enum");

            migrationBuilder.AddColumn<int>(
                name: "OccupancyType",
                table: "StagingHouseholds",
                type: "integer",
                nullable: true,
                comment: "OccupancyType enum");

            // Convert ClaimType from varchar to integer.
            // Map existing text values: 'Ownership'/'OwnershipClaim' → 1, everything else → 2 (OccupancyClaim).
            // Then alter the column type using the converted values.
            migrationBuilder.Sql(@"
                ALTER TABLE ""StagingClaims""
                    ALTER COLUMN ""ClaimType"" TYPE integer
                    USING CASE
                        WHEN ""ClaimType"" ILIKE '%ownership%' THEN 1
                        ELSE 2
                    END;
            ");

            migrationBuilder.AddColumn<Guid>(
                name: "OriginalOriginatingSurveyId",
                table: "StagingClaims",
                type: "uuid",
                nullable: true,
                comment: "Original Survey UUID from .uhc — maps to production OriginatingSurveyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DurationMinutes",
                table: "StagingSurveys");

            migrationBuilder.DropColumn(
                name: "HasEvidence",
                table: "StagingPersonPropertyRelations");

            migrationBuilder.DropColumn(
                name: "OccupancyType",
                table: "StagingPersonPropertyRelations");

            migrationBuilder.DropColumn(
                name: "OccupancyNature",
                table: "StagingHouseholds");

            migrationBuilder.DropColumn(
                name: "OccupancyType",
                table: "StagingHouseholds");

            migrationBuilder.DropColumn(
                name: "OriginalOriginatingSurveyId",
                table: "StagingClaims");

            migrationBuilder.AddColumn<string>(
                name: "IntervieweeName",
                table: "StagingSurveys",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IntervieweeRelationship",
                table: "StagingSurveys",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ClaimType",
                table: "StagingClaims",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
