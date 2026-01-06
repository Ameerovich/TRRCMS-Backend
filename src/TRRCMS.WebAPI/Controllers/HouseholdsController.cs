using MediatR;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.Households.Commands.CreateHousehold;
using TRRCMS.Application.Households.Dtos;
using TRRCMS.Application.Households.Queries.GetAllHouseholds;
using TRRCMS.Application.Households.Queries.GetHousehold;

namespace TRRCMS.WebAPI.Controllers;

/// <summary>
/// API Controller for Household operations
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class HouseholdsController : ControllerBase
{
    private readonly IMediator _mediator;

    public HouseholdsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create a new household
    /// </summary>
    /// <param name="command">Household creation data</param>
    /// <returns>Created household</returns>
    [HttpPost]
    [ProducesResponseType(typeof(HouseholdDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<HouseholdDto>> CreateHousehold([FromBody] CreateHouseholdCommand command)
    {
        try
        {
            var household = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetHousehold), new { id = household.Id }, household);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get household by ID
    /// </summary>
    /// <param name="id">Household ID</param>
    /// <returns>Household details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(HouseholdDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HouseholdDto>> GetHousehold(Guid id)
    {
        var query = new GetHouseholdQuery(id);
        var household = await _mediator.Send(query);

        if (household == null)
        {
            return NotFound(new { error = $"Household with ID {id} not found" });
        }

        return Ok(household);
    }

    /// <summary>
    /// Get all households
    /// </summary>
    /// <returns>List of all households</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<HouseholdDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<HouseholdDto>>> GetAllHouseholds()
    {
        var query = new GetAllHouseholdsQuery();
        var households = await _mediator.Send(query);
        return Ok(households);
    }
}
