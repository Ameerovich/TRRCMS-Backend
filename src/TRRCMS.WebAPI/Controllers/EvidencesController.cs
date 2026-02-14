using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.Common.Models;
using TRRCMS.Application.Evidences.Commands.CreateEvidence;
using TRRCMS.Application.Evidences.Commands.DeleteEvidence;
using TRRCMS.Application.Evidences.Dtos;
using TRRCMS.Application.Evidences.Queries.GetAllEvidences;
using TRRCMS.Application.Evidences.Queries.GetEvidence;

namespace TRRCMS.WebAPI.Controllers;

/// <summary>
/// Evidence management API for documents, photos, and supporting files
/// </summary>
/// <remarks>
/// Manages digital evidence files that support tenure rights claims in TRRCMS.
/// Evidence represents the actual uploaded files (PDFs, images, scans),
/// while Documents contain metadata about official records.
/// 
/// **Evidence vs Document:**
/// - **Evidence**: The actual digital file (PDF scan, photo, document image)
/// - **Document**: Metadata about an official record (can link to Evidence via `evidenceId`)
/// 
/// **Common Workflow:**
/// 1. Upload file to storage → get `filePath`
/// 2. Create Evidence record with file metadata
/// 3. Optionally create Document record linking to Evidence
/// 4. Link Evidence to Person, Claim, or PersonPropertyRelation
/// 
/// **EvidenceType Values (نوع الدليل):**
/// 
/// | Value | Name | Arabic | Description |
/// |-------|------|--------|-------------|
/// | 1 | IdentificationDocument | بطاقة هوية | National ID, Passport |
/// | 2 | OwnershipDeed | سند ملكية | Property deed, Tabu |
/// | 3 | RentalContract | عقد إيجار | Lease agreement |
/// | 4 | UtilityBill | فاتورة مرافق | Electricity, water bills |
/// | 5 | Photo | صورة | Property/building photos |
/// | 6 | OfficialLetter | رسالة رسمية | Government correspondence |
/// | 7 | CourtOrder | أمر محكمة | Legal judgments |
/// | 8 | InheritanceDocument | وثيقة ميراث | Inheritance papers |
/// | 9 | TaxReceipt | إيصال ضريبة | Property tax receipts |
/// | 99 | Other | أخرى | Other evidence types |
/// 
/// **Supported File Types:**
/// - Documents: `application/pdf`, `application/msword`
/// - Images: `image/jpeg`, `image/png`, `image/tiff`
/// - Max file size: Configured in application settings
/// 
/// **Versioning:**
/// Evidence supports versioning - when a document is updated, a new version
/// is created while maintaining reference to previous versions via `previousVersionId`.
/// </remarks>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[Authorize]
public class EvidencesController : ControllerBase
{
    private readonly IMediator _mediator;

    public EvidencesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create new evidence record
    /// </summary>
    /// <remarks>
    /// Creates a new evidence record with file metadata and optional links to
    /// persons, claims, or property relations.
    /// 
    /// **Use Case**: UC-001 Field Survey - Upload supporting documents
    /// 
    /// **Required Permission**: Evidence_Upload (2001)
    /// 
    /// **Required Fields:**
    /// - `evidenceType` (integer): Type of evidence (1-9, 99)
    /// - `description`: Brief description of the evidence
    /// - `originalFileName`: Original filename as uploaded
    /// - `filePath`: Path where file is stored
    /// - `fileSizeBytes`: File size in bytes
    /// - `mimeType`: MIME type (e.g., "application/pdf", "image/jpeg")
    /// 
    /// **Optional Fields:**
    /// - `fileHash`: SHA-256 hash for integrity verification
    /// - `documentIssuedDate`: When the document was issued
    /// - `documentExpiryDate`: When the document expires
    /// - `issuingAuthority`: Organization that issued the document
    /// - `documentReferenceNumber`: Official reference/registration number
    /// - `notes`: Additional notes
    /// - `personId`: Link to a person (for ID documents)
    /// - `personPropertyRelationId`: Link to ownership/tenancy relation
    /// - `claimId`: Link to a claim
    /// 
    /// **Example - National ID Card:**
    /// ```json
    /// {
    ///   "evidenceType": 1,
    ///   "description": "بطاقة هوية وطنية - أحمد محمد",
    ///   "originalFileName": "national_id_ahmed.pdf",
    ///   "filePath": "/uploads/2024/01/abc123-national_id.pdf",
    ///   "fileSizeBytes": 102400,
    ///   "mimeType": "application/pdf",
    ///   "fileHash": "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855",
    ///   "documentIssuedDate": "2020-01-15T00:00:00Z",
    ///   "documentExpiryDate": "2030-01-15T00:00:00Z",
    ///   "issuingAuthority": "وزارة الداخلية",
    ///   "documentReferenceNumber": "01234567890",
    ///   "personId": "7bc92e51-8234-4123-a1bc-9d852f33bcd7"
    /// }
    /// ```
    /// 
    /// **Example - Property Deed (طابو):**
    /// ```json
    /// {
    ///   "evidenceType": 2,
    ///   "description": "سند ملكية - عقار رقم 00001",
    ///   "originalFileName": "tabu_green.pdf",
    ///   "filePath": "/uploads/2024/01/def456-tabu.pdf",
    ///   "fileSizeBytes": 256000,
    ///   "mimeType": "application/pdf",
    ///   "documentIssuedDate": "2010-05-20T00:00:00Z",
    ///   "issuingAuthority": "السجل العقاري - حلب",
    ///   "documentReferenceNumber": "TABU-123456",
    ///   "personPropertyRelationId": "d6ad6c6f-9e89-4190-930d-c6d3ab7b8f8d"
    /// }
    /// ```
    /// 
    /// **Example - Property Photo:**
    /// ```json
    /// {
    ///   "evidenceType": 5,
    ///   "description": "صورة واجهة البناء الأمامية",
    ///   "originalFileName": "building_front.jpg",
    ///   "filePath": "/uploads/2024/01/ghi789-building.jpg",
    ///   "fileSizeBytes": 2048000,
    ///   "mimeType": "image/jpeg",
    ///   "notes": "تم التقاط الصورة أثناء المسح الميداني"
    /// }
    /// ```
    /// 
    /// **Example - Rental Contract:**
    /// ```json
    /// {
    ///   "evidenceType": 3,
    ///   "description": "عقد إيجار سنوي - 2024",
    ///   "originalFileName": "rental_contract_2024.pdf",
    ///   "filePath": "/uploads/2024/01/jkl012-rental.pdf",
    ///   "fileSizeBytes": 180000,
    ///   "mimeType": "application/pdf",
    ///   "documentIssuedDate": "2024-01-01T00:00:00Z",
    ///   "documentExpiryDate": "2024-12-31T00:00:00Z",
    ///   "issuingAuthority": "كاتب العدل",
    ///   "personPropertyRelationId": "d6ad6c6f-9e89-4190-930d-c6d3ab7b8f8d"
    /// }
    /// ```
    /// </remarks>
    /// <param name="command">Evidence creation data with file metadata</param>
    /// <returns>Created evidence with generated ID</returns>
    /// <response code="201">Evidence created successfully</response>
    /// <response code="400">Invalid request - validation failed (missing required fields, invalid enum, etc.)</response>
    /// <response code="401">Not authenticated - valid JWT token required</response>
    /// <response code="403">Missing required permission (Evidence_Upload)</response>
    [HttpPost]
    [Authorize(Policy = "CanUploadEvidence")]
    [ProducesResponseType(typeof(EvidenceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<EvidenceDto>> Create([FromBody] CreateEvidenceCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Get all evidence records
    /// </summary>
    /// <remarks>
    /// Retrieves all evidence records in the system.
    /// For large datasets, consider using search/filter endpoints or
    /// retrieving evidence through parent entities (Surveys, Claims).
    /// 
    /// **Use Case**: Evidence review, verification workflows, reporting
    /// 
    /// **Required Permission**: Evidence_View (2000)
    /// 
    /// **Response includes:**
    /// - Evidence metadata (type, description, file info)
    /// - Document dates (issued, expiry)
    /// - Linked entities (personId, claimId, personPropertyRelationId)
    /// - Version information (versionNumber, isCurrentVersion)
    /// - Computed fields (isExpired)
    /// - Audit trail (created/modified timestamps)
    /// 
    /// **Example response:**
    /// ```json
    /// [
    ///   {
    ///     "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///     "evidenceType": 1,
    ///     "description": "بطاقة هوية وطنية - أحمد محمد",
    ///     "originalFileName": "national_id.pdf",
    ///     "filePath": "/uploads/2024/01/abc123.pdf",
    ///     "fileSizeBytes": 102400,
    ///     "mimeType": "application/pdf",
    ///     "fileHash": "e3b0c44298fc1c149afbf4c8996fb924...",
    ///     "documentIssuedDate": "2020-01-15T00:00:00Z",
    ///     "documentExpiryDate": "2030-01-15T00:00:00Z",
    ///     "issuingAuthority": "وزارة الداخلية",
    ///     "documentReferenceNumber": "01234567890",
    ///     "versionNumber": 1,
    ///     "isCurrentVersion": true,
    ///     "personId": "7bc92e51-8234-4123-a1bc-9d852f33bcd7",
    ///     "isExpired": false,
    ///     "createdAtUtc": "2024-06-15T08:00:00Z",
    ///     "createdBy": "fd9dc9d5-9757-44b9-b14a-0cbe4715ede5"
    ///   },
    ///   {
    ///     "id": "8cd45e67-1234-5678-abcd-ef0123456789",
    ///     "evidenceType": 5,
    ///     "description": "صورة واجهة البناء",
    ///     "originalFileName": "building_photo.jpg",
    ///     "filePath": "/uploads/2024/01/def456.jpg",
    ///     "fileSizeBytes": 2048000,
    ///     "mimeType": "image/jpeg",
    ///     "versionNumber": 1,
    ///     "isCurrentVersion": true,
    ///     "isExpired": false,
    ///     "createdAtUtc": "2024-06-15T09:30:00Z"
    ///   }
    /// ]
    /// ```
    /// 
    /// **Note:** The `evidenceType` field returns the integer enum value.
    /// Use the enum reference in the class documentation to map values to names.
    /// </remarks>
    /// <returns>List of all evidence records</returns>
    /// <response code="200">Evidence records retrieved successfully</response>
    /// <response code="401">Not authenticated - valid JWT token required</response>
    /// <response code="403">Missing required permission (Evidence_View)</response>
    [HttpGet]
    [Authorize(Policy = "CanViewEvidence")]
    [ProducesResponseType(typeof(IEnumerable<EvidenceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<EvidenceDto>>> GetAll()
    {
        var query = new GetAllEvidencesQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get evidence by ID
    /// </summary>
    /// <remarks>
    /// Retrieves a single evidence record with all its details.
    /// 
    /// **Use Case**: View evidence details, verify document, download preparation
    /// 
    /// **Required Permission**: Evidence_View (2000)
    /// 
    /// **Response includes:**
    /// - Complete file metadata
    /// - Document validity information
    /// - Version history reference
    /// - All linked entity IDs
    /// - Full audit trail
    /// 
    /// **Example response:**
    /// ```json
    /// {
    ///   "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "evidenceType": 2,
    ///   "description": "سند ملكية - طابو أخضر",
    ///   "originalFileName": "tabu_green_scan.pdf",
    ///   "filePath": "/uploads/2024/01/abc123-tabu.pdf",
    ///   "fileSizeBytes": 256000,
    ///   "mimeType": "application/pdf",
    ///   "fileHash": "a7ffc6f8bf1ed76651c14756a061d662...",
    ///   "documentIssuedDate": "2010-05-20T00:00:00Z",
    ///   "documentExpiryDate": null,
    ///   "issuingAuthority": "السجل العقاري - حلب",
    ///   "documentReferenceNumber": "TABU-2010-123456",
    ///   "notes": "نسخة مصدقة من الطابو الأخضر",
    ///   "versionNumber": 1,
    ///   "previousVersionId": null,
    ///   "isCurrentVersion": true,
    ///   "personId": null,
    ///   "personPropertyRelationId": "d6ad6c6f-9e89-4190-930d-c6d3ab7b8f8d",
    ///   "claimId": "5ef67890-abcd-1234-5678-901234567890",
    ///   "createdAtUtc": "2024-06-15T08:00:00Z",
    ///   "createdBy": "fd9dc9d5-9757-44b9-b14a-0cbe4715ede5",
    ///   "lastModifiedAtUtc": null,
    ///   "lastModifiedBy": null,
    ///   "isDeleted": false,
    ///   "isExpired": false
    /// }
    /// ```
    /// 
    /// **EvidenceType Quick Reference:**
    /// - 1 = IdentificationDocument (بطاقة هوية)
    /// - 2 = OwnershipDeed (سند ملكية)
    /// - 3 = RentalContract (عقد إيجار)
    /// - 4 = UtilityBill (فاتورة مرافق)
    /// - 5 = Photo (صورة)
    /// - 6 = OfficialLetter (رسالة رسمية)
    /// - 7 = CourtOrder (أمر محكمة)
    /// - 8 = InheritanceDocument (وثيقة ميراث)
    /// - 9 = TaxReceipt (إيصال ضريبة)
    /// - 99 = Other (أخرى)
    /// </remarks>
    /// <param name="id">Evidence GUID</param>
    /// <returns>Evidence details</returns>
    /// <response code="200">Evidence found and returned</response>
    /// <response code="401">Not authenticated - valid JWT token required</response>
    /// <response code="403">Missing required permission (Evidence_View)</response>
    /// <response code="404">Evidence not found with the specified ID</response>
    [HttpGet("{id}")]
    [Authorize(Policy = "CanViewEvidence")]
    [ProducesResponseType(typeof(EvidenceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EvidenceDto>> GetById(Guid id)
    {
        var query = new GetEvidenceQuery(id);
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound(new { message = $"Evidence with ID {id} not found" });

        return Ok(result);
    }

    // ==================== DELETE ====================

    /// <summary>
    /// Soft delete an evidence record
    /// حذف مستند/دليل
    /// </summary>
    /// <remarks>
    /// **Use Case**: Remove evidence that was uploaded by mistake or is no longer relevant
    ///
    /// **Required Permission**: Evidence_Upload (2001) - CanUploadEvidence policy
    ///
    /// **What it does**:
    /// - Marks the evidence record as deleted (soft delete)
    /// - Does NOT cascade to other entities (evidence is a leaf entity)
    /// - Creates an audit trail entry
    ///
    /// **Important**: Only works when the related survey is in **Draft** status.
    /// If the survey is Finalized or Completed, the delete will be rejected.
    ///
    /// **Example Request**:
    /// ```
    /// DELETE /api/v1/Evidences/3fa85f64-5717-4562-b3fc-2c963f66afa6
    /// ```
    ///
    /// **Example Response**:
    /// ```json
    /// {
    ///   "primaryEntityId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "primaryEntityType": "Evidence",
    ///   "affectedEntities": [
    ///     { "entityId": "3fa85f64-...", "entityType": "Evidence", "entityIdentifier": "national_id.pdf" }
    ///   ],
    ///   "totalAffected": 1,
    ///   "deletedAtUtc": "2026-02-14T10:00:00Z",
    ///   "message": "Evidence deleted successfully"
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">Evidence ID (GUID) to delete</param>
    /// <returns>Delete result with affected entity ID</returns>
    /// <response code="200">Evidence deleted successfully</response>
    /// <response code="400">Survey is not in Draft status or evidence is already deleted</response>
    /// <response code="401">Not authenticated - valid JWT token required</response>
    /// <response code="403">Not authorized - requires Evidence_Upload (2001) permission</response>
    /// <response code="404">Evidence not found</response>
    [HttpDelete("{id}")]
    [Authorize(Policy = "CanUploadEvidence")]
    [ProducesResponseType(typeof(DeleteResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DeleteResultDto>> Delete(Guid id)
    {
        var command = new Application.Evidences.Commands.DeleteEvidence.DeleteEvidenceCommand { EvidenceId = id };
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}