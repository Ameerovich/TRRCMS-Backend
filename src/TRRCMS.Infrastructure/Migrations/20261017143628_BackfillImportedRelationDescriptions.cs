using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <summary>
    /// V2 Bug 3: tablet packages carry the claim description in claims.claim_description, but the
    /// desktop claim form shows the relation's ContractDetails ("الوصف"), which import never filled.
    /// Import now copies it at staging; this backfills relations already imported with an empty
    /// description from the field-collected claim of the same claimant and unit.
    /// Data-only and idempotent; Down is a no-op (the copied text cannot be told apart from user edits).
    /// </summary>
    public partial class BackfillImportedRelationDescriptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE ""PersonPropertyRelations"" r
                SET ""ContractDetails"" = LEFT(c.""ClaimDescription"", 2000)
                FROM ""Claims"" c
                WHERE c.""PrimaryClaimantId"" = r.""PersonId""
                  AND c.""PropertyUnitId"" = r.""PropertyUnitId""
                  AND c.""ClaimSource"" = 1            -- FieldCollection (tablet import)
                  AND NOT c.""IsDeleted""
                  AND NOT r.""IsDeleted""
                  AND (r.""ContractDetails"" IS NULL OR btrim(r.""ContractDetails"") = '')
                  AND c.""ClaimDescription"" IS NOT NULL
                  AND btrim(c.""ClaimDescription"") <> '';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
