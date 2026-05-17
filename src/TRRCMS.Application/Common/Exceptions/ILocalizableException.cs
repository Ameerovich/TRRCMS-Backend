namespace TRRCMS.Application.Common.Exceptions;

/// <summary>
/// Marker interface implemented by exceptions whose user-facing message can be
/// rendered from a localized resource string instead of the raw English message.
///
/// When <see cref="LocalizationKey"/> is non-null, <see cref="GlobalExceptionHandlingMiddleware"/>
/// resolves it via <c>IStringLocalizer&lt;ErrorMessages&gt;</c> and formats with
/// <see cref="LocalizationArgs"/>; the raw <see cref="System.Exception.Message"/> is preserved
/// in the response <c>detail</c> field for developer logs (per the existing contract).
///
/// When <see cref="LocalizationKey"/> is null, the middleware falls back to the
/// generic localized title/message (current behavior). Callers can migrate
/// piecemeal — no need to retrofit every handler at once.
/// </summary>
public interface ILocalizableException
{
    /// <summary>Resource key in <c>ErrorMessages.resx</c> / <c>ErrorMessages.ar.resx</c>. Null = no specific localization.</summary>
    string? LocalizationKey { get; }

    /// <summary>Format args interpolated into the resource string via <c>string.Format</c>.</summary>
    object[] LocalizationArgs { get; }
}
