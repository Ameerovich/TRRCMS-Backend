namespace TRRCMS.Application.Landmarks.Dtos;

/// <summary>
/// Lightweight landmark DTO for map rendering (point layer)
/// </summary>
public class LandmarkMapDto
{
    public Guid Id { get; set; }
    public int Identifier { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
}
