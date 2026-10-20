namespace TRRCMS.Application.Import.Models;

/// <summary>
/// Raw building identity read from the <c>buildings</c> table of a .uhc package,
/// used only to describe the package in the import packages list.
/// Tablet packages usually leave the <c>*_name</c> columns NULL and fill only the codes,
/// so names are resolved from the codes by the caller (never cached with the file data).
/// </summary>
public sealed record UhcBuildingSummary(
    string BuildingNumber,
    string? BuildingCode,
    string? GovernorateCode,
    string? DistrictCode,
    string? SubDistrictCode,
    string? CommunityCode,
    string? NeighborhoodCode,
    string? GovernorateName,
    string? DistrictName,
    string? SubDistrictName,
    string? CommunityName,
    string? NeighborhoodName);
