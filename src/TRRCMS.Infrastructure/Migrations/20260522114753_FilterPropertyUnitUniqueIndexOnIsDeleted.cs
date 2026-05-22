using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FilterPropertyUnitUniqueIndexOnIsDeleted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PropertyUnits_BuildingId_UnitIdentifier",
                table: "PropertyUnits");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyUnits_BuildingId_UnitIdentifier",
                table: "PropertyUnits",
                columns: new[] { "BuildingId", "UnitIdentifier" },
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PropertyUnits_BuildingId_UnitIdentifier",
                table: "PropertyUnits");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyUnits_BuildingId_UnitIdentifier",
                table: "PropertyUnits",
                columns: new[] { "BuildingId", "UnitIdentifier" },
                unique: true);
        }
    }
}
