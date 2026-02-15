using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.Vocabularies.Dtos;
using TRRCMS.Application.Vocabularies.Queries.GetAllVocabularies;

namespace TRRCMS.WebAPI.Controllers;

/// <summary>
/// Vocabulary API controller.
/// Provides controlled vocabulary data for client dropdown population.
/// Public endpoint — clients fetch on startup and cache locally.
///
/// **Endpoints:**
/// | Method | Path | Description |
/// |--------|------|-------------|
/// | GET | /api/v1/vocabularies | Get all vocabularies (optionally filtered by category) |
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class VocabulariesController : ControllerBase
{
    private readonly IMediator _mediator;

    public VocabulariesController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>
    /// Get all current vocabularies with bilingual labels.
    /// </summary>
    /// <remarks>
    /// Returns all active vocabulary definitions with their values.
    /// Each value includes an integer code and Arabic/English labels.
    /// Clients should cache this data and use integer codes for all API requests.
    ///
    /// **Authentication**: Not required (public endpoint)
    ///
    /// **Example Response:**
    /// ```json
    /// [
    ///   {
    ///     "vocabularyName": "gender",
    ///     "displayNameArabic": "الجنس",
    ///     "displayNameEnglish": "Gender",
    ///     "version": "1.0.0",
    ///     "category": "Demographics",
    ///     "values": [
    ///       { "code": 1, "labelArabic": "ذكر", "labelEnglish": "Male", "displayOrder": 0 },
    ///       { "code": 2, "labelArabic": "أنثى", "labelEnglish": "Female", "displayOrder": 1 }
    ///     ]
    ///   }
    /// ]
    /// ```
    /// </remarks>
    /// <param name="category">Optional category filter (e.g., "Demographics", "Property", "Legal", "Claims", "Survey", "Operations")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of vocabularies with bilingual values</returns>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<VocabularyDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<VocabularyDto>>> GetVocabularies(
        [FromQuery] string? category = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllVocabulariesQuery { Category = category };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}
