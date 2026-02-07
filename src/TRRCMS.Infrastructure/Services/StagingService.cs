using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Entities.Staging;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Infrastructure.Services;

/// <summary>
/// Implementation of IStagingService.
/// Opens the .uhc file as a read-only SQLite database, reads each data table,
/// maps rows to staging entities via their factory Create methods, and bulk-inserts
/// into the PostgreSQL staging tables.
///
/// Attachment blobs (evidence files) are extracted from the SQLite 'attachments'
/// table and saved to the file system via IFileStorageService.
///
/// UC-003 Stage 2 — S13 (Load to Staging).
/// </summary>
public class StagingService : IStagingService
{
    private readonly IStagingRepository<StagingBuilding> _buildingRepo;
    private readonly IStagingRepository<StagingPropertyUnit> _unitRepo;
    private readonly IStagingRepository<StagingPerson> _personRepo;
    private readonly IStagingRepository<StagingHousehold> _householdRepo;
    private readonly IStagingRepository<StagingPersonPropertyRelation> _relationRepo;
    private readonly IStagingRepository<StagingEvidence> _evidenceRepo;
    private readonly IStagingRepository<StagingClaim> _claimRepo;
    private readonly IStagingRepository<StagingSurvey> _surveyRepo;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<StagingService> _logger;

    public StagingService(
        IStagingRepository<StagingBuilding> buildingRepo,
        IStagingRepository<StagingPropertyUnit> unitRepo,
        IStagingRepository<StagingPerson> personRepo,
        IStagingRepository<StagingHousehold> householdRepo,
        IStagingRepository<StagingPersonPropertyRelation> relationRepo,
        IStagingRepository<StagingEvidence> evidenceRepo,
        IStagingRepository<StagingClaim> claimRepo,
        IStagingRepository<StagingSurvey> surveyRepo,
        IFileStorageService fileStorageService,
        ILogger<StagingService> logger)
    {
        _buildingRepo = buildingRepo;
        _unitRepo = unitRepo;
        _personRepo = personRepo;
        _householdRepo = householdRepo;
        _relationRepo = relationRepo;
        _evidenceRepo = evidenceRepo;
        _claimRepo = claimRepo;
        _surveyRepo = surveyRepo;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<StagingResult> UnpackAndStageAsync(
        Guid importPackageId,
        string uhcFilePath,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(uhcFilePath))
            throw new FileNotFoundException("Package file not found", uhcFilePath);

        var result = new StagingResult { ImportPackageId = importPackageId };

        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = uhcFilePath,
            Mode = SqliteOpenMode.ReadOnly
        }.ToString();

        using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        // Stage each entity type in dependency order
        // (buildings first, then units, then persons, etc.)
        result.BuildingCount = await StageBuildingsAsync(connection, importPackageId, cancellationToken);
        result.PropertyUnitCount = await StagePropertyUnitsAsync(connection, importPackageId, cancellationToken);
        result.PersonCount = await StagePersonsAsync(connection, importPackageId, cancellationToken);
        result.HouseholdCount = await StageHouseholdsAsync(connection, importPackageId, cancellationToken);
        result.PersonPropertyRelationCount = await StageRelationsAsync(connection, importPackageId, cancellationToken);
        result.ClaimCount = await StageClaimsAsync(connection, importPackageId, cancellationToken);
        result.SurveyCount = await StageSurveysAsync(connection, importPackageId, cancellationToken);

        // Evidence + attachments last (depends on person/claim references)
        var (evidenceCount, fileCount, fileBytes) = await StageEvidenceAndAttachmentsAsync(
            connection, importPackageId, cancellationToken);
        result.EvidenceCount = evidenceCount;
        result.AttachmentFilesExtracted = fileCount;
        result.AttachmentBytesExtracted = fileBytes;

        _logger.LogInformation(
            "Staging complete for package {PackageId}: {Total} records, {Files} attachments ({Bytes} bytes)",
            importPackageId, result.TotalRecordCount, result.AttachmentFilesExtracted, result.AttachmentBytesExtracted);

        return result;
    }

    public async Task CleanupStagingAsync(Guid importPackageId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Cleaning up staging data for package {PackageId}", importPackageId);

        // Delete in reverse dependency order
        await _evidenceRepo.DeleteByPackageIdAsync(importPackageId, cancellationToken);
        await _claimRepo.DeleteByPackageIdAsync(importPackageId, cancellationToken);
        await _surveyRepo.DeleteByPackageIdAsync(importPackageId, cancellationToken);
        await _relationRepo.DeleteByPackageIdAsync(importPackageId, cancellationToken);
        await _householdRepo.DeleteByPackageIdAsync(importPackageId, cancellationToken);
        await _personRepo.DeleteByPackageIdAsync(importPackageId, cancellationToken);
        await _unitRepo.DeleteByPackageIdAsync(importPackageId, cancellationToken);
        await _buildingRepo.DeleteByPackageIdAsync(importPackageId, cancellationToken);

        _logger.LogInformation("Staging cleanup complete for package {PackageId}", importPackageId);
    }

    // ==================== ENTITY STAGING METHODS ====================

    private async Task<int> StageBuildingsAsync(
        SqliteConnection connection, Guid packageId, CancellationToken ct)
    {
        if (!await TableExistsAsync(connection, "buildings", ct)) return 0;

        var entities = new List<StagingBuilding>();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT * FROM buildings";
        using var reader = await cmd.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            var entity = StagingBuilding.Create(
                importPackageId: packageId,
                originalEntityId: GetGuid(reader, "id"),
                governorateCode: GetString(reader, "governorate_code"),
                districtCode: GetString(reader, "district_code"),
                subDistrictCode: GetString(reader, "sub_district_code"),
                communityCode: GetString(reader, "community_code"),
                neighborhoodCode: GetString(reader, "neighborhood_code"),
                buildingNumber: GetString(reader, "building_number"),
                buildingType: GetEnum<BuildingType>(reader, "building_type"),
                status: GetEnum<BuildingStatus>(reader, "building_status"),
                numberOfPropertyUnits: GetInt(reader, "number_of_property_units"),
                numberOfApartments: GetInt(reader, "number_of_apartments"),
                numberOfShops: GetInt(reader, "number_of_shops"),
                latitude: GetNullableDecimal(reader, "latitude"),
                longitude: GetNullableDecimal(reader, "longitude"),
                buildingGeometryWkt: GetNullableString(reader, "building_geometry_wkt"),
                locationDescription: GetNullableString(reader, "location_description"),
                notes: GetNullableString(reader, "notes"),
                buildingId: GetNullableString(reader, "building_id"),
                governorateName: GetNullableString(reader, "governorate_name"),
                districtName: GetNullableString(reader, "district_name"),
                subDistrictName: GetNullableString(reader, "sub_district_name"),
                communityName: GetNullableString(reader, "community_name"),
                neighborhoodName: GetNullableString(reader, "neighborhood_name"),
                damageLevel: GetNullableEnum<DamageLevel>(reader, "damage_level"),
                numberOfFloors: GetNullableInt(reader, "number_of_floors"),
                yearOfConstruction: GetNullableInt(reader, "year_of_construction"));

            entities.Add(entity);
        }

        if (entities.Count > 0)
        {
            await _buildingRepo.AddRangeAsync(entities, ct);
            await _buildingRepo.SaveChangesAsync(ct);
        }

        _logger.LogDebug("Staged {Count} buildings", entities.Count);
        return entities.Count;
    }

    private async Task<int> StagePropertyUnitsAsync(
        SqliteConnection connection, Guid packageId, CancellationToken ct)
    {
        if (!await TableExistsAsync(connection, "property_units", ct)) return 0;

        var entities = new List<StagingPropertyUnit>();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT * FROM property_units";
        using var reader = await cmd.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            var entity = StagingPropertyUnit.Create(
                importPackageId: packageId,
                originalEntityId: GetGuid(reader, "id"),
                originalBuildingId: GetGuid(reader, "building_id"),
                unitIdentifier: GetString(reader, "unit_identifier"),
                unitType: GetEnum<PropertyUnitType>(reader, "unit_type"),
                status: GetEnum<PropertyUnitStatus>(reader, "status"),
                floorNumber: GetNullableInt(reader, "floor_number"),
                numberOfRooms: GetNullableInt(reader, "number_of_rooms"),
                areaSquareMeters: GetNullableDecimal(reader, "area_square_meters"),
                description: GetNullableString(reader, "description"),
                occupancyStatus: GetNullableString(reader, "occupancy_status"),
                damageLevel: GetNullableEnum<DamageLevel>(reader, "damage_level"),
                estimatedAreaSqm: GetNullableDecimal(reader, "estimated_area_sqm"),
                occupancyType: GetNullableEnum<OccupancyType>(reader, "occupancy_type"),
                occupancyNature: GetNullableEnum<OccupancyNature>(reader, "occupancy_nature"));

            entities.Add(entity);
        }

        if (entities.Count > 0)
        {
            await _unitRepo.AddRangeAsync(entities, ct);
            await _unitRepo.SaveChangesAsync(ct);
        }

        _logger.LogDebug("Staged {Count} property units", entities.Count);
        return entities.Count;
    }

    private async Task<int> StagePersonsAsync(
        SqliteConnection connection, Guid packageId, CancellationToken ct)
    {
        if (!await TableExistsAsync(connection, "persons", ct)) return 0;

        var entities = new List<StagingPerson>();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT * FROM persons";
        using var reader = await cmd.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            var entity = StagingPerson.Create(
                importPackageId: packageId,
                originalEntityId: GetGuid(reader, "id"),
                familyNameArabic: GetString(reader, "family_name_arabic"),
                firstNameArabic: GetString(reader, "first_name_arabic"),
                fatherNameArabic: GetString(reader, "father_name_arabic"),
                motherNameArabic: GetNullableString(reader, "mother_name_arabic"),
                nationalId: GetNullableString(reader, "national_id"),
                yearOfBirth: GetNullableInt(reader, "year_of_birth"),
                email: GetNullableString(reader, "email"),
                mobileNumber: GetNullableString(reader, "mobile_number"),
                phoneNumber: GetNullableString(reader, "phone_number"),
                fullNameEnglish: GetNullableString(reader, "full_name_english"),
                gender: GetNullableString(reader, "gender"),
                nationality: GetNullableString(reader, "nationality"),
                originalHouseholdId: GetNullableGuid(reader, "household_id"),
                relationshipToHead: GetNullableString(reader, "relationship_to_head"));

            entities.Add(entity);
        }

        if (entities.Count > 0)
        {
            await _personRepo.AddRangeAsync(entities, ct);
            await _personRepo.SaveChangesAsync(ct);
        }

        _logger.LogDebug("Staged {Count} persons", entities.Count);
        return entities.Count;
    }

    private async Task<int> StageHouseholdsAsync(
        SqliteConnection connection, Guid packageId, CancellationToken ct)
    {
        if (!await TableExistsAsync(connection, "households", ct)) return 0;

        var entities = new List<StagingHousehold>();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT * FROM households";
        using var reader = await cmd.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            var entity = StagingHousehold.Create(
                importPackageId: packageId,
                originalEntityId: GetGuid(reader, "id"),
                originalPropertyUnitId: GetGuid(reader, "property_unit_id"),
                headOfHouseholdName: GetString(reader, "head_of_household_name"),
                householdSize: GetInt(reader, "household_size"),
                originalHeadOfHouseholdPersonId: GetNullableGuid(reader, "head_of_household_person_id"),
                maleCount: GetInt(reader, "male_count", 0),
                femaleCount: GetInt(reader, "female_count", 0),
                maleChildCount: GetInt(reader, "male_child_count", 0),
                femaleChildCount: GetInt(reader, "female_child_count", 0),
                maleElderlyCount: GetInt(reader, "male_elderly_count", 0),
                femaleElderlyCount: GetInt(reader, "female_elderly_count", 0),
                maleDisabledCount: GetInt(reader, "male_disabled_count", 0),
                femaleDisabledCount: GetInt(reader, "female_disabled_count", 0),
                isFemaleHeaded: GetBool(reader, "is_female_headed", false),
                isDisplaced: GetBool(reader, "is_displaced", false),
                notes: GetNullableString(reader, "notes"));

            entities.Add(entity);
        }

        if (entities.Count > 0)
        {
            await _householdRepo.AddRangeAsync(entities, ct);
            await _householdRepo.SaveChangesAsync(ct);
        }

        _logger.LogDebug("Staged {Count} households", entities.Count);
        return entities.Count;
    }

    private async Task<int> StageRelationsAsync(
        SqliteConnection connection, Guid packageId, CancellationToken ct)
    {
        if (!await TableExistsAsync(connection, "person_property_relations", ct)) return 0;

        var entities = new List<StagingPersonPropertyRelation>();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT * FROM person_property_relations";
        using var reader = await cmd.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            var entity = StagingPersonPropertyRelation.Create(
                importPackageId: packageId,
                originalEntityId: GetGuid(reader, "id"),
                originalPersonId: GetGuid(reader, "person_id"),
                originalPropertyUnitId: GetGuid(reader, "property_unit_id"),
                relationType: GetEnum<RelationType>(reader, "relation_type"),
                relationTypeOtherDesc: GetNullableString(reader, "relation_type_other_desc"),
                contractType: GetNullableEnum<TenureContractType>(reader, "contract_type"),
                ownershipShare: GetNullableDecimal(reader, "ownership_share"),
                startDate: GetNullableDateTime(reader, "start_date"),
                endDate: GetNullableDateTime(reader, "end_date"),
                notes: GetNullableString(reader, "notes"));

            entities.Add(entity);
        }

        if (entities.Count > 0)
        {
            await _relationRepo.AddRangeAsync(entities, ct);
            await _relationRepo.SaveChangesAsync(ct);
        }

        _logger.LogDebug("Staged {Count} person-property relations", entities.Count);
        return entities.Count;
    }

    private async Task<int> StageClaimsAsync(
        SqliteConnection connection, Guid packageId, CancellationToken ct)
    {
        if (!await TableExistsAsync(connection, "claims", ct)) return 0;

        var entities = new List<StagingClaim>();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT * FROM claims";
        using var reader = await cmd.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            var entity = StagingClaim.Create(
                importPackageId: packageId,
                originalEntityId: GetGuid(reader, "id"),
                originalPropertyUnitId: GetGuid(reader, "property_unit_id"),
                claimType: GetString(reader, "claim_type"),
                claimSource: GetEnum<ClaimSource>(reader, "claim_source"),
                originalPrimaryClaimantId: GetNullableGuid(reader, "primary_claimant_id"),
                priority: GetEnum(reader, "priority", CasePriority.Normal),
                tenureContractType: GetNullableEnum<TenureContractType>(reader, "tenure_contract_type"),
                ownershipShare: GetNullableDecimal(reader, "ownership_share"),
                tenureStartDate: GetNullableDateTime(reader, "tenure_start_date"),
                tenureEndDate: GetNullableDateTime(reader, "tenure_end_date"),
                claimDescription: GetNullableString(reader, "claim_description"),
                legalBasis: GetNullableString(reader, "legal_basis"),
                supportingNarrative: GetNullableString(reader, "supporting_narrative"),
                processingNotes: GetNullableString(reader, "processing_notes"));

            entities.Add(entity);
        }

        if (entities.Count > 0)
        {
            await _claimRepo.AddRangeAsync(entities, ct);
            await _claimRepo.SaveChangesAsync(ct);
        }

        _logger.LogDebug("Staged {Count} claims", entities.Count);
        return entities.Count;
    }

    private async Task<int> StageSurveysAsync(
        SqliteConnection connection, Guid packageId, CancellationToken ct)
    {
        if (!await TableExistsAsync(connection, "surveys", ct)) return 0;

        var entities = new List<StagingSurvey>();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT * FROM surveys";
        using var reader = await cmd.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            var entity = StagingSurvey.Create(
                importPackageId: packageId,
                originalEntityId: GetGuid(reader, "id"),
                originalBuildingId: GetGuid(reader, "building_id"),
                surveyDate: GetDateTime(reader, "survey_date"),
                originalPropertyUnitId: GetNullableGuid(reader, "property_unit_id"),
                gpsCoordinates: GetNullableString(reader, "gps_coordinates"),
                intervieweeName: GetNullableString(reader, "interviewee_name"),
                intervieweeRelationship: GetNullableString(reader, "interviewee_relationship"),
                notes: GetNullableString(reader, "notes"),
                originalFieldCollectorId: GetNullableGuid(reader, "field_collector_id"),
                referenceCode: GetNullableString(reader, "reference_code"),
                type: GetNullableEnum<SurveyType>(reader, "type"),
                source: GetNullableEnum<SurveySource>(reader, "source"),
                status: GetNullableEnum<SurveyStatus>(reader, "status"));

            entities.Add(entity);
        }

        if (entities.Count > 0)
        {
            await _surveyRepo.AddRangeAsync(entities, ct);
            await _surveyRepo.SaveChangesAsync(ct);
        }

        _logger.LogDebug("Staged {Count} surveys", entities.Count);
        return entities.Count;
    }

    private async Task<(int count, int files, long bytes)> StageEvidenceAndAttachmentsAsync(
        SqliteConnection connection, Guid packageId, CancellationToken ct)
    {
        if (!await TableExistsAsync(connection, "evidences", ct)) return (0, 0, 0);

        var entities = new List<StagingEvidence>();
        int fileCount = 0;
        long totalBytes = 0;

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT * FROM evidences";
        using var reader = await cmd.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            var originalId = GetGuid(reader, "id");
            var originalFileName = GetString(reader, "original_file_name");
            var fileSizeBytes = GetLong(reader, "file_size_bytes", 0);

            // Try to extract the attachment blob if available
            string filePath = GetString(reader, "file_path", "");
            var fileHash = GetNullableString(reader, "file_hash");

            // Check if attachment blob is embedded in the SQLite
            if (await HasAttachmentBlobAsync(connection, originalId, ct))
            {
                var savedPath = await ExtractAttachmentAsync(
                    connection, originalId, originalFileName, packageId, ct);
                if (savedPath != null)
                {
                    filePath = savedPath;
                    fileCount++;
                    totalBytes += fileSizeBytes;
                }
            }

            var entity = StagingEvidence.Create(
                importPackageId: packageId,
                originalEntityId: originalId,
                evidenceType: GetEnum<EvidenceType>(reader, "evidence_type"),
                description: GetString(reader, "description", ""),
                originalFileName: originalFileName,
                filePath: filePath,
                fileSizeBytes: fileSizeBytes,
                mimeType: _fileStorageService.GetMimeType(originalFileName),
                originalPersonId: GetNullableGuid(reader, "person_id"),
                originalPersonPropertyRelationId: GetNullableGuid(reader, "person_property_relation_id"),
                originalClaimId: GetNullableGuid(reader, "claim_id"),
                fileHash: fileHash,
                documentIssuedDate: GetNullableDateTime(reader, "document_issued_date"),
                documentExpiryDate: GetNullableDateTime(reader, "document_expiry_date"),
                issuingAuthority: GetNullableString(reader, "issuing_authority"),
                documentReferenceNumber: GetNullableString(reader, "document_reference_number"),
                notes: GetNullableString(reader, "notes"));

            entities.Add(entity);
        }

        if (entities.Count > 0)
        {
            await _evidenceRepo.AddRangeAsync(entities, ct);
            await _evidenceRepo.SaveChangesAsync(ct);
        }

        _logger.LogDebug("Staged {Count} evidence records, extracted {Files} files ({Bytes} bytes)",
            entities.Count, fileCount, totalBytes);

        return (entities.Count, fileCount, totalBytes);
    }

    // ==================== ATTACHMENT EXTRACTION ====================

    private static async Task<bool> HasAttachmentBlobAsync(
        SqliteConnection connection, Guid evidenceId, CancellationToken ct)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText =
            "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='attachments'";
        var tableExists = Convert.ToInt32(await cmd.ExecuteScalarAsync(ct));
        if (tableExists == 0) return false;

        using var checkCmd = connection.CreateCommand();
        checkCmd.CommandText = "SELECT COUNT(*) FROM attachments WHERE evidence_id = @id";
        checkCmd.Parameters.AddWithValue("@id", evidenceId.ToString());
        var count = Convert.ToInt32(await checkCmd.ExecuteScalarAsync(ct));
        return count > 0;
    }

    private async Task<string?> ExtractAttachmentAsync(
        SqliteConnection connection, Guid evidenceId, string fileName,
        Guid packageId, CancellationToken ct)
    {
        try
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT data FROM attachments WHERE evidence_id = @id LIMIT 1";
            cmd.Parameters.AddWithValue("@id", evidenceId.ToString());

            var blob = await cmd.ExecuteScalarAsync(ct) as byte[];
            if (blob == null || blob.Length == 0) return null;

            using var stream = new MemoryStream(blob);
            var savedPath = await _fileStorageService.SaveFileAsync(
                stream, fileName, "import-attachments", packageId, ct);

            return savedPath;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to extract attachment for evidence {EvidenceId}", evidenceId);
            return null;
        }
    }

    // ==================== SQLite HELPER METHODS ====================

    private static async Task<bool> TableExistsAsync(
        SqliteConnection connection, string tableName, CancellationToken ct)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText =
            "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name=@name";
        cmd.Parameters.AddWithValue("@name", tableName);
        return Convert.ToInt32(await cmd.ExecuteScalarAsync(ct)) > 0;
    }

    private static bool HasColumn(SqliteDataReader reader, string columnName)
    {
        for (int i = 0; i < reader.FieldCount; i++)
        {
            if (string.Equals(reader.GetName(i), columnName, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    private static Guid GetGuid(SqliteDataReader reader, string column)
    {
        if (!HasColumn(reader, column)) return Guid.NewGuid();
        var value = reader[column];
        if (value is DBNull || value == null) return Guid.NewGuid();
        return Guid.TryParse(value.ToString(), out var result) ? result : Guid.NewGuid();
    }

    private static Guid? GetNullableGuid(SqliteDataReader reader, string column)
    {
        if (!HasColumn(reader, column)) return null;
        var value = reader[column];
        if (value is DBNull || value == null) return null;
        return Guid.TryParse(value.ToString(), out var result) ? result : null;
    }

    private static string GetString(SqliteDataReader reader, string column, string defaultValue = "")
    {
        if (!HasColumn(reader, column)) return defaultValue;
        var value = reader[column];
        return value is DBNull || value == null ? defaultValue : value.ToString() ?? defaultValue;
    }

    private static string? GetNullableString(SqliteDataReader reader, string column)
    {
        if (!HasColumn(reader, column)) return null;
        var value = reader[column];
        return value is DBNull || value == null ? null : value.ToString();
    }

    private static int GetInt(SqliteDataReader reader, string column, int defaultValue = 0)
    {
        if (!HasColumn(reader, column)) return defaultValue;
        var value = reader[column];
        if (value is DBNull || value == null) return defaultValue;
        return int.TryParse(value.ToString(), out var result) ? result : defaultValue;
    }

    private static int? GetNullableInt(SqliteDataReader reader, string column)
    {
        if (!HasColumn(reader, column)) return null;
        var value = reader[column];
        if (value is DBNull || value == null) return null;
        return int.TryParse(value.ToString(), out var result) ? result : null;
    }

    private static long GetLong(SqliteDataReader reader, string column, long defaultValue = 0)
    {
        if (!HasColumn(reader, column)) return defaultValue;
        var value = reader[column];
        if (value is DBNull || value == null) return defaultValue;
        return long.TryParse(value.ToString(), out var result) ? result : defaultValue;
    }

    private static decimal? GetNullableDecimal(SqliteDataReader reader, string column)
    {
        if (!HasColumn(reader, column)) return null;
        var value = reader[column];
        if (value is DBNull || value == null) return null;
        return decimal.TryParse(value.ToString(), out var result) ? result : null;
    }

    private static bool GetBool(SqliteDataReader reader, string column, bool defaultValue = false)
    {
        if (!HasColumn(reader, column)) return defaultValue;
        var value = reader[column];
        if (value is DBNull || value == null) return defaultValue;
        var str = value.ToString()?.ToLowerInvariant();
        return str is "1" or "true" or "yes";
    }

    private static DateTime GetDateTime(SqliteDataReader reader, string column)
    {
        if (!HasColumn(reader, column)) return DateTime.UtcNow;
        var value = reader[column];
        if (value is DBNull || value == null) return DateTime.UtcNow;
        return DateTime.TryParse(value.ToString(), out var result) ? result.ToUniversalTime() : DateTime.UtcNow;
    }

    private static DateTime? GetNullableDateTime(SqliteDataReader reader, string column)
    {
        if (!HasColumn(reader, column)) return null;
        var value = reader[column];
        if (value is DBNull || value == null) return null;
        return DateTime.TryParse(value.ToString(), out var result) ? result.ToUniversalTime() : null;
    }

    private static T GetEnum<T>(SqliteDataReader reader, string column, T defaultValue = default!)
        where T : struct, Enum
    {
        if (!HasColumn(reader, column)) return defaultValue;
        var value = reader[column];
        if (value is DBNull || value == null) return defaultValue;
        var str = value.ToString();
        if (int.TryParse(str, out var intVal) && Enum.IsDefined(typeof(T), intVal))
            return (T)(object)intVal;
        if (Enum.TryParse<T>(str, ignoreCase: true, out var enumVal))
            return enumVal;
        return defaultValue;
    }

    private static T? GetNullableEnum<T>(SqliteDataReader reader, string column)
        where T : struct, Enum
    {
        if (!HasColumn(reader, column)) return null;
        var value = reader[column];
        if (value is DBNull || value == null) return null;
        var str = value.ToString();
        if (int.TryParse(str, out var intVal) && Enum.IsDefined(typeof(T), intVal))
            return (T)(object)intVal;
        if (Enum.TryParse<T>(str, ignoreCase: true, out var enumVal))
            return enumVal;
        return null;
    }
}
