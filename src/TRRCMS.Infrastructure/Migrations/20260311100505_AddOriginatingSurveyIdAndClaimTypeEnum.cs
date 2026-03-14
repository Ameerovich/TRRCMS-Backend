using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOriginatingSurveyIdAndClaimTypeEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Convert ClaimType from varchar to integer (idempotent — skips if already integer)
            //    "Ownership Claim" / "Ownership" → 1 (OwnershipClaim)
            //    Everything else → 2 (OccupancyClaim)
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'Claims' AND column_name = 'ClaimType'
                          AND data_type <> 'integer'
                    ) THEN
                        ALTER TABLE ""Claims""
                            ALTER COLUMN ""ClaimType"" TYPE integer
                            USING CASE
                                WHEN ""ClaimType"" ILIKE '%Ownership%' THEN 1
                                ELSE 2
                            END;
                    END IF;
                END $$;
                COMMENT ON COLUMN ""Claims"".""ClaimType"" IS 'Claim type: 1=OwnershipClaim (مطالبة ملكية), 2=OccupancyClaim (مطالبة إشغال)';
            ");

            // 2. Add OriginatingSurveyId column
            migrationBuilder.AddColumn<Guid>(
                name: "OriginatingSurveyId",
                table: "Claims",
                type: "uuid",
                nullable: true,
                comment: "Foreign key to Survey that originated this claim (معرف الزيارة المنشئة)");

            // 3. Create index on OriginatingSurveyId
            migrationBuilder.CreateIndex(
                name: "IX_Claims_OriginatingSurveyId",
                table: "Claims",
                column: "OriginatingSurveyId");

            // 4. Add FK constraint
            migrationBuilder.AddForeignKey(
                name: "FK_Claims_Surveys_OriginatingSurveyId",
                table: "Claims",
                column: "OriginatingSurveyId",
                principalTable: "Surveys",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Claims_Surveys_OriginatingSurveyId",
                table: "Claims");

            migrationBuilder.DropIndex(
                name: "IX_Claims_OriginatingSurveyId",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "OriginatingSurveyId",
                table: "Claims");

            // Convert ClaimType back from integer to varchar
            migrationBuilder.Sql(@"
                ALTER TABLE ""Claims""
                    ALTER COLUMN ""ClaimType"" TYPE character varying(100)
                    USING CASE
                        WHEN ""ClaimType"" = 1 THEN 'Ownership Claim'
                        ELSE 'Occupancy Claim'
                    END;
                COMMENT ON COLUMN ""Claims"".""ClaimType"" IS 'Claim type from controlled vocabulary - e.g., Ownership Claim, Occupancy Claim (نوع المطالبة)';
            ");
        }
    }
}
