using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEvidenceRelationsManyToMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ──────────────────────────────────────────────────────────
            // PHASE 1: Create the new EvidenceRelations join table
            // ──────────────────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "EvidenceRelations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EvidenceId = table.Column<Guid>(type: "uuid", nullable: false, comment: "Foreign key to Evidence"),
                    PersonPropertyRelationId = table.Column<Guid>(type: "uuid", nullable: false, comment: "Foreign key to PersonPropertyRelation"),
                    LinkReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "Reason why evidence was linked to this relation"),
                    LinkedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "When the link was created (UTC)"),
                    LinkedBy = table.Column<Guid>(type: "uuid", nullable: false, comment: "User ID who created this link"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true, comment: "Whether this link is currently active"),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true, comment: "Concurrency token"),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Creation timestamp (UTC)"),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false, comment: "User who created this record"),
                    LastModifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Last modification timestamp (UTC)"),
                    LastModifiedBy = table.Column<Guid>(type: "uuid", nullable: true, comment: "User who last modified this record"),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false, comment: "Soft delete flag"),
                    DeletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Deletion timestamp (UTC)"),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true, comment: "User who deleted this record")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvidenceRelations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EvidenceRelations_Evidences_EvidenceId",
                        column: x => x.EvidenceId,
                        principalTable: "Evidences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EvidenceRelations_PersonPropertyRelations_PersonPropertyRel~",
                        column: x => x.PersonPropertyRelationId,
                        principalTable: "PersonPropertyRelations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EvidenceRelations_EvidenceId",
                table: "EvidenceRelations",
                column: "EvidenceId");

            migrationBuilder.CreateIndex(
                name: "IX_EvidenceRelations_EvidenceId_RelationId_IsActive_Unique",
                table: "EvidenceRelations",
                columns: new[] { "EvidenceId", "PersonPropertyRelationId", "IsActive" },
                unique: true,
                filter: "\"IsActive\" = true AND \"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_EvidenceRelations_IsActive_IsDeleted",
                table: "EvidenceRelations",
                columns: new[] { "IsActive", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_EvidenceRelations_LinkedBy",
                table: "EvidenceRelations",
                column: "LinkedBy");

            migrationBuilder.CreateIndex(
                name: "IX_EvidenceRelations_PersonPropertyRelationId",
                table: "EvidenceRelations",
                column: "PersonPropertyRelationId");

            // ──────────────────────────────────────────────────────────
            // PHASE 2: Migrate existing FK data to the join table
            //
            // For every Evidence row that has a non-null PersonPropertyRelationId,
            // insert a corresponding EvidenceRelation row.
            //
            // - Fresh installs: the column won't exist or will have 0 rows → no-op
            // - Existing DBs:   all links are preserved in the new table
            // ──────────────────────────────────────────────────────────
            migrationBuilder.Sql(@"
                INSERT INTO ""EvidenceRelations"" (
                    ""Id"",
                    ""EvidenceId"",
                    ""PersonPropertyRelationId"",
                    ""LinkReason"",
                    ""LinkedAtUtc"",
                    ""LinkedBy"",
                    ""IsActive"",
                    ""CreatedAtUtc"",
                    ""CreatedBy"",
                    ""IsDeleted""
                )
                SELECT
                    gen_random_uuid(),
                    e.""Id"",
                    e.""PersonPropertyRelationId"",
                    'Migrated from direct FK',
                    COALESCE(e.""CreatedAtUtc"", NOW()),
                    COALESCE(e.""CreatedBy"", '00000000-0000-0000-0000-000000000000'),
                    true,
                    COALESCE(e.""CreatedAtUtc"", NOW()),
                    COALESCE(e.""CreatedBy"", '00000000-0000-0000-0000-000000000000'),
                    false
                FROM ""Evidences"" e
                WHERE e.""PersonPropertyRelationId"" IS NOT NULL
                  AND e.""IsDeleted"" = false;
            ");

            // ──────────────────────────────────────────────────────────
            // PHASE 3: Drop the old FK column (data is now in join table)
            // ──────────────────────────────────────────────────────────
            migrationBuilder.DropForeignKey(
                name: "FK_Evidences_PersonPropertyRelations_PersonPropertyRelationId",
                table: "Evidences");

            migrationBuilder.DropIndex(
                name: "IX_Evidences_PersonPropertyRelationId",
                table: "Evidences");

            migrationBuilder.DropColumn(
                name: "PersonPropertyRelationId",
                table: "Evidences");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restore the old FK column
            migrationBuilder.AddColumn<Guid>(
                name: "PersonPropertyRelationId",
                table: "Evidences",
                type: "uuid",
                nullable: true,
                comment: "Foreign key to PersonPropertyRelation");

            // Migrate data back: pick the first active link per evidence
            migrationBuilder.Sql(@"
                UPDATE ""Evidences"" e
                SET ""PersonPropertyRelationId"" = er.""PersonPropertyRelationId""
                FROM (
                    SELECT DISTINCT ON (""EvidenceId"") ""EvidenceId"", ""PersonPropertyRelationId""
                    FROM ""EvidenceRelations""
                    WHERE ""IsActive"" = true AND ""IsDeleted"" = false
                    ORDER BY ""EvidenceId"", ""CreatedAtUtc"" ASC
                ) er
                WHERE e.""Id"" = er.""EvidenceId"";
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Evidences_PersonPropertyRelationId",
                table: "Evidences",
                column: "PersonPropertyRelationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Evidences_PersonPropertyRelations_PersonPropertyRelationId",
                table: "Evidences",
                column: "PersonPropertyRelationId",
                principalTable: "PersonPropertyRelations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // Drop the join table last
            migrationBuilder.DropTable(
                name: "EvidenceRelations");
        }
    }
}
