namespace TRRCMS.Application.Common.Exceptions;

public class NotFoundException : Exception, ILocalizableException
{
    /// <inheritdoc/>
    public string? LocalizationKey { get; }

    /// <inheritdoc/>
    public object[] LocalizationArgs { get; }

    public NotFoundException(string message) : base(message)
    {
        LocalizationArgs = Array.Empty<object>();
    }

    /// <summary>
    /// Localizable form. <paramref name="message"/> is the English fallback (lands in
    /// <c>detail</c>); the response <c>message</c> is rendered from <paramref name="localizationKey"/>.
    /// </summary>
    public NotFoundException(string message, string localizationKey, params object[] args)
        : base(message)
    {
        LocalizationKey = localizationKey;
        LocalizationArgs = args ?? Array.Empty<object>();
    }

    public NotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
        LocalizationArgs = Array.Empty<object>();
    }
}
