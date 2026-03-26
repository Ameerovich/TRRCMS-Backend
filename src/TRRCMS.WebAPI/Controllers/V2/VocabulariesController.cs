using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.Common.Models;
using TRRCMS.Application.Vocabularies.Dtos;
using TRRCMS.Application.Vocabularies.Queries.ExportVocabularies;
using TRRCMS.Application.Vocabularies.Queries.GetAllVocabularies;
using TRRCMS.Application.Vocabularies.Queries.GetVocabularyVersionHistory;

namespace TRRCMS.WebAPI.Controllers.V2;

/// <summary>
/// Vocabularies API v2 — list endpoints with ListResponse wrapper.
/// </summary>
[Route("api/v2/[controller]")]
[ApiController]
[Produces("application/json")]
public class VocabulariesController : ControllerBase
{
    private readonly IMediator _mediator;
    public VocabulariesController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// List all current vocabularies.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ListResponse<VocabularyDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ListResponse<VocabularyDto>>> GetVocabularies(
        [FromQuery] string? category = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllVocabulariesQuery { Category = category };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(ListResponse<VocabularyDto>.From(result));
    }

    /// <summary>
    /// Get version history for a vocabulary.
    /// </summary>
    [HttpGet("{name}/versions")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ListResponse<VocabularyDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ListResponse<VocabularyDto>>> GetVocabularyVersionHistory(
        string name,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetVocabularyVersionHistoryQuery { VocabularyName = name },
            cancellationToken);
        return Ok(ListResponse<VocabularyDto>.From(result));
    }

    /// <summary>
    /// Export all current vocabularies as a JSON snapshot.
    /// </summary>
    [HttpGet("export")]
    [Authorize(Policy = "CanManageVocabularies")]
    [ProducesResponseType(typeof(ListResponse<VocabularyExportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ListResponse<VocabularyExportDto>>> ExportVocabularies(
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new ExportVocabulariesQuery(), cancellationToken);
        return Ok(ListResponse<VocabularyExportDto>.From(result));
    }
}
