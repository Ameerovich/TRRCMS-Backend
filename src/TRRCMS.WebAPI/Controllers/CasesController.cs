using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.Cases.Commands.SetCaseEditable;
using TRRCMS.Application.Cases.Dtos;
using TRRCMS.Application.Cases.Queries.GetAllCases;
using TRRCMS.Application.Cases.Queries.GetCase;
using TRRCMS.Application.Cases.Queries.GetCaseByPropertyUnit;
using TRRCMS.Application.Common.Models;
using TRRCMS.Domain.Enums;

namespace TRRCMS.WebAPI.Controllers;

/// <summary>
/// Cases management
/// إدارة الحالات
/// </summary>
/// <remarks>
/// A Case aggregates all work done on a PropertyUnit — surveys, claims, and person-property relations.
/// Cases are created automatically when the first survey is created for a PropertyUnit.
/// Cases are closed automatically when an ownership/heir claim is created.
///
/// **CaseLifecycleStatus Values:**
/// | Value | Name | Arabic |
/// |-------|------|--------|
/// | 1 | Open | مفتوحة |
/// | 2 | Closed | مغلقة |
///
/// **Permissions:**
/// - Claims_ViewAll (1000) — view cases
/// - Claims_Transition (1011) — manage case editable flag
/// </remarks>
[ApiController]
[Route("api/v1/cases")]
[Authorize]
[Produces("application/json")]
public class CasesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CasesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all cases (paginated)
    /// عرض جميع الحالات
    /// </summary>
    /// <remarks>
    /// Returns a paginated list of cases with summary information.
    /// Supports two-step filtering: enter a building code to get all cases in that building,
    /// then optionally narrow down to a specific property unit by its identifier.
    ///
    /// **Filters:**
    /// - `buildingCode`: 17-digit building number (with or without dashes). Returns all cases for this building.
    /// - `unitIdentifier`: Property unit identifier within the building (e.g., "Apt 1"). Requires buildingCode or buildingId.
    /// - `status`: 1 = Open, 2 = Closed
    /// - `buildingId`: Filter by building GUID (alternative to buildingCode)
    /// - `page`: Page number (default: 1)
    /// - `pageSize`: Items per page (default: 20, max: 100)
    ///
    /// **Example — All cases in a building:**
    /// ```
    /// GET /api/v1/cases?buildingCode=01010100100100001
    /// ```
    ///
    /// **Example — Specific unit in a building:**
    /// ```
    /// GET /api/v1/cases?buildingCode=01010100100100001&amp;unitIdentifier=Apt 1
    /// ```
    /// </remarks>
    /// <param name="status">Filter by case status (1=Open, 2=Closed)</param>
    /// <param name="buildingId">Filter by building GUID</param>
    /// <param name="buildingCode">Filter by 17-digit building number (with or without dashes)</param>
    /// <param name="unitIdentifier">Filter by property unit identifier within the building</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size (max 100)</param>
    /// <response code="200">List of cases</response>
    /// <response code="401">Not authenticated</response>
    [HttpGet]
    [Authorize(Policy = "CanViewAllClaims")]
    [ProducesResponseType(typeof(ListResponse<CaseSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ListResponse<CaseSummaryDto>>> GetAll(
        [FromQuery] CaseLifecycleStatus? status = null,
        [FromQuery] Guid? buildingId = null,
        [FromQuery] string? buildingCode = null,
        [FromQuery] string? unitIdentifier = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetAllCasesQuery
        {
            Status = status,
            BuildingId = buildingId,
            BuildingCode = buildingCode,
            UnitIdentifier = unitIdentifier,
            Page = page,
            PageSize = pageSize
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get case by ID
    /// عرض تفاصيل الحالة
    /// </summary>
    /// <param name="id">Case ID (GUID)</param>
    /// <response code="200">Case details with related entity summaries</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="404">Case not found</response>
    [HttpGet("{id}")]
    [Authorize(Policy = "CanViewAllClaims")]
    [ProducesResponseType(typeof(CaseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CaseDto>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetCaseQuery(id));
        return Ok(result);
    }

    /// <summary>
    /// Get case by property unit ID
    /// البحث عن حالة حسب الوحدة العقارية
    /// </summary>
    /// <param name="propertyUnitId">Property Unit ID (GUID)</param>
    /// <response code="200">Case details (or null if no case exists for this property unit)</response>
    /// <response code="401">Not authenticated</response>
    [HttpGet("by-property-unit/{propertyUnitId}")]
    [Authorize(Policy = "CanViewAllClaims")]
    [ProducesResponseType(typeof(CaseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CaseDto>> GetByPropertyUnit(Guid propertyUnitId)
    {
        var result = await _mediator.Send(new GetCaseByPropertyUnitQuery(propertyUnitId));
        if (result == null)
            return NoContent();
        return Ok(result);
    }

    /// <summary>
    /// Set case editable flag
    /// تعيين حالة التعديل للحالة
    /// </summary>
    /// <remarks>
    /// Sets whether the case data can be edited.
    ///
    /// **Example request:**
    /// ```json
    /// {
    ///   "isEditable": false
    /// }
    /// ```
    /// </remarks>
    /// <param name="id">Case ID (GUID)</param>
    /// <param name="request">Editable state to set</param>
    /// <response code="204">Case editable flag updated</response>
    /// <response code="401">Not authenticated</response>
    /// <response code="403">Missing required permission</response>
    /// <response code="404">Case not found</response>
    [HttpPut("{id}/editable")]
    [Authorize(Policy = "CanTransitionClaims")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetEditable(Guid id, [FromBody] SetCaseEditableRequest request)
    {
        await _mediator.Send(new SetCaseEditableCommand(id, request.IsEditable));
        return NoContent();
    }
}

public class SetCaseEditableRequest
{
    public bool IsEditable { get; set; }
}
