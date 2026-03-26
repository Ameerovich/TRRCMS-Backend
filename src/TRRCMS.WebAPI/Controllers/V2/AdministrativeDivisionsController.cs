using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.AdministrativeDivisions.Dtos;
using TRRCMS.Application.AdministrativeDivisions.Queries.GetCommunities;
using TRRCMS.Application.AdministrativeDivisions.Queries.GetDistricts;
using TRRCMS.Application.AdministrativeDivisions.Queries.GetGovernorates;
using TRRCMS.Application.AdministrativeDivisions.Queries.GetSubDistricts;
using TRRCMS.Application.Common.Models;

namespace TRRCMS.WebAPI.Controllers.V2;

/// <summary>
/// Administrative divisions v2 — list endpoints wrapped in ListResponse.
/// </summary>
[Route("api/v2/administrative-divisions")]
[ApiController]
[Authorize]
[Produces("application/json")]
public class AdministrativeDivisionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdministrativeDivisionsController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Get all governorates.
    /// </summary>
    [HttpGet("governorates")]
    [Authorize(Policy = "CanViewAllBuildings")]
    [ProducesResponseType(typeof(ListResponse<GovernorateDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ListResponse<GovernorateDto>>> GetGovernorates(
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetGovernoratesQuery(), cancellationToken);
        return Ok(ListResponse<GovernorateDto>.From(result));
    }

    /// <summary>
    /// Get districts, optionally filtered by governorate.
    /// </summary>
    [HttpGet("districts")]
    [Authorize(Policy = "CanViewAllBuildings")]
    [ProducesResponseType(typeof(ListResponse<DistrictDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ListResponse<DistrictDto>>> GetDistricts(
        [FromQuery] string? governorateCode,
        CancellationToken cancellationToken = default)
    {
        var query = new GetDistrictsQuery { GovernorateCode = governorateCode };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(ListResponse<DistrictDto>.From(result));
    }

    /// <summary>
    /// Get sub-districts, optionally filtered by governorate and district.
    /// </summary>
    [HttpGet("sub-districts")]
    [Authorize(Policy = "CanViewAllBuildings")]
    [ProducesResponseType(typeof(ListResponse<SubDistrictDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ListResponse<SubDistrictDto>>> GetSubDistricts(
        [FromQuery] string? governorateCode,
        [FromQuery] string? districtCode,
        CancellationToken cancellationToken = default)
    {
        var query = new GetSubDistrictsQuery
        {
            GovernorateCode = governorateCode,
            DistrictCode = districtCode
        };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(ListResponse<SubDistrictDto>.From(result));
    }

    /// <summary>
    /// Get communities, optionally filtered by governorate, district, and sub-district.
    /// </summary>
    [HttpGet("communities")]
    [Authorize(Policy = "CanViewAllBuildings")]
    [ProducesResponseType(typeof(ListResponse<CommunityDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ListResponse<CommunityDto>>> GetCommunities(
        [FromQuery] string? governorateCode,
        [FromQuery] string? districtCode,
        [FromQuery] string? subDistrictCode,
        CancellationToken cancellationToken = default)
    {
        var query = new GetCommunitiesQuery
        {
            GovernorateCode = governorateCode,
            DistrictCode = districtCode,
            SubDistrictCode = subDistrictCode
        };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(ListResponse<CommunityDto>.From(result));
    }
}
