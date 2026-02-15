using FluentValidation.Results;

namespace TRRCMS.Application.Common.Exceptions;

public class ValidationException : Exception
{
    /// <summary>
    /// Field-level validation errors grouped by property name.
    /// Populated when thrown from FluentValidation pipeline.
    /// </summary>
    public IDictionary<string, string[]> Errors { get; }

    /// <summary>
    /// Creates a ValidationException with a single message (used in command handlers).
    /// </summary>
    public ValidationException(string message) : base(message)
    {
        Errors = new Dictionary<string, string[]>();
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
    }

    public ValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
        Errors = new Dictionary<string, string[]>();
    }
}
