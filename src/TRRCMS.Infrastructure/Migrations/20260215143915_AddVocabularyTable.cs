using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVocabularyTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Vocabularies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VocabularyName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Vocabulary identifier (e.g., 'gender', 'relation_type')"),
                    DisplayNameArabic = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "Display name in Arabic"),
                    DisplayNameEnglish = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true, comment: "Display name in English"),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true, comment: "Description of this vocabulary"),
                    Version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, comment: "Semantic version: MAJOR.MINOR.PATCH"),
                    MajorVersion = table.Column<int>(type: "integer", nullable: false, comment: "Major version number"),
                    MinorVersion = table.Column<int>(type: "integer", nullable: false, comment: "Minor version number"),
                    PatchVersion = table.Column<int>(type: "integer", nullable: false, comment: "Patch version number"),
                    VersionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date when this version was created"),
                    IsCurrentVersion = table.Column<bool>(type: "boolean", nullable: false, comment: "Whether this is the active version"),
                    PreviousVersionId = table.Column<Guid>(type: "uuid", nullable: true, comment: "Reference to previous version"),
                    ValuesJson = table.Column<string>(type: "jsonb", nullable: false, comment: "Vocabulary values as JSON array"),
                    ValueCount = table.Column<int>(type: "integer", nullable: false, comment: "Number of values in this vocabulary"),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "Category grouping (e.g., Demographics, Property, Legal)"),
                    IsSystemVocabulary = table.Column<bool>(type: "boolean", nullable: false, comment: "System-defined vocabulary (cannot be deleted)"),
                    AllowCustomValues = table.Column<bool>(type: "boolean", nullable: false, comment: "Whether custom values can be added"),
                    IsMandatory = table.Column<bool>(type: "boolean", nullable: false, comment: "Whether this vocabulary is mandatory"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, comment: "Whether this vocabulary is active"),
                    MinimumCompatibleVersion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true, comment: "Minimum compatible version for imports"),
                    ChangeLog = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true, comment: "Changelog for this version"),
                    LastUsedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Date when vocabulary was last used"),
                    UsageCount = table.Column<int>(type: "integer", nullable: false, comment: "How many times this vocabulary has been used"),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: true),
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
                    table.PrimaryKey("PK_Vocabularies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vocabularies_Vocabularies_PreviousVersionId",
                        column: x => x.PreviousVersionId,
                        principalTable: "Vocabularies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Vocabularies_Category",
                table: "Vocabularies",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Vocabularies_Name_IsCurrent",
                table: "Vocabularies",
                columns: new[] { "VocabularyName", "IsCurrentVersion" });

            migrationBuilder.CreateIndex(
                name: "IX_Vocabularies_PreviousVersionId",
                table: "Vocabularies",
                column: "PreviousVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_Vocabularies_VocabularyName_Current",
                table: "Vocabularies",
                column: "VocabularyName",
                unique: true,
                filter: "\"IsCurrentVersion\" = true AND \"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Vocabularies");
        }
    }
}
