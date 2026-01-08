using MediatR;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.Documents.Commands.CreateDocument;
using TRRCMS.Application.Documents.Dtos;
using TRRCMS.Application.Documents.Queries.GetAllDocuments;
using TRRCMS.Application.Documents.Queries.GetDocument;

namespace TRRCMS.WebAPI.Controllers;

/// <summary>
/// API endpoints for Document management
/// </summary>
[ApiController]
[Route("api/[controller]")]
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
    /// <returns>List of all documents</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DocumentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DocumentDto>>> GetAllDocuments(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all documents");

        var query = new GetAllDocumentsQuery();
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Get document by ID
    /// </summary>
    /// <param name="id">Document ID</param>
    /// <returns>Document details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status200OK)]
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
    /// <param name="command">Document creation details</param>
    /// <returns>Created document</returns>
    [HttpPost]
    [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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