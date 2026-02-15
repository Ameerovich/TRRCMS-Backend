namespace TRRCMS.Application.Vocabularies.Dtos;

/// <summary>
/// Single vocabulary value with integer code and bilingual labels.
/// Clients use Code (int) for API communication and labels for UI display.
/// </summary>
public class VocabularyValueDto
{
    /// <summary>
    /// Integer code matching the C# enum value — used in all API requests/responses.
    /// </summary>
    public int Code { get; set; }

    /// <summary>
    /// Arabic label for UI display.
    /// </summary>
    public string LabelArabic { get; set; } = string.Empty;

    /// <summary>
    /// English label for UI display.
    /// </summary>
    public string LabelEnglish { get; set; } = string.Empty;

    /// <summary>
    /// Optional description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Display order for UI rendering.
    /// </summary>
    public int DisplayOrder { get; set; }
}
