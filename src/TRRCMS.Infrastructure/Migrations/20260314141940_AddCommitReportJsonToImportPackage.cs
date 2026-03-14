using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCommitReportJsonToImportPackage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CommitReportJson",
                table: "ImportPackages",
                type: "text",
                nullable: true,
                comment: "JSON snapshot of the full commit report (entity breakdowns, idMappings, errors)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CommitReportJson",
                table: "ImportPackages");
        }
    }
}
