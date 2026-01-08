using MediatR;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.Evidences.Commands.CreateEvidence;
using TRRCMS.Application.Evidences.Dtos;
using TRRCMS.Application.Evidences.Queries.GetAllEvidences;
using TRRCMS.Application.Evidences.Queries.GetEvidence;

namespace TRRCMS.WebAPI.Controllers;

/// <summary>
/// API controller for evidence (documents/photos/files) management
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class EvidencesController : ControllerBase
{
    private readonly IMediator _mediator;

    public EvidencesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create a new evidence (upload document/photo metadata)
    /// </summary>
    /// <param name="command">Evidence creation data</param>
    /// <returns>Created evidence</returns>
    /// <response code="201">Evidence created successfully</response>
    /// <response code="400">Invalid request data</response>
    [HttpPost]
    [ProducesResponseType(typeof(EvidenceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EvidenceDto>> Create([FromBody] CreateEvidenceCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Get all evidences
    /// </summary>
    /// <returns>List of all evidences</returns>
    /// <response code="200">Returns the list of evidences</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EvidenceDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<EvidenceDto>>> GetAll()
    {
        var query = new GetAllEvidencesQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get evidence by ID
    /// </summary>
    /// <param name="id">Evidence ID</param>
    /// <returns>Evidence details</returns>
    /// <response code="200">Returns the evidence</response>
    /// <response code="404">Evidence not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(EvidenceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EvidenceDto>> GetById(Guid id)
    {
        var query = new GetEvidenceQuery(id);
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound();

        return Ok(result);
    }
}
