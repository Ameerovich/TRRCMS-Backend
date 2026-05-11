using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPreviousStatusToImportPackage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "OwnershipShare",
                table: "PersonPropertyRelations",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: true,
                comment: "حصة الملكية - Ownership share (0 to 2400, qirat-based; 2400 = 100%)",
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldNullable: true,
                oldComment: "حصة الملكية - Ownership share (0.0 to 1.0)");

            migrationBuilder.AddColumn<int>(
                name: "PreviousStatus",
                table: "ImportPackages",
                type: "integer",
                nullable: true,
                comment: "Status before the last Cancel() call; null for pre-uncancel packages");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreviousStatus",
                table: "ImportPackages");

            migrationBuilder.AlterColumn<decimal>(
                name: "OwnershipShare",
                table: "PersonPropertyRelations",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: true,
                comment: "حصة الملكية - Ownership share (0.0 to 1.0)",
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldNullable: true,
                oldComment: "حصة الملكية - Ownership share (0 to 2400, qirat-based; 2400 = 100%)");
        }
    }
}
