using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixStagingOwnershipSharePrecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "OwnershipShare",
                table: "StagingPersonPropertyRelations",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: true,
                comment: "Ownership share out of 2400 (traditional inheritance denominator); matches production precision",
                oldClrType: typeof(decimal),
                oldType: "numeric(5,2)",
                oldPrecision: 5,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "OwnershipShare",
                table: "StagingClaims",
                type: "numeric",
                nullable: true,
                comment: "Ownership share out of 2400",
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true,
                oldComment: "Ownership percentage (0-100)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "OwnershipShare",
                table: "StagingPersonPropertyRelations",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldNullable: true,
                oldComment: "Ownership share out of 2400 (traditional inheritance denominator); matches production precision");

            migrationBuilder.AlterColumn<decimal>(
                name: "OwnershipShare",
                table: "StagingClaims",
                type: "numeric",
                nullable: true,
                comment: "Ownership percentage (0-100)",
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true,
                oldComment: "Ownership share out of 2400");
        }
    }
}
