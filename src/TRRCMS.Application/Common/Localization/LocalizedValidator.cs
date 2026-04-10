using FluentValidation;
using Microsoft.Extensions.Localization;
using TRRCMS.Application;

namespace TRRCMS.Application.Common.Localization;

/// <summary>
/// Base class for all FluentValidation validators that need localized error messages.
/// Exposes L("key") and L("key", args) helpers that resolve strings from
/// ValidationMessages.resx (English) or ValidationMessages.ar.resx (Arabic)
/// based on CultureInfo.CurrentUICulture, which is set per-request by
/// UseRequestLocalization middleware from the Accept-Language header.
/// </summary>
public abstract class LocalizedValidator<T> : AbstractValidator<T>
{
    private readonly IStringLocalizer<ValidationMessages> _localizer;

    protected LocalizedValidator(IStringLocalizer<ValidationMessages> localizer)
    {
        _localizer = localizer;
    }

    /// <summary>Returns the localized string for the given resource key.</summary>
    protected string L(string key) => _localizer[key].Value;

    /// <summary>Returns the localized string with format arguments substituted.</summary>
    protected string L(string key, params object[] args)
        => string.Format(_localizer[key].Value, args);
}
