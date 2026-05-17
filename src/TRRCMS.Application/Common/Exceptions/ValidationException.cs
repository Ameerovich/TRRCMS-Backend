using FluentValidation.Results;

namespace TRRCMS.Application.Common.Exceptions;

public class ValidationException : Exception, ILocalizableException
{
    /// <summary>
    /// Field-level validation errors grouped by property name.
    /// Populated when thrown from FluentValidation pipeline.
    /// </summary>
    public IDictionary<string, string[]> Errors { get; }

    /// <inheritdoc/>
    public string? LocalizationKey { get; }

    /// <inheritdoc/>
    public object[] LocalizationArgs { get; }

    /// <summary>
    /// Creates a ValidationException with a single English message (used in command handlers).
    /// The message lands in <c>detail</c>; the user-facing <c>message</c> falls back to the
    /// generic localized validation message.
    /// </summary>
    public ValidationException(string message) : base(message)
    {
        Errors = new Dictionary<string, string[]>();
        LocalizationArgs = Array.Empty<object>();
    }

    /// <summary>
    /// Creates a ValidationException whose user-facing message is rendered from a localized
    /// resource. Pass the English fallback as <paramref name="message"/> (still surfaced via
    /// <c>detail</c> for developer logs), and a resource key + optional format args. The
    /// middleware resolves <paramref name="localizationKey"/> against
    /// <c>IStringLocalizer&lt;ErrorMessages&gt;</c> for the active culture.
    /// </summary>
    public ValidationException(string message, string localizationKey, params object[] args)
        : base(message)
    {
        Errors = new Dictionary<string, string[]>();
        LocalizationKey = localizationKey;
        LocalizationArgs = args ?? Array.Empty<object>();
    }

    /// <summary>
    /// Creates a ValidationException from FluentValidation failures (used in ValidationBehavior).
    /// </summary>
    public ValidationException(IEnumerable<ValidationFailure> failures)
        : base("One or more validation errors occurred.")
    {
        Errors = failures
            .GroupBy(f => f.PropertyName, f => f.ErrorMessage)
            .ToDictionary(g => g.Key, g => g.ToArray());
        LocalizationArgs = Array.Empty<object>();
    }

    public ValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
        Errors = new Dictionary<string, string[]>();
        LocalizationArgs = Array.Empty<object>();
    }
}
