using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNeighborhoodAndSpatialIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Neighborhoods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GovernorateCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    DistrictCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    SubDistrictCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    CommunityCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    NeighborhoodCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    FullCode = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    NameArabic = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NameEnglish = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CenterPoint = table.Column<Point>(type: "geometry(Point, 4326)", nullable: true),
                    CenterLatitude = table.Column<decimal>(type: "numeric(10,7)", precision: 10, scale: 7, nullable: false),
                    CenterLongitude = table.Column<decimal>(type: "numeric(10,7)", precision: 10, scale: 7, nullable: false),
                    BoundaryGeometry = table.Column<Geometry>(type: "geometry(Geometry, 4326)", nullable: true),
                    AreaSquareKm = table.Column<double>(type: "double precision", precision: 10, scale: 4, nullable: true),
                    ZoomLevel = table.Column<int>(type: "integer", nullable: false, defaultValue: 15),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_Neighborhoods", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Buildings_AdminHierarchy",
                table: "Buildings",
                columns: new[] { "GovernorateCode", "DistrictCode", "SubDistrictCode", "CommunityCode", "NeighborhoodCode" },
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Buildings_BuildingGeometry",
                table: "Buildings",
                column: "BuildingGeometry")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.CreateIndex(
                name: "IX_Neighborhoods_AdminHierarchy",
                table: "Neighborhoods",
                columns: new[] { "GovernorateCode", "DistrictCode", "SubDistrictCode", "CommunityCode" });

            migrationBuilder.CreateIndex(
                name: "IX_Neighborhoods_BoundaryGeometry",
                table: "Neighborhoods",
                column: "BoundaryGeometry")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.CreateIndex(
                name: "IX_Neighborhoods_FullCode",
                table: "Neighborhoods",
                column: "FullCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Neighborhoods");

            migrationBuilder.DropIndex(
                name: "IX_Buildings_AdminHierarchy",
                table: "Buildings");

            migrationBuilder.DropIndex(
                name: "IX_Buildings_BuildingGeometry",
                table: "Buildings");
        }
    }
}
