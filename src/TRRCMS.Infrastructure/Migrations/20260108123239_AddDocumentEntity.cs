using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "Document type from controlled vocabulary (e.g., TabuGreen, RentalContract, NationalIdCard)"),
                    DocumentNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "Document number/reference (e.g., Tabu number, ID number, contract number)"),
                    DocumentTitle = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "Document title or description in Arabic"),
                    IssueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Date when document was issued"),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Date when document expires (if applicable)"),
                    IssuingAuthority = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true, comment: "Issuing authority/organization (e.g., Ministry of Interior, Aleppo Municipality)"),
                    IssuingPlace = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true, comment: "Place where document was issued"),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false, comment: "Indicates if document has been verified"),
                    VerificationStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Pending", comment: "Verification status (Pending, Verified, Rejected, RequiresAdditionalInfo)"),
                    VerificationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Date when document was verified"),
                    VerifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true, comment: "User who verified the document"),
                    VerificationNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true, comment: "Verification notes or comments"),
                    EvidenceId = table.Column<Guid>(type: "uuid", nullable: true, comment: "Foreign key to Evidence entity (the actual file/scan)"),
                    DocumentHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true, comment: "SHA-256 hash of the document for integrity verification"),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true, comment: "Additional notes about this document"),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: true, comment: "Foreign key to Person (if document belongs to a person)"),
                    PropertyUnitId = table.Column<Guid>(type: "uuid", nullable: true, comment: "Foreign key to PropertyUnit (if document relates to property)"),
                    PersonPropertyRelationId = table.Column<Guid>(type: "uuid", nullable: true, comment: "Foreign key to PersonPropertyRelation (if document proves a relation)"),
                    ClaimId = table.Column<Guid>(type: "uuid", nullable: true, comment: "Foreign key to Claim (if document supports a claim)"),
                    IsLegallyValid = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true, comment: "Indicates if document is legally valid"),
                    LegalValidityNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true, comment: "Legal validity notes (why valid or invalid)"),
                    IsOriginal = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true, comment: "Indicates if document is original or a copy"),
                    OriginalDocumentId = table.Column<Guid>(type: "uuid", nullable: true, comment: "If copy, reference to original document (if in system)"),
                    IsNotarized = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false, comment: "Indicates if document is notarized"),
                    NotaryOffice = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true, comment: "Notary office name/number"),
                    NotarizationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Notarization date"),
                    NotarizationNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "Notarization number"),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "UTC timestamp when record was created"),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false, comment: "User ID who created this record"),
                    LastModifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "UTC timestamp when record was last modified"),
                    LastModifiedBy = table.Column<Guid>(type: "uuid", nullable: true, comment: "User ID who last modified this record"),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false, comment: "Soft delete flag"),
                    DeletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "UTC timestamp when record was soft deleted"),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true, comment: "User ID who soft deleted this record")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Documents_Claims_ClaimId",
                        column: x => x.ClaimId,
                        principalTable: "Claims",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Documents_Documents_OriginalDocumentId",
                        column: x => x.OriginalDocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_Evidences_EvidenceId",
                        column: x => x.EvidenceId,
                        principalTable: "Evidences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_PersonPropertyRelations_PersonPropertyRelationId",
                        column: x => x.PersonPropertyRelationId,
                        principalTable: "PersonPropertyRelations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_Persons_PersonId",
                        column: x => x.PersonId,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_PropertyUnits_PropertyUnitId",
                        column: x => x.PropertyUnitId,
                        principalTable: "PropertyUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ClaimId",
                table: "Documents",
                column: "ClaimId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_DocumentNumber",
                table: "Documents",
                column: "DocumentNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_DocumentType",
                table: "Documents",
                column: "DocumentType");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_DocumentType_VerificationStatus",
                table: "Documents",
                columns: new[] { "DocumentType", "VerificationStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_EvidenceId",
                table: "Documents",
                column: "EvidenceId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ExpiryDate",
                table: "Documents",
                column: "ExpiryDate");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_IsDeleted",
                table: "Documents",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_IsVerified",
                table: "Documents",
                column: "IsVerified");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_OriginalDocumentId",
                table: "Documents",
                column: "OriginalDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_PersonId",
                table: "Documents",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_PersonPropertyRelationId",
                table: "Documents",
                column: "PersonPropertyRelationId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_PropertyUnitId",
                table: "Documents",
                column: "PropertyUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_VerificationStatus",
                table: "Documents",
                column: "VerificationStatus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Documents");
        }
    }
}
