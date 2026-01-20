using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPropertyUnitSurveyFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedAreaSqm",
                table: "PropertyUnits",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasElectricity",
                table: "PropertyUnits",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasSewage",
                table: "PropertyUnits",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasWater",
                table: "PropertyUnits",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OccupancyStatus",
                table: "PropertyUnits",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PositionOnFloor",
                table: "PropertyUnits",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UtilitiesNotes",
                table: "PropertyUnits",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstimatedAreaSqm",
                table: "PropertyUnits");

            migrationBuilder.DropColumn(
                name: "HasElectricity",
                table: "PropertyUnits");

            migrationBuilder.DropColumn(
                name: "HasSewage",
                table: "PropertyUnits");

            migrationBuilder.DropColumn(
                name: "HasWater",
                table: "PropertyUnits");

            migrationBuilder.DropColumn(
                name: "OccupancyStatus",
                table: "PropertyUnits");

            migrationBuilder.DropColumn(
                name: "PositionOnFloor",
                table: "PropertyUnits");

            migrationBuilder.DropColumn(
                name: "UtilitiesNotes",
                table: "PropertyUnits");
        }
    }
}
