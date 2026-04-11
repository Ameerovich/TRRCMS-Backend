namespace TRRCMS.Application.Common.Exceptions;

/// <summary>
/// Thrown when authentication fails: wrong credentials, locked account, inactive account,
/// expired password, or invalid/expired refresh token. Maps to HTTP 401 in
/// GlobalExceptionHandlingMiddleware. Carries a resource key so the middleware can
/// localize the message via IStringLocalizer&lt;ErrorMessages&gt;.
/// </summary>
public class InvalidCredentialsException : Exception
{
    public string LocalizationKey { get; }
    public object[] LocalizationArgs { get; }

    public InvalidCredentialsException(string localizationKey, string englishMessage, params object[] args)
        : base(englishMessage)
    {
        LocalizationKey = localizationKey;
        LocalizationArgs = args ?? Array.Empty<object>();
    }
}
