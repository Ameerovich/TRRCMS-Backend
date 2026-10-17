using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmptyPropertyToHouseholds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmptyPropertyJustification",
                table: "StagingHouseholds",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEmptyProperty",
                table: "StagingHouseholds",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "EmptyPropertyJustification",
                table: "Households",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                comment: "مبرر العقار الفارغ - Why the unit was found empty");

            migrationBuilder.AddColumn<bool>(
                name: "IsEmptyProperty",
                table: "Households",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "عقار فارغ - Unit found empty during the field survey (tablet only)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmptyPropertyJustification",
                table: "StagingHouseholds");

            migrationBuilder.DropColumn(
                name: "IsEmptyProperty",
                table: "StagingHouseholds");

            migrationBuilder.DropColumn(
                name: "EmptyPropertyJustification",
                table: "Households");

            migrationBuilder.DropColumn(
                name: "IsEmptyProperty",
                table: "Households");
        }
    }
}
