namespace TRRCMS.Application.Streets.Dtos;

/// <summary>
/// Full street DTO with all details
/// </summary>
public class StreetDto
{
    public Guid Id { get; set; }
    public int Identifier { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? GeometryWkt { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? LastModifiedAtUtc { get; set; }
}
