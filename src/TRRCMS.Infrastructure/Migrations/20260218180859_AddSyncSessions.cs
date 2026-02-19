using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSyncSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SyncSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FieldCollectorId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ServerIpAddress = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    SessionStatus = table.Column<int>(type: "integer", nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PackagesUploaded = table.Column<int>(type: "integer", nullable: false),
                    PackagesFailed = table.Column<int>(type: "integer", nullable: false),
                    AssignmentsDownloaded = table.Column<int>(type: "integer", nullable: false),
                    AssignmentsAcknowledged = table.Column<int>(type: "integer", nullable: false),
                    VocabularyVersionsSent = table.Column<string>(type: "jsonb", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("PK_SyncSessions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SyncSessions_DeviceId",
                table: "SyncSessions",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_SyncSessions_DeviceId_StartedAtUtc",
                table: "SyncSessions",
                columns: new[] { "DeviceId", "StartedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_SyncSessions_FieldCollectorId",
                table: "SyncSessions",
                column: "FieldCollectorId");

            migrationBuilder.CreateIndex(
                name: "IX_SyncSessions_FieldCollectorId_StartedAtUtc",
                table: "SyncSessions",
                columns: new[] { "FieldCollectorId", "StartedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_SyncSessions_SessionStatus",
                table: "SyncSessions",
                column: "SessionStatus");

            migrationBuilder.CreateIndex(
                name: "IX_SyncSessions_StartedAtUtc",
                table: "SyncSessions",
                column: "StartedAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SyncSessions");
        }
    }
}
