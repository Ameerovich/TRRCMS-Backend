using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.Documents.Commands.CreateDocument;
using TRRCMS.Application.Documents.Dtos;
using TRRCMS.Application.Documents.Queries.GetAllDocuments;
using TRRCMS.Application.Documents.Queries.GetDocument;

namespace TRRCMS.WebAPI.Controllers;

/// <summary>
/// API endpoints for Document management
/// All endpoints require authentication and specific permissions
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication for all endpoints
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
    /// Requires: Documents_ViewAll permission
    /// </summary>
    /// <returns>List of all documents</returns>
    /// <response code="200">Documents retrieved successfully</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Missing required permission (Documents_ViewAll)</response>
    [HttpGet]
    [Authorize(Policy = "CanViewAllDocuments")]
    [ProducesResponseType(typeof(IEnumerable<DocumentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<DocumentDto>>> GetAllDocuments(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all documents");

        var query = new GetAllDocumentsQuery();
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Get document by ID
    /// Requires: Documents_ViewAll permission
    /// </summary>
    /// <param name="id">Document ID</param>
    /// <returns>Document details</returns>
    /// <response code="200">Document found</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Missing required permission (Documents_ViewAll)</response>
    /// <response code="404">Document not found</response>
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
    /// Requires: Documents_Create permission
    /// </summary>
    /// <param name="command">Document creation details</param>
    /// <returns>Created document</returns>
    /// <response code="201">Document created successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Missing required permission (Documents_Create)</response>
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
