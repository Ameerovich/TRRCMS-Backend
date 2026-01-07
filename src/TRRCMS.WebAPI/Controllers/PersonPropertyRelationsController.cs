using MediatR;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.PersonPropertyRelations.Commands.CreatePersonPropertyRelation;
using TRRCMS.Application.PersonPropertyRelations.Dtos;
using TRRCMS.Application.PersonPropertyRelations.Queries.GetAllPersonPropertyRelations;
using TRRCMS.Application.PersonPropertyRelations.Queries.GetPersonPropertyRelation;

namespace TRRCMS.WebAPI.Controllers;

/// <summary>
/// API controller for person-property relations management
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class PersonPropertyRelationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PersonPropertyRelationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create a new person-property relation
    /// </summary>
    /// <param name="command">Person-property relation creation data</param>
    /// <returns>Created person-property relation</returns>
    /// <response code="201">Person-property relation created successfully</response>
    /// <response code="400">Invalid request data</response>
    [HttpPost]
    [ProducesResponseType(typeof(PersonPropertyRelationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PersonPropertyRelationDto>> Create([FromBody] CreatePersonPropertyRelationCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Get all person-property relations
    /// </summary>
    /// <returns>List of all person-property relations</returns>
    /// <response code="200">Returns the list of person-property relations</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PersonPropertyRelationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PersonPropertyRelationDto>>> GetAll()
    {
        var query = new GetAllPersonPropertyRelationsQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get person-property relation by ID
    /// </summary>
    /// <param name="id">Person-property relation ID</param>
    /// <returns>Person-property relation details</returns>
    /// <response code="200">Returns the person-property relation</response>
    /// <response code="404">Person-property relation not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PersonPropertyRelationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PersonPropertyRelationDto>> GetById(Guid id)
    {
        var query = new GetPersonPropertyRelationQuery(id);
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound();

        return Ok(result);
    }
}
