namespace TRRCMS.Application.Common.Exceptions;

/// <summary>
/// Thrown when a business rule or domain state conflict prevents the operation.
/// Maps to HTTP 409 Conflict.
///
/// Examples:
///   - Entity already exists (duplicate)
///   - Invalid state transition (e.g., "Cannot modify survey in Submitted status")
///   - Conflict resolution failures
/// </summary>
public class ConflictException : Exception, ILocalizableException
{
    /// <summary>
    /// Optional payload with the conflicting entity data (e.g., existing person with same NationalId).
    /// Serialized into the 409 response body as "conflictData".
    /// </summary>
    public object? ConflictData { get; }

    /// <inheritdoc/>
    public string? LocalizationKey { get; }

    /// <inheritdoc/>
    public object[] LocalizationArgs { get; }

    public ConflictException(string message) : base(message)
    {
        LocalizationArgs = Array.Empty<object>();
    }

    public ConflictException(string message, object conflictData) : base(message)
    {
        ConflictData = conflictData;
        LocalizationArgs = Array.Empty<object>();
    }

    /// <summary>
    /// Localizable form. <paramref name="message"/> is the English fallback (lands in
    /// <c>detail</c>); the response <c>message</c> is rendered from <paramref name="localizationKey"/>.
    /// </summary>
    public ConflictException(string message, string localizationKey, params object[] args)
        : base(message)
    {
        LocalizationKey = localizationKey;
        LocalizationArgs = args ?? Array.Empty<object>();
    }

    public ConflictException(string message, Exception innerException)
        : base(message, innerException)
    {
        LocalizationArgs = Array.Empty<object>();
    }
}
