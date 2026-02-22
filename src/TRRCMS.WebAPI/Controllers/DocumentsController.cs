using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.Common.Models;
using TRRCMS.Application.Documents.Commands.CreateDocument;
using TRRCMS.Application.Documents.Dtos;
using TRRCMS.Application.Documents.Queries.GetAllDocuments;
using TRRCMS.Application.Documents.Queries.GetDocument;

namespace TRRCMS.WebAPI.Controllers;

/// <summary>
/// Document management API for legal and official documents
/// </summary>
/// <remarks>
/// Manages official documents that support tenure rights claims in TRRCMS.
/// Documents are distinct from Evidence - Evidence is the digital file/scan,
/// while Document contains metadata about the official record.
/// 
/// **Document vs Evidence:**
/// - **Document**: Metadata about an official record (type, number, dates, validity)
/// - **Evidence**: The actual digital file (PDF, image, scan)
/// - A Document can link to an Evidence record via `evidenceId`
/// 
/// **Document Categories:**
/// 
/// **Property Ownership (����� ������):**
/// - 1 = TabuGreen (���� ����) - Official property deed
/// - 2 = TabuRed (���� ����) - Temporary/conditional deed
/// - 3 = AgriculturalDeed (��� �����)
/// - 4 = RealEstateRegistryExtract (��� �����)
/// - 5 = OwnershipCertificate (����� �����)
/// 
/// **Rental &amp; Tenancy (�������):**
/// - 10 = RentalContract (��� �����)
/// - 11 = TenancyAgreement (������� �����)
/// - 12 = RentReceipt (����� �����)
/// 
/// **Personal Identification (������ �������):**
/// - 20 = NationalIdCard (����� ���� �����)
/// - 21 = Passport (���� ���)
/// - 22 = FamilyRegistry (��� �����)
/// - 23 = BirthCertificate (����� �����)
/// - 24 = MarriageCertificate (��� ����)
/// 
/// **Utility Bills (������ �������):**
/// - 30 = ElectricityBill (������ ������)
/// - 31 = WaterBill (������ ����)
/// - 32 = GasBill (������ ���)
/// - 33 = TelephoneBill (������ ����)
/// 
/// **Legal Documents (������� ���������):**
/// - 40 = CourtOrder (��� �����)
/// - 41 = LegalNotification (����� ������)
/// - 42 = PowerOfAttorney (�����)
/// - 43 = InheritanceDocument (����� �����)
/// - 44 = DeathCertificate (����� ����)
/// - 45 = DivorceCertificate (����� ����)
/// 
/// **Municipal Documents (����� �������):**
/// - 50 = BuildingPermit (���� ����)
/// - 51 = OccupancyPermit (����� �����)
/// - 52 = PropertyTaxReceipt (����� ����� ������)
/// - 53 = MunicipalityCertificate (����� �����)
/// - 54 = PlanningCertificate (����� �����)
/// 
/// **Supporting Documents (����� �����):**
/// - 60 = Photograph (���� ����������)
/// - 61 = PropertySketch (����)
/// - 62 = SurveyMap (����� ������)
/// - 63 = WitnessStatement (����� ����)
/// - 64 = StatutoryDeclaration (����� ������)
/// 
/// **Sale Documents (����� �����):**
/// - 70 = SaleContract (��� ���)
/// - 71 = PurchaseReceipt (����� ����)
/// - 72 = PreliminarySaleAgreement (��� ��� �������)
/// 
/// **Other:**
/// - 80 = BankStatement (��� ���� ����)
/// - 81 = OfficialLetter (����� �����)
/// - 82 = NotarizedDocument (����� �����)
/// - 999 = Other (����)
/// </remarks>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<DocumentsController> _logger;

    public DocumentsController(IMediator mediator, ILogger<DocumentsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all documents
    /// </summary>
    /// <remarks>
    /// Retrieves all documents in the system. For large datasets, consider using 
    /// search/filter endpoints (when available).
    /// 
    /// **Use Case**: Document review, verification workflows, reporting
    /// 
    /// **Required Permission**: Documents_ViewSensitive (3000)
    /// 
    /// **Response includes:**
    /// - Document metadata (type, number, dates)
    /// - Verification status and history
    /// - Legal validity flags
    /// - Notarization details
    /// - Linked entities (person, property, claim)
    /// - Computed fields (isExpired, isExpiringSoon)
    /// 
    /// **Example response:**
    /// ```json
    /// [
    ///   {
    ///     "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///     "documentType": "TabuGreen",
    ///     "documentNumber": "123456",
    ///     "documentTitle": "��� ����� - ���",
    ///     "issueDate": "2010-05-15T00:00:00Z",
    ///     "expiryDate": null,
    ///     "issuingAuthority": "����� ������� - ���",
    ///     "issuingPlace": "���",
    ///     "isVerified": true,
    ///     "verificationStatus": "Verified",
    ///     "isLegallyValid": true,
    ///     "isOriginal": true,
    ///     "isNotarized": true,
    ///     "notaryOffice": "���� ����� - ���",
    ///     "personId": "7bc92e51-8234-4123-a1bc-9d852f33bcd7",
    ///     "propertyUnitId": "1ab34c56-7890-4def-b123-456789abcdef",
    ///     "isExpired": false,
    ///     "isExpiringSoon": false
    ///   }
    /// ]
    /// ```
    /// </remarks>
    /// <param name="query">Pagination parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of all documents</returns>
    /// <response code="200">Documents retrieved successfully</response>
    /// <response code="401">Not authenticated - valid JWT token required</response>
    /// <response code="403">Missing required permission (Documents_ViewSensitive)</response>
    [HttpGet]
    [Authorize(Policy = "CanViewAllDocuments")]
    [ProducesResponseType(typeof(PagedResult<DocumentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<DocumentDto>>> GetAllDocuments([FromQuery] GetAllDocumentsQuery query, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all documents");

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get document by ID
    /// </summary>
    /// <remarks>
    /// Retrieves a single document with all its details, verification status,
    /// and linked entities.
    /// 
    /// **Use Case**: View document details, verification review
    /// 
    /// **Required Permission**: Documents_ViewSensitive (3000)
    /// 
    /// **Response includes:**
    /// - Full document metadata
    /// - Verification details (who verified, when, notes)
    /// - Legal validity and notarization info
    /// - Links to Person, PropertyUnit, Claim
    /// - Audit trail (created/modified timestamps)
    /// 
    /// **Example response:**
    /// ```json
    /// {
    ///   "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///   "documentType": "NationalIdCard",
    ///   "documentNumber": "01234567890",
    ///   "documentTitle": "����� ���� - ���� ����",
    ///   "issueDate": "2020-01-15T00:00:00Z",
    ///   "expiryDate": "2030-01-15T00:00:00Z",
    ///   "issuingAuthority": "����� ��������",
    ///   "issuingPlace": "����",
    ///   "isVerified": true,
    ///   "verificationStatus": "Verified",
    ///   "verificationDate": "2024-06-20T10:30:00Z",
    ///   "verifiedByUserId": "fd9dc9d5-9757-44b9-b14a-0cbe4715ede5",
    ///   "verificationNotes": "�� ������ �� ��� �������",
    ///   "evidenceId": "8cd45e67-1234-5678-abcd-ef0123456789",
    ///   "personId": "7bc92e51-8234-4123-a1bc-9d852f33bcd7",
    ///   "isLegallyValid": true,
    ///   "isOriginal": true,
    ///   "isNotarized": false,
    ///   "isExpired": false,
    ///   "isExpiringSoon": false,
    ///   "createdAtUtc": "2024-06-15T08:00:00Z",
    ///   "createdBy": "fd9dc9d5-9757-44b9-b14a-0cbe4715ede5"
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">Document GUID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Document details</returns>
    /// <response code="200">Document found and returned</response>
    /// <response code="401">Not authenticated - valid JWT token required</response>
    /// <response code="403">Missing required permission (Documents_ViewSensitive)</response>
    /// <response code="404">Document not found with the specified ID</response>
    [HttpGet("{id}")]
    [Authorize(Policy = "CanViewAllDocuments")]
    [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DocumentDto>> GetDocument(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting document with ID: {DocumentId}", id);

        var query = new GetDocumentQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
        {
            _logger.LogWarning("Document with ID {DocumentId} not found", id);
            return NotFound(new { message = $"Document with ID {id} not found" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Create a new document
    /// </summary>
    /// <remarks>
    /// Creates a new document record with metadata about an official document.
    /// 
    /// **Use Case**: Register official documents that support tenure claims
    /// 
    /// **Required Permission**: Documents_Upload (3002)
    /// 
    /// **Required Fields:**
    /// - `documentType` (integer): Type of document (see enum values below)
    /// 
    /// **Optional Fields:**
    /// - `documentNumber`: Official reference number (e.g., Tabu number, ID number)
    /// - `documentTitle`: Description or title
    /// - `issueDate`: When the document was issued
    /// - `expiryDate`: When the document expires (if applicable)
    /// - `issuingAuthority`: Organization that issued the document
    /// - `issuingPlace`: Location where issued
    /// - `evidenceId`: Link to uploaded file (Evidence record)
    /// - `personId`: Link to person this document belongs to
    /// - `propertyUnitId`: Link to property this document relates to
    /// - `claimId`: Link to claim this document supports
    /// 
    /// **Common Document Types:**
    /// - 1 = TabuGreen (���� ����)
    /// - 2 = TabuRed (���� ����)
    /// - 10 = RentalContract (��� �����)
    /// - 20 = NationalIdCard (����� ����)
    /// - 22 = FamilyRegistry (��� �����)
    /// - 40 = CourtOrder (��� �����)
    /// - 43 = InheritanceDocument (����� �����)
    /// - 70 = SaleContract (��� ���)
    /// 
    /// **Example request - Property Deed (����):**
    /// ```json
    /// {
    ///   "documentType": 1,
    ///   "documentNumber": "123456",
    ///   "documentTitle": "��� ����� - ���� ��� 00001",
    ///   "issueDate": "2010-05-15",
    ///   "issuingAuthority": "����� ������� - ���",
    ///   "issuingPlace": "���",
    ///   "evidenceId": "8cd45e67-1234-5678-abcd-ef0123456789",
    ///   "personId": "7bc92e51-8234-4123-a1bc-9d852f33bcd7",
    ///   "propertyUnitId": "1ab34c56-7890-4def-b123-456789abcdef",
    ///   "notes": "���� ��� ����� �� ����� ������"
    /// }
    /// ```
    /// 
    /// **Example request - National ID:**
    /// ```json
    /// {
    ///   "documentType": 20,
    ///   "documentNumber": "01234567890",
    ///   "documentTitle": "����� ���� - ���� ���� ���",
    ///   "issueDate": "2020-01-15",
    ///   "expiryDate": "2030-01-15",
    ///   "issuingAuthority": "����� ��������",
    ///   "issuingPlace": "����",
    ///   "personId": "7bc92e51-8234-4123-a1bc-9d852f33bcd7"
    /// }
    /// ```
    /// 
    /// **Example request - Rental Contract:**
    /// ```json
    /// {
    ///   "documentType": 10,
    ///   "documentNumber": "RC-2024-001",
    ///   "documentTitle": "��� ����� ����",
    ///   "issueDate": "2024-01-01",
    ///   "expiryDate": "2024-12-31",
    ///   "issuingAuthority": "���� �����",
    ///   "personId": "7bc92e51-8234-4123-a1bc-9d852f33bcd7",
    ///   "propertyUnitId": "1ab34c56-7890-4def-b123-456789abcdef",
    ///   "personPropertyRelationId": "d6ad6c6f-9e89-4190-930d-c6d3ab7b8f8d"
    /// }
    /// ```
    /// </remarks>
    /// <param name="command">Document creation details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created document with generated ID</returns>
    /// <response code="201">Document created successfully</response>
    /// <response code="400">Invalid request - validation failed</response>
    /// <response code="401">Not authenticated - valid JWT token required</response>
    /// <response code="403">Missing required permission (Documents_Upload)</response>
    [HttpPost]
    [Authorize(Policy = "CanCreateDocuments")]
    [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<DocumentDto>> CreateDocument(
        [FromBody] CreateDocumentCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating new document of type: {DocumentType}", command.DocumentType);

        var result = await _mediator.Send(command, cancellationToken);

        return CreatedAtAction(
            nameof(GetDocument),
            new { id = result.Id },
            result);
    }
}