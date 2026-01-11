using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.Auth.Commands.Seed;

namespace TRRCMS.WebAPI.Controllers;

/// <summary>
/// Seed data controller for development and testing
/// DISABLE OR REMOVE IN PRODUCTION!
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class SeedController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SeedController> _logger;

    public SeedController(
        IMediator mediator,
        IConfiguration configuration,
        ILogger<SeedController> logger)
    {
        _mediator = mediator;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Seed initial test users for development
    /// </summary>
    /// <param name="forceReseed">If true, deletes existing test users and recreates them</param>
    /// <returns>Seed result with created users and their passwords</returns>
    [HttpPost("users")]
    [AllowAnonymous] // Allow anonymous access for initial setup
    [ProducesResponseType(typeof(SeedResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SeedResult>> SeedUsers([FromQuery] bool forceReseed = false)
    {
        // SECURITY: Only allow seeding in Development environment
        var environment = _configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT");
        if (environment != "Development")
        {
            _logger.LogWarning("Seed endpoint called in non-development environment: {Environment}", environment);
            return Forbid();
        }

        try
        {
            _logger.LogInformation("Seeding test users (ForceReseed: {ForceReseed})", forceReseed);

            var command = new SeedCommand(forceReseed);
            var result = await _mediator.Send(command);

            _logger.LogInformation("Seed completed: {Message}", result.Message);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user seeding");
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred during seeding.",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Get information about available seed operations
    /// </summary>
    [HttpGet("info")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<object> GetSeedInfo()
    {
        var environment = _configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT");

        return Ok(new
        {
            environment,
            seedingEnabled = environment == "Development",
            message = environment == "Development"
                ? "Seeding is enabled. Use POST /api/seed/users to create test users."
                : "Seeding is disabled in non-development environments.",
            testUsers = new[]
            {
                new { username = "admin", role = "Administrator", password = "Admin@123" },
                new { username = "datamanager", role = "DataManager", password = "Data@123" },
                new { username = "clerk", role = "OfficeClerk", password = "Clerk@123" },
                new { username = "collector", role = "FieldCollector", password = "Field@123" },
                new { username = "supervisor", role = "FieldSupervisor", password = "Super@123" },
                new { username = "analyst", role = "Analyst", password = "Analyst@123" }
            }
        });
    }
}