using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DropUnusedBuildingAndPropertyUnitFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DamageLevel",
                table: "StagingPropertyUnits");

            migrationBuilder.DropColumn(
                name: "EstimatedAreaSqm",
                table: "StagingPropertyUnits");

            migrationBuilder.DropColumn(
                name: "OccupancyNature",
                table: "StagingPropertyUnits");

            migrationBuilder.DropColumn(
                name: "OccupancyStatus",
                table: "StagingPropertyUnits");

            migrationBuilder.DropColumn(
                name: "OccupancyType",
                table: "StagingPropertyUnits");

            migrationBuilder.DropColumn(
                name: "PositionOnFloor",
                table: "StagingPropertyUnits");

            migrationBuilder.DropColumn(
                name: "SpecialFeatures",
                table: "StagingPropertyUnits");

            migrationBuilder.DropColumn(
                name: "UtilitiesNotes",
                table: "StagingPropertyUnits");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "StagingBuildings");

            migrationBuilder.DropColumn(
                name: "DamageLevel",
                table: "StagingBuildings");

            migrationBuilder.DropColumn(
                name: "Landmark",
                table: "StagingBuildings");

            migrationBuilder.DropColumn(
                name: "NumberOfFloors",
                table: "StagingBuildings");

            migrationBuilder.DropColumn(
                name: "YearOfConstruction",
                table: "StagingBuildings");

            migrationBuilder.DropColumn(
                name: "DamageLevel",
                table: "PropertyUnits");

            migrationBuilder.DropColumn(
                name: "EstimatedAreaSqm",
                table: "PropertyUnits");

            migrationBuilder.DropColumn(
                name: "HasBalcony",
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
                name: "NumberOfBathrooms",
                table: "PropertyUnits");

            migrationBuilder.DropColumn(
                name: "NumberOfHouseholds",
                table: "PropertyUnits");

            migrationBuilder.DropColumn(
                name: "OccupancyNature",
                table: "PropertyUnits");

            migrationBuilder.DropColumn(
                name: "OccupancyStatus",
                table: "PropertyUnits");

            migrationBuilder.DropColumn(
                name: "OccupancyType",
                table: "PropertyUnits");

            migrationBuilder.DropColumn(
                name: "PositionOnFloor",
                table: "PropertyUnits");

            migrationBuilder.DropColumn(
                name: "SpecialFeatures",
                table: "PropertyUnits");

            migrationBuilder.DropColumn(
                name: "TotalOccupants",
                table: "PropertyUnits");

            migrationBuilder.DropColumn(
                name: "UtilitiesNotes",
                table: "PropertyUnits");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Buildings");

            migrationBuilder.DropColumn(
                name: "DamageLevel",
                table: "Buildings");

            migrationBuilder.DropColumn(
                name: "Landmark",
                table: "Buildings");

            migrationBuilder.DropColumn(
                name: "NumberOfFloors",
                table: "Buildings");

            migrationBuilder.DropColumn(
                name: "YearOfConstruction",
                table: "Buildings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DamageLevel",
                table: "StagingPropertyUnits",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedAreaSqm",
                table: "StagingPropertyUnits",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OccupancyNature",
                table: "StagingPropertyUnits",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OccupancyStatus",
                table: "StagingPropertyUnits",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OccupancyType",
                table: "StagingPropertyUnits",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PositionOnFloor",
                table: "StagingPropertyUnits",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpecialFeatures",
                table: "StagingPropertyUnits",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UtilitiesNotes",
                table: "StagingPropertyUnits",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "StagingBuildings",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DamageLevel",
                table: "StagingBuildings",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Landmark",
                table: "StagingBuildings",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfFloors",
                table: "StagingBuildings",
                type: "integer",
                nullable: true,
                comment: "Future expansion — not in current mobile package");

            migrationBuilder.AddColumn<int>(
                name: "YearOfConstruction",
                table: "StagingBuildings",
                type: "integer",
                nullable: true,
                comment: "Future expansion — not in current mobile package");

            migrationBuilder.AddColumn<int>(
                name: "DamageLevel",
                table: "PropertyUnits",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedAreaSqm",
                table: "PropertyUnits",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasBalcony",
                table: "PropertyUnits",
                type: "boolean",
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

            migrationBuilder.AddColumn<int>(
                name: "NumberOfBathrooms",
                table: "PropertyUnits",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfHouseholds",
                table: "PropertyUnits",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OccupancyNature",
                table: "PropertyUnits",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OccupancyStatus",
                table: "PropertyUnits",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OccupancyType",
                table: "PropertyUnits",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PositionOnFloor",
                table: "PropertyUnits",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpecialFeatures",
                table: "PropertyUnits",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalOccupants",
                table: "PropertyUnits",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UtilitiesNotes",
                table: "PropertyUnits",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Buildings",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DamageLevel",
                table: "Buildings",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Landmark",
                table: "Buildings",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfFloors",
                table: "Buildings",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "YearOfConstruction",
                table: "Buildings",
                type: "integer",
                nullable: true);
        }
    }
}
