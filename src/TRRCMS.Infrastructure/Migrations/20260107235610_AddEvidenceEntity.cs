using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEvidenceEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Document_Evidence_EvidenceId",
                table: "Document");

            migrationBuilder.DropForeignKey(
                name: "FK_Evidence_Claims_ClaimId",
                table: "Evidence");

            migrationBuilder.DropForeignKey(
                name: "FK_Evidence_Evidence_PreviousVersionId",
                table: "Evidence");

            migrationBuilder.DropForeignKey(
                name: "FK_Evidence_PersonPropertyRelations_PersonPropertyRelationId",
                table: "Evidence");

            migrationBuilder.DropForeignKey(
                name: "FK_Evidence_Persons_PersonId",
                table: "Evidence");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Evidence",
                table: "Evidence");

            migrationBuilder.RenameTable(
                name: "Evidence",
                newName: "Evidences");

            migrationBuilder.RenameIndex(
                name: "IX_Evidence_PreviousVersionId",
                table: "Evidences",
                newName: "IX_Evidences_PreviousVersionId");

            migrationBuilder.AlterColumn<int>(
                name: "VersionNumber",
                table: "Evidences",
                type: "integer",
                nullable: false,
                defaultValue: 1,
                comment: "Version number for document versioning",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                table: "Evidences",
                type: "bytea",
                rowVersion: true,
                nullable: true,
                comment: "Concurrency token",
                oldClrType: typeof(byte[]),
                oldType: "bytea",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "PreviousVersionId",
                table: "Evidences",
                type: "uuid",
                nullable: true,
                comment: "Reference to previous version (if this is an updated version)",
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "PersonPropertyRelationId",
                table: "Evidences",
                type: "uuid",
                nullable: true,
                comment: "Foreign key to PersonPropertyRelation (if evidence supports a relation)",
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "PersonId",
                table: "Evidences",
                type: "uuid",
                nullable: true,
                comment: "Foreign key to Person (if evidence is linked to a person)",
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OriginalFileName",
                table: "Evidences",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                comment: "Original filename as uploaded",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Evidences",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                comment: "Additional notes about this evidence",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MimeType",
                table: "Evidences",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                comment: "MIME type (e.g., image/jpeg, application/pdf)",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<Guid>(
                name: "LastModifiedBy",
                table: "Evidences",
                type: "uuid",
                nullable: true,
                comment: "User who last modified this record",
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastModifiedAtUtc",
                table: "Evidences",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Last modification timestamp (UTC)",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "IssuingAuthority",
                table: "Evidences",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                comment: "Issuing authority or organization",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Evidences",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "Soft delete flag",
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "IsCurrentVersion",
                table: "Evidences",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                comment: "Indicates if this is the current/latest version",
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<long>(
                name: "FileSizeBytes",
                table: "Evidences",
                type: "bigint",
                nullable: false,
                comment: "File size in bytes",
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<string>(
                name: "FilePath",
                table: "Evidences",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                comment: "File path in storage system",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "FileHash",
                table: "Evidences",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true,
                comment: "SHA-256 hash of the file for integrity verification",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EvidenceType",
                table: "Evidences",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                comment: "Evidence type (controlled vocabulary)",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "DocumentReferenceNumber",
                table: "Evidences",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                comment: "Document reference number (if any)",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DocumentIssuedDate",
                table: "Evidences",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Date when the document was issued (if applicable)",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DocumentExpiryDate",
                table: "Evidences",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Date when the document expires (if applicable)",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Evidences",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                comment: "Document or evidence description",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<Guid>(
                name: "DeletedBy",
                table: "Evidences",
                type: "uuid",
                nullable: true,
                comment: "User who deleted this record",
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAtUtc",
                table: "Evidences",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Deletion timestamp (UTC)",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedBy",
                table: "Evidences",
                type: "uuid",
                nullable: false,
                comment: "User who created this record",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "Evidences",
                type: "timestamp with time zone",
                nullable: false,
                comment: "Creation timestamp (UTC)",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<Guid>(
                name: "ClaimId",
                table: "Evidences",
                type: "uuid",
                nullable: true,
                comment: "Foreign key to Claim (if evidence supports a claim)",
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Evidences",
                table: "Evidences",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Evidence_DocumentExpiryDate",
                table: "Evidences",
                column: "DocumentExpiryDate");

            migrationBuilder.CreateIndex(
                name: "IX_Evidence_EvidenceType",
                table: "Evidences",
                column: "EvidenceType");

            migrationBuilder.CreateIndex(
                name: "IX_Evidence_IsCurrentVersion",
                table: "Evidences",
                column: "IsCurrentVersion");

            migrationBuilder.CreateIndex(
                name: "IX_Evidence_IsCurrentVersion_IsDeleted",
                table: "Evidences",
                columns: new[] { "IsCurrentVersion", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Evidence_IsDeleted",
                table: "Evidences",
                column: "IsDeleted");

            migrationBuilder.AddForeignKey(
                name: "FK_Document_Evidences_EvidenceId",
                table: "Document",
                column: "EvidenceId",
                principalTable: "Evidences",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Evidences_Claims_ClaimId",
                table: "Evidences",
                column: "ClaimId",
                principalTable: "Claims",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Evidences_Evidences_PreviousVersionId",
                table: "Evidences",
                column: "PreviousVersionId",
                principalTable: "Evidences",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Evidences_PersonPropertyRelations_PersonPropertyRelationId",
                table: "Evidences",
                column: "PersonPropertyRelationId",
                principalTable: "PersonPropertyRelations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Evidences_Persons_PersonId",
                table: "Evidences",
                column: "PersonId",
                principalTable: "Persons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Document_Evidences_EvidenceId",
                table: "Document");

            migrationBuilder.DropForeignKey(
                name: "FK_Evidences_Claims_ClaimId",
                table: "Evidences");

            migrationBuilder.DropForeignKey(
                name: "FK_Evidences_Evidences_PreviousVersionId",
                table: "Evidences");

            migrationBuilder.DropForeignKey(
                name: "FK_Evidences_PersonPropertyRelations_PersonPropertyRelationId",
                table: "Evidences");

            migrationBuilder.DropForeignKey(
                name: "FK_Evidences_Persons_PersonId",
                table: "Evidences");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Evidences",
                table: "Evidences");

            migrationBuilder.DropIndex(
                name: "IX_Evidence_DocumentExpiryDate",
                table: "Evidences");

            migrationBuilder.DropIndex(
                name: "IX_Evidence_EvidenceType",
                table: "Evidences");

            migrationBuilder.DropIndex(
                name: "IX_Evidence_IsCurrentVersion",
                table: "Evidences");

            migrationBuilder.DropIndex(
                name: "IX_Evidence_IsCurrentVersion_IsDeleted",
                table: "Evidences");

            migrationBuilder.DropIndex(
                name: "IX_Evidence_IsDeleted",
                table: "Evidences");

            migrationBuilder.RenameTable(
                name: "Evidences",
                newName: "Evidence");

            migrationBuilder.RenameIndex(
                name: "IX_Evidences_PreviousVersionId",
                table: "Evidence",
                newName: "IX_Evidence_PreviousVersionId");

            migrationBuilder.AlterColumn<int>(
                name: "VersionNumber",
                table: "Evidence",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 1,
                oldComment: "Version number for document versioning");

            migrationBuilder.AlterColumn<byte[]>(
                name: "RowVersion",
                table: "Evidence",
                type: "bytea",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "bytea",
                oldRowVersion: true,
                oldNullable: true,
                oldComment: "Concurrency token");

            migrationBuilder.AlterColumn<Guid>(
                name: "PreviousVersionId",
                table: "Evidence",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true,
                oldComment: "Reference to previous version (if this is an updated version)");

            migrationBuilder.AlterColumn<Guid>(
                name: "PersonPropertyRelationId",
                table: "Evidence",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true,
                oldComment: "Foreign key to PersonPropertyRelation (if evidence supports a relation)");

            migrationBuilder.AlterColumn<Guid>(
                name: "PersonId",
                table: "Evidence",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true,
                oldComment: "Foreign key to Person (if evidence is linked to a person)");

            migrationBuilder.AlterColumn<string>(
                name: "OriginalFileName",
                table: "Evidence",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldComment: "Original filename as uploaded");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Evidence",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true,
                oldComment: "Additional notes about this evidence");

            migrationBuilder.AlterColumn<string>(
                name: "MimeType",
                table: "Evidence",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldComment: "MIME type (e.g., image/jpeg, application/pdf)");

            migrationBuilder.AlterColumn<Guid>(
                name: "LastModifiedBy",
                table: "Evidence",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true,
                oldComment: "User who last modified this record");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastModifiedAtUtc",
                table: "Evidence",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "Last modification timestamp (UTC)");

            migrationBuilder.AlterColumn<string>(
                name: "IssuingAuthority",
                table: "Evidence",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true,
                oldComment: "Issuing authority or organization");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "Evidence",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false,
                oldComment: "Soft delete flag");

            migrationBuilder.AlterColumn<bool>(
                name: "IsCurrentVersion",
                table: "Evidence",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true,
                oldComment: "Indicates if this is the current/latest version");

            migrationBuilder.AlterColumn<long>(
                name: "FileSizeBytes",
                table: "Evidence",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldComment: "File size in bytes");

            migrationBuilder.AlterColumn<string>(
                name: "FilePath",
                table: "Evidence",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldComment: "File path in storage system");

            migrationBuilder.AlterColumn<string>(
                name: "FileHash",
                table: "Evidence",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64,
                oldNullable: true,
                oldComment: "SHA-256 hash of the file for integrity verification");

            migrationBuilder.AlterColumn<string>(
                name: "EvidenceType",
                table: "Evidence",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldComment: "Evidence type (controlled vocabulary)");

            migrationBuilder.AlterColumn<string>(
                name: "DocumentReferenceNumber",
                table: "Evidence",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true,
                oldComment: "Document reference number (if any)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DocumentIssuedDate",
                table: "Evidence",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "Date when the document was issued (if applicable)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DocumentExpiryDate",
                table: "Evidence",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "Date when the document expires (if applicable)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Evidence",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldComment: "Document or evidence description");

            migrationBuilder.AlterColumn<Guid>(
                name: "DeletedBy",
                table: "Evidence",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true,
                oldComment: "User who deleted this record");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeletedAtUtc",
                table: "Evidence",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "Deletion timestamp (UTC)");

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedBy",
                table: "Evidence",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldComment: "User who created this record");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "Evidence",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldComment: "Creation timestamp (UTC)");

            migrationBuilder.AlterColumn<Guid>(
                name: "ClaimId",
                table: "Evidence",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true,
                oldComment: "Foreign key to Claim (if evidence supports a claim)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Evidence",
                table: "Evidence",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Document_Evidence_EvidenceId",
                table: "Document",
                column: "EvidenceId",
                principalTable: "Evidence",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Evidence_Claims_ClaimId",
                table: "Evidence",
                column: "ClaimId",
                principalTable: "Claims",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Evidence_Evidence_PreviousVersionId",
                table: "Evidence",
                column: "PreviousVersionId",
                principalTable: "Evidence",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Evidence_PersonPropertyRelations_PersonPropertyRelationId",
                table: "Evidence",
                column: "PersonPropertyRelationId",
                principalTable: "PersonPropertyRelations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Evidence_Persons_PersonId",
                table: "Evidence",
                column: "PersonId",
                principalTable: "Persons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
