using MediatR;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.PropertyUnits.Commands.CreatePropertyUnit;
using TRRCMS.Application.PropertyUnits.Dtos;
using TRRCMS.Application.PropertyUnits.Queries.GetAllPropertyUnits;
using TRRCMS.Application.PropertyUnits.Queries.GetPropertyUnit;

namespace TRRCMS.WebAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class PropertyUnitsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PropertyUnitsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create a new property unit
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> CreatePropertyUnit([FromBody] CreatePropertyUnitCommand command)
    {
        try
        {
            var propertyUnitId = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetPropertyUnit), new { id = propertyUnitId }, propertyUnitId);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Get property unit by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PropertyUnitDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PropertyUnitDto>> GetPropertyUnit(Guid id)
    {
        var query = new GetPropertyUnitQuery(id);
        var propertyUnit = await _mediator.Send(query);

        if (propertyUnit == null)
        {
            return NotFound();
        }

        return Ok(propertyUnit);
    }

    /// <summary>
    /// Get all property units
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<PropertyUnitDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PropertyUnitDto>>> GetAllPropertyUnits()
    {
        var query = new GetAllPropertyUnitsQuery();
        var propertyUnits = await _mediator.Send(query);

        return Ok(propertyUnits);
    }
}