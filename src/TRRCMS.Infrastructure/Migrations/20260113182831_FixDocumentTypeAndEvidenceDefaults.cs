using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixDocumentTypeAndEvidenceDefaults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "VerificationStatus",
                table: "Documents",
                type: "integer",
                maxLength: 50,
                nullable: false,
                defaultValue: 1,
                comment: "Verification status (Pending, Verified, Rejected, RequiresAdditionalInfo)",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldDefaultValue: "Pending",
                oldComment: "Verification status (Pending, Verified, Rejected, RequiresAdditionalInfo)");

            migrationBuilder.AlterColumn<int>(
                name: "DocumentType",
                table: "Documents",
                type: "integer",
                maxLength: 100,
                nullable: false,
                comment: "Document type from controlled vocabulary (e.g., TabuGreen, RentalContract, NationalIdCard)",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldComment: "Document type from controlled vocabulary (e.g., TabuGreen, RentalContract, NationalIdCard)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "VerificationStatus",
                table: "Documents",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Pending",
                comment: "Verification status (Pending, Verified, Rejected, RequiresAdditionalInfo)",
                oldClrType: typeof(int),
                oldType: "integer",
                oldMaxLength: 50,
                oldDefaultValue: 1,
                oldComment: "Verification status (Pending, Verified, Rejected, RequiresAdditionalInfo)");

            migrationBuilder.AlterColumn<string>(
                name: "DocumentType",
                table: "Documents",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                comment: "Document type from controlled vocabulary (e.g., TabuGreen, RentalContract, NationalIdCard)",
                oldClrType: typeof(int),
                oldType: "integer",
                oldMaxLength: 100,
                oldComment: "Document type from controlled vocabulary (e.g., TabuGreen, RentalContract, NationalIdCard)");
        }
    }
}
