namespace TRRCMS.Application.Streets.Dtos;

/// <summary>
/// Lightweight street DTO for map rendering (line layer)
/// </summary>
public class StreetMapDto
{
    public Guid Id { get; set; }
    public int Identifier { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? GeometryWkt { get; set; }
}
