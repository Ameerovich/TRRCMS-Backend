using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.Dashboard.Dtos;
using TRRCMS.Application.Dashboard.Queries.GetDashboardSummary;
using TRRCMS.Application.Dashboard.Queries.GetDashboardTrends;
using TRRCMS.Application.Dashboard.Queries.GetGeographicDashboard;
using TRRCMS.Application.Dashboard.Queries.GetPersonnelDashboard;
using TRRCMS.Application.Dashboard.Queries.GetRegistrationCoverage;

namespace TRRCMS.WebAPI.Controllers;

/// <summary>
/// Dashboard statistics API controller
/// </summary>
/// <remarks>
/// Provides aggregated data for the desktop application's dashboard page.
/// All endpoints are read-only GET requests returning pre-aggregated statistics.
///
/// FR-D-12: Dashboard Statistics.
///
/// **Permission:** Dashboard_View (8500) - CanViewDashboard
///
/// **Authorized roles:** Administrator, DataManager, FieldSupervisor, Analyst
/// </remarks>
[ApiController]
[Route("api/v1/dashboard")]
[Authorize(Policy = "CanViewDashboard")]
[Produces("application/json")]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>
    /// Get aggregated dashboard summary
    /// </summary>
    /// <remarks>
    /// Returns top-level counts for claims, surveys, import pipeline, and buildings.
    /// Use this as the main landing page data source.
    ///
    /// **Response example:**
    /// ```json
    /// {
    ///   "claims": {
    ///     "totalClaims": 142,
    ///     "byStatus": { "Open": 95, "Closed": 47 },
    ///     "byLifecycleStage": { "Submitted": 30, "UnderReview": 25 },
    ///     "overdueCount": 3,
    ///     "withConflictsCount": 5,
    ///     "awaitingDocumentsCount": 12,
    ///     "pendingVerificationCount": 8
    ///   },
    ///   "surveys": {
    ///     "totalSurveys": 310,
    ///     "byStatus": { "Draft": 45, "Completed": 250 },
    ///     "fieldSurveyCount": 200,
    ///     "officeSurveyCount": 110,
    ///     "completedLast7Days": 18,
    ///     "completedLast30Days": 62
    ///   },
    ///   "imports": {
    ///     "totalPackages": 25,
    ///     "byStatus": { "Pending": 2, "Completed": 20 },
    ///     "activeCount": 2,
    ///     "withUnresolvedConflicts": 1,
    ///     "totalSurveysImported": 180,
    ///     "totalBuildingsImported": 90,
    ///     "totalPersonsImported": 350
    ///   },
    ///   "buildings": {
    ///     "totalBuildings": 500,
    ///     "totalPropertyUnits": 1200,
    ///     "byStatus": { "Occupied": 300, "Damaged": 80 },
    ///     "averageUnitsPerBuilding": 2.4
    ///   },
    ///   "generatedAtUtc": "2026-03-11T14:30:00Z"
    /// }
    /// ```
    /// </remarks>
    /// <response code="200">Dashboard summary returned successfully.</response>
    /// <response code="401">Not authenticated. Login required.</response>
    /// <response code="403">Not authorized. User lacks CanViewDashboard permission.</response>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(DashboardSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary()
    {
        var result = await _mediator.Send(new GetDashboardSummaryQuery());
        return Ok(result);
    }

    /// <summary>
    /// Get monthly creation trends
    /// </summary>
    /// <remarks>
    /// Returns monthly time-series for claims, surveys, buildings, persons, and imports.
    /// Each series contains one entry per month with a count of records created that month.
    /// Use for line charts or bar charts.
    ///
    /// **Query parameters:**
    /// - `from` (optional): Start date filter (inclusive). ISO 8601 format.
    /// - `to` (optional): End date filter (inclusive). ISO 8601 format.
    /// - If both omitted, returns all-time data.
    ///
    /// **Example:** `GET /api/v1/dashboard/trends?from=2026-01-01&amp;to=2026-03-31`
    ///
    /// **Response example:**
    /// ```json
    /// {
    ///   "claims": [
    ///     { "year": 2026, "month": 1, "label": "2026-01", "count": 25 },
    ///     { "year": 2026, "month": 2, "label": "2026-02", "count": 38 }
    ///   ],
    ///   "surveys": [
    ///     { "year": 2026, "month": 1, "label": "2026-01", "count": 50 }
    ///   ],
    ///   "buildings": [ ... ],
    ///   "persons": [ ... ],
    ///   "imports": [ ... ],
    ///   "generatedAtUtc": "2026-03-11T14:30:00Z"
    /// }
    /// ```
    /// </remarks>
    /// <param name="from">Optional start date filter (inclusive)</param>
    /// <param name="to">Optional end date filter (inclusive)</param>
    /// <response code="200">Monthly trends returned successfully.</response>
    /// <response code="401">Not authenticated. Login required.</response>
    /// <response code="403">Not authorized. User lacks CanViewDashboard permission.</response>
    [HttpGet("trends")]
    [ProducesResponseType(typeof(DashboardTrendsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<DashboardTrendsDto>> GetTrends(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var result = await _mediator.Send(new GetDashboardTrendsQuery(from, to));
        return Ok(result);
    }

    /// <summary>
    /// Get registration coverage statistics
    /// </summary>
    /// <remarks>
    /// Returns a full snapshot of registration data completeness:
    /// person demographics, person-property relations, claim status (Open/Closed),
    /// claim types, document completeness, and evidence counts.
    ///
    /// **Response example:**
    /// ```json
    /// {
    ///   "totalPersons": 850,
    ///   "totalHouseholds": 220,
    ///   "personsByGender": { "Male": 440, "Female": 410 },
    ///   "personsWithNationalId": 620,
    ///   "personsWithIdentificationDocument": 0,
    ///   "totalPersonPropertyRelations": 380,
    ///   "relationsByType": { "Owner": 180, "Occupant": 90, "Tenant": 60, "Heir": 30 },
    ///   "relationsWithEvidence": 250,
    ///   "claimsOpen": 95,
    ///   "claimsClosed": 47,
    ///   "claimsByType": { "OwnershipClaim": 90, "OccupancyClaim": 52 },
    ///   "claimsWithAllDocuments": 80,
    ///   "claimsMissingDocuments": 62,
    ///   "totalEvidenceItems": 1200,
    ///   "generatedAtUtc": "2026-03-11T14:30:00Z"
    /// }
    /// ```
    /// </remarks>
    /// <response code="200">Registration coverage returned successfully.</response>
    /// <response code="401">Not authenticated. Login required.</response>
    /// <response code="403">Not authorized. User lacks CanViewDashboard permission.</response>
    [HttpGet("registration-coverage")]
    [ProducesResponseType(typeof(RegistrationCoverageDashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<RegistrationCoverageDashboardDto>> GetRegistrationCoverage()
    {
        var result = await _mediator.Send(new GetRegistrationCoverageQuery());
        return Ok(result);
    }

    /// <summary>
    /// Get geographic coverage by neighborhood
    /// </summary>
    /// <remarks>
    /// Returns building and property unit counts per neighborhood.
    /// Use for coverage tables or choropleth maps.
    ///
    /// **Response example:**
    /// ```json
    /// {
    ///   "totalNeighborhoods": 45,
    ///   "neighborhoodsWithBuildings": 32,
    ///   "neighborhoods": [
    ///     {
    ///       "code": "SY-AL-01-001-002-N001",
    ///       "nameArabic": "حي السريان",
    ///       "nameEnglish": "Al-Suryan",
    ///       "buildingCount": 85,
    ///       "propertyUnitCount": 210
    ///     }
    ///   ],
    ///   "generatedAtUtc": "2026-03-11T14:30:00Z"
    /// }
    /// ```
    /// </remarks>
    /// <response code="200">Geographic coverage returned successfully.</response>
    /// <response code="401">Not authenticated. Login required.</response>
    /// <response code="403">Not authorized. User lacks CanViewDashboard permission.</response>
    [HttpGet("geographic")]
    [ProducesResponseType(typeof(GeographicDashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<GeographicDashboardDto>> GetGeographic()
    {
        var result = await _mediator.Send(new GetGeographicDashboardQuery());
        return Ok(result);
    }

    /// <summary>
    /// Get personnel workload distribution
    /// </summary>
    /// <remarks>
    /// Returns survey and building assignment workloads for field collectors and office clerks.
    /// Building assignment fields (assignedBuildings, completedBuildings) are only relevant
    /// for field collectors and will be 0 for office clerks.
    ///
    /// **Query parameters:**
    /// - `from` (optional): Start date filter (inclusive). ISO 8601 format.
    /// - `to` (optional): End date filter (inclusive). ISO 8601 format.
    /// - If both omitted, returns all-time data.
    ///
    /// **Example:** `GET /api/v1/dashboard/personnel?from=2026-01-01&amp;to=2026-03-31`
    ///
    /// **Response example:**
    /// ```json
    /// {
    ///   "fieldCollectors": [
    ///     {
    ///       "userId": "a1b2c3d4-...",
    ///       "username": "fc_ahmad",
    ///       "fullName": "أحمد محمد",
    ///       "surveysCompleted": 45,
    ///       "surveysDraft": 3,
    ///       "totalSurveys": 48,
    ///       "assignedBuildings": 60,
    ///       "completedBuildings": 45
    ///     }
    ///   ],
    ///   "officeClerks": [
    ///     {
    ///       "userId": "e5f6g7h8-...",
    ///       "username": "oc_fatima",
    ///       "fullName": "فاطمة علي",
    ///       "surveysCompleted": 30,
    ///       "surveysDraft": 5,
    ///       "totalSurveys": 35,
    ///       "assignedBuildings": 0,
    ///       "completedBuildings": 0
    ///     }
    ///   ],
    ///   "generatedAtUtc": "2026-03-11T14:30:00Z"
    /// }
    /// ```
    /// </remarks>
    /// <param name="from">Optional start date filter (inclusive)</param>
    /// <param name="to">Optional end date filter (inclusive)</param>
    /// <response code="200">Personnel workload returned successfully.</response>
    /// <response code="401">Not authenticated. Login required.</response>
    /// <response code="403">Not authorized. User lacks CanViewDashboard permission.</response>
    [HttpGet("personnel")]
    [ProducesResponseType(typeof(PersonnelDashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PersonnelDashboardDto>> GetPersonnel(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var result = await _mediator.Send(new GetPersonnelDashboardQuery(from, to));
        return Ok(result);
    }
}
