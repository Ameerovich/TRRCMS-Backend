using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePersonPropertyRelationConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDate",
                table: "PersonPropertyRelations",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Start date of the relation",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                table: "PersonPropertyRelations",
                type: "bytea",
                rowVersion: true,
                nullable: true,
                comment: "Concurrency token",
                oldClrType: typeof(byte[]),
                oldType: "bytea",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RelationType",
                table: "PersonPropertyRelations",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                comment: "Type of relation (owner, tenant, occupant, guest, heir, other, etc.)",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<Guid>(
                name: "PropertyUnitId",
                table: "PersonPropertyRelations",
                type: "uuid",
                nullable: false,
                comment: "Foreign key to PropertyUnit",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "PersonId",
                table: "PersonPropertyRelations",
                type: "uuid",
                nullable: false,
                comment: "Foreign key to Person",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<decimal>(
                name: "OwnershipShare",
                table: "PersonPropertyRelations",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: true,
                comment: "Ownership or occupancy share (0.0 to 1.0 for percentage)",
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "PersonPropertyRelations",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                comment: "Additional notes about this relation",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "LastModifiedBy",
                table: "PersonPropertyRelations",
                type: "uuid",
                nullable: true,
                comment: "User who last modified this record",
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastModifiedAtUtc",
                table: "PersonPropertyRelations",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Last modification timestamp (UTC)",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "PersonPropertyRelations",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "Soft delete flag",
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "PersonPropertyRelations",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                comment: "Indicates if this relation is currently active",
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndDate",
                table: "PersonPropertyRelations",
                type: "timestamp with time zone",
                nullable: true,
                comment: "End date of the relation (for terminated relations)",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "DeletedBy",
                table: "PersonPropertyRelations",
                type: "uuid",
                nullable: true,
                comment: "User who deleted this record",
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAtUtc",
                table: "PersonPropertyRelations",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Deletion timestamp (UTC)",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedBy",
                table: "PersonPropertyRelations",
                type: "uuid",
                nullable: false,
                comment: "User who created this record",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "PersonPropertyRelations",
                type: "timestamp with time zone",
                nullable: false,
                comment: "Creation timestamp (UTC)",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "ContractDetails",
                table: "PersonPropertyRelations",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                comment: "Contract or agreement details",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RelationTypeOtherDesc",
                table: "PersonPropertyRelations",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                comment: "Description when relation type is 'Other'");

            migrationBuilder.CreateIndex(
                name: "IX_PersonPropertyRelation_IsActive_IsDeleted",
                table: "PersonPropertyRelations",
                columns: new[] { "IsActive", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_PersonPropertyRelation_RelationType",
                table: "PersonPropertyRelations",
                column: "RelationType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PersonPropertyRelation_IsActive_IsDeleted",
                table: "PersonPropertyRelations");

            migrationBuilder.DropIndex(
                name: "IX_PersonPropertyRelation_RelationType",
                table: "PersonPropertyRelations");

            migrationBuilder.DropColumn(
                name: "RelationTypeOtherDesc",
                table: "PersonPropertyRelations");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDate",
                table: "PersonPropertyRelations",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "Start date of the relation");

            migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                table: "PersonPropertyRelations",
                type: "bytea",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "bytea",
                oldRowVersion: true,
                oldNullable: true,
                oldComment: "Concurrency token");

            migrationBuilder.AlterColumn<string>(
                name: "RelationType",
                table: "PersonPropertyRelations",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldComment: "Type of relation (owner, tenant, occupant, guest, heir, other, etc.)");

            migrationBuilder.AlterColumn<Guid>(
                name: "PropertyUnitId",
                table: "PersonPropertyRelations",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldComment: "Foreign key to PropertyUnit");

            migrationBuilder.AlterColumn<Guid>(
                name: "PersonId",
                table: "PersonPropertyRelations",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldComment: "Foreign key to Person");

            migrationBuilder.AlterColumn<decimal>(
                name: "OwnershipShare",
                table: "PersonPropertyRelations",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldNullable: true,
                oldComment: "Ownership or occupancy share (0.0 to 1.0 for percentage)");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "PersonPropertyRelations",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true,
                oldComment: "Additional notes about this relation");

            migrationBuilder.AlterColumn<Guid>(
                name: "LastModifiedBy",
                table: "PersonPropertyRelations",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true,
                oldComment: "User who last modified this record");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastModifiedAtUtc",
                table: "PersonPropertyRelations",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "Last modification timestamp (UTC)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "PersonPropertyRelations",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false,
                oldComment: "Soft delete flag");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "PersonPropertyRelations",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true,
                oldComment: "Indicates if this relation is currently active");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndDate",
                table: "PersonPropertyRelations",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "End date of the relation (for terminated relations)");

            migrationBuilder.AlterColumn<Guid>(
                name: "DeletedBy",
                table: "PersonPropertyRelations",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true,
                oldComment: "User who deleted this record");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAtUtc",
                table: "PersonPropertyRelations",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "Deletion timestamp (UTC)");

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedBy",
                table: "PersonPropertyRelations",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldComment: "User who created this record");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "PersonPropertyRelations",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldComment: "Creation timestamp (UTC)");

            migrationBuilder.AlterColumn<string>(
                name: "ContractDetails",
                table: "PersonPropertyRelations",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true,
                oldComment: "Contract or agreement details");
        }
    }
}
