using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.Common.Models;
using TRRCMS.Application.Neighborhoods.Dtos;
using TRRCMS.Application.Neighborhoods.Queries.GetNeighborhoods;

namespace TRRCMS.WebAPI.Controllers.V2;

/// <summary>
/// Neighborhoods v2 — list endpoints wrapped in ListResponse.
/// </summary>
[Route("api/v2/[controller]")]
[ApiController]
[Authorize]
[Produces("application/json")]
public class NeighborhoodsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NeighborhoodsController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Get all neighborhoods, optionally filtered by parent hierarchy.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "CanViewAllBuildings")]
    [ProducesResponseType(typeof(ListResponse<NeighborhoodDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ListResponse<NeighborhoodDto>>> GetNeighborhoods(
        [FromQuery] string? governorateCode,
        [FromQuery] string? districtCode,
        [FromQuery] string? subDistrictCode,
        [FromQuery] string? communityCode,
        [FromQuery] string? governoratePCode,
        [FromQuery] string? districtPCode,
        [FromQuery] string? subDistrictPCode,
        [FromQuery] string? communityPCode,
        CancellationToken cancellationToken = default)
    {
        var query = new GetNeighborhoodsQuery
        {
            GovernorateCode = governorateCode,
            DistrictCode = districtCode,
            SubDistrictCode = subDistrictCode,
            CommunityCode = communityCode,
            GovernoratePCode = governoratePCode,
            DistrictPCode = districtPCode,
            SubDistrictPCode = subDistrictPCode,
            CommunityPCode = communityPCode
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(ListResponse<NeighborhoodDto>.From(result));
    }

    /// <summary>
    /// Get neighborhoods visible in map viewport (by bounding box).
    /// </summary>
    [HttpGet("by-bounds")]
    [Authorize(Policy = "CanViewAllBuildings")]
    [ProducesResponseType(typeof(ListResponse<NeighborhoodDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ListResponse<NeighborhoodDto>>> GetNeighborhoodsByBounds(
        [FromQuery] decimal swLat,
        [FromQuery] decimal swLng,
        [FromQuery] decimal neLat,
        [FromQuery] decimal neLng,
        CancellationToken cancellationToken = default)
    {
        if (swLat >= neLat || swLng >= neLng)
        {
            return BadRequest(new { message = "Invalid bounding box: SW must be less than NE" });
        }

        var repository = HttpContext.RequestServices
            .GetRequiredService<Application.Common.Interfaces.INeighborhoodRepository>();

        var neighborhoods = await repository.GetInBoundingBoxAsync(
            swLat, swLng, neLat, neLng, cancellationToken);

        var dtos = neighborhoods.Select(n => new NeighborhoodDto
        {
            Id = n.Id,
            GovernorateCode = n.GovernorateCode,
            DistrictCode = n.DistrictCode,
            SubDistrictCode = n.SubDistrictCode,
            CommunityCode = n.CommunityCode,
            NeighborhoodCode = n.NeighborhoodCode,
            FullCode = n.FullCode,
            NameArabic = n.NameArabic,
            NameEnglish = n.NameEnglish,
            CenterLatitude = n.CenterLatitude,
            CenterLongitude = n.CenterLongitude,
            BoundaryWkt = n.BoundaryWkt,
            AreaSquareKm = n.AreaSquareKm,
            ZoomLevel = n.ZoomLevel,
            IsActive = n.IsActive,
            // OCHA P-Codes (Community uses synthetic fallback in this hot path; the
            // single-row by-codes endpoint resolves the real Community.ExternalPCode).
            GovernoratePCode = Application.Common.OchaPCodeConverter.ToGovPCode(n.GovernorateCode),
            DistrictPCode = Application.Common.OchaPCodeConverter.ToDistrictPCode(n.GovernorateCode, n.DistrictCode),
            SubDistrictPCode = Application.Common.OchaPCodeConverter.ToSubDistrictPCode(n.GovernorateCode, n.DistrictCode, n.SubDistrictCode),
            CommunityPCode = Application.Common.OchaPCodeConverter.ToCommunityPCode(null, n.CommunityCode),
            PCode = Application.Common.OchaPCodeConverter.ToNeighborhoodPCode(n.NeighborhoodCode)
        }).ToList();

        return Ok(ListResponse<NeighborhoodDto>.From(dtos));
    }
}
