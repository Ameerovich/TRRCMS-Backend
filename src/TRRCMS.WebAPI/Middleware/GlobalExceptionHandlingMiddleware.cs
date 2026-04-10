using System.Globalization;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.WebAPI;

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
///
/// Localization:
///   UseRequestLocalization (registered before this middleware) sets CultureInfo.CurrentUICulture
///   from the Accept-Language header. Supported: "ar" (Arabic), default: English.
///   When Arabic is requested, Title and Message are returned in Arabic; the original
///   English exception message is preserved in the Detail field for debugging.
/// </summary>
public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
    private readonly IStringLocalizer<ErrorMessages> _localizer;

    public GlobalExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlingMiddleware> logger,
        IStringLocalizer<ErrorMessages> localizer)
    {
        _next = next;
        _logger = logger;
        _localizer = localizer;
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
        if (context.Response.HasStarted)
        {
            _logger.LogError(exception,
                "Exception occurred after response started. Cannot write error response: {Message}",
                exception.Message);
            throw exception;
        }

        bool isArabic = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ar";

        var (statusCode, response) = BuildResponse(context, exception, isArabic);

        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
        else
            _logger.LogWarning("Handled exception ({StatusCode}): {Type} - {Message}",
                (int)statusCode, exception.GetType().Name, exception.Message);

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }

    private string L(string key) => _localizer[key].Value;

    private (HttpStatusCode, ErrorResponse) BuildResponse(
        HttpContext context, Exception exception, bool isArabic)
    {
        return exception switch
        {
            ValidationException validationEx when validationEx.Errors.Any() => (
                HttpStatusCode.BadRequest,
                new ErrorResponse
                {
                    Status = (int)HttpStatusCode.BadRequest,
                    Title = L("Title_ValidationFailed"),
                    Message = L("Message_ValidationFailed_WithErrors"),
                    Detail = isArabic ? validationEx.Message : null,
                    Errors = validationEx.Errors
                }),

            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                new ErrorResponse
                {
                    Status = (int)HttpStatusCode.BadRequest,
                    Title = L("Title_ValidationFailed"),
                    Message = isArabic ? L("Message_ValidationFailed") : validationEx.Message,
                    Detail = isArabic ? validationEx.Message : null
                }),

            NotFoundException notFoundEx => (
                HttpStatusCode.NotFound,
                new ErrorResponse
                {
                    Status = (int)HttpStatusCode.NotFound,
                    Title = L("Title_NotFound"),
                    Message = isArabic ? L("Message_NotFound") : notFoundEx.Message,
                    Detail = isArabic ? notFoundEx.Message : null
                }),

            KeyNotFoundException keyNotFoundEx => (
                HttpStatusCode.NotFound,
                new ErrorResponse
                {
                    Status = (int)HttpStatusCode.NotFound,
                    Title = L("Title_NotFound"),
                    Message = isArabic ? L("Message_NotFound") : keyNotFoundEx.Message,
                    Detail = isArabic ? keyNotFoundEx.Message : null
                }),

            UnauthorizedAccessException unauthorizedEx => (
                HttpStatusCode.Forbidden,
                new ErrorResponse
                {
                    Status = (int)HttpStatusCode.Forbidden,
                    Title = L("Title_Forbidden"),
                    Message = isArabic ? L("Message_Forbidden") : unauthorizedEx.Message,
                    Detail = isArabic ? unauthorizedEx.Message : null
                }),

            ConflictException conflictEx => (
                HttpStatusCode.Conflict,
                new ErrorResponse
                {
                    Status = (int)HttpStatusCode.Conflict,
                    Title = L("Title_Conflict"),
                    Message = isArabic ? L("Message_Conflict") : conflictEx.Message,
                    Detail = isArabic ? conflictEx.Message : null,
                    ConflictData = conflictEx.ConflictData
                }),

            InvalidOperationException invalidOpEx => (
                HttpStatusCode.Conflict,
                new ErrorResponse
                {
                    Status = (int)HttpStatusCode.Conflict,
                    Title = L("Title_Conflict"),
                    Message = isArabic ? L("Message_Conflict") : invalidOpEx.Message,
                    Detail = isArabic ? invalidOpEx.Message : null
                }),

            ArgumentException argEx => (
                HttpStatusCode.BadRequest,
                new ErrorResponse
                {
                    Status = (int)HttpStatusCode.BadRequest,
                    Title = L("Title_BadRequest"),
                    Message = isArabic ? L("Message_BadRequest") : argEx.Message,
                    Detail = isArabic ? argEx.Message : null
                }),

            _ => BuildServerErrorResponse(context, exception)
        };
    }

    private (HttpStatusCode, ErrorResponse) BuildServerErrorResponse(
        HttpContext context, Exception exception)
    {
        var isDev = context.RequestServices
            .GetService<IHostEnvironment>()?.IsDevelopment() == true;

        return (HttpStatusCode.InternalServerError, new ErrorResponse
        {
            Status = (int)HttpStatusCode.InternalServerError,
            Title = L("Title_InternalServerError"),
            Message = isDev
                ? $"{exception.GetType().Name}: {exception.Message}"
                : L("Message_InternalServerError_Production"),
            Errors = isDev
                ? new Dictionary<string, string[]>
                  { ["exception"] = new[] { exception.ToString() } }
                : null
        });
    }
}
