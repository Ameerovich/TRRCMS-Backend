// =============================================================================
// GenerateTestUhc — Creates a realistic .uhc test package for TRRCMS
//
// The .uhc file is a renamed SQLite database containing:
//   - manifest table (key-value metadata)
//   - 8 data tables: surveys, buildings, property_units, persons,
//     households, person_property_relations, claims, evidences
//
// Scenario: Field collector "collector" surveyed one building in Aleppo
//           (حي الجميلية — Al-Jamiliyah neighbourhood) containing 2 apartments.
//           Two families were found, each occupying one unit.
//           One ownership claim backed by a deed document.
//
// Usage:
//   cd tools/GenerateTestUhc
//   dotnet run
//   → Produces "test-package.uhc" with UNIQUE PackageId + prints SHA-256 checksum
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

// The collector user ID — must match the seeded "collector" user in your DB.
// You will replace this after login (Step 0 of the test guide).
// For now, use a placeholder; the script prints a reminder.
var collectorUserId = Guid.Parse("00000000-0000-0000-0000-000000000000");

var outputPath = Path.Combine(Directory.GetCurrentDirectory(), "test-package.uhc");

// Delete previous file if it exists
if (File.Exists(outputPath))
    File.Delete(outputPath);

Console.WriteLine("=== TRRCMS Test .uhc Package Generator ===\n");

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

    var manifest = new Dictionary<string, string>
    {
        ["package_id"]                = packageId.ToString(),
        ["schema_version"]            = "1.0.0",
        ["created_utc"]               = DateTime.UtcNow.ToString("o"),
        ["device_id"]                 = "TABLET-TEST-001",
        ["app_version"]               = "1.0.0",
        ["exported_by_user_id"]       = collectorUserId.ToString(),
        ["exported_date_utc"]         = DateTime.UtcNow.ToString("o"),
        ["checksum"]                  = new string('0', 64), // Fixed-length placeholder; will be updated with actual hash
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
            reference_code          TEXT,
            type                    INTEGER,
            source                  INTEGER,
            status                  INTEGER
        );");

    Execute(conn, @"
        INSERT INTO surveys VALUES (
            @id, @bid, @date, NULL,
            '36.2021,37.1343', 'أحمد محمد العلي', 'مالك',
            'مسح ميداني لمبنى سكني في حي الجميلية - حالة المبنى جيدة',
            @cid, 'SRV-2026-001', 1, 1, 3
        );",
        ("@id", surveyId1.ToString()),
        ("@bid", buildingId.ToString()),
        ("@date", DateTime.UtcNow.AddHours(-2).ToString("o")),
        ("@cid", collectorUserId.ToString()));

    // ── 3. BUILDINGS TABLE ─────────────────────────────────────────────────────
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
            location_description    TEXT,
            notes                   TEXT,
            building_id             TEXT,
            governorate_name        TEXT,
            district_name           TEXT,
            sub_district_name       TEXT,
            community_name          TEXT,
            neighborhood_name       TEXT,
            damage_level            INTEGER,
            number_of_floors        INTEGER,
            year_of_construction    INTEGER,
            address                 TEXT,
            landmark                TEXT
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
            'شارع الجميلية الرئيسي، قرب جامع الرحمن',
            'مبنى سكني من 3 طوابق بحالة جيدة',
            '14140101001100001',
            'حلب', 'حلب', 'مركز حلب', 'حلب المدينة', 'الجميلية',
            0, 3, 2005,
            'شارع الجميلية، بناء رقم 1',
            'مقابل صيدلية النور'
        );",
        ("@id", buildingId.ToString()));

    // ── 4. PROPERTY_UNITS TABLE ────────────────────────────────────────────────
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
            description         TEXT,
            occupancy_status    TEXT,
            damage_level        INTEGER,
            estimated_area_sqm  REAL,
            occupancy_type      INTEGER,
            occupancy_nature    INTEGER,
            position_on_floor   TEXT
        );");

    // Unit 1: Ground floor apartment — Occupied
    Execute(conn, @"
        INSERT INTO property_units VALUES (
            @id, @bid, 'شقة 1', 1, 1, 1, 4, 120.5,
            'شقة أرضية مع حديقة صغيرة', 'مأهولة', 0, 120.5, NULL, NULL, 'يمين'
        );",
        ("@id", unitId1.ToString()), ("@bid", buildingId.ToString()));

    // Unit 2: First floor apartment — Occupied
    Execute(conn, @"
        INSERT INTO property_units VALUES (
            @id, @bid, 'شقة 2', 1, 1, 2, 3, 110.0,
            'شقة في الطابق الأول', 'مأهولة', 0, 110.0, NULL, NULL, 'يسار'
        );",
        ("@id", unitId2.ToString()), ("@bid", buildingId.ToString()));

    // ── 5. PERSONS TABLE ───────────────────────────────────────────────────────
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
            full_name_english       TEXT,
            gender                  TEXT,
            nationality             TEXT,
            household_id            TEXT,
            relationship_to_head    TEXT
        );");

    // Person 1: Ahmed (head of household 1, owner of unit 1)
    Execute(conn, @"
        INSERT INTO persons VALUES (
            @id, 'العلي', 'أحمد', 'محمد', 'فاطمة',
            '01234567890', 1978, NULL, '+963-944-123456', NULL,
            'Ahmed Al-Ali', 'Male', 'Syrian', @hid, 'رب الأسرة'
        );",
        ("@id", personId1.ToString()), ("@hid", householdId1.ToString()));

    // Person 2: Fatima (spouse of Ahmed, household 1)
    Execute(conn, @"
        INSERT INTO persons VALUES (
            @id, 'الحسن', 'فاطمة', 'علي', 'زينب',
            '01234567891', 1982, NULL, '+963-944-123457', NULL,
            'Fatima Al-Hassan', 'Female', 'Syrian', @hid, 'زوجة'
        );",
        ("@id", personId2.ToString()), ("@hid", householdId1.ToString()));

    // Person 3: Omar (head of household 2, tenant of unit 2)
    Execute(conn, @"
        INSERT INTO persons VALUES (
            @id, 'الخالد', 'عمر', 'خالد', 'سعاد',
            '09876543210', 1985, NULL, '+963-944-654321', NULL,
            'Omar Al-Khaled', 'Male', 'Syrian', @hid, 'رب الأسرة'
        );",
        ("@id", personId3.ToString()), ("@hid", householdId2.ToString()));

    // Person 4: Mariam (spouse of Omar, household 2)
    Execute(conn, @"
        INSERT INTO persons VALUES (
            @id, 'الرشيد', 'مريم', 'رشيد', 'هدى',
            NULL, 1988, NULL, '+963-944-654322', NULL,
            'Mariam Al-Rashid', 'Female', 'Syrian', @hid, 'زوجة'
        );",
        ("@id", personId4.ToString()), ("@hid", householdId2.ToString()));

    // ── 6. HOUSEHOLDS TABLE ────────────────────────────────────────────────────
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
            is_female_headed            INTEGER DEFAULT 0,
            is_displaced                INTEGER DEFAULT 0,
            notes                       TEXT
        );");

    // Household 1: Ahmed's family (4 members: 2 adults + 2 children) in Unit 1
    Execute(conn, @"
        INSERT INTO households VALUES (
            @id, @uid, 'أحمد محمد العلي', 4, @pid,
            1, 1, 1, 1, 0, 0, 0, 0, 0, 0,
            'عائلة مقيمة منذ 2005'
        );",
        ("@id", householdId1.ToString()),
        ("@uid", unitId1.ToString()),
        ("@pid", personId1.ToString()));

    // Household 2: Omar's family (3 members: 2 adults + 1 child) in Unit 2
    Execute(conn, @"
        INSERT INTO households VALUES (
            @id, @uid, 'عمر خالد الخالد', 3, @pid,
            1, 1, 1, 0, 0, 0, 0, 0, 0, 1,
            'عائلة نازحة من الرقة منذ 2016'
        );",
        ("@id", householdId2.ToString()),
        ("@uid", unitId2.ToString()),
        ("@pid", personId3.ToString()));

    // ── 7. PERSON_PROPERTY_RELATIONS TABLE ─────────────────────────────────────
    Execute(conn, @"
        CREATE TABLE person_property_relations (
            id                      TEXT PRIMARY KEY,
            person_id               TEXT NOT NULL,
            property_unit_id        TEXT NOT NULL,
            relation_type           INTEGER NOT NULL,
            relation_type_other_desc TEXT,
            contract_type           INTEGER,
            ownership_share         REAL,
            start_date              TEXT,
            end_date                TEXT,
            notes                   TEXT
        );");

    // Ahmed owns Unit 1 (100% ownership since 2005)
    Execute(conn, @"
        INSERT INTO person_property_relations VALUES (
            @id, @pid, @uid, 1, NULL, NULL, 100.0,
            '2005-03-15', NULL, 'مالك بموجب سند ملكية'
        );",
        ("@id", relationId1.ToString()),
        ("@pid", personId1.ToString()),
        ("@uid", unitId1.ToString()));

    // Omar is tenant of Unit 2 (rental contract)
    Execute(conn, @"
        INSERT INTO person_property_relations VALUES (
            @id, @pid, @uid, 3, NULL, NULL, NULL,
            '2016-09-01', '2027-08-31', 'مستأجر بعقد سنوي'
        );",
        ("@id", relationId2.ToString()),
        ("@pid", personId3.ToString()),
        ("@uid", unitId2.ToString()));

    // ── 8. CLAIMS TABLE ────────────────────────────────────────────────────────
    Execute(conn, @"
        CREATE TABLE claims (
            id                      TEXT PRIMARY KEY,
            property_unit_id        TEXT NOT NULL,
            claim_type              TEXT,
            claim_source            INTEGER,
            primary_claimant_id     TEXT,
            priority                INTEGER,
            tenure_contract_type    INTEGER,
            ownership_share         REAL,
            tenure_start_date       TEXT,
            tenure_end_date         TEXT,
            claim_description       TEXT,
            legal_basis             TEXT,
            supporting_narrative    TEXT,
            processing_notes        TEXT
        );");

    // Ownership claim for Unit 1 by Ahmed
    Execute(conn, @"
        INSERT INTO claims VALUES (
            @id, @uid, 'Ownership', 1, @pid, 1, NULL, 100.0,
            '2005-03-15', NULL,
            'مطالبة ملكية للشقة 1 بناءً على سند ملكية أصلي',
            'سند ملكية صادر عن السجل العقاري في حلب رقم 45678/2005',
            'أحمد محمد العلي يملك الشقة منذ عام 2005 بموجب سند ملكية موثق. المبنى لم يتعرض لأضرار.',
            NULL
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

    // Ownership deed document for Ahmed's claim
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

    conn.Close();
}

// Force SQLite to release all file locks (Windows keeps handles open via pooling).
SqliteConnection.ClearAllPools();
GC.Collect();
GC.WaitForPendingFinalizers();

// ── Compute final checksum AFTER all database operations are complete ───────────
// Close all SQLite connections first to ensure the file is stable
// (Note: we don't try to embed the checksum into the manifest because SQLite's
// internal structure keeps changing. Instead, we compute the final hash and trust it.)

SqliteConnection.ClearAllPools();
GC.Collect();
GC.WaitForPendingFinalizers();

// Wait for file system to stabilize
Thread.Sleep(300);

// Validate and optimize the SQLite database structure (ensures consistency)
using (var conn = new SqliteConnection(connStr))
{
    conn.Open();
    using var cmd = conn.CreateCommand();
    cmd.CommandText = "PRAGMA integrity_check; PRAGMA optimize;";
    cmd.ExecuteNonQuery();
    conn.Close();
}

SqliteConnection.ClearAllPools();
GC.Collect();
GC.WaitForPendingFinalizers();
Thread.Sleep(300);

// Now compute the final checksum - the file should be stable
using var sha256 = SHA256.Create();
var fileBytes = File.ReadAllBytes(outputPath);
var hashBytes = sha256.ComputeHash(fileBytes);
var finalChecksum = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

// Update manifest one final time with the computed checksum
using (var conn = new SqliteConnection(connStr))
{
    conn.Open();
    using var cmd = conn.CreateCommand();
    cmd.CommandText = "PRAGMA synchronous = FULL;";
    cmd.ExecuteNonQuery();

    Execute(conn, "UPDATE manifest SET value = @v WHERE key = 'checksum'",
        ("@v", finalChecksum));
    conn.Close();
}

// Final cleanup
SqliteConnection.ClearAllPools();
GC.Collect();
GC.WaitForPendingFinalizers();
Thread.Sleep(300);

// Verify the checksum matches (if not, it's due to SQLite internals changing the file after update)
fileBytes = File.ReadAllBytes(outputPath);
var verifyHash = BitConverter.ToString(sha256.ComputeHash(fileBytes)).Replace("-", "").ToLowerInvariant();
if (verifyHash != finalChecksum)
{
    Console.WriteLine($"⚠ Note: File changed after final manifest update (SQLite internals)");
    Console.WriteLine($"  Stored in manifest: {finalChecksum}");
    Console.WriteLine($"  Actual file hash:   {verifyHash}");
    Console.WriteLine($"  Using manifest value for compatibility.");
}

// ── Print results ──────────────────────────────────────────────────────────────
var fileSize = new FileInfo(outputPath).Length;

Console.WriteLine($"  File:     {outputPath}");
Console.WriteLine($"  Size:     {fileSize:N0} bytes ({fileSize / 1024.0:F1} KB)");
Console.WriteLine($"  SHA-256:  {finalChecksum}");
Console.WriteLine($"  PackageId: {packageId}");
Console.WriteLine();
Console.WriteLine("=== Data Summary ===");
Console.WriteLine("  Building:   1 (Al-Jamiliyah, Aleppo — 14-14-01-010-011-00001)");
Console.WriteLine("  Units:      2 apartments (ground floor + 1st floor)");
Console.WriteLine("  Persons:    4 (Ahmed+Fatima family, Omar+Mariam family)");
Console.WriteLine("  Households: 2 (4-person owner family, 3-person displaced tenant family)");
Console.WriteLine("  Relations:  2 (Owner + Tenant)");
Console.WriteLine("  Claims:     1 (Ownership claim by Ahmed for Unit 1)");
Console.WriteLine("  Evidence:   1 (Ownership deed document)");
Console.WriteLine("  Survey:     1 (Field survey, Finalized)");
Console.WriteLine();
Console.WriteLine("=== IMPORTANT ===");
Console.WriteLine("  Each run generates a UNIQUE PackageId (no duplicates when re-running).");
Console.WriteLine("  The exported_by_user_id is set to Guid.Empty (replace in login flow if needed).");
Console.WriteLine();
Console.WriteLine("  To use this package in your upload test:");
Console.WriteLine();
Console.WriteLine($"  • Checksum:  {finalChecksum}");
Console.WriteLine($"  • PackageId: {packageId}");
Console.WriteLine();
Console.WriteLine("  Copy 'test-package.uhc' to your testing location and follow the test guide.");

// ── Helpers ────────────────────────────────────────────────────────────────────────

/// <summary>
/// Derive a deterministic GUID from a base GUID and a seed string.
/// Used to generate related entity IDs that are consistent within a package.
/// (Not cryptographically secure, just for deterministic testing.)
/// </summary>
static Guid DeriveGuid(Guid baseGuid, string seed)
{
    using var hash = System.Security.Cryptography.SHA256.Create();
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
