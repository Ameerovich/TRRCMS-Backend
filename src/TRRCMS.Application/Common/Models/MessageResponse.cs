namespace TRRCMS.Application.Common.Models;

/// <summary>
/// Standard response for operations that return a message without entity data.
/// </summary>
public class MessageResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    public static MessageResponse Ok(string message) => new() { Success = true, Message = message };
}
