namespace TRRCMS.Domain.Enums;

/// <summary>
/// Custom attribute to store Arabic label for enum values.
/// Used by vocabulary seed data to build bilingual vocabulary entries.
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class ArabicLabelAttribute : Attribute
{
    public string Label { get; }

    public ArabicLabelAttribute(string label)
    {
        Label = label;
    }
}
