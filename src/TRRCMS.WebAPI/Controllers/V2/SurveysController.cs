using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.Common.Models;
using TRRCMS.Application.Evidences.Dtos;
using TRRCMS.Application.Households.Dtos;
using TRRCMS.Application.PersonPropertyRelations.Dtos;
using TRRCMS.Application.Persons.Dtos;
using TRRCMS.Application.PropertyUnits.Dtos;
using TRRCMS.Application.Surveys.Dtos;
using TRRCMS.Application.Surveys.Queries.GetEvidencesByRelation;
using TRRCMS.Application.Surveys.Queries.GetHouseholdPersons;
using TRRCMS.Application.Surveys.Queries.GetHouseholdsForSurvey;
using TRRCMS.Application.Surveys.Queries.GetOfficeDraftSurveys;
using TRRCMS.Application.Surveys.Queries.GetPropertyUnitsForSurvey;
using TRRCMS.Application.Surveys.Queries.GetRelationsForPropertyUnitInSurvey;
using TRRCMS.Application.Surveys.Queries.GetSurveyEvidence;
using TRRCMS.Domain.Enums;

namespace TRRCMS.WebAPI.Controllers.V2;

/// <summary>
/// Surveys API v2 — list endpoints with ListResponse wrapper.
/// </summary>
[Route("api/v2/[controller]")]
[ApiController]
[Authorize]
[Produces("application/json")]
public class SurveysController : ControllerBase
{
    private readonly IMediator _mediator;
    public SurveysController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Get draft office surveys for the current user.
    /// </summary>
    [HttpGet("office/drafts")]
    [Authorize(Policy = "CanViewOwnSurveys")]
    [ProducesResponseType(typeof(ListResponse<SurveyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ListResponse<SurveyDto>>> GetOfficeDraftSurveys()
    {
        var query = new GetOfficeDraftSurveysQuery();
        var result = await _mediator.Send(query);
        return Ok(ListResponse<SurveyDto>.From(result));
    }

    /// <summary>
    /// Get property units for a survey.
    /// </summary>
    [HttpGet("{surveyId}/property-units")]
    [Authorize(Policy = "CanViewOwnSurveys")]
    [ProducesResponseType(typeof(ListResponse<PropertyUnitDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ListResponse<PropertyUnitDto>>> GetPropertyUnitsForSurvey(Guid surveyId)
    {
        var query = new GetPropertyUnitsForSurveyQuery { SurveyId = surveyId };
        var result = await _mediator.Send(query);
        return Ok(ListResponse<PropertyUnitDto>.From(result));
    }

    /// <summary>
    /// Get households for a survey.
    /// </summary>
    [HttpGet("{surveyId}/households")]
    [Authorize(Policy = "CanViewOwnSurveys")]
    [ProducesResponseType(typeof(ListResponse<HouseholdDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ListResponse<HouseholdDto>>> GetHouseholdsForSurvey(Guid surveyId)
    {
        var query = new GetHouseholdsForSurveyQuery { SurveyId = surveyId };
        var result = await _mediator.Send(query);
        return Ok(ListResponse<HouseholdDto>.From(result));
    }

    /// <summary>
    /// Get persons in a household within a survey.
    /// </summary>
    [HttpGet("{surveyId}/households/{householdId}/persons")]
    [Authorize(Policy = "CanViewOwnSurveys")]
    [ProducesResponseType(typeof(ListResponse<PersonDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ListResponse<PersonDto>>> GetHouseholdPersons(
        Guid surveyId,
        Guid householdId)
    {
        var query = new GetHouseholdPersonsQuery
        {
            SurveyId = surveyId,
            HouseholdId = householdId
        };
        var result = await _mediator.Send(query);
        return Ok(ListResponse<PersonDto>.From(result));
    }

    /// <summary>
    /// Get person-property relations for a property unit in a survey.
    /// </summary>
    [HttpGet("{surveyId}/property-units/{unitId}/relations")]
    [Authorize(Policy = "CanViewOwnSurveys")]
    [ProducesResponseType(typeof(ListResponse<PersonPropertyRelationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ListResponse<PersonPropertyRelationDto>>> GetRelationsForPropertyUnit(
        Guid surveyId,
        Guid unitId)
    {
        var query = new GetRelationsForPropertyUnitInSurveyQuery
        {
            SurveyId = surveyId,
            PropertyUnitId = unitId
        };
        var result = await _mediator.Send(query);
        return Ok(ListResponse<PersonPropertyRelationDto>.From(result));
    }

    /// <summary>
    /// Get all evidence for a survey with optional filters.
    /// </summary>
    [HttpGet("{surveyId}/evidence")]
    [Authorize(Policy = "CanViewOwnSurveys")]
    [ProducesResponseType(typeof(ListResponse<EvidenceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ListResponse<EvidenceDto>>> GetSurveyEvidence(
        Guid surveyId,
        [FromQuery] string? evidenceType = null,
        [FromQuery] Guid? personId = null)
    {
        EvidenceType? parsedType = null;
        if (!string.IsNullOrEmpty(evidenceType))
        {
            if (int.TryParse(evidenceType, out var intValue) && Enum.IsDefined(typeof(EvidenceType), intValue))
            {
                parsedType = (EvidenceType)intValue;
            }
            else if (Enum.TryParse<EvidenceType>(evidenceType, ignoreCase: true, out var namedValue))
            {
                parsedType = namedValue;
            }
        }

        var query = new GetSurveyEvidenceQuery
        {
            SurveyId = surveyId,
            EvidenceType = parsedType,
            PersonId = personId
        };
        var result = await _mediator.Send(query);
        return Ok(ListResponse<EvidenceDto>.From(result));
    }

    /// <summary>
    /// Get evidences linked to a specific relation in a survey.
    /// </summary>
    [HttpGet("{surveyId}/relations/{relationId}/evidences")]
    [Authorize(Policy = "CanViewOwnSurveys")]
    [ProducesResponseType(typeof(ListResponse<EvidenceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ListResponse<EvidenceDto>>> GetEvidencesByRelation(
        Guid surveyId,
        Guid relationId,
        [FromQuery] EvidenceType? evidenceType = null,
        [FromQuery] bool onlyCurrentVersions = true)
    {
        var query = new GetEvidencesByRelationQuery
        {
            SurveyId = surveyId,
            RelationId = relationId,
            EvidenceType = evidenceType,
            OnlyCurrentVersions = onlyCurrentVersions
        };
        var result = await _mediator.Send(query);
        return Ok(ListResponse<EvidenceDto>.From(result));
    }
}
