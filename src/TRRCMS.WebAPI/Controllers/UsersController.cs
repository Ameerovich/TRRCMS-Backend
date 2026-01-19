using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRRCMS.Application.Users.Commands.ActivateUser; // Commands for activating user
using TRRCMS.Application.Users.Commands.CreateUser;
using TRRCMS.Application.Users.Commands.DeactivateUser; // Commands for deactivating user
using TRRCMS.Application.Users.Commands.GrantPermissions; // Grant Permissions command
using TRRCMS.Application.Users.Commands.RevokePermission; // Revoke Permissions command
using TRRCMS.Application.Users.Commands.UnlockUser; // Commands for unlocking user
using TRRCMS.Application.Users.Commands.UpdateUser;
using TRRCMS.Application.Users.Dtos; // User DTOs
using TRRCMS.Application.Users.Queries.GetAllUsers; // Queries for getting users
using TRRCMS.Application.Users.Queries.GetUser;
using TRRCMS.Application.Users.Queries.GetUserAuditLog;
using TRRCMS.Domain.Enums; // Queries to get single user

namespace TRRCMS.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // CREATE User
        [HttpPost]
        [Authorize(Policy = "CanCreateUsers")]
        public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserCommand command)
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetUser), new { id = result.Id }, result);
        }

        // GET User by ID
        [HttpGet("{id}")]
        [Authorize(Policy = "CanViewAllUsers")]
        public async Task<ActionResult<UserDetailDto>> GetUser(Guid id)
        {
            var result = await _mediator.Send(new GetUserQuery { UserId = id });
            return Ok(result);
        }

        // GET All Users (with filters)
        [HttpGet]
        [Authorize(Policy = "CanViewAllUsers")]
        public async Task<ActionResult<GetAllUsersResponse>> GetAllUsers([FromQuery] GetAllUsersQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        // UPDATE User
        [HttpPut("{id}")]
        [Authorize(Policy = "CanEditUsers")]
        public async Task<ActionResult<UserDto>> UpdateUser(Guid id, [FromBody] UpdateUserCommand command)
        {
            command.UserId = id;
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        // ACTIVATE User
        [HttpPut("{id}/activate")]
        [Authorize(Policy = "CanEditUsers")]
        public async Task<ActionResult<UserDto>> ActivateUser(Guid id)
        {
            var result = await _mediator.Send(new ActivateUserCommand { UserId = id });
            return Ok(result);
        }

        // DEACTIVATE User
        [HttpPut("{id}/deactivate")]
        [Authorize(Policy = "CanEditUsers")]
        public async Task<ActionResult<UserDto>> DeactivateUser(Guid id, [FromBody] DeactivateUserCommand command)
        {
            command.UserId = id;
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        // UNLOCK User (for locked users)
        [HttpPut("{id}/unlock")]
        [Authorize(Policy = "CanEditUsers")]
        public async Task<ActionResult<UserDto>> UnlockUser(Guid id)
        {
            var result = await _mediator.Send(new UnlockUserCommand { UserId = id });
            return Ok(result);
        }

        // GRANT Permissions to a User
        [HttpPost("{id}/permissions")]
        [Authorize(Policy = "CanManageUserRoles")]
        public async Task<ActionResult<UserDto>> GrantPermissions(Guid id, [FromBody] GrantPermissionsCommand command)
        {
            command.UserId = id;
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        // REVOKE Permission from a User
    

    [HttpDelete("{id}/permissions/{permission}")]
    [Authorize(Policy = "CanManageUserRoles")]
    public async Task<IActionResult> RevokePermission(
    Guid id,
    string permission,
    [FromBody] RevokePermissionCommand command)
    {
        // Convert route string -> Permission enum
        if (!Enum.TryParse<Permission>(permission, ignoreCase: true, out var parsedPermission))
        {
            return BadRequest($"Invalid permission value: '{permission}'.");
        }

        command.UserId = id;
        command.Permission = parsedPermission;

        await _mediator.Send(command);
        return NoContent();
    }



       // GET User Audit Logs
        [HttpGet("{id}/audit-log")]
        [Authorize(Policy = "CanViewAuditLogs")]
        public async Task<ActionResult<List<AuditLogDto>>> GetUserAuditLog(Guid id)
        {
            var result = await _mediator.Send(new GetUserAuditLogQuery { UserId = id });
            return Ok(result);
        }
    }
}
