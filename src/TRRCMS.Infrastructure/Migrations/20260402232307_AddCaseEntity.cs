using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCaseEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create PostgreSQL sequence for Case number generation
            migrationBuilder.Sql("CREATE SEQUENCE IF NOT EXISTS \"CaseNumberSequence\" START WITH 1 INCREMENT BY 1;");

            migrationBuilder.AddColumn<Guid>(
                name: "CaseId",
                table: "Surveys",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CaseId",
                table: "PersonPropertyRelations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CaseId",
                table: "Claims",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Cases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CaseNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PropertyUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    OpenedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ClosedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClosedByClaimId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cases_Claims_ClosedByClaimId",
                        column: x => x.ClosedByClaimId,
                        principalTable: "Claims",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Cases_PropertyUnits_PropertyUnitId",
                        column: x => x.PropertyUnitId,
                        principalTable: "PropertyUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Surveys_CaseId",
                table: "Surveys",
                column: "CaseId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonPropertyRelations_CaseId",
                table: "PersonPropertyRelations",
                column: "CaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Claims_CaseId",
                table: "Claims",
                column: "CaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Cases_CaseNumber",
                table: "Cases",
                column: "CaseNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cases_ClosedByClaimId",
                table: "Cases",
                column: "ClosedByClaimId");

            migrationBuilder.CreateIndex(
                name: "IX_Cases_PropertyUnitId",
                table: "Cases",
                column: "PropertyUnitId",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Cases_Status",
                table: "Cases",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_Claims_Cases_CaseId",
                table: "Claims",
                column: "CaseId",
                principalTable: "Cases",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PersonPropertyRelations_Cases_CaseId",
                table: "PersonPropertyRelations",
                column: "CaseId",
                principalTable: "Cases",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Surveys_Cases_CaseId",
                table: "Surveys",
                column: "CaseId",
                principalTable: "Cases",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Claims_Cases_CaseId",
                table: "Claims");

            migrationBuilder.DropForeignKey(
                name: "FK_PersonPropertyRelations_Cases_CaseId",
                table: "PersonPropertyRelations");

            migrationBuilder.DropForeignKey(
                name: "FK_Surveys_Cases_CaseId",
                table: "Surveys");

            migrationBuilder.DropTable(
                name: "Cases");

            migrationBuilder.DropIndex(
                name: "IX_Surveys_CaseId",
                table: "Surveys");

            migrationBuilder.DropIndex(
                name: "IX_PersonPropertyRelations_CaseId",
                table: "PersonPropertyRelations");

            migrationBuilder.DropIndex(
                name: "IX_Claims_CaseId",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "CaseId",
                table: "Surveys");

            migrationBuilder.DropColumn(
                name: "CaseId",
                table: "PersonPropertyRelations");

            migrationBuilder.DropColumn(
                name: "CaseId",
                table: "Claims");

            migrationBuilder.Sql("DROP SEQUENCE IF EXISTS \"CaseNumberSequence\";");
        }
    }
}
