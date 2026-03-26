using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.Buildings.Dtos;
using TRRCMS.Application.Buildings.Queries.GetBuildingDocumentsByBuilding;
using TRRCMS.Application.Common.Models;

namespace TRRCMS.WebAPI.Controllers.V2;

/// <summary>
/// Building documents v2 — list endpoints wrapped in ListResponse.
/// </summary>
[Route("api/v2/building-documents")]
[ApiController]
[Authorize]
[Produces("application/json")]
public class BuildingDocumentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public BuildingDocumentsController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Get all building documents for a specific building.
    /// </summary>
    [HttpGet("by-building/{buildingId:guid}")]
    [Authorize(Policy = "CanViewAllBuildings")]
    [ProducesResponseType(typeof(ListResponse<BuildingDocumentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ListResponse<BuildingDocumentDto>>> GetByBuildingId(Guid buildingId)
    {
        var result = await _mediator.Send(new GetBuildingDocumentsByBuildingQuery(buildingId));
        return Ok(ListResponse<BuildingDocumentDto>.From(result));
    }
}
