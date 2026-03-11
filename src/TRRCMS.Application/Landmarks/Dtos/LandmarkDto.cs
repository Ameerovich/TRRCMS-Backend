namespace TRRCMS.Application.Landmarks.Dtos;

/// <summary>
/// Full landmark DTO with all details
/// </summary>
public class LandmarkDto
{
    public Guid Id { get; set; }
    public int Identifier { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? LastModifiedAtUtc { get; set; }
}
