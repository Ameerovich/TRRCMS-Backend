using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddKeepSeparateReconciliationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OriginalUnitIdentifier",
                table: "PropertyUnits",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                comment: "UnitIdentifier as received before commit-time suffix adjustment for a Keep-Separate decision");

            migrationBuilder.AddColumn<bool>(
                name: "UnitIdentifierAdjustedByKeepSeparate",
                table: "PropertyUnits",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "True if UnitIdentifier was suffix-disambiguated at import commit for a Keep-Separate decision (awaits reconciliation)");

            migrationBuilder.AddColumn<bool>(
                name: "NationalIdClearedByKeepSeparate",
                table: "Persons",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "True if NationalId was cleared at import commit for a Keep-Separate decision (awaits reconciliation)");

            migrationBuilder.AddColumn<string>(
                name: "PreservedNationalId",
                table: "Persons",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                comment: "Original NationalId removed at commit time for a Keep-Separate decision, preserved for reconciliation");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OriginalUnitIdentifier",
                table: "PropertyUnits");

            migrationBuilder.DropColumn(
                name: "UnitIdentifierAdjustedByKeepSeparate",
                table: "PropertyUnits");

            migrationBuilder.DropColumn(
                name: "NationalIdClearedByKeepSeparate",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "PreservedNationalId",
                table: "Persons");
        }
    }
}
