using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UC012_BuildingAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                comment: "Indicates if field collector is available for new assignments");

            migrationBuilder.CreateTable(
                name: "BuildingAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BuildingId = table.Column<Guid>(type: "uuid", nullable: false),
                    FieldCollectorId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssignedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date when building was assigned"),
                    TargetCompletionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Target completion date for the assignment"),
                    ActualCompletionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Actual completion date"),
                    TransferStatus = table.Column<int>(type: "integer", nullable: false, comment: "Transfer status (1=Pending, 2=InProgress, 3=Transferred, 4=Failed, 5=Cancelled, 6=PartialTransfer, 7=Synchronized)"),
                    TransferredToTabletDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Date when data was transferred to tablet"),
                    SynchronizedFromTabletDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Date when data was synchronized back from tablet"),
                    UnitsForRevisit = table.Column<string>(type: "jsonb", nullable: true, comment: "JSON array of property unit IDs for revisit"),
                    RevisitReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true, comment: "Reason for revisit"),
                    IsRevisit = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false, comment: "Indicates if this is a revisit assignment"),
                    OriginalAssignmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Priority = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Normal", comment: "Assignment priority (Normal, High, Urgent)"),
                    AssignmentNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true, comment: "Assignment notes/instructions"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true, comment: "Indicates if assignment is currently active"),
                    TotalPropertyUnits = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Total property units in the building"),
                    CompletedPropertyUnits = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Number of property units surveyed"),
                    TransferErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true, comment: "Error message if transfer failed"),
                    TransferRetryCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Number of transfer retry attempts"),
                    LastTransferAttemptDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Last transfer attempt date"),
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
                    table.PrimaryKey("PK_BuildingAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BuildingAssignments_BuildingAssignments_OriginalAssignmentId",
                        column: x => x.OriginalAssignmentId,
                        principalTable: "BuildingAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BuildingAssignments_Buildings_BuildingId",
                        column: x => x.BuildingId,
                        principalTable: "Buildings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BuildingAssignments_AssignedDate",
                table: "BuildingAssignments",
                column: "AssignedDate");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingAssignments_Building_Active",
                table: "BuildingAssignments",
                columns: new[] { "BuildingId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_BuildingAssignments_BuildingId",
                table: "BuildingAssignments",
                column: "BuildingId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingAssignments_FieldCollector_Active",
                table: "BuildingAssignments",
                columns: new[] { "FieldCollectorId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_BuildingAssignments_FieldCollectorId",
                table: "BuildingAssignments",
                column: "FieldCollectorId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingAssignments_IsDeleted",
                table: "BuildingAssignments",
                column: "IsDeleted",
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingAssignments_OriginalAssignmentId",
                table: "BuildingAssignments",
                column: "OriginalAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingAssignments_TransferStatus_Active",
                table: "BuildingAssignments",
                columns: new[] { "TransferStatus", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BuildingAssignments");

            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "Users");
        }
    }
}
