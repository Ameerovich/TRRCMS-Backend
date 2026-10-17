using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <summary>
    /// Client request: clearer dictionary names.
    ///   document_type: "نوع الوثيقة" (or the original seed default "نوع المستند") / Document Type
    ///       -> "نوع وثيقة إثبات الشخصية" / Personal Identification Document Type
    ///   evidence_type: "نوع الدليل" / Evidence Type
    ///       -> "نوع الوثيقة الداعمة للادعاء" / Claim Supporting Document Type
    /// The vocabulary seeder never overwrites display names, so existing databases are renamed here.
    /// Each name is only replaced while it still holds a previous seed default, keeping any admin customisation.
    /// All version rows are updated so history shows the same name.
    /// </summary>
    public partial class RenameDocumentAndEvidenceTypeVocabularies : Migration
    {
        // OldAr[0] is the latest previous default (restored by Down)
        private static readonly (string Vocabulary, string[] OldAr, string OldEn, string NewAr, string NewEn)[] Renames =
        {
            ("document_type", new[] { "نوع الوثيقة", "نوع المستند" }, "Document Type",
                "نوع وثيقة إثبات الشخصية", "Personal Identification Document Type"),
            ("evidence_type", new[] { "نوع الدليل" }, "Evidence Type",
                "نوع الوثيقة الداعمة للادعاء", "Claim Supporting Document Type"),
        };

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var r in Renames)
                Rename(migrationBuilder, r.Vocabulary, r.OldAr, r.OldEn, r.NewAr, r.NewEn);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var r in Renames)
                Rename(migrationBuilder, r.Vocabulary, new[] { r.NewAr }, r.NewEn, r.OldAr[0], r.OldEn);
        }

        private static void Rename(MigrationBuilder mb, string vocabulary,
            string[] fromAr, string fromEn, string toAr, string toEn)
        {
            var fromArList = string.Join(", ", fromAr.Select(n => $"'{n}'"));
            mb.Sql($@"
                UPDATE ""Vocabularies"" SET ""DisplayNameArabic"" = '{toAr}'
                WHERE ""VocabularyName"" = '{vocabulary}' AND ""DisplayNameArabic"" IN ({fromArList});
                UPDATE ""Vocabularies"" SET ""DisplayNameEnglish"" = '{toEn}'
                WHERE ""VocabularyName"" = '{vocabulary}' AND ""DisplayNameEnglish"" = '{fromEn}';");
        }
    }
}
