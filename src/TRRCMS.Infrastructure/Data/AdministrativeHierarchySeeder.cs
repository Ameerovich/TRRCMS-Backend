using Microsoft.Extensions.Logging;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.AdministrativeDivisions.Commands.ImportAdministrativeHierarchy;

namespace TRRCMS.Infrastructure.Data;

/// <summary>
/// Seeds administrative hierarchy and neighborhood data on application startup
/// </summary>
public class AdministrativeHierarchySeeder
{
    private readonly IGovernorateRepository _governorateRepository;
    private readonly ILogger<AdministrativeHierarchySeeder> _logger;
    private readonly ImportAdministrativeHierarchyCommandHandler _importHandler;

    public AdministrativeHierarchySeeder(
        IGovernorateRepository governorateRepository,
        IDistrictRepository districtRepository,
        ISubDistrictRepository subDistrictRepository,
        ICommunityRepository communityRepository,
        INeighborhoodRepository neighborhoodRepository,
        ILogger<AdministrativeHierarchySeeder> logger)
    {
        _governorateRepository = governorateRepository;
        _logger = logger;

        // Create the import handler
        _importHandler = new ImportAdministrativeHierarchyCommandHandler(
            governorateRepository,
            districtRepository,
            subDistrictRepository,
            communityRepository,
            neighborhoodRepository);
    }

    /// <summary>
    /// Seeds administrative hierarchy data if tables are empty
    /// </summary>
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if data already exists
            var existingGovernorates = await _governorateRepository.GetAllAsync(cancellationToken);
            if (existingGovernorates.Any())
            {
                _logger.LogInformation("Administrative hierarchy data already exists, skipping seed");
                return;
            }

            _logger.LogInformation("Starting administrative hierarchy data seeding...");

            // Read the JSON file
            var jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "administrative_divisions.json");

            if (!File.Exists(jsonFilePath))
            {
                _logger.LogWarning("Administrative hierarchy seed file not found at: {FilePath}", jsonFilePath);
                return;
            }

            var jsonContent = await File.ReadAllTextAsync(jsonFilePath, cancellationToken);

            // Normalize JSON format (handle both "subdistricts" and "sub_districts")
            jsonContent = jsonContent.Replace("\"subdistricts\":", "\"sub_districts\":");

            // Use system user ID for seeding (you may want to use a specific seed user ID)
            var systemUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

            var command = new ImportAdministrativeHierarchyCommand
            {
                JsonContent = jsonContent,
                GeneratePlaceholderNeighborhoods = true,
                ImportedByUserId = systemUserId
            };

            var result = await _importHandler.Handle(command, cancellationToken);

            if (result.Success)
            {
                _logger.LogInformation(
                    "Administrative hierarchy seeding completed successfully: {Message}",
                    result.Message);

                _logger.LogInformation(
                    "Seeded: {Governorates} governorates, {Districts} districts, {SubDistricts} sub-districts, {Communities} communities, {Neighborhoods} neighborhoods",
                    result.GovernoratesImported,
                    result.DistrictsImported,
                    result.SubDistrictsImported,
                    result.CommunitiesImported,
                    result.NeighborhoodsGenerated);
            }
            else
            {
                _logger.LogError("Administrative hierarchy seeding failed: {Message}", result.Message);

                if (result.Errors.Any())
                {
                    foreach (var error in result.Errors)
                    {
                        _logger.LogError("Seed error: {Error}", error);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while seeding administrative hierarchy data");
        }
    }
}
