using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLogTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AuditLogNumber = table.Column<long>(type: "bigint", nullable: false, comment: "Sequential audit log entry number"),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "When the action occurred (UTC)"),
                    ActionType = table.Column<int>(type: "integer", nullable: false, comment: "Type of action performed (enum)"),
                    ActionDescription = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false, comment: "Human-readable description of the action"),
                    ActionResult = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "Result of the action (Success, Failed, Partial)"),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false, comment: "User who performed the action"),
                    Username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "Username at the time of action"),
                    UserRole = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "User's role at the time of action"),
                    UserFullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "Full name of user for historical record"),
                    EntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "Type of entity affected (e.g., Claim, Building)"),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: true, comment: "ID of the entity affected"),
                    EntityIdentifier = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "Human-readable identifier (e.g., Claim Number)"),
                    OldValues = table.Column<string>(type: "jsonb", nullable: true, comment: "Previous state stored as JSON"),
                    NewValues = table.Column<string>(type: "jsonb", nullable: true, comment: "New state stored as JSON"),
                    ChangedFields = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true, comment: "Comma-separated list of changed fields"),
                    IpAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "IP address from which action was performed"),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "User agent (browser/app information)"),
                    SourceApplication = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "Source application (Mobile, Desktop, API)"),
                    DeviceId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "Device ID for mobile/tablet actions"),
                    SessionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "Session ID"),
                    AdditionalData = table.Column<string>(type: "jsonb", nullable: true, comment: "Additional contextual information as JSON"),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true, comment: "Error message if action failed"),
                    StackTrace = table.Column<string>(type: "text", nullable: true, comment: "Stack trace if action failed"),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: true, comment: "Correlation ID to group related actions"),
                    ParentAuditLogId = table.Column<Guid>(type: "uuid", nullable: true, comment: "Parent audit log ID for nested actions"),
                    IsSecuritySensitive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false, comment: "Indicates if this is a security-sensitive action"),
                    RequiresLegalRetention = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false, comment: "Indicates if this action requires legal retention"),
                    RetentionEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Retention end date (10+ years for legal hold)"),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_AuditLogs_ParentAuditLogId",
                        column: x => x.ParentAuditLogId,
                        principalTable: "AuditLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_ActionResult_Timestamp",
                table: "AuditLogs",
                columns: new[] { "ActionResult", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_ActionType",
                table: "AuditLogs",
                column: "ActionType");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_AuditLogNumber",
                table: "AuditLogs",
                column: "AuditLogNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_CorrelationId",
                table: "AuditLogs",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityType_EntityId",
                table: "AuditLogs",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityType_EntityId_Timestamp",
                table: "AuditLogs",
                columns: new[] { "EntityType", "EntityId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_IsSecuritySensitive",
                table: "AuditLogs",
                column: "IsSecuritySensitive");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_IsSecuritySensitive_Timestamp",
                table: "AuditLogs",
                columns: new[] { "IsSecuritySensitive", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_ParentAuditLogId",
                table: "AuditLogs",
                column: "ParentAuditLogId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_RetentionEndDate",
                table: "AuditLogs",
                column: "RetentionEndDate");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Timestamp",
                table: "AuditLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId_Timestamp",
                table: "AuditLogs",
                columns: new[] { "UserId", "Timestamp" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");
        }
    }
}
