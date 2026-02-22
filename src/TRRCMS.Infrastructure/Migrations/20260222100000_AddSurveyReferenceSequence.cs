using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSurveyReferenceSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE SEQUENCE ""SurveyReferenceSequence""
                START WITH 1
                INCREMENT BY 1
                NO MINVALUE
                NO MAXVALUE
                CACHE 1;
            ");

            migrationBuilder.Sql(@"
                COMMENT ON SEQUENCE ""SurveyReferenceSequence"" IS
                'Sequential number generator for survey reference codes. Used to generate {PREFIX}-YYYY-NNNNN format (ALG/OFC).';
            ");

            // Seed the sequence to the current max so existing codes are not duplicated
            migrationBuilder.Sql(@"
                DO $$
                DECLARE
                    max_seq int;
                BEGIN
                    SELECT COALESCE(MAX(
                        CASE
                            WHEN split_part(""ReferenceCode"", '-', 3) ~ '^\d+$'
                            THEN split_part(""ReferenceCode"", '-', 3)::int
                            ELSE 0
                        END
                    ), 0) INTO max_seq
                    FROM ""Surveys""
                    WHERE ""ReferenceCode"" IS NOT NULL;

                    IF max_seq > 0 THEN
                        PERFORM setval('""SurveyReferenceSequence""', max_seq);
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP SEQUENCE IF EXISTS ""SurveyReferenceSequence"";");
        }
    }
}
