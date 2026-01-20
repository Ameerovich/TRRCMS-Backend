using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.PropertyUnits.Dtos;
using TRRCMS.Application.Surveys.Queries.GetPropertyUnitsForSurvey;
using TRRCMS.Application.Surveys.Commands.CreateFieldSurvey;
using TRRCMS.Application.Surveys.Commands.CreatePropertyUnitInSurvey;
using TRRCMS.Application.Surveys.Commands.LinkPropertyUnitToSurvey;
using TRRCMS.Application.Surveys.Commands.SaveDraftSurvey;
using TRRCMS.Application.Surveys.Commands.UpdatePropertyUnitInSurvey;
using TRRCMS.Application.Surveys.Dtos;
using TRRCMS.Application.Surveys.Queries.GetDraftSurvey;

namespace TRRCMS.WebAPI.Controllers;

/// <summary>
/// Surveys controller for field and office survey operations
/// Supports UC-001 (Field Survey) and UC-004 (Office Survey)
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class SurveysController : ControllerBase
{
    private readonly IMediator _mediator;

    public SurveysController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>
    /// Create new field survey
    /// Corresponds to UC-001 Stage 1: Building Identification
    /// </summary>
    /// <param name="command">Field survey creation data</param>
    /// <returns>Created survey with reference code</returns>
    [HttpPost("field")]
    [Authorize(Policy = "CanCreateSurveys")]
    [ProducesResponseType(typeof(SurveyDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SurveyDto>> CreateFieldSurvey(
        [FromBody] CreateFieldSurveyCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetSurvey), new { id = result.Id }, result);
    }

    /// <summary>
    /// Save survey progress as draft
    /// Corresponds to UC-002: Save draft and exit safely
    /// </summary>
    /// <param name="id">Survey ID</param>
    /// <param name="command">Draft updates</param>
    /// <returns>Updated survey</returns>
    [HttpPut("{id}/draft")]
    [Authorize(Policy = "CanEditOwnSurveys")]
    [ProducesResponseType(typeof(SurveyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SurveyDto>> SaveDraft(
        Guid id,
        [FromBody] SaveDraftSurveyCommand command)
    {
        command.SurveyId = id;
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Get survey by ID
    /// Corresponds to UC-002: Resume draft survey
    /// </summary>
    /// <param name="id">Survey ID</param>
    /// <returns>Survey details</returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "CanViewOwnSurveys")]
    [ProducesResponseType(typeof(SurveyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SurveyDto>> GetSurvey(Guid id)
    {
        var query = new GetDraftSurveyQuery { SurveyId = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }
    // ==================== PROPERTY UNIT MANAGEMENT ====================

    /// <summary>
    /// Get all property units for survey's building
    /// Corresponds to UC-001 Stage 2: View available property units
    /// </summary>
    /// <param name="surveyId">Survey ID</param>
    /// <returns>List of property units in the building</returns>
    [HttpGet("{surveyId}/property-units")]
    [Authorize(Policy = "CanViewOwnSurveys")]
    [ProducesResponseType(typeof(List<PropertyUnitDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<PropertyUnitDto>>> GetPropertyUnitsForSurvey(Guid surveyId)
    {
        var query = new GetPropertyUnitsForSurveyQuery { SurveyId = surveyId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Create new property unit in survey context
    /// Corresponds to UC-001 Stage 2: Create new property unit during survey
    /// </summary>
    /// <param name="surveyId">Survey ID</param>
    /// <param name="command">Property unit creation data</param>
    /// <returns>Created property unit</returns>
    [HttpPost("{surveyId}/property-units")]
    [Authorize(Policy = "CanEditOwnSurveys")]
    [ProducesResponseType(typeof(PropertyUnitDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PropertyUnitDto>> CreatePropertyUnitInSurvey(
        Guid surveyId,
        [FromBody] CreatePropertyUnitInSurveyCommand command)
    {
        command.SurveyId = surveyId;
        var result = await _mediator.Send(command);
        return CreatedAtAction(
            nameof(GetSurvey),
            new { id = surveyId },
            result);
    }

    /// <summary>
    /// Update property unit in survey context
    /// Corresponds to UC-001 Stage 2: Update property unit details during survey
    /// </summary>
    /// <param name="surveyId">Survey ID</param>
    /// <param name="unitId">Property unit ID</param>
    /// <param name="command">Property unit update data</param>
    /// <returns>Updated property unit</returns>
    [HttpPut("{surveyId}/property-units/{unitId}")]
    [Authorize(Policy = "CanEditOwnSurveys")]
    [ProducesResponseType(typeof(PropertyUnitDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PropertyUnitDto>> UpdatePropertyUnitInSurvey(
        Guid surveyId,
        Guid unitId,
        [FromBody] UpdatePropertyUnitInSurveyCommand command)
    {
        command.SurveyId = surveyId;
        command.PropertyUnitId = unitId;
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Link existing property unit to survey
    /// Corresponds to UC-001 Stage 2: Select existing property unit
    /// </summary>
    /// <param name="surveyId">Survey ID</param>
    /// <param name="command">Link command with property unit ID</param>
    /// <returns>Updated survey with linked property unit</returns>
    [HttpPost("{surveyId}/link-property-unit")]
    [Authorize(Policy = "CanEditOwnSurveys")]
    [ProducesResponseType(typeof(SurveyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SurveyDto>> LinkPropertyUnitToSurvey(
        Guid surveyId,
        [FromBody] LinkPropertyUnitToSurveyCommand command)
    {
        command.SurveyId = surveyId;
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}