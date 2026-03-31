using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLandmarkTypeIcons : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LandmarkTypeIcons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    SvgContent = table.Column<string>(type: "text", nullable: false),
                    DisplayNameArabic = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DisplayNameEnglish = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LandmarkTypeIcons", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LandmarkTypeIcons_Type",
                table: "LandmarkTypeIcons",
                column: "Type",
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LandmarkTypeIcons");
        }
    }
}
