using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCertificateAndReferralEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Certificate");

            migrationBuilder.DropTable(
                name: "Referrals");

            migrationBuilder.DropIndex(
                name: "IX_Claims_CertificateStatus",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "CertificateStatus",
                table: "Claims");

            migrationBuilder.AlterColumn<int>(
                name: "LifecycleStage",
                table: "Claims",
                type: "integer",
                nullable: false,
                comment: "Current lifecycle stage: 1=DraftPendingSubmission, 2=Submitted, 3=InitialScreening, 4=UnderReview, 5=AwaitingDocuments, 6=ConflictDetected, 7=InAdjudication, 8=PendingApproval, 9=Approved, 10=Rejected, 11=OnHold, 12=Reassigned, 99=Archived (مرحلة دورة الحياة)",
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Current lifecycle stage: 1=DraftPendingSubmission, 2=Submitted, 3=InitialScreening, 4=UnderReview, 5=AwaitingDocuments, 6=ConflictDetected, 7=InAdjudication, 8=Approved, 9=Rejected, 10=CertificateIssued, 11=Archived (مرحلة دورة الحياة)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "LifecycleStage",
                table: "Claims",
                type: "integer",
                nullable: false,
                comment: "Current lifecycle stage: 1=DraftPendingSubmission, 2=Submitted, 3=InitialScreening, 4=UnderReview, 5=AwaitingDocuments, 6=ConflictDetected, 7=InAdjudication, 8=Approved, 9=Rejected, 10=CertificateIssued, 11=Archived (مرحلة دورة الحياة)",
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Current lifecycle stage: 1=DraftPendingSubmission, 2=Submitted, 3=InitialScreening, 4=UnderReview, 5=AwaitingDocuments, 6=ConflictDetected, 7=InAdjudication, 8=PendingApproval, 9=Approved, 10=Rejected, 11=OnHold, 12=Reassigned, 99=Archived (مرحلة دورة الحياة)");

            migrationBuilder.AddColumn<int>(
                name: "CertificateStatus",
                table: "Claims",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Certificate status: 1=NotRequired, 2=PendingGeneration, 3=Generated, 4=Issued, 5=Collected, 6=Rejected, 7=Revoked (حالة الشهادة)");

            migrationBuilder.CreateTable(
                name: "Certificate",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BeneficiaryPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimId = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalCertificateId = table.Column<Guid>(type: "uuid", nullable: true),
                    PropertyUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Barcode = table.Column<string>(type: "text", nullable: true),
                    CertificateNumber = table.Column<string>(type: "text", nullable: false),
                    CertificateType = table.Column<string>(type: "text", nullable: false),
                    CollectedByName = table.Column<string>(type: "text", nullable: true),
                    CollectedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CollectorIdNumber = table.Column<string>(type: "text", nullable: true),
                    CollectorRelationship = table.Column<string>(type: "text", nullable: true),
                    CollectorSignature = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DigitalSignature = table.Column<string>(type: "text", nullable: true),
                    GeneratedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    GeneratedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    HandedOverByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsPermanent = table.Column<bool>(type: "boolean", nullable: false),
                    IsReissued = table.Column<bool>(type: "boolean", nullable: false),
                    IssuedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IssuedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IssuingOrganization = table.Column<string>(type: "text", nullable: false),
                    LastModifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    LegalBasis = table.Column<string>(type: "text", nullable: true),
                    LegalReference = table.Column<string>(type: "text", nullable: true),
                    PdfFileHash = table.Column<string>(type: "text", nullable: true),
                    PdfFilePath = table.Column<string>(type: "text", nullable: true),
                    PdfFileSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    QrCodeData = table.Column<string>(type: "text", nullable: true),
                    ReissueNumber = table.Column<int>(type: "integer", nullable: true),
                    ReissueReason = table.Column<string>(type: "text", nullable: true),
                    Remarks = table.Column<string>(type: "text", nullable: true),
                    RightsSummaryArabic = table.Column<string>(type: "text", nullable: false),
                    RightsSummaryEnglish = table.Column<string>(type: "text", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: true),
                    SignedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SigningAuthority = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TermsAndConditions = table.Column<string>(type: "text", nullable: true),
                    TitleArabic = table.Column<string>(type: "text", nullable: false),
                    TitleEnglish = table.Column<string>(type: "text", nullable: true),
                    ValidityEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValidityStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    VoidReason = table.Column<string>(type: "text", nullable: true),
                    VoidedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    VoidedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Certificate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Certificate_Certificate_OriginalCertificateId",
                        column: x => x.OriginalCertificateId,
                        principalTable: "Certificate",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Certificate_Claims_ClaimId",
                        column: x => x.ClaimId,
                        principalTable: "Claims",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Certificate_Persons_BeneficiaryPersonId",
                        column: x => x.BeneficiaryPersonId,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Certificate_PropertyUnits_PropertyUnitId",
                        column: x => x.PropertyUnitId,
                        principalTable: "PropertyUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Referrals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimId = table.Column<Guid>(type: "uuid", nullable: false, comment: "Foreign key to Claim being referred"),
                    PreviousReferralId = table.Column<Guid>(type: "uuid", nullable: true),
                    AcknowledgedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    AcknowledgedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActionsRequired = table.Column<string>(type: "text", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DocumentsRequired = table.Column<string>(type: "text", nullable: true),
                    EscalationLevel = table.Column<int>(type: "integer", nullable: true),
                    ExpectedResponseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FromRole = table.Column<int>(type: "integer", nullable: false, comment: "Role referring the claim"),
                    FromUserId = table.Column<Guid>(type: "uuid", nullable: false, comment: "User who initiated the referral"),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsEscalation = table.Column<bool>(type: "boolean", nullable: false),
                    IsOverdue = table.Column<bool>(type: "boolean", nullable: false),
                    LastModifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    Outcome = table.Column<string>(type: "text", nullable: true),
                    Priority = table.Column<string>(type: "text", nullable: false),
                    ReferralDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReferralNotes = table.Column<string>(type: "text", nullable: true),
                    ReferralNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "Referral number - Format: REF-YYYY-NNNN"),
                    ReferralReason = table.Column<string>(type: "text", nullable: false),
                    ReferralType = table.Column<string>(type: "text", nullable: false),
                    ResponseNotes = table.Column<string>(type: "text", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: true),
                    StartedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    TargetResolutionHours = table.Column<int>(type: "integer", nullable: true),
                    ToRole = table.Column<int>(type: "integer", nullable: false, comment: "Role receiving the claim"),
                    ToUserId = table.Column<Guid>(type: "uuid", nullable: true, comment: "Specific user assigned (optional)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Referrals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Referrals_Claims_ClaimId",
                        column: x => x.ClaimId,
                        principalTable: "Claims",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Referrals_Referrals_PreviousReferralId",
                        column: x => x.PreviousReferralId,
                        principalTable: "Referrals",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Claims_CertificateStatus",
                table: "Claims",
                column: "CertificateStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Certificate_BeneficiaryPersonId",
                table: "Certificate",
                column: "BeneficiaryPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificate_ClaimId",
                table: "Certificate",
                column: "ClaimId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificate_OriginalCertificateId",
                table: "Certificate",
                column: "OriginalCertificateId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificate_PropertyUnitId",
                table: "Certificate",
                column: "PropertyUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Referrals_ClaimId",
                table: "Referrals",
                column: "ClaimId");

            migrationBuilder.CreateIndex(
                name: "IX_Referrals_IsDeleted",
                table: "Referrals",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Referrals_PreviousReferralId",
                table: "Referrals",
                column: "PreviousReferralId");

            migrationBuilder.CreateIndex(
                name: "IX_Referrals_ReferralNumber",
                table: "Referrals",
                column: "ReferralNumber",
                unique: true);
        }
    }
}
