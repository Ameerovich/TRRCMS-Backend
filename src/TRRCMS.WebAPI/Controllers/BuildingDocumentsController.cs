using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.Buildings.Dtos;
using TRRCMS.Application.Buildings.Queries.DownloadBuildingDocument;
using TRRCMS.Application.Buildings.Queries.GetBuildingDocument;
using TRRCMS.Application.Buildings.Queries.GetBuildingDocumentsByBuilding;

namespace TRRCMS.WebAPI.Controllers;

/// <summary>
/// API for retrieving building documents (photos, PDFs).
/// </summary>
[ApiController]
[Route("api/v1/building-documents")]
[Produces("application/json")]
[Authorize]
public class BuildingDocumentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public BuildingDocumentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get a building document by its ID.
    /// </summary>
    /// <param name="id">Building document ID (GUID)</param>
    /// <returns>The building document details</returns>
    /// <response code="200">Building document found</response>
    /// <response code="404">Building document not found</response>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "CanViewAllBuildings")]
    [ProducesResponseType(typeof(BuildingDocumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BuildingDocumentDto>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetBuildingDocumentQuery(id));

        if (result == null)
            return NotFound(new { message = $"Building document with ID {id} not found" });

        return Ok(result);
    }

    /// <summary>
    /// Get all building documents linked to a specific building.
    /// </summary>
    /// <param name="buildingId">Building ID (GUID)</param>
    /// <returns>List of building documents for the given building</returns>
    /// <response code="200">List of building documents (may be empty)</response>
    [HttpGet("by-building/{buildingId:guid}")]
    [Authorize(Policy = "CanViewAllBuildings")]
    [ProducesResponseType(typeof(List<BuildingDocumentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<BuildingDocumentDto>>> GetByBuildingId(Guid buildingId)
    {
        var result = await _mediator.Send(new GetBuildingDocumentsByBuildingQuery(buildingId));
        return Ok(result);
    }

    /// <summary>
    /// Download the binary file (photo/PDF) of a building document.
    /// </summary>
    /// <remarks>
    /// Returns the file as a binary stream with the appropriate Content-Type header.
    ///
    /// **Example Usage:**
    /// ```
    /// GET /api/v1/building-documents/a1b2c3d4-e5f6-4a8b-9c0d-1e2f3a4b5c6d/download
    /// ```
    /// </remarks>
    /// <param name="id">Building document ID (GUID)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>File stream of the document</returns>
    /// <response code="200">File downloaded successfully</response>
    /// <response code="404">Document not found, or its file is missing on the server</response>
    [HttpGet("{id:guid}/download")]
    [Authorize(Policy = "CanViewAllBuildings")]
    [Produces("application/pdf", "image/jpeg", "image/png")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DownloadBuildingDocumentQuery(id), cancellationToken);
        return File(result.FileStream, result.MimeType, result.FileName, enableRangeProcessing: true);
    }
}
