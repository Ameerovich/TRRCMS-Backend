using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Buildings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BuildingId = table.Column<string>(type: "character varying(17)", maxLength: 17, nullable: false),
                    GovernorateCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    DistrictCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    SubDistrictCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    CommunityCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    NeighborhoodCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    BuildingNumber = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    GovernorateName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DistrictName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SubDistrictName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CommunityName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NeighborhoodName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BuildingType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DamageLevel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    NumberOfPropertyUnits = table.Column<int>(type: "integer", nullable: false),
                    NumberOfApartments = table.Column<int>(type: "integer", nullable: false),
                    NumberOfShops = table.Column<int>(type: "integer", nullable: false),
                    NumberOfFloors = table.Column<int>(type: "integer", nullable: true),
                    YearOfConstruction = table.Column<int>(type: "integer", nullable: true),
                    BuildingGeometryWkt = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    Latitude = table.Column<decimal>(type: "numeric(10,7)", precision: 10, scale: 7, nullable: true),
                    Longitude = table.Column<decimal>(type: "numeric(10,7)", precision: 10, scale: 7, nullable: true),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Landmark = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Buildings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Buildings_BuildingId",
                table: "Buildings",
                column: "BuildingId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Buildings");
        }
    }
}
