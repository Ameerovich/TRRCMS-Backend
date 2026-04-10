namespace TRRCMS.WebAPI;

/// <summary>
/// Marker class for IStringLocalizer resource resolution.
/// Maps to ErrorMessages.resx (English/fallback) and ErrorMessages.ar.resx (Arabic)
/// in the Resources folder. Culture is selected based on the Accept-Language request header.
/// </summary>
public sealed class ErrorMessages { }
