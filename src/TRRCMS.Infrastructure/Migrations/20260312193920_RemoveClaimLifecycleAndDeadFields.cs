using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveClaimLifecycleAndDeadFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Claims_AssignedToUserId",
                table: "Claims");

            migrationBuilder.DropIndex(
                name: "IX_Claims_DecisionDate",
                table: "Claims");

            migrationBuilder.DropIndex(
                name: "IX_Claims_HasConflicts",
                table: "Claims");

            migrationBuilder.DropIndex(
                name: "IX_Claims_HasConflicts_LifecycleStage",
                table: "Claims");

            migrationBuilder.DropIndex(
                name: "IX_Claims_IsDeleted_LifecycleStage",
                table: "Claims");

            migrationBuilder.DropIndex(
                name: "IX_Claims_LifecycleStage",
                table: "Claims");

            migrationBuilder.DropIndex(
                name: "IX_Claims_LifecycleStage_AssignedToUserId",
                table: "Claims");

            migrationBuilder.DropIndex(
                name: "IX_Claims_Priority",
                table: "Claims");

            migrationBuilder.DropIndex(
                name: "IX_Claims_Priority_TargetCompletionDate",
                table: "Claims");

            migrationBuilder.DropIndex(
                name: "IX_Claims_TargetCompletionDate",
                table: "Claims");

            migrationBuilder.DropIndex(
                name: "IX_Claims_VerificationStatus",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "AllRequiredDocumentsSubmitted",
                table: "StagingClaims");

            migrationBuilder.DropColumn(
                name: "EvidenceCount",
                table: "StagingClaims");

            migrationBuilder.DropColumn(
                name: "LegalBasis",
                table: "StagingClaims");

            migrationBuilder.DropColumn(
                name: "LifecycleStage",
                table: "StagingClaims");

            migrationBuilder.DropColumn(
                name: "MissingDocuments",
                table: "StagingClaims");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "StagingClaims");

            migrationBuilder.DropColumn(
                name: "ProcessingNotes",
                table: "StagingClaims");

            migrationBuilder.DropColumn(
                name: "PublicRemarks",
                table: "StagingClaims");

            migrationBuilder.DropColumn(
                name: "SupportingNarrative",
                table: "StagingClaims");

            migrationBuilder.DropColumn(
                name: "TargetCompletionDate",
                table: "StagingClaims");

            migrationBuilder.DropColumn(
                name: "TenureEndDate",
                table: "StagingClaims");

            migrationBuilder.DropColumn(
                name: "TenureStartDate",
                table: "StagingClaims");

            migrationBuilder.DropColumn(
                name: "VerificationNotes",
                table: "StagingClaims");

            migrationBuilder.DropColumn(
                name: "VerificationStatus",
                table: "StagingClaims");

            migrationBuilder.DropColumn(
                name: "AllRequiredDocumentsSubmitted",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "AssignedDate",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "AssignedToUserId",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "ConflictCount",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "ConflictResolutionStatus",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "DecisionByUserId",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "DecisionDate",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "DecisionNotes",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "DecisionReason",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "EvidenceCount",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "FinalDecision",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "HasConflicts",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "LegalBasis",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "LifecycleStage",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "MissingDocuments",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "ProcessingNotes",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "PublicRemarks",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "SupportingNarrative",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "TargetCompletionDate",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "TenureEndDate",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "TenureStartDate",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "VerificationDate",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "VerificationNotes",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "VerificationStatus",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "VerifiedByUserId",
                table: "Claims");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllRequiredDocumentsSubmitted",
                table: "StagingClaims",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "EvidenceCount",
                table: "StagingClaims",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "LegalBasis",
                table: "StagingClaims",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LifecycleStage",
                table: "StagingClaims",
                type: "integer",
                nullable: true,
                comment: "Optional — auto-set to DraftPendingSubmission during commit");

            migrationBuilder.AddColumn<string>(
                name: "MissingDocuments",
                table: "StagingClaims",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                comment: "JSON array of missing required document types");

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "StagingClaims",
                type: "integer",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.AddColumn<string>(
                name: "ProcessingNotes",
                table: "StagingClaims",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublicRemarks",
                table: "StagingClaims",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupportingNarrative",
                table: "StagingClaims",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TargetCompletionDate",
                table: "StagingClaims",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Target completion date for claim processing");

            migrationBuilder.AddColumn<DateTime>(
                name: "TenureEndDate",
                table: "StagingClaims",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Date when tenure/occupancy ended");

            migrationBuilder.AddColumn<DateTime>(
                name: "TenureStartDate",
                table: "StagingClaims",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Date from which tenure/occupancy started");

            migrationBuilder.AddColumn<string>(
                name: "VerificationNotes",
                table: "StagingClaims",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VerificationStatus",
                table: "StagingClaims",
                type: "integer",
                nullable: true,
                comment: "Optional — auto-set to Pending during commit");

            migrationBuilder.AddColumn<bool>(
                name: "AllRequiredDocumentsSubmitted",
                table: "Claims",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "Indicates if all required documents are submitted (جميع المستندات المطلوبة مقدمة)");

            migrationBuilder.AddColumn<DateTime>(
                name: "AssignedDate",
                table: "Claims",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Date when assigned to current officer (تاريخ التعيين)");

            migrationBuilder.AddColumn<Guid>(
                name: "AssignedToUserId",
                table: "Claims",
                type: "uuid",
                nullable: true,
                comment: "Currently assigned case officer (معرف الموظف المسؤول)");

            migrationBuilder.AddColumn<int>(
                name: "ConflictCount",
                table: "Claims",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Number of conflicting claims detected (عدد المطالبات المتعارضة)");

            migrationBuilder.AddColumn<string>(
                name: "ConflictResolutionStatus",
                table: "Claims",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                comment: "Conflict resolution status - e.g., Pending, Resolved (حالة حل التعارض)");

            migrationBuilder.AddColumn<Guid>(
                name: "DecisionByUserId",
                table: "Claims",
                type: "uuid",
                nullable: true,
                comment: "User who made final decision (معرف المستخدم القرار)");

            migrationBuilder.AddColumn<DateTime>(
                name: "DecisionDate",
                table: "Claims",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Date when final decision was made (تاريخ القرار)");

            migrationBuilder.AddColumn<string>(
                name: "DecisionNotes",
                table: "Claims",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                comment: "Decision notes (ملاحظات القرار)");

            migrationBuilder.AddColumn<string>(
                name: "DecisionReason",
                table: "Claims",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                comment: "Reason for approval or rejection (سبب القرار)");

            migrationBuilder.AddColumn<int>(
                name: "EvidenceCount",
                table: "Claims",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Number of evidence items attached (عدد الأدلة)");

            migrationBuilder.AddColumn<string>(
                name: "FinalDecision",
                table: "Claims",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                comment: "Final decision on the claim - e.g., Approved, Rejected (القرار النهائي)");

            migrationBuilder.AddColumn<bool>(
                name: "HasConflicts",
                table: "Claims",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "Indicates if there are conflicting claims (وجود تعارضات)");

            migrationBuilder.AddColumn<string>(
                name: "LegalBasis",
                table: "Claims",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                comment: "Legal basis for the claim (الأساس القانوني)");

            migrationBuilder.AddColumn<int>(
                name: "LifecycleStage",
                table: "Claims",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Current lifecycle stage: 1=DraftPendingSubmission, 2=Submitted, 3=InitialScreening, 4=UnderReview, 5=AwaitingDocuments, 6=ConflictDetected, 7=InAdjudication, 8=PendingApproval, 9=Approved, 10=Rejected, 11=OnHold, 12=Reassigned, 99=Archived (مرحلة دورة الحياة)");

            migrationBuilder.AddColumn<string>(
                name: "MissingDocuments",
                table: "Claims",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                comment: "List of missing document types - stored as JSON (المستندات المفقودة)");

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "Claims",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Priority level: 1=Low, 2=Normal, 3=Medium, 4=High, 5=Critical, 6=VIP, 7=Escalated (الأولوية)");

            migrationBuilder.AddColumn<string>(
                name: "ProcessingNotes",
                table: "Claims",
                type: "character varying(5000)",
                maxLength: 5000,
                nullable: true,
                comment: "Internal processing notes (ملاحظات المعالجة الداخلية)");

            migrationBuilder.AddColumn<string>(
                name: "PublicRemarks",
                table: "Claims",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                comment: "Public remarks visible to claimant (الملاحظات العامة)");

            migrationBuilder.AddColumn<string>(
                name: "SupportingNarrative",
                table: "Claims",
                type: "character varying(5000)",
                maxLength: 5000,
                nullable: true,
                comment: "Supporting narrative or story (السرد الداعم)");

            migrationBuilder.AddColumn<DateTime>(
                name: "TargetCompletionDate",
                table: "Claims",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Target completion/decision date (تاريخ الإنجاز المستهدف)");

            migrationBuilder.AddColumn<DateTime>(
                name: "TenureEndDate",
                table: "Claims",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Date when tenure/occupancy ended (تاريخ انتهاء الحيازة)");

            migrationBuilder.AddColumn<DateTime>(
                name: "TenureStartDate",
                table: "Claims",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Date from which tenure/occupancy started (تاريخ بدء الحيازة)");

            migrationBuilder.AddColumn<DateTime>(
                name: "VerificationDate",
                table: "Claims",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Date when verification was completed (تاريخ التحقق)");

            migrationBuilder.AddColumn<string>(
                name: "VerificationNotes",
                table: "Claims",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                comment: "Verification notes (ملاحظات التحقق)");

            migrationBuilder.AddColumn<int>(
                name: "VerificationStatus",
                table: "Claims",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Verification status: 1=Pending, 2=Verified, 3=Rejected, 4=RequiresAdditionalInfo (حالة التحقق)");

            migrationBuilder.AddColumn<Guid>(
                name: "VerifiedByUserId",
                table: "Claims",
                type: "uuid",
                nullable: true,
                comment: "User who verified the claim (معرف المستخدم المحقق)");

            migrationBuilder.CreateIndex(
                name: "IX_Claims_AssignedToUserId",
                table: "Claims",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Claims_DecisionDate",
                table: "Claims",
                column: "DecisionDate");

            migrationBuilder.CreateIndex(
                name: "IX_Claims_HasConflicts",
                table: "Claims",
                column: "HasConflicts");

            migrationBuilder.CreateIndex(
                name: "IX_Claims_HasConflicts_LifecycleStage",
                table: "Claims",
                columns: new[] { "HasConflicts", "LifecycleStage" });

            migrationBuilder.CreateIndex(
                name: "IX_Claims_IsDeleted_LifecycleStage",
                table: "Claims",
                columns: new[] { "IsDeleted", "LifecycleStage" });

            migrationBuilder.CreateIndex(
                name: "IX_Claims_LifecycleStage",
                table: "Claims",
                column: "LifecycleStage");

            migrationBuilder.CreateIndex(
                name: "IX_Claims_LifecycleStage_AssignedToUserId",
                table: "Claims",
                columns: new[] { "LifecycleStage", "AssignedToUserId" });

            migrationBuilder.CreateIndex(
                name: "IX_Claims_Priority",
                table: "Claims",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_Claims_Priority_TargetCompletionDate",
                table: "Claims",
                columns: new[] { "Priority", "TargetCompletionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Claims_TargetCompletionDate",
                table: "Claims",
                column: "TargetCompletionDate");

            migrationBuilder.CreateIndex(
                name: "IX_Claims_VerificationStatus",
                table: "Claims",
                column: "VerificationStatus");
        }
    }
}
