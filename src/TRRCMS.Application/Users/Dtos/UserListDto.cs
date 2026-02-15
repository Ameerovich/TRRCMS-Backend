namespace TRRCMS.Application.Users.Dtos;

/// <summary>
/// Lightweight DTO for user list views
/// Optimized for GetAllUsers query with minimal data transfer
/// </summary>
public class UserListDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullNameArabic { get; set; } = string.Empty;
    public string? Email { get; set; }
    public int Role { get; set; }
    public bool IsActive { get; set; }
    public bool IsLockedOut { get; set; }
    public string? Organization { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}