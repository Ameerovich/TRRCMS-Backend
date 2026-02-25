using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using TRRCMS.Application.Common.Exceptions;

namespace TRRCMS.WebAPI.Middleware;

/// <summary>
/// Global exception handling middleware that converts known exceptions to proper HTTP responses.
/// Eliminates the need for try-catch blocks in individual controller actions.
///
/// Exception → HTTP mapping:
///   ValidationException (with Errors)   → 400 Bad Request  (field-level validation errors)
///   ValidationException (message only)  → 400 Bad Request  (business rule violation)
///   ArgumentException / ArgumentNull    → 400 Bad Request
///   NotFoundException                   → 404 Not Found
///   KeyNotFoundException                → 404 Not Found
///   UnauthorizedAccessException         → 403 Forbidden
///   ConflictException                   → 409 Conflict     (business rule / state conflict)
///   InvalidOperationException           → 409 Conflict     (domain state violation)
///   Everything else                     → 500 Internal Server Error
/// </summary>
public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // If the response has already started (e.g., headers sent during streaming),
        // we cannot modify status code or write a JSON body — just log and re-throw.
        if (context.Response.HasStarted)
        {
            _logger.LogError(exception,
                "Exception occurred after response started. Cannot write error response: {Message}",
                exception.Message);
            throw exception;
        }

        var (statusCode, response) = exception switch
        {
            // Field-level validation errors (FluentValidation pipeline + manual handler throws)
            ValidationException validationEx when validationEx.Errors.Any() => (
                HttpStatusCode.BadRequest,
                new ErrorResponse
                {
                    Status = (int)HttpStatusCode.BadRequest,
                    Title = "Validation Failed",
                    Message = validationEx.Message,
                    Errors = validationEx.Errors
                }),

            // Business rule validation (single message, no field errors)
            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                new ErrorResponse
                {
                    Status = (int)HttpStatusCode.BadRequest,
                    Title = "Validation Failed",
                    Message = validationEx.Message
                }),

            // Entity not found (custom application exception)
            NotFoundException notFoundEx => (
                HttpStatusCode.NotFound,
                new ErrorResponse
                {
                    Status = (int)HttpStatusCode.NotFound,
                    Title = "Not Found",
                    Message = notFoundEx.Message
                }),

            // Entity not found (standard .NET exception used by some handlers)
            KeyNotFoundException keyNotFoundEx => (
                HttpStatusCode.NotFound,
                new ErrorResponse
                {
                    Status = (int)HttpStatusCode.NotFound,
                    Title = "Not Found",
                    Message = keyNotFoundEx.Message
                }),

            // Unauthorized access
            UnauthorizedAccessException unauthorizedEx => (
                HttpStatusCode.Forbidden,
                new ErrorResponse
                {
                    Status = (int)HttpStatusCode.Forbidden,
                    Title = "Forbidden",
                    Message = unauthorizedEx.Message
                }),

            // Business rule / state conflict (custom application exception)
            ConflictException conflictEx => (
                HttpStatusCode.Conflict,
                new ErrorResponse
                {
                    Status = (int)HttpStatusCode.Conflict,
                    Title = "Conflict",
                    Message = conflictEx.Message
                }),

            // Domain state violation (e.g., invalid state transitions, duplicate entities)
            InvalidOperationException invalidOpEx => (
                HttpStatusCode.Conflict,
                new ErrorResponse
                {
                    Status = (int)HttpStatusCode.Conflict,
                    Title = "Conflict",
                    Message = invalidOpEx.Message
                }),

            // Bad arguments
            ArgumentException argEx => (
                HttpStatusCode.BadRequest,
                new ErrorResponse
                {
                    Status = (int)HttpStatusCode.BadRequest,
                    Title = "Bad Request",
                    Message = argEx.Message
                }),

            // Everything else → 500
            _ => (
                HttpStatusCode.InternalServerError,
                new ErrorResponse
                {
                    Status = (int)HttpStatusCode.InternalServerError,
                    Title = "Internal Server Error",
                    Message = context.RequestServices
                        .GetService<IHostEnvironment>()?.IsDevelopment() == true
                            ? $"{exception.GetType().Name}: {exception.Message}"
                            : "An unexpected error occurred. Please try again later.",
                    Errors = context.RequestServices
                        .GetService<IHostEnvironment>()?.IsDevelopment() == true
                            ? new Dictionary<string, string[]>
                            {
                                ["exception"] = new[] { exception.ToString() }
                            }
                            : null
                })
        };

        // Log based on severity
        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
        }
        else
        {
            _logger.LogWarning("Handled exception ({StatusCode}): {Type} - {Message}",
                (int)statusCode, exception.GetType().Name, exception.Message);
        }

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }
}
