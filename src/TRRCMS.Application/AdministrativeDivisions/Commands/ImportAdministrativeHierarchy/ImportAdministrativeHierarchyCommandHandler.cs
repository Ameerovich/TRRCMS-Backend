using MediatR;
using System.Text.Json;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Entities;

namespace TRRCMS.Application.AdministrativeDivisions.Commands.ImportAdministrativeHierarchy;

/// <summary>
/// Handler for ImportAdministrativeHierarchyCommand
/// Parses JSON and imports governorate → district → sub-district → community hierarchy
/// </summary>
public class ImportAdministrativeHierarchyCommandHandler
    : IRequestHandler<ImportAdministrativeHierarchyCommand, ImportAdministrativeHierarchyResult>
{
    private readonly IGovernorateRepository _governorateRepository;
    private readonly IDistrictRepository _districtRepository;
    private readonly ISubDistrictRepository _subDistrictRepository;
    private readonly ICommunityRepository _communityRepository;
    private readonly INeighborhoodRepository _neighborhoodRepository;

    public ImportAdministrativeHierarchyCommandHandler(
        IGovernorateRepository governorateRepository,
        IDistrictRepository districtRepository,
        ISubDistrictRepository subDistrictRepository,
        ICommunityRepository communityRepository,
        INeighborhoodRepository neighborhoodRepository)
    {
        _governorateRepository = governorateRepository;
        _districtRepository = districtRepository;
        _subDistrictRepository = subDistrictRepository;
        _communityRepository = communityRepository;
        _neighborhoodRepository = neighborhoodRepository;
    }

    public async Task<ImportAdministrativeHierarchyResult> Handle(
        ImportAdministrativeHierarchyCommand request,
        CancellationToken cancellationToken)
    {
        var result = new ImportAdministrativeHierarchyResult();
        var errors = new List<string>();

        try
        {
            // Parse JSON
            var jsonDocument = JsonDocument.Parse(request.JsonContent);
            var root = jsonDocument.RootElement;

            // Import governorates
            if (root.TryGetProperty("governorates", out var governoratesArray))
            {
                foreach (var govElement in governoratesArray.EnumerateArray())
                {
                    await ImportGovernorate(govElement, request.ImportedByUserId, request.GeneratePlaceholderNeighborhoods, result, errors, cancellationToken);
                }
            }

            // Save all changes
            await _governorateRepository.SaveChangesAsync(cancellationToken);
            await _districtRepository.SaveChangesAsync(cancellationToken);
            await _subDistrictRepository.SaveChangesAsync(cancellationToken);
            await _communityRepository.SaveChangesAsync(cancellationToken);

            if (request.GeneratePlaceholderNeighborhoods)
            {
                await _neighborhoodRepository.SaveChangesAsync(cancellationToken);
            }

            result.Success = true;
            result.Message = $"Successfully imported {result.GovernoratesImported} governorates, " +
                           $"{result.DistrictsImported} districts, {result.SubDistrictsImported} sub-districts, " +
                           $"{result.CommunitiesImported} communities";

            if (request.GeneratePlaceholderNeighborhoods)
            {
                result.Message += $", and generated {result.NeighborhoodsGenerated} placeholder neighborhoods";
            }

            result.Errors = errors;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = $"Import failed: {ex.Message}";
            result.Errors.Add(ex.ToString());
        }

        return result;
    }

    private async Task ImportGovernorate(
        JsonElement govElement,
        Guid userId,
        bool generateNeighborhoods,
        ImportAdministrativeHierarchyResult result,
        List<string> errors,
        CancellationToken cancellationToken)
    {
        try
        {
            var code = govElement.GetProperty("code").GetString() ?? string.Empty;
            var nameAr = govElement.GetProperty("name_ar").GetString() ?? string.Empty;
            var nameEn = govElement.GetProperty("name_en").GetString() ?? string.Empty;

            // Check if governorate already exists
            if (await _governorateRepository.ExistsAsync(code, cancellationToken))
            {
                errors.Add($"Governorate {code} already exists, skipping");
                return;
            }

            var governorate = Governorate.Create(code, nameAr, nameEn, userId);
            await _governorateRepository.AddAsync(governorate, cancellationToken);
            result.GovernoratesImported++;

            // Import districts
            if (govElement.TryGetProperty("districts", out var districtsArray))
            {
                foreach (var distElement in districtsArray.EnumerateArray())
                {
                    await ImportDistrict(code, distElement, userId, generateNeighborhoods, result, errors, cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            errors.Add($"Error importing governorate: {ex.Message}");
        }
    }

    private async Task ImportDistrict(
        string governorateCode,
        JsonElement distElement,
        Guid userId,
        bool generateNeighborhoods,
        ImportAdministrativeHierarchyResult result,
        List<string> errors,
        CancellationToken cancellationToken)
    {
        try
        {
            var code = distElement.GetProperty("code").GetString() ?? string.Empty;
            var nameAr = distElement.GetProperty("name_ar").GetString() ?? string.Empty;
            var nameEn = distElement.GetProperty("name_en").GetString() ?? string.Empty;

            // Check if district already exists
            if (await _districtRepository.ExistsAsync(governorateCode, code, cancellationToken))
            {
                errors.Add($"District {governorateCode}-{code} already exists, skipping");
                return;
            }

            var district = District.Create(code, governorateCode, nameAr, nameEn, userId);
            await _districtRepository.AddAsync(district, cancellationToken);
            result.DistrictsImported++;

            // Import sub-districts
            if (distElement.TryGetProperty("sub_districts", out var subDistrictsArray))
            {
                foreach (var subDistElement in subDistrictsArray.EnumerateArray())
                {
                    await ImportSubDistrict(governorateCode, code, subDistElement, userId, generateNeighborhoods, result, errors, cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            errors.Add($"Error importing district {governorateCode}-{distElement.GetProperty("code").GetString()}: {ex.Message}");
        }
    }

    private async Task ImportSubDistrict(
        string governorateCode,
        string districtCode,
        JsonElement subDistElement,
        Guid userId,
        bool generateNeighborhoods,
        ImportAdministrativeHierarchyResult result,
        List<string> errors,
        CancellationToken cancellationToken)
    {
        try
        {
            var code = subDistElement.GetProperty("code").GetString() ?? string.Empty;
            var nameAr = subDistElement.GetProperty("name_ar").GetString() ?? string.Empty;
            var nameEn = subDistElement.GetProperty("name_en").GetString() ?? string.Empty;

            // Check if sub-district already exists
            if (await _subDistrictRepository.ExistsAsync(governorateCode, districtCode, code, cancellationToken))
            {
                errors.Add($"SubDistrict {governorateCode}-{districtCode}-{code} already exists, skipping");
                return;
            }

            var subDistrict = SubDistrict.Create(code, governorateCode, districtCode, nameAr, nameEn, userId);
            await _subDistrictRepository.AddAsync(subDistrict, cancellationToken);
            result.SubDistrictsImported++;

            // Import communities
            if (subDistElement.TryGetProperty("communities", out var communitiesArray))
            {
                foreach (var commElement in communitiesArray.EnumerateArray())
                {
                    await ImportCommunity(governorateCode, districtCode, code, commElement, userId, generateNeighborhoods, result, errors, cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            errors.Add($"Error importing sub-district {governorateCode}-{districtCode}-{subDistElement.GetProperty("code").GetString()}: {ex.Message}");
        }
    }

    private async Task ImportCommunity(
        string governorateCode,
        string districtCode,
        string subDistrictCode,
        JsonElement commElement,
        Guid userId,
        bool generateNeighborhoods,
        ImportAdministrativeHierarchyResult result,
        List<string> errors,
        CancellationToken cancellationToken)
    {
        try
        {
            var code = commElement.GetProperty("code").GetString() ?? string.Empty;
            var nameAr = commElement.GetProperty("name_ar").GetString() ?? string.Empty;
            var nameEn = commElement.GetProperty("name_en").GetString() ?? string.Empty;

            // Check if community already exists
            if (await _communityRepository.ExistsAsync(governorateCode, districtCode, subDistrictCode, code, cancellationToken))
            {
                errors.Add($"Community {governorateCode}-{districtCode}-{subDistrictCode}-{code} already exists, skipping");
                return;
            }

            var community = Community.Create(code, governorateCode, districtCode, subDistrictCode, nameAr, nameEn, userId);
            await _communityRepository.AddAsync(community, cancellationToken);
            result.CommunitiesImported++;

            // Generate placeholder neighborhood if requested
            if (generateNeighborhoods)
            {
                await GeneratePlaceholderNeighborhood(governorateCode, districtCode, subDistrictCode, code, userId, result, errors, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            errors.Add($"Error importing community {governorateCode}-{districtCode}-{subDistrictCode}-{commElement.GetProperty("code").GetString()}: {ex.Message}");
        }
    }

    private async Task GeneratePlaceholderNeighborhood(
        string governorateCode,
        string districtCode,
        string subDistrictCode,
        string communityCode,
        Guid userId,
        ImportAdministrativeHierarchyResult result,
        List<string> errors,
        CancellationToken cancellationToken)
    {
        try
        {
            const string placeholderNeighborhoodCode = "001";
            const string placeholderNameAr = "المنطقة الرئيسية";
            const string placeholderNameEn = "Main Area";

            // Check if neighborhood already exists
            var fullCode = $"{governorateCode}{districtCode}{subDistrictCode}{communityCode}{placeholderNeighborhoodCode}";
            var existing = await _neighborhoodRepository.GetByFullCodeAsync(fullCode, cancellationToken);

            if (existing != null)
            {
                // Neighborhood already exists, skip
                return;
            }

            var neighborhood = Neighborhood.Create(
                governorateCode,
                districtCode,
                subDistrictCode,
                communityCode,
                placeholderNeighborhoodCode,
                placeholderNameAr,
                placeholderNameEn,
                0m, // centerLatitude - placeholder value
                0m, // centerLongitude - placeholder value
                null, // boundaryGeometry - no boundary for placeholder
                null, // areaSquareKm - unknown for placeholder
                15, // zoomLevel - default zoom
                userId);

            await _neighborhoodRepository.AddAsync(neighborhood, cancellationToken);
            result.NeighborhoodsGenerated++;
        }
        catch (Exception ex)
        {
            errors.Add($"Error generating placeholder neighborhood for {governorateCode}-{districtCode}-{subDistrictCode}-{communityCode}: {ex.Message}");
        }
    }
}
