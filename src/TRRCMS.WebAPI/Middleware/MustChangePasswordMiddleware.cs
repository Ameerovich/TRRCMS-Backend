namespace TRRCMS.WebAPI.Middleware;

/// <summary>
/// Middleware that blocks all API requests for users with a restricted "must_change_password" token,
/// except for the change-password and logout endpoints.
/// This enforces the first-login password change requirement.
/// </summary>
public class MustChangePasswordMiddleware
{
    private readonly RequestDelegate _next;

    private static readonly string[] AllowedPaths =
    [
        "/api/v1/auth/change-password",
        "/api/v1/auth/logout",
        "/api/v2/auth/change-password",
        "/api/v2/auth/logout",
        "/health"
    ];

    public MustChangePasswordMiddleware(RequestDelegate next)
    {
        _next = next;
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
                    await context.Response.WriteAsJsonAsync(new
                    {
                        error = "PasswordChangeRequired",
                        message = "You must change your password before accessing other resources. Use POST /api/v1/auth/change-password or POST /api/v2/auth/change-password."
                    });
                    return;
                }
            }
        }

        await _next(context);
    }
}
