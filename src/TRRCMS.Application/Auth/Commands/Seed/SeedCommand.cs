using MediatR;

namespace TRRCMS.Application.Auth.Commands.Seed;

/// <summary>
/// Command to seed initial test users for development
/// </summary>
public class SeedCommand : IRequest<SeedResult>
{
    public bool ForceReseed { get; set; }

    public SeedCommand(bool forceReseed = false)
    {
        ForceReseed = forceReseed;
    }
}

/// <summary>
/// Result of seed operation
/// </summary>
public class SeedResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> CreatedUsers { get; set; } = new();
    public List<string> SkippedUsers { get; set; } = new();
}