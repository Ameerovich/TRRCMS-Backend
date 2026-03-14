// =============================================================================
// GenerateTestUhc — Creates a realistic .uhc test package for TRRCMS
//
// The .uhc file is a renamed SQLite database containing:
//   - manifest table (key-value metadata)
//   - 9 data tables: surveys, buildings, building_documents, property_units,
//     persons, households, person_property_relations, claims, evidences
//
// Scenario: Field collector "collector" surveyed one building in Aleppo
//           (حي الجميلية — Al-Jamiliyah neighbourhood) containing 2 apartments.
//           Two families were found, each occupying one unit.
//           One ownership claim backed by a deed document.
//
// Checksum algorithm:
//   The manifest's "checksum" field contains a SHA-256 hash of all DATA TABLE
//   contents, EXCLUDING the manifest and attachments tables. This avoids the
//   circular dependency where the checksum field's value would change the hash.
//   The same algorithm is used by both the Import Pipeline and the Sync Protocol.
//
// Usage:
//   cd tools/GenerateTestUhc
//   dotnet run
//   → Produces "test-package.uhc" with UNIQUE PackageId + prints content checksum
//   → Run multiple times to generate different packages for testing without duplicates
// =============================================================================

using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.Sqlite;
using System.Linq;

// ── Unique PackageId per run (supports testing multiple packages without duplicates) ──
// All other IDs are deterministically derived from the packageId for consistency.
// This allows running the generator multiple times to create different packages.

var packageId       = Guid.NewGuid();                          // Unique per run
var buildingId      = DeriveGuid(packageId, "building");
var unitId1         = DeriveGuid(packageId, "unit_1");
var unitId2         = DeriveGuid(packageId, "unit_2");
var personId1       = DeriveGuid(packageId, "person_1");
var personId2       = DeriveGuid(packageId, "person_2");
var personId3       = DeriveGuid(packageId, "person_3");
var personId4       = DeriveGuid(packageId, "person_4");
var householdId1    = DeriveGuid(packageId, "household_1");
var householdId2    = DeriveGuid(packageId, "household_2");
var relationId1     = DeriveGuid(packageId, "relation_1");
var relationId2     = DeriveGuid(packageId, "relation_2");
var claimId1        = DeriveGuid(packageId, "claim_1");
var evidenceId1     = DeriveGuid(packageId, "evidence_1");
var surveyId1       = DeriveGuid(packageId, "survey_1");
var buildingDocId1  = DeriveGuid(packageId, "building_doc_1");

// The collector user ID — must match the seeded "collector" user in your DB.
// You will replace this after login (Step 0 of the test guide).
// For now, use a placeholder; the script prints a reminder.
var collectorUserId = Guid.Parse("00000000-0000-0000-0000-000000000000");

var outputPath = Path.Combine(Directory.GetCurrentDirectory(), "test-package.uhc");

// Delete previous file if it exists
if (File.Exists(outputPath))
    File.Delete(outputPath);

Console.WriteLine("=== TRRCMS Test .uhc Package Generator ===\n");

string contentChecksum;

// ── Create SQLite database (the .uhc file) ─────────────────────────────────────
var connStr = new SqliteConnectionStringBuilder
{
    DataSource = outputPath,
    Mode = SqliteOpenMode.ReadWriteCreate,
    Pooling = false
}.ToString();

using (var conn = new SqliteConnection(connStr))
{
    conn.Open();

    // ── 1. MANIFEST TABLE ──────────────────────────────────────────────────────
    Execute(conn, @"
        CREATE TABLE manifest (
            key   TEXT PRIMARY KEY,
            value TEXT
        );");

    var vocabVersions = @"{
        ""ownership_type"": ""1.0.0"",
        ""document_type"": ""1.0.0"",
        ""building_type"": ""1.0.0"",
        ""property_unit_type"": ""1.0.0"",
        ""claim_type"": ""1.0.0"",
        ""relation_type"": ""1.0.0"",
        ""evidence_type"": ""1.0.0""
    }";

    // Insert manifest with placeholder checksum — will be updated after data tables are populated
    var manifest = new Dictionary<string, string>
    {
        ["package_id"]                = packageId.ToString(),
        ["schema_version"]            = "1.2.0",
        ["created_utc"]               = DateTime.UtcNow.ToString("o"),
        ["device_id"]                 = "TABLET-TEST-001",
        ["app_version"]               = "1.0.0",
        ["exported_by_user_id"]       = collectorUserId.ToString(),
        ["exported_date_utc"]         = DateTime.UtcNow.ToString("o"),
        ["checksum"]                  = "", // Placeholder — computed after data tables are populated
        ["digital_signature"]         = "",
        ["form_schema_version"]       = "1.0.0",
        ["survey_count"]              = "1",
        ["building_count"]            = "1",
        ["property_unit_count"]       = "2",
        ["person_count"]              = "4",
        ["household_count"]           = "2",
        ["relation_count"]            = "2",
        ["claim_count"]               = "1",
        ["document_count"]            = "1",
        ["building_document_count"]   = "1",
        ["total_attachment_size_bytes"]= "0",
        ["vocab_versions"]            = vocabVersions
    };

    foreach (var kv in manifest)
    {
        Execute(conn, "INSERT INTO manifest (key, value) VALUES (@k, @v)",
            ("@k", kv.Key), ("@v", kv.Value));
    }

    // ── 2. SURVEYS TABLE ───────────────────────────────────────────────────────
    Execute(conn, @"
        CREATE TABLE surveys (
            id                      TEXT PRIMARY KEY,
            building_id             TEXT NOT NULL,
            survey_date             TEXT NOT NULL,
            property_unit_id        TEXT,
            gps_coordinates         TEXT,
            interviewee_name        TEXT,
            interviewee_relationship TEXT,
            notes                   TEXT,
            field_collector_id      TEXT,
            contact_person_id       TEXT,
            reference_code          TEXT,
            type                    INTEGER,
            source                  INTEGER,
            status                  INTEGER
        );");

    // Survey linked to Building AND to Unit 1 (Ahmed's apartment — the interviewee)
    // contact_person_id → Ahmed (personId1) is the contact person for this survey
    Execute(conn, @"
        INSERT INTO surveys VALUES (
            @id, @bid, @date, @uid,
            '36.2021,37.1343', 'أحمد محمد العلي', 'مالك',
            'مسح ميداني لمبنى سكني في حي الجميلية - حالة المبنى جيدة',
            @cid, @cpid, 'SRV-2026-001', 1, 1, 3
        );",
        ("@id", surveyId1.ToString()),
        ("@bid", buildingId.ToString()),
        ("@date", DateTime.UtcNow.AddHours(-2).ToString("o")),
        ("@uid", unitId1.ToString()),
        ("@cid", collectorUserId.ToString()),
        ("@cpid", personId1.ToString()));

    // ── 3. BUILDINGS TABLE ─────────────────────────────────────────────────────
    // Columns aligned with StagingBuilding entity (no damage_level/number_of_floors/year_of_construction)
    Execute(conn, @"
        CREATE TABLE buildings (
            id                      TEXT PRIMARY KEY,
            governorate_code        TEXT NOT NULL,
            district_code           TEXT NOT NULL,
            sub_district_code       TEXT NOT NULL,
            community_code          TEXT NOT NULL,
            neighborhood_code       TEXT NOT NULL,
            building_number         TEXT NOT NULL,
            building_type           INTEGER,
            building_status         INTEGER,
            number_of_property_units INTEGER,
            number_of_apartments    INTEGER,
            number_of_shops         INTEGER,
            latitude                REAL,
            longitude               REAL,
            building_geometry_wkt   TEXT,
            notes                   TEXT,
            building_id             TEXT,
            governorate_name        TEXT,
            district_name           TEXT,
            sub_district_name       TEXT,
            community_name          TEXT,
            neighborhood_name       TEXT
        );");

    // Aleppo — Al-Jamiliyah neighbourhood
    // Governorate=14 (Aleppo), District=14, SubDistrict=01, Community=010, Neighborhood=011
    // Building number=00001 → BuildingId = 14140101001100001
    Execute(conn, @"
        INSERT INTO buildings VALUES (
            @id, '14', '14', '01', '010', '011', '00001',
            1, 1, 2, 2, 0,
            36.2021, 37.1343,
            'POINT(37.1343 36.2021)',
            'مبنى سكني من 3 طوابق بحالة جيدة',
            '14140101001100001',
            'حلب', 'حلب', 'مركز حلب', 'حلب المدينة', 'الجميلية'
        );",
        ("@id", buildingId.ToString()));

    // ── 3b. BUILDING_DOCUMENTS TABLE ─────────────────────────────────────────
    Execute(conn, @"
        CREATE TABLE building_documents (
            id                  TEXT PRIMARY KEY,
            building_id         TEXT NOT NULL,
            original_file_name  TEXT NOT NULL,
            file_size_bytes     INTEGER NOT NULL,
            file_path           TEXT NOT NULL,
            file_hash           TEXT,
            description         TEXT,
            notes               TEXT
        );");

    // Building photo document — linked to the building
    Execute(conn, @"
        INSERT INTO building_documents VALUES (
            @id, @bid,
            'building_photo_front.jpg',
            512000,
            'documents/building_photo_front.jpg',
            'a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2',
            'صورة واجهة المبنى الأمامية',
            'تم التقاط الصورة أثناء المسح الميداني'
        );",
        ("@id", buildingDocId1.ToString()),
        ("@bid", buildingId.ToString()));

    // ── 4. PROPERTY_UNITS TABLE ────────────────────────────────────────────────
    // Columns aligned with StagingPropertyUnit entity (no occupancy_status/damage_level/estimated_area_sqm)
    Execute(conn, @"
        CREATE TABLE property_units (
            id                  TEXT PRIMARY KEY,
            building_id         TEXT NOT NULL,
            unit_identifier     TEXT NOT NULL,
            unit_type           INTEGER,
            status              INTEGER,
            floor_number        INTEGER,
            number_of_rooms     INTEGER,
            area_square_meters  REAL,
            description         TEXT
        );");

    // Unit 1: Ground floor apartment — Occupied
    Execute(conn, @"
        INSERT INTO property_units VALUES (
            @id, @bid, 'شقة 1', 1, 1, 1, 4, 120.5,
            'شقة أرضية مع حديقة صغيرة'
        );",
        ("@id", unitId1.ToString()), ("@bid", buildingId.ToString()));

    // Unit 2: First floor apartment — Occupied
    Execute(conn, @"
        INSERT INTO property_units VALUES (
            @id, @bid, 'شقة 2', 1, 1, 2, 3, 110.0,
            'شقة في الطابق الأول'
        );",
        ("@id", unitId2.ToString()), ("@bid", buildingId.ToString()));

    // ── 5. PERSONS TABLE ───────────────────────────────────────────────────────
    // Columns aligned with StagingPerson entity
    // gender/nationality/relationship_to_head stored as INTEGER enum codes (not TEXT)
    // Enum codes: Gender(Male=1,Female=2), Nationality(Syrian=1), RelationshipToHead(Head=1,Spouse=2)
    Execute(conn, @"
        CREATE TABLE persons (
            id                      TEXT PRIMARY KEY,
            family_name_arabic      TEXT NOT NULL,
            first_name_arabic       TEXT NOT NULL,
            father_name_arabic      TEXT NOT NULL,
            mother_name_arabic      TEXT,
            national_id             TEXT,
            year_of_birth           INTEGER,
            email                   TEXT,
            mobile_number           TEXT,
            phone_number            TEXT,
            gender                  INTEGER,
            nationality             INTEGER,
            household_id            TEXT,
            relationship_to_head    INTEGER,
            is_contact_person       INTEGER DEFAULT 0
        );");

    // Person 1: Ahmed (head of household 1, owner of unit 1, CONTACT PERSON for the survey)
    // Gender=1(Male), Nationality=1(Syrian), RelationshipToHead=1(Head)
    Execute(conn, @"
        INSERT INTO persons VALUES (
            @id, 'العلي', 'أحمد', 'محمد', 'فاطمة',
            '01234567890', 1978, NULL, '+963-944-123456', NULL,
            1, 1, @hid, 1, 1
        );",
        ("@id", personId1.ToString()), ("@hid", householdId1.ToString()));

    // Person 2: Fatima (spouse of Ahmed, household 1)
    // Gender=2(Female), Nationality=1(Syrian), RelationshipToHead=2(Spouse)
    Execute(conn, @"
        INSERT INTO persons VALUES (
            @id, 'الحسن', 'فاطمة', 'علي', 'زينب',
            '01234567891', 1982, NULL, '+963-944-123457', NULL,
            2, 1, @hid, 2, 0
        );",
        ("@id", personId2.ToString()), ("@hid", householdId1.ToString()));

    // Person 3: Omar (head of household 2, tenant of unit 2)
    // Gender=1(Male), Nationality=1(Syrian), RelationshipToHead=1(Head)
    Execute(conn, @"
        INSERT INTO persons VALUES (
            @id, 'الخالد', 'عمر', 'خالد', 'سعاد',
            '09876543210', 1985, NULL, '+963-944-654321', NULL,
            1, 1, @hid, 1, 0
        );",
        ("@id", personId3.ToString()), ("@hid", householdId2.ToString()));

    // Person 4: Mariam (spouse of Omar, household 2)
    // Gender=2(Female), Nationality=1(Syrian), RelationshipToHead=2(Spouse)
    Execute(conn, @"
        INSERT INTO persons VALUES (
            @id, 'الرشيد', 'مريم', 'رشيد', 'هدى',
            NULL, 1988, NULL, '+963-944-654322', NULL,
            2, 1, @hid, 2, 0
        );",
        ("@id", personId4.ToString()), ("@hid", householdId2.ToString()));

    // ── 6. HOUSEHOLDS TABLE ────────────────────────────────────────────────────
    // Columns aligned with StagingHousehold entity (no is_female_headed/is_displaced)
    Execute(conn, @"
        CREATE TABLE households (
            id                          TEXT PRIMARY KEY,
            property_unit_id            TEXT NOT NULL,
            head_of_household_name      TEXT NOT NULL,
            household_size              INTEGER NOT NULL,
            head_of_household_person_id TEXT,
            male_count                  INTEGER DEFAULT 0,
            female_count                INTEGER DEFAULT 0,
            male_child_count            INTEGER DEFAULT 0,
            female_child_count          INTEGER DEFAULT 0,
            male_elderly_count          INTEGER DEFAULT 0,
            female_elderly_count        INTEGER DEFAULT 0,
            male_disabled_count         INTEGER DEFAULT 0,
            female_disabled_count       INTEGER DEFAULT 0,
            notes                       TEXT
        );");

    // Household 1: Ahmed's family (4 members: 2 adults + 2 children) in Unit 1
    Execute(conn, @"
        INSERT INTO households VALUES (
            @id, @uid, 'أحمد محمد العلي', 4, @pid,
            1, 1, 1, 1, 0, 0, 0, 0,
            'عائلة مقيمة منذ 2005'
        );",
        ("@id", householdId1.ToString()),
        ("@uid", unitId1.ToString()),
        ("@pid", personId1.ToString()));

    // Household 2: Omar's family (3 members: 2 adults + 1 child) in Unit 2
    Execute(conn, @"
        INSERT INTO households VALUES (
            @id, @uid, 'عمر خالد الخالد', 3, @pid,
            1, 1, 1, 0, 0, 0, 0, 0,
            'عائلة نازحة من الرقة منذ 2016'
        );",
        ("@id", householdId2.ToString()),
        ("@uid", unitId2.ToString()),
        ("@pid", personId3.ToString()));

    // ── 7. PERSON_PROPERTY_RELATIONS TABLE ─────────────────────────────────────
    // Columns aligned with StagingPersonPropertyRelation entity
    // RelationType enum: Owner=1, Occupant=2, Tenant=3, Guest=4, Heir=5, Other=99
    Execute(conn, @"
        CREATE TABLE person_property_relations (
            id                      TEXT PRIMARY KEY,
            person_id               TEXT NOT NULL,
            property_unit_id        TEXT NOT NULL,
            relation_type           INTEGER NOT NULL,
            ownership_share         REAL,
            notes                   TEXT
        );");

    // Ahmed owns Unit 1 (100% ownership since 2005)
    Execute(conn, @"
        INSERT INTO person_property_relations VALUES (
            @id, @pid, @uid, 1, 100.0, 'مالك بموجب سند ملكية'
        );",
        ("@id", relationId1.ToString()),
        ("@pid", personId1.ToString()),
        ("@uid", unitId1.ToString()));

    // Omar is tenant of Unit 2 (rental contract)
    Execute(conn, @"
        INSERT INTO person_property_relations VALUES (
            @id, @pid, @uid, 3, NULL, 'مستأجر بعقد سنوي'
        );",
        ("@id", relationId2.ToString()),
        ("@pid", personId3.ToString()),
        ("@uid", unitId2.ToString()));

    // ── 8. CLAIMS TABLE ────────────────────────────────────────────────────────
    // Columns aligned with StagingClaim entity
    // ClaimSource enum: FieldCollection=1, OfficeSubmission=2
    // TenureContractType enum: FullOwnership=1, SharedOwnership=2, LongTermRental=3, etc.
    Execute(conn, @"
        CREATE TABLE claims (
            id                      TEXT PRIMARY KEY,
            property_unit_id        TEXT NOT NULL,
            claim_type              TEXT NOT NULL,
            claim_source            INTEGER NOT NULL,
            primary_claimant_id     TEXT,
            tenure_contract_type    INTEGER,
            ownership_share         REAL,
            claim_description       TEXT
        );");

    // Ownership claim for Unit 1 by Ahmed
    // ClaimSource=1(FieldCollection), TenureContractType=1(FullOwnership)
    Execute(conn, @"
        INSERT INTO claims VALUES (
            @id, @uid, 'Ownership', 1, @pid, 1, 100.0,
            'مطالبة ملكية للشقة 1 بناءً على سند ملكية أصلي'
        );",
        ("@id", claimId1.ToString()),
        ("@uid", unitId1.ToString()),
        ("@pid", personId1.ToString()));

    // ── 9. EVIDENCES TABLE ─────────────────────────────────────────────────────
    Execute(conn, @"
        CREATE TABLE evidences (
            id                          TEXT PRIMARY KEY,
            evidence_type               INTEGER NOT NULL,
            description                 TEXT,
            original_file_name          TEXT,
            file_path                   TEXT,
            file_size_bytes             INTEGER,
            person_id                   TEXT,
            person_property_relation_id TEXT,
            claim_id                    TEXT,
            file_hash                   TEXT,
            document_issued_date        TEXT,
            document_expiry_date        TEXT,
            issuing_authority           TEXT,
            document_reference_number   TEXT,
            notes                       TEXT
        );");

    // Ownership deed document — linked to Ahmed (person), his ownership relation, and his claim
    Execute(conn, @"
        INSERT INTO evidences VALUES (
            @id, 2,
            'سند ملكية أصلي صادر عن السجل العقاري في حلب',
            'ownership_deed_ahmed_45678.pdf',
            'documents/ownership_deed_ahmed_45678.pdf',
            245760,
            @pid, @rid, @cid,
            'e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855',
            '2005-03-15', NULL,
            'السجل العقاري - حلب',
            '45678/2005',
            'نسخة مصورة عن السند الأصلي'
        );",
        ("@id", evidenceId1.ToString()),
        ("@pid", personId1.ToString()),
        ("@rid", relationId1.ToString()),
        ("@cid", claimId1.ToString()));

    // ── 10. COMPUTE CONTENT CHECKSUM ───────────────────────────────────────────
    // Algorithm (must match server-side IImportService.ComputeContentChecksumAsync):
    //   1. Enumerate all tables except 'manifest', 'attachments', sqlite_* — sorted alphabetically
    //   2. For each table: emit "TABLE:tablename\n"
    //   3. Read all rows ordered by rowid
    //   4. For each row: columns sorted alphabetically as "col=value" tab-separated,
    //      NULL → "\0", terminated by "\n"
    //   5. SHA-256 hash the entire UTF-8 byte sequence → lowercase hex

    contentChecksum = ComputeContentChecksum(conn);

    // Write the computed checksum into the manifest
    Execute(conn, "UPDATE manifest SET value = @v WHERE key = 'checksum'",
        ("@v", contentChecksum));

    conn.Close();
}

// Force SQLite to release all file locks
SqliteConnection.ClearAllPools();
GC.Collect();
GC.WaitForPendingFinalizers();

// ── Print results ──────────────────────────────────────────────────────────────
var fileSize = new FileInfo(outputPath).Length;

Console.WriteLine($"  File:     {outputPath}");
Console.WriteLine($"  Size:     {fileSize:N0} bytes ({fileSize / 1024.0:F1} KB)");
Console.WriteLine($"  Content Checksum (SHA-256): {contentChecksum}");
Console.WriteLine($"  PackageId: {packageId}");
Console.WriteLine();
Console.WriteLine("=== Data Summary ===");
Console.WriteLine("  Building:   1 (Al-Jamiliyah, Aleppo — 14-14-01-010-011-00001)");
Console.WriteLine("  Bldg Docs:  1 (Front photo of the building)");
Console.WriteLine("  Units:      2 apartments (ground floor + 1st floor)");
Console.WriteLine("  Persons:    4 (Ahmed+Fatima family, Omar+Mariam family)");
Console.WriteLine("  Households: 2 (4-person owner family, 3-person displaced tenant family)");
Console.WriteLine("  Relations:  2 (Owner + Tenant)");
Console.WriteLine("  Claims:     1 (Ownership claim by Ahmed for Unit 1)");
Console.WriteLine("  Evidence:   1 (Ownership deed — linked to person, relation, and claim)");
Console.WriteLine("  Survey:     1 (Field survey, Finalized — linked to building and Unit 1)");
Console.WriteLine();
Console.WriteLine("=== FK Linkages ===");
Console.WriteLine("  Survey  → Building ✓, PropertyUnit ✓ (Unit 1), ContactPerson ✓ (Ahmed)");
Console.WriteLine("  Evidence → Person ✓ (Ahmed), Relation ✓ (ownership), Claim ✓ (ownership)");
Console.WriteLine("  Claim   → PropertyUnit ✓ (Unit 1), PrimaryClaimant ✓ (Ahmed)");
Console.WriteLine("  Household → PropertyUnit ✓, HeadPerson ✓");
Console.WriteLine("  Relation → Person ✓, PropertyUnit ✓");
Console.WriteLine("  BldgDoc → Building ✓");
Console.WriteLine("  Person[Ahmed] → IsContactPerson ✓");
Console.WriteLine();
Console.WriteLine("=== Checksum ===");
Console.WriteLine("  Algorithm: SHA-256 of data table contents only (excluding manifest table)");
Console.WriteLine("  Tables sorted alphabetically, rows by rowid, columns by name");
Console.WriteLine("  Same algorithm used by Import Pipeline and Sync Protocol");
Console.WriteLine();
Console.WriteLine("=== IMPORTANT ===");
Console.WriteLine("  Each run generates a UNIQUE PackageId (no duplicates when re-running).");
Console.WriteLine("  The exported_by_user_id is set to Guid.Empty (replace in login flow if needed).");
Console.WriteLine();
Console.WriteLine("  To use this package in your upload test:");
Console.WriteLine();
Console.WriteLine($"  • Content Checksum:  {contentChecksum}");
Console.WriteLine($"  • PackageId:         {packageId}");
Console.WriteLine();
Console.WriteLine("  Copy 'test-package.uhc' to your testing location and follow the test guide.");

// ── Content Checksum Computation ──────────────────────────────────────────────
// Must exactly match IImportService.ComputeContentChecksumAsync on the server.

static string ComputeContentChecksum(SqliteConnection conn)
{
    var excludedTables = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "manifest", "attachments", "sqlite_sequence"
    };

    // 1. Enumerate data tables, sorted alphabetically
    var tables = new List<string>();
    using (var listCmd = conn.CreateCommand())
    {
        listCmd.CommandText =
            "SELECT name FROM sqlite_master WHERE type='table' ORDER BY name";
        using var reader = listCmd.ExecuteReader();
        while (reader.Read())
        {
            var tableName = reader.GetString(0);
            if (!tableName.StartsWith("sqlite_", StringComparison.OrdinalIgnoreCase)
                && !excludedTables.Contains(tableName))
            {
                tables.Add(tableName);
            }
        }
    }

    // 2. Build canonical representation and hash incrementally
    using var sha256 = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
    var encoding = Encoding.UTF8;

    foreach (var table in tables)
    {
        // Table header
        sha256.AppendData(encoding.GetBytes($"TABLE:{table}\n"));

        // Get column names sorted alphabetically (ordinal)
        var columns = new List<string>();
        using (var pragmaCmd = conn.CreateCommand())
        {
            pragmaCmd.CommandText = $"PRAGMA table_info(\"{table}\")";
            using var pragmaReader = pragmaCmd.ExecuteReader();
            while (pragmaReader.Read())
            {
                columns.Add(pragmaReader.GetString(1)); // column name is at index 1
            }
        }
        columns.Sort(StringComparer.Ordinal);

        // Read all rows ordered by rowid
        using var rowCmd = conn.CreateCommand();
        rowCmd.CommandText = $"SELECT * FROM \"{table}\" ORDER BY rowid";
        using var rowReader = rowCmd.ExecuteReader();

        // Build column index lookup
        var colIndexMap = new Dictionary<string, int>(StringComparer.Ordinal);
        for (var i = 0; i < rowReader.FieldCount; i++)
        {
            colIndexMap[rowReader.GetName(i)] = i;
        }

        while (rowReader.Read())
        {
            var parts = new List<string>(columns.Count);
            foreach (var col in columns)
            {
                if (colIndexMap.TryGetValue(col, out var idx))
                {
                    var value = rowReader.IsDBNull(idx) ? "\\0" : rowReader.GetValue(idx)?.ToString() ?? "\\0";
                    parts.Add($"{col}={value}");
                }
            }

            var rowLine = string.Join("\t", parts) + "\n";
            sha256.AppendData(encoding.GetBytes(rowLine));
        }
    }

    var hashBytes = sha256.GetHashAndReset();
    return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
}

// ── Helpers ────────────────────────────────────────────────────────────────────────

/// <summary>
/// Derive a deterministic GUID from a base GUID and a seed string.
/// Used to generate related entity IDs that are consistent within a package.
/// (Not cryptographically secure, just for deterministic testing.)
/// </summary>
static Guid DeriveGuid(Guid baseGuid, string seed)
{
    using var hash = SHA256.Create();
    var input = Encoding.UTF8.GetBytes(baseGuid.ToString() + ":" + seed);
    var hashBytes = hash.ComputeHash(input);
    return new Guid(hashBytes.Take(16).ToArray());
}

static void Execute(SqliteConnection conn, string sql, params (string name, string value)[] parameters)
{
    using var cmd = conn.CreateCommand();
    cmd.CommandText = sql;
    foreach (var (name, value) in parameters)
        cmd.Parameters.AddWithValue(name, value);
    cmd.ExecuteNonQuery();
}
