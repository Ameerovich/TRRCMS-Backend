using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStagingEvidenceRelationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StagingEvidenceRelations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalEvidenceId = table.Column<Guid>(type: "uuid", nullable: false, comment: "Original Evidence UUID from .uhc"),
                    OriginalPersonPropertyRelationId = table.Column<Guid>(type: "uuid", nullable: false, comment: "Original PersonPropertyRelation UUID from .uhc"),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    ImportPackageId = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    ValidationStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ValidationErrors = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    ValidationWarnings = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    IsApprovedForCommit = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CommittedEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    StagedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StagingEvidenceRelations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StagingEvidenceRelations_ImportPackages_ImportPackageId",
                        column: x => x.ImportPackageId,
                        principalTable: "ImportPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StagingEvidenceRelations_ImportPackageId",
                table: "StagingEvidenceRelations",
                column: "ImportPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_StagingEvidenceRelations_ImportPackageId_OriginalEntityId",
                table: "StagingEvidenceRelations",
                columns: new[] { "ImportPackageId", "OriginalEntityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StagingEvidenceRelations_ImportPackageId_OriginalEvidenceId",
                table: "StagingEvidenceRelations",
                columns: new[] { "ImportPackageId", "OriginalEvidenceId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StagingEvidenceRelations");
        }
    }
}
