using MediatR;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Buildings.Commands.CreateBuilding;

public record CreateBuildingCommand : IRequest<Guid>
{
    // Administrative Codes
    public string GovernorateCode { get; init; } = string.Empty;
    public string DistrictCode { get; init; } = string.Empty;
    public string SubDistrictCode { get; init; } = string.Empty;
    public string CommunityCode { get; init; } = string.Empty;
    public string NeighborhoodCode { get; init; } = string.Empty;
    public string BuildingNumber { get; init; } = string.Empty;

    // Location Names (Arabic)
    public string GovernorateName { get; init; } = string.Empty;
    public string DistrictName { get; init; } = string.Empty;
    public string SubDistrictName { get; init; } = string.Empty;
    public string CommunityName { get; init; } = string.Empty;
    public string NeighborhoodName { get; init; } = string.Empty;

    // Building Type
    public BuildingType BuildingType { get; init; }

    // Optional Location
    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }

    // Optional Details (NEW!)
    public int? NumberOfFloors { get; init; }
    public int? YearOfConstruction { get; init; }
    public string? Address { get; init; }
    public string? Landmark { get; init; }
    public string? Notes { get; init; }
}