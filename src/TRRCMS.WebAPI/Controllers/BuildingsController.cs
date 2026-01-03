using MediatR;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.Buildings.Commands.CreateBuilding;
using TRRCMS.Application.Buildings.Dtos;
using TRRCMS.Application.Buildings.Queries.GetAllBuildings;
using TRRCMS.Application.Buildings.Queries.GetBuilding;

namespace TRRCMS.WebAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class BuildingsController : ControllerBase
{
    private readonly IMediator _mediator;


    public BuildingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create a new building
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> CreateBuilding(CreateBuildingCommand command)
    {
        try
        {
            var buildingId = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetBuilding), new { id = buildingId }, buildingId);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get building by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BuildingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BuildingDto>> GetBuilding(Guid id)
    {
        var query = new GetBuildingQuery { Id = id };
        var building = await _mediator.Send(query);

        if (building == null)
            return NotFound();

        return Ok(building);
    }

    /// <summary>
    /// Get all buildings
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<BuildingDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<BuildingDto>>> GetAllBuildings()
    {
        var query = new GetAllBuildingsQuery();
        var buildings = await _mediator.Send(query);
        return Ok(buildings);
    }
}