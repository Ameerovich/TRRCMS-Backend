using System.Globalization;
using Microsoft.Extensions.Localization;
using TRRCMS.WebAPI.Resources;

namespace TRRCMS.WebAPI.Middleware;

/// <summary>
/// Middleware that blocks all API requests for users with a restricted "must_change_password" token,
/// except for the change-password and logout endpoints.
/// This enforces the first-login password change requirement.
/// </summary>
public class MustChangePasswordMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IStringLocalizer<ErrorMessages> _localizer;

    private static readonly string[] AllowedPaths =
    [
        "/api/v1/auth/change-password",
        "/api/v1/auth/logout",
        "/api/v2/auth/change-password",
        "/api/v2/auth/logout",
        "/health"
    ];

    public MustChangePasswordMiddleware(RequestDelegate next, IStringLocalizer<ErrorMessages> localizer)
    {
        _next = next;
        _localizer = localizer;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var mustChange = context.User.FindFirst("must_change_password")?.Value;
            if (mustChange == "true")
            {
                var path = context.Request.Path.Value;
                var isAllowed = AllowedPaths.Any(p =>
                    path != null && path.StartsWith(p, StringComparison.OrdinalIgnoreCase));

                if (!isAllowed)
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/json";

                    var response = new ErrorResponse
                    {
                        Status = StatusCodes.Status403Forbidden,
                        Title = _localizer["Title_PasswordChangeRequired"].Value,
                        Message = _localizer["Message_PasswordChangeRequired"].Value
                    };

                    await context.Response.WriteAsJsonAsync(response);
                    return;
                }
            }
        }

        await _next(context);
    }
}
