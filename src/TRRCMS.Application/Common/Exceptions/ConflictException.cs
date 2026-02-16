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
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }

    public ConflictException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
