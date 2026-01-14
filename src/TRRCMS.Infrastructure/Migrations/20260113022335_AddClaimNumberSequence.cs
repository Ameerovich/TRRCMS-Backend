using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClaimNumberSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create PostgreSQL sequence for claim numbers
            migrationBuilder.Sql(@"
                CREATE SEQUENCE ""ClaimNumberSequence""
                START WITH 1
                INCREMENT BY 1
                NO MINVALUE
                NO MAXVALUE
                CACHE 1;
            ");

            // Add comment for documentation
            migrationBuilder.Sql(@"
                COMMENT ON SEQUENCE ""ClaimNumberSequence"" IS 
                'Sequential number generator for claim numbers. Used to generate CLM-YYYY-NNNNNNNNN format.';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP SEQUENCE IF EXISTS ""ClaimNumberSequence"";");
        }
    }
}