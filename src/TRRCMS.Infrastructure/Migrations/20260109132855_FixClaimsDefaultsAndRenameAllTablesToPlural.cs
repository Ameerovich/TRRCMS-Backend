using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixClaimsDefaultsAndRenameAllTablesToPlural : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ==================== PART 1: RENAME EVIDENCE TO EVIDENCES ====================

            migrationBuilder.DropForeignKey(
                name: "FK_Evidence_Claims_ClaimId",
                table: "Evidence");

            migrationBuilder.DropForeignKey(
                name: "FK_Evidence_PersonPropertyRelations_PersonPropertyRelationId",
                table: "Evidence");

            migrationBuilder.DropForeignKey(
                name: "FK_Evidence_Persons_PersonId",
                table: "Evidence");

            migrationBuilder.DropForeignKey(
                name: "FK_Evidence_PropertyUnits_PropertyUnitId",
                table: "Evidence");

            migrationBuilder.RenameTable(
                name: "Evidence",
                newName: "Evidences");

            migrationBuilder.RenameIndex(
                name: "IX_Evidence_PropertyUnitId",
                table: "Evidences",
                newName: "IX_Evidences_PropertyUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_Evidence_PersonPropertyRelationId",
                table: "Evidences",
                newName: "IX_Evidences_PersonPropertyRelationId");

            migrationBuilder.RenameIndex(
                name: "IX_Evidence_PersonId",
                table: "Evidences",
                newName: "IX_Evidences_PersonId");

            migrationBuilder.RenameIndex(
                name: "IX_Evidence_ClaimId",
                table: "Evidences",
                newName: "IX_Evidences_ClaimId");

            migrationBuilder.RenameIndex(
                name: "IX_Evidence_IsDeleted",
                table: "Evidences",
                newName: "IX_Evidences_IsDeleted");

            migrationBuilder.AddForeignKey(
                name: "FK_Evidences_Claims_ClaimId",
                table: "Evidences",
                column: "ClaimId",
                principalTable: "Claims",
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

            migrationBuilder.AddForeignKey(
                name: "FK_Evidences_PropertyUnits_PropertyUnitId",
                table: "Evidences",
                column: "PropertyUnitId",
                principalTable: "PropertyUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // ==================== PART 2: RENAME DOCUMENT TO DOCUMENTS ====================

            migrationBuilder.DropForeignKey(
                name: "FK_Document_Evidence_EvidenceId",
                table: "Document");

            migrationBuilder.DropForeignKey(
                name: "FK_Document_PersonPropertyRelations_PersonPropertyRelationId",
                table: "Document");

            migrationBuilder.DropForeignKey(
                name: "FK_Document_Persons_PersonId",
                table: "Document");

            migrationBuilder.RenameTable(
                name: "Document",
                newName: "Documents");

            migrationBuilder.RenameIndex(
                name: "IX_Document_PersonPropertyRelationId",
                table: "Documents",
                newName: "IX_Documents_PersonPropertyRelationId");

            migrationBuilder.RenameIndex(
                name: "IX_Document_PersonId",
                table: "Documents",
                newName: "IX_Documents_PersonId");

            migrationBuilder.RenameIndex(
                name: "IX_Document_EvidenceId",
                table: "Documents",
                newName: "IX_Documents_EvidenceId");

            // Update FK reference from Evidence to Evidences
            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Evidences_EvidenceId",
                table: "Documents",
                column: "EvidenceId",
                principalTable: "Evidences",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_PersonPropertyRelations_PersonPropertyRelationId",
                table: "Documents",
                column: "PersonPropertyRelationId",
                principalTable: "PersonPropertyRelations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Persons_PersonId",
                table: "Documents",
                column: "PersonId",
                principalTable: "Persons",
                principalColumn: "Id");

            // ==================== PART 3: RENAME REFERRAL TO REFERRALS ====================

            migrationBuilder.DropForeignKey(
                name: "FK_Referral_Claims_ClaimId",
                table: "Referral");

            migrationBuilder.DropForeignKey(
                name: "FK_Referral_Referral_PreviousReferralId",
                table: "Referral");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Referral",
                table: "Referral");

            migrationBuilder.RenameTable(
                name: "Referral",
                newName: "Referrals");

            migrationBuilder.RenameIndex(
                name: "IX_Referral_PreviousReferralId",
                table: "Referrals",
                newName: "IX_Referrals_PreviousReferralId");

            migrationBuilder.RenameIndex(
                name: "IX_Referral_ClaimId",
                table: "Referrals",
                newName: "IX_Referrals_ClaimId");

            migrationBuilder.AlterColumn<Guid>(
                name: "ToUserId",
                table: "Referrals",
                type: "uuid",
                nullable: true,
                comment: "Specific user assigned (optional)",
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ToRole",
                table: "Referrals",
                type: "integer",
                nullable: false,
                comment: "Role receiving the claim",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "ReferralNumber",
                table: "Referrals",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                comment: "Referral number - Format: REF-YYYY-NNNN",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<Guid>(
                name: "FromUserId",
                table: "Referrals",
                type: "uuid",
                nullable: false,
                comment: "User who initiated the referral",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "FromRole",
                table: "Referrals",
                type: "integer",
                nullable: false,
                comment: "Role referring the claim",
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<Guid>(
                name: "ClaimId",
                table: "Referrals",
                type: "uuid",
                nullable: false,
                comment: "Foreign key to Claim being referred",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Referrals",
                table: "Referrals",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Referrals_IsDeleted",
                table: "Referrals",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Referrals_ReferralNumber",
                table: "Referrals",
                column: "ReferralNumber",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Referrals_Claims_ClaimId",
                table: "Referrals",
                column: "ClaimId",
                principalTable: "Claims",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Referrals_Referrals_PreviousReferralId",
                table: "Referrals",
                column: "PreviousReferralId",
                principalTable: "Referrals",
                principalColumn: "Id");

            // ==================== PART 4: FIX CLAIMS TABLE DEFAULTS ====================

            // Add default values to Claims table columns
            migrationBuilder.Sql(@"
        ALTER TABLE ""Claims"" ALTER COLUMN ""HasConflicts"" SET DEFAULT false;
        ALTER TABLE ""Claims"" ALTER COLUMN ""ConflictCount"" SET DEFAULT 0;
        ALTER TABLE ""Claims"" ALTER COLUMN ""EvidenceCount"" SET DEFAULT 0;
        ALTER TABLE ""Claims"" ALTER COLUMN ""AllRequiredDocumentsSubmitted"" SET DEFAULT false;
        ALTER TABLE ""Claims"" ALTER COLUMN ""IsDeleted"" SET DEFAULT false;
    ");
        }
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ==================== PART 4 REVERSE: REMOVE CLAIMS DEFAULTS ====================

            // Remove default values from Claims table
            migrationBuilder.Sql(@"
        ALTER TABLE ""Claims"" ALTER COLUMN ""HasConflicts"" DROP DEFAULT;
        ALTER TABLE ""Claims"" ALTER COLUMN ""ConflictCount"" DROP DEFAULT;
        ALTER TABLE ""Claims"" ALTER COLUMN ""EvidenceCount"" DROP DEFAULT;
        ALTER TABLE ""Claims"" ALTER COLUMN ""AllRequiredDocumentsSubmitted"" DROP DEFAULT;
        ALTER TABLE ""Claims"" ALTER COLUMN ""IsDeleted"" DROP DEFAULT;
    ");

            // ==================== PART 3 REVERSE: RENAME REFERRALS BACK TO REFERRAL ====================

            migrationBuilder.DropForeignKey(
                name: "FK_Referrals_Claims_ClaimId",
                table: "Referrals");

            migrationBuilder.DropForeignKey(
                name: "FK_Referrals_Referrals_PreviousReferralId",
                table: "Referrals");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Referrals",
                table: "Referrals");

            migrationBuilder.DropIndex(
                name: "IX_Referrals_IsDeleted",
                table: "Referrals");

            migrationBuilder.DropIndex(
                name: "IX_Referrals_ReferralNumber",
                table: "Referrals");

            migrationBuilder.RenameTable(
                name: "Referrals",
                newName: "Referral");

            migrationBuilder.RenameIndex(
                name: "IX_Referrals_PreviousReferralId",
                table: "Referral",
                newName: "IX_Referral_PreviousReferralId");

            migrationBuilder.RenameIndex(
                name: "IX_Referrals_ClaimId",
                table: "Referral",
                newName: "IX_Referral_ClaimId");

            migrationBuilder.AlterColumn<Guid>(
                name: "ToUserId",
                table: "Referral",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true,
                oldComment: "Specific user assigned (optional)");

            migrationBuilder.AlterColumn<int>(
                name: "ToRole",
                table: "Referral",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Role receiving the claim");

            migrationBuilder.AlterColumn<string>(
                name: "ReferralNumber",
                table: "Referral",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldComment: "Referral number - Format: REF-YYYY-NNNN");

            migrationBuilder.AlterColumn<Guid>(
                name: "FromUserId",
                table: "Referral",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldComment: "User who initiated the referral");

            migrationBuilder.AlterColumn<int>(
                name: "FromRole",
                table: "Referral",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Role referring the claim");

            migrationBuilder.AlterColumn<Guid>(
                name: "ClaimId",
                table: "Referral",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldComment: "Foreign key to Claim being referred");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Referral",
                table: "Referral",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Referral_Claims_ClaimId",
                table: "Referral",
                column: "ClaimId",
                principalTable: "Claims",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Referral_Referral_PreviousReferralId",
                table: "Referral",
                column: "PreviousReferralId",
                principalTable: "Referral",
                principalColumn: "Id");

            // ==================== PART 2 REVERSE: RENAME DOCUMENTS BACK TO DOCUMENT ====================

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Evidences_EvidenceId",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_PersonPropertyRelations_PersonPropertyRelationId",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Persons_PersonId",
                table: "Documents");

            migrationBuilder.RenameTable(
                name: "Documents",
                newName: "Document");

            migrationBuilder.RenameIndex(
                name: "IX_Documents_PersonPropertyRelationId",
                table: "Document",
                newName: "IX_Document_PersonPropertyRelationId");

            migrationBuilder.RenameIndex(
                name: "IX_Documents_PersonId",
                table: "Document",
                newName: "IX_Document_PersonId");

            migrationBuilder.RenameIndex(
                name: "IX_Documents_EvidenceId",
                table: "Document",
                newName: "IX_Document_EvidenceId");

            // Restore FK reference to Evidence (singular)
            migrationBuilder.AddForeignKey(
                name: "FK_Document_Evidence_EvidenceId",
                table: "Document",
                column: "EvidenceId",
                principalTable: "Evidence",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Document_PersonPropertyRelations_PersonPropertyRelationId",
                table: "Document",
                column: "PersonPropertyRelationId",
                principalTable: "PersonPropertyRelations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Document_Persons_PersonId",
                table: "Document",
                column: "PersonId",
                principalTable: "Persons",
                principalColumn: "Id");

            // ==================== PART 1 REVERSE: RENAME EVIDENCES BACK TO EVIDENCE ====================

            migrationBuilder.DropForeignKey(
                name: "FK_Evidences_Claims_ClaimId",
                table: "Evidences");

            migrationBuilder.DropForeignKey(
                name: "FK_Evidences_PersonPropertyRelations_PersonPropertyRelationId",
                table: "Evidences");

            migrationBuilder.DropForeignKey(
                name: "FK_Evidences_Persons_PersonId",
                table: "Evidences");

            migrationBuilder.DropForeignKey(
                name: "FK_Evidences_PropertyUnits_PropertyUnitId",
                table: "Evidences");

            migrationBuilder.RenameTable(
                name: "Evidences",
                newName: "Evidence");

            migrationBuilder.RenameIndex(
                name: "IX_Evidences_PropertyUnitId",
                table: "Evidence",
                newName: "IX_Evidence_PropertyUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_Evidences_PersonPropertyRelationId",
                table: "Evidence",
                newName: "IX_Evidence_PersonPropertyRelationId");

            migrationBuilder.RenameIndex(
                name: "IX_Evidences_PersonId",
                table: "Evidence",
                newName: "IX_Evidence_PersonId");

            migrationBuilder.RenameIndex(
                name: "IX_Evidences_ClaimId",
                table: "Evidence",
                newName: "IX_Evidence_ClaimId");

            migrationBuilder.RenameIndex(
                name: "IX_Evidences_IsDeleted",
                table: "Evidence",
                newName: "IX_Evidence_IsDeleted");

            migrationBuilder.AddForeignKey(
                name: "FK_Evidence_Claims_ClaimId",
                table: "Evidence",
                column: "ClaimId",
                principalTable: "Claims",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

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

            migrationBuilder.AddForeignKey(
                name: "FK_Evidence_PropertyUnits_PropertyUnitId",
                table: "Evidence",
                column: "PropertyUnitId",
                principalTable: "PropertyUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
