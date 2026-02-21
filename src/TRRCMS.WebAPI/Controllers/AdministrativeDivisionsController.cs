using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.AdministrativeDivisions.Commands.ImportAdministrativeHierarchy;
using TRRCMS.Application.AdministrativeDivisions.Dtos;
using TRRCMS.Application.AdministrativeDivisions.Queries.GetCommunities;
using TRRCMS.Application.AdministrativeDivisions.Queries.GetDistricts;
using TRRCMS.Application.AdministrativeDivisions.Queries.GetGovernorates;
using TRRCMS.Application.AdministrativeDivisions.Queries.GetSubDistricts;

namespace TRRCMS.WebAPI.Controllers;

/// <summary>
/// Administrative hierarchy API controller for cascading dropdown filters.
/// Provides governorate → district → sub-district → community hierarchy data.
/// التقسيمات الإدارية
///
/// **Hierarchy Levels:**
/// 1. Governorate (محافظة) - 2-digit code
/// 2. District (منطقة/قضاء) - 2-digit code
/// 3. SubDistrict (ناحية) - 2-digit code
/// 4. Community (قرية/مجتمع) - 3-digit code
///
/// **Endpoints:**
/// | Method | Path | Description |
/// |--------|------|-------------|
/// | GET | /api/v1/administrative-divisions/governorates | List all governorates |
/// | GET | /api/v1/administrative-divisions/districts | List districts (filterable by governorate) |
/// | GET | /api/v1/administrative-divisions/sub-districts | List sub-districts (filterable by governorate/district) |
/// | GET | /api/v1/administrative-divisions/communities | List communities (filterable by hierarchy) |
///
/// **Frontend Integration:**
/// - Use for cascading dropdown filters in building search
/// - Selecting governorate filters available districts
/// - Selecting district filters available sub-districts
/// - Selecting sub-district filters available communities
/// </summary>
[ApiController]
[Route("api/v1/administrative-divisions")]
[Produces("application/json")]
public class AdministrativeDivisionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdministrativeDivisionsController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    // ==================== GOVERNORATES ====================

    /// <summary>
    /// Get all governorates
    /// </summary>
    /// <remarks>
    /// Returns all active governorates (top-level administrative divisions).
    /// Used for populating the first dropdown in cascading filters.
    ///
    /// **Required Permission**: Buildings_View (4000) - CanViewAllBuildings policy
    ///
    /// **Example Request:**
    /// ```
    /// GET /api/v1/administrative-divisions/governorates
    /// ```
    ///
    /// **Example Response:**
    /// ```json
    /// [
    ///   {
    ///     "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///     "code": "01",
    ///     "nameArabic": "حلب",
    ///     "nameEnglish": "Aleppo",
    ///     "isActive": true
    ///   },
    ///   {
    ///     "id": "4fa85f64-5717-4562-b3fc-2c963f66afa7",
    ///     "code": "02",
    ///     "nameArabic": "دمشق",
    ///     "nameEnglish": "Damascus",
    ///     "isActive": true
    ///   }
    /// ]
    /// ```
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of governorates</returns>
    /// <response code="200">Governorates retrieved successfully</response>
    /// <response code="401">Not authenticated</response>
    [HttpGet("governorates")]
    [Authorize(Policy = "CanViewAllBuildings")]
    [ProducesResponseType(typeof(List<GovernorateDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<GovernorateDto>>> GetGovernorates(
        CancellationToken cancellationToken = default)
    {
        var query = new GetGovernoratesQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    // ==================== DISTRICTS ====================

    /// <summary>
    /// Get all districts, optionally filtered by governorate
    /// </summary>
    /// <remarks>
    /// Returns all active districts, optionally filtered by parent governorate.
    /// Used for populating the second dropdown in cascading filters.
    ///
    /// **Required Permission**: Buildings_View (4000) - CanViewAllBuildings policy
    ///
    /// **Example Request - All districts in Aleppo governorate:**
    /// ```
    /// GET /api/v1/administrative-divisions/districts?governorateCode=01
    /// ```
    ///
    /// **Example Response:**
    /// ```json
    /// [
    ///   {
    ///     "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///     "code": "01",
    ///     "governorateCode": "01",
    ///     "nameArabic": "جبل سمعان",
    ///     "nameEnglish": "Mount Simeon",
    ///     "isActive": true
    ///   },
    ///   {
    ///     "id": "4fa85f64-5717-4562-b3fc-2c963f66afa7",
    ///     "code": "02",
    ///     "governorateCode": "01",
    ///     "nameArabic": "منبج",
    ///     "nameEnglish": "Manbij",
    ///     "isActive": true
    ///   }
    /// ]
    /// ```
    /// </remarks>
    /// <param name="governorateCode">Filter by governorate code (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of districts</returns>
    /// <response code="200">Districts retrieved successfully</response>
    /// <response code="401">Not authenticated</response>
    [HttpGet("districts")]
    [Authorize(Policy = "CanViewAllBuildings")]
    [ProducesResponseType(typeof(List<DistrictDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<DistrictDto>>> GetDistricts(
        [FromQuery] string? governorateCode,
        CancellationToken cancellationToken = default)
    {
        var query = new GetDistrictsQuery { GovernorateCode = governorateCode };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    // ==================== SUB-DISTRICTS ====================

    /// <summary>
    /// Get all sub-districts, optionally filtered by governorate and district
    /// </summary>
    /// <remarks>
    /// Returns all active sub-districts, optionally filtered by parent governorate and district.
    /// Used for populating the third dropdown in cascading filters.
    ///
    /// **Required Permission**: Buildings_View (4000) - CanViewAllBuildings policy
    ///
    /// **Example Request - All sub-districts in Aleppo governorate, Mount Simeon district:**
    /// ```
    /// GET /api/v1/administrative-divisions/sub-districts?governorateCode=01&amp;districtCode=01
    /// ```
    ///
    /// **Example Response:**
    /// ```json
    /// [
    ///   {
    ///     "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///     "code": "01",
    ///     "governorateCode": "01",
    ///     "districtCode": "01",
    ///     "nameArabic": "مدينة حلب",
    ///     "nameEnglish": "Aleppo City",
    ///     "isActive": true
    ///   }
    /// ]
    /// ```
    /// </remarks>
    /// <param name="governorateCode">Filter by governorate code (optional)</param>
    /// <param name="districtCode">Filter by district code (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of sub-districts</returns>
    /// <response code="200">Sub-districts retrieved successfully</response>
    /// <response code="401">Not authenticated</response>
    [HttpGet("sub-districts")]
    [Authorize(Policy = "CanViewAllBuildings")]
    [ProducesResponseType(typeof(List<SubDistrictDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<SubDistrictDto>>> GetSubDistricts(
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
        return Ok(result);
    }

    // ==================== COMMUNITIES ====================

    /// <summary>
    /// Get all communities, optionally filtered by governorate, district, and sub-district
    /// </summary>
    /// <remarks>
    /// Returns all active communities, optionally filtered by parent administrative hierarchy.
    /// Used for populating the fourth dropdown in cascading filters.
    ///
    /// **Required Permission**: Buildings_View (4000) - CanViewAllBuildings policy
    ///
    /// **Example Request - All communities in Aleppo/Mount Simeon/Aleppo City:**
    /// ```
    /// GET /api/v1/administrative-divisions/communities?governorateCode=01&amp;districtCode=01&amp;subDistrictCode=01
    /// ```
    ///
    /// **Example Response:**
    /// ```json
    /// [
    ///   {
    ///     "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///     "code": "001",
    ///     "governorateCode": "01",
    ///     "districtCode": "01",
    ///     "subDistrictCode": "01",
    ///     "nameArabic": "القطاع الغربي",
    ///     "nameEnglish": "Western Sector",
    ///     "isActive": true
    ///   }
    /// ]
    /// ```
    /// </remarks>
    /// <param name="governorateCode">Filter by governorate code (optional)</param>
    /// <param name="districtCode">Filter by district code (optional)</param>
    /// <param name="subDistrictCode">Filter by sub-district code (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of communities</returns>
    /// <response code="200">Communities retrieved successfully</response>
    /// <response code="401">Not authenticated</response>
    [HttpGet("communities")]
    [Authorize(Policy = "CanViewAllBuildings")]
    [ProducesResponseType(typeof(List<CommunityDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<CommunityDto>>> GetCommunities(
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
        return Ok(result);
    }

    // ==================== IMPORT ====================

    /// <summary>
    /// Import administrative hierarchy data from JSON
    /// </summary>
    /// <remarks>
    /// Imports governorate → district → sub-district → community hierarchy from JSON.
    /// Optionally generates placeholder neighborhoods (code='001', name='Main Area') for each community.
    ///
    /// **Required Permission**: Admin (role 99) - System Administrator only
    ///
    /// **JSON Format:**
    /// ```json
    /// {
    ///   "governorates": [
    ///     {
    ///       "code": "01",
    ///       "name_ar": "حلب",
    ///       "name_en": "Aleppo",
    ///       "districts": [
    ///         {
    ///           "code": "01",
    ///           "name_ar": "جبل سمعان",
    ///           "name_en": "Mount Simeon",
    ///           "sub_districts": [
    ///             {
    ///               "code": "01",
    ///               "name_ar": "مدينة حلب",
    ///               "name_en": "Aleppo City",
    ///               "communities": [
    ///                 {
    ///                   "code": "001",
    ///                   "name_ar": "القطاع الغربي",
    ///                   "name_en": "Western Sector"
    ///                 }
    ///               ]
    ///             }
    ///           ]
    ///         }
    ///       ]
    ///     }
    ///   ]
    /// }
    /// ```
    ///
    /// **Example Response:**
    /// ```json
    /// {
    ///   "governoratesImported": 14,
    ///   "districtsImported": 65,
    ///   "subDistrictsImported": 97,
    ///   "communitiesImported": 1945,
    ///   "neighborhoodsGenerated": 1945,
    ///   "success": true,
    ///   "message": "Successfully imported...",
    ///   "errors": []
    /// }
    /// ```
    ///
    /// See docs/AdministrativeHierarchy_ImportFormat.md for full format specification.
    /// </remarks>
    /// <param name="command">Import command with JSON content</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Import result with counts and errors</returns>
    /// <response code="200">Import completed (check success field and errors list)</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Not authorized (admin only)</response>
    [HttpPost("import")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ImportAdministrativeHierarchyResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ImportAdministrativeHierarchyResult>> ImportAdministrativeHierarchy(
        [FromBody] ImportAdministrativeHierarchyCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}
