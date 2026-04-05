// =============================================================================
// GenerateTestUhc — Creates a realistic .uhc test package for TRRCMS (v1.7)
//
// The .uhc file is a renamed SQLite database containing:
//   - manifest table (key-value metadata)
//   - 10 data tables: surveys, buildings, building_documents, property_units,
//     persons, households, person_property_relations, claims, evidences,
//     identification_documents
//
// v1.7 changes:
//   - New identification_documents table (ID docs separated from evidence)
//   - evidences: person_id removed, evidence_type values 1,5 removed
//   - persons: relationship_to_head removed from schema
//   - households: occupancy_type removed from schema
//   - Audio files accepted as evidence (voice recordings)
//   - SurveyStatus: Obstructed(4) added (was Interrupted)
//   - Randomized values per run for realistic testing
//
// Usage:
//   cd tools/GenerateTestUhc
//   dotnet run
//   → Produces "test-package.uhc" with UNIQUE data per run
// =============================================================================

using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.Sqlite;
using System.Linq;

// ── Random data generation ────────────────────────────────────────────────────
var rng = new Random();

var arabicFirstNames = new[] { "أحمد", "محمد", "عمر", "خالد", "سعيد", "حسن", "ياسر", "فاطمة", "زينب", "مريم", "سارة", "هدى", "ليلى", "نور" };
var arabicFamilyNames = new[] { "العلي", "الحسن", "الخالد", "الرشيد", "الأحمد", "العمر", "السعيد", "الشامي", "الحلبي", "النعيمي" };
var arabicFatherNames = new[] { "محمد", "علي", "خالد", "رشيد", "حسن", "أحمد", "عمر", "سعيد" };
var arabicMotherNames = new[] { "فاطمة", "زينب", "سعاد", "هدى", "نور", "ليلى", "مريم" };

string RandomNationalId() => string.Concat(Enumerable.Range(0, 11).Select(_ => rng.Next(0, 10)));
string RandomName(string[] pool) => pool[rng.Next(pool.Length)];
int RandomGender() => rng.Next(1, 3); // 1=Male, 2=Female
int RandomNationality() => new[] { 1, 2, 3, 99 }[rng.Next(4)];
int RandomOccupancyNature() => rng.Next(1, 9); // 1-8
int RandomEvidenceType() => new[] { 2, 3, 4, 6, 7, 8, 9, 99 }[rng.Next(8)];
int RandomDocumentType() => rng.Next(1, 4); // 1-3
int RandomSurveyStatus() => new[] { 1, 3, 4 }[rng.Next(3)]; // Draft, Finalized, Obstructed
int RandomFloor() => rng.Next(0, 6);
int RandomRooms() => rng.Next(2, 7);
double RandomArea() => Math.Round(60.0 + rng.NextDouble() * 140.0, 1);
double RandomLatOffset() => (rng.NextDouble() - 0.5) * 0.01;
double RandomLonOffset() => (rng.NextDouble() - 0.5) * 0.01;

// ── Unique IDs per run ────────────────────────────────────────────────────────
var packageId       = Guid.NewGuid();
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
var claimId2        = DeriveGuid(packageId, "claim_2");
var evidenceId1     = DeriveGuid(packageId, "evidence_1");
var idDocId1        = DeriveGuid(packageId, "id_doc_1");
var surveyId1       = DeriveGuid(packageId, "survey_1");
var buildingDocId1  = DeriveGuid(packageId, "building_doc_1");

// ── Randomized values for this run ────────────────────────────────────────────
var buildingNumber  = rng.Next(1, 100000).ToString("D5");
var unitName1       = $"شقة {rng.Next(1, 100)}";
var unitName2       = $"شقة {rng.Next(1, 100)}";
var baseLat         = 36.2021 + RandomLatOffset();
var baseLon         = 37.1343 + RandomLonOffset();

// Person 1 - Owner (contact person)
var p1First = RandomName(arabicFirstNames); var p1Family = RandomName(arabicFamilyNames);
var p1Father = RandomName(arabicFatherNames); var p1Mother = RandomName(arabicMotherNames);
var p1NatId = RandomNationalId(); var p1Gender = RandomGender();

// Person 2 - Spouse of P1
var p2First = RandomName(arabicFirstNames); var p2Family = RandomName(arabicFamilyNames);
var p2Father = RandomName(arabicFatherNames); var p2Mother = RandomName(arabicMotherNames);
var p2NatId = RandomNationalId(); var p2Gender = p1Gender == 1 ? 2 : 1;

// Person 3 - Non-owner (tenant/occupant/heir)
var p3First = RandomName(arabicFirstNames); var p3Family = RandomName(arabicFamilyNames);
var p3Father = RandomName(arabicFatherNames); var p3Mother = RandomName(arabicMotherNames);
var p3NatId = RandomNationalId(); var p3Gender = RandomGender();

// Person 4 - Spouse of P3
var p4First = RandomName(arabicFirstNames); var p4Family = RandomName(arabicFamilyNames);
var p4Father = RandomName(arabicFatherNames); var p4Mother = RandomName(arabicMotherNames);
var p4Gender = p3Gender == 1 ? 2 : 1;

// Relation types: P1=Owner(1), P3=random non-owner
var relation2Type = new[] { 2, 3, 4, 5 }[rng.Next(4)]; // Occupant, Tenant, Guest, Heir
var ownershipShare = rng.Next(100, 2401); // 100-2400
var surveyStatus = RandomSurveyStatus();
var evidenceType = RandomEvidenceType();
var docType = RandomDocumentType();
var occNature1 = RandomOccupancyNature();
var occNature2 = RandomOccupancyNature();

var floor1 = 0; var floor2 = 0; // set inside using block

var collectorUserId = Guid.Parse("00000000-0000-0000-0000-000000000000");

var outputPath = Path.Combine(Directory.GetCurrentDirectory(), "test-package.uhc");
if (File.Exists(outputPath)) File.Delete(outputPath);

Console.WriteLine("=== TRRCMS Test .uhc Package Generator (v1.7) ===\n");

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
        ["schema_version"]            = "1.8.0",
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
        ["claim_count"]               = "2",
        ["document_count"]            = "1",
        ["identification_document_count"] = "1",
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
    // v1.6: Added duration_minutes
    Execute(conn, @"
        CREATE TABLE surveys (
            id                      TEXT PRIMARY KEY,
            building_id             TEXT NOT NULL,
            survey_date             TEXT NOT NULL,
            property_unit_id        TEXT,
            gps_coordinates         TEXT,
            duration_minutes        INTEGER,
            notes                   TEXT,
            field_collector_id      TEXT,
            contact_person_id       TEXT,
            reference_code          TEXT,
            type                    INTEGER,
            source                  INTEGER,
            status                  INTEGER
        );");

    Execute(conn, @"
        INSERT INTO surveys VALUES (
            @id, @bid, @date, @uid,
            @gps,
            @dur,
            'مسح ميداني لمبنى سكني في حي الجميلية',
            @cid, @cpid, @refcode, 1, 1, @status
        );",
        ("@id", surveyId1.ToString()),
        ("@bid", buildingId.ToString()),
        ("@date", DateTime.UtcNow.AddHours(-2).ToString("o")),
        ("@uid", unitId1.ToString()),
        ("@gps", $"{baseLat:F4},{baseLon:F4}"),
        ("@dur", rng.Next(15, 90).ToString()),
        ("@cid", collectorUserId.ToString()),
        ("@cpid", personId1.ToString()),
        ("@refcode", $"SRV-TABLET-TEST-001-{DateTime.UtcNow:yyyyMMddHHmmss}"),
        ("@status", surveyStatus.ToString()));

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

    var fullBuildingCode = $"141401010011{buildingNumber}";
    Execute(conn, @"
        INSERT INTO buildings VALUES (
            @id, '14', '14', '01', '010', '011', @bnum,
            1, 1, 2, 2, 0,
            @lat, @lon,
            @wkt,
            'مبنى سكني بحالة جيدة',
            @bcode,
            'حلب', 'حلب', 'مركز حلب', 'حلب المدينة', 'الجميلية'
        );",
        ("@id", buildingId.ToString()),
        ("@bnum", buildingNumber),
        ("@lat", baseLat.ToString("F4")),
        ("@lon", baseLon.ToString("F4")),
        ("@wkt", $"POINT({baseLon:F4} {baseLat:F4})"),
        ("@bcode", fullBuildingCode));

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

    floor1 = RandomFloor(); floor2 = RandomFloor();
    Execute(conn, @"
        INSERT INTO property_units VALUES (
            @id, @bid, @name, 1, 1, @floor, @rooms, @area, 'وحدة سكنية'
        );",
        ("@id", unitId1.ToString()), ("@bid", buildingId.ToString()),
        ("@name", unitName1), ("@floor", floor1.ToString()),
        ("@rooms", RandomRooms().ToString()), ("@area", RandomArea().ToString()));

    Execute(conn, @"
        INSERT INTO property_units VALUES (
            @id, @bid, @name, 1, 1, @floor, @rooms, @area, 'وحدة سكنية'
        );",
        ("@id", unitId2.ToString()), ("@bid", buildingId.ToString()),
        ("@name", unitName2), ("@floor", floor2.ToString()),
        ("@rooms", RandomRooms().ToString()), ("@area", RandomArea().ToString()));

    // ── 5. PERSONS TABLE ───────────────────────────────────────────────────────
    // v1.7: relationship_to_head removed from schema
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
            is_contact_person       INTEGER DEFAULT 0
        );");

    // Person 1: Owner, contact person
    Execute(conn, @"
        INSERT INTO persons VALUES (
            @id, @family, @first, @father, @mother,
            @natid, @yob, NULL, @phone, NULL,
            @gender, @nat, @hid, 1
        );",
        ("@id", personId1.ToString()), ("@family", p1Family), ("@first", p1First),
        ("@father", p1Father), ("@mother", p1Mother), ("@natid", p1NatId),
        ("@yob", rng.Next(1960, 1995).ToString()), ("@phone", $"+963-9{rng.Next(10,100)}-{rng.Next(100000,999999)}"),
        ("@gender", p1Gender.ToString()), ("@nat", RandomNationality().ToString()),
        ("@hid", householdId1.ToString()));

    // Person 2: Spouse of P1
    Execute(conn, @"
        INSERT INTO persons VALUES (
            @id, @family, @first, @father, @mother,
            @natid, @yob, NULL, @phone, NULL,
            @gender, @nat, @hid, 0
        );",
        ("@id", personId2.ToString()), ("@family", p2Family), ("@first", p2First),
        ("@father", p2Father), ("@mother", p2Mother), ("@natid", p2NatId),
        ("@yob", rng.Next(1965, 1998).ToString()), ("@phone", $"+963-9{rng.Next(10,100)}-{rng.Next(100000,999999)}"),
        ("@gender", p2Gender.ToString()), ("@nat", RandomNationality().ToString()),
        ("@hid", householdId1.ToString()));

    // Person 3: Non-owner relation
    Execute(conn, @"
        INSERT INTO persons VALUES (
            @id, @family, @first, @father, @mother,
            @natid, @yob, NULL, @phone, NULL,
            @gender, @nat, @hid, 0
        );",
        ("@id", personId3.ToString()), ("@family", p3Family), ("@first", p3First),
        ("@father", p3Father), ("@mother", p3Mother), ("@natid", p3NatId),
        ("@yob", rng.Next(1970, 1998).ToString()), ("@phone", $"+963-9{rng.Next(10,100)}-{rng.Next(100000,999999)}"),
        ("@gender", p3Gender.ToString()), ("@nat", RandomNationality().ToString()),
        ("@hid", householdId2.ToString()));

    // Person 4: Spouse of P3
    Execute(conn, @"
        INSERT INTO persons VALUES (
            @id, @family, @first, @father, @mother,
            NULL, @yob, NULL, @phone, NULL,
            @gender, @nat, @hid, 0
        );",
        ("@id", personId4.ToString()), ("@family", p4Family), ("@first", p4First),
        ("@father", p4Father), ("@mother", p4Mother),
        ("@yob", rng.Next(1970, 2000).ToString()), ("@phone", $"+963-9{rng.Next(10,100)}-{rng.Next(100000,999999)}"),
        ("@gender", p4Gender.ToString()), ("@nat", RandomNationality().ToString()),
        ("@hid", householdId2.ToString()));

    // ── 6. HOUSEHOLDS TABLE ────────────────────────────────────────────────────
    // v1.7: occupancy_type removed from schema
    Execute(conn, @"
        CREATE TABLE households (
            id                          TEXT PRIMARY KEY,
            property_unit_id            TEXT NOT NULL,
            household_size              INTEGER NOT NULL,
            male_count                  INTEGER DEFAULT 0,
            female_count                INTEGER DEFAULT 0,
            male_child_count            INTEGER DEFAULT 0,
            female_child_count          INTEGER DEFAULT 0,
            male_elderly_count          INTEGER DEFAULT 0,
            female_elderly_count        INTEGER DEFAULT 0,
            male_disabled_count         INTEGER DEFAULT 0,
            female_disabled_count       INTEGER DEFAULT 0,
            occupancy_nature            INTEGER,
            notes                       TEXT
        );");

    var mc1 = rng.Next(1, 3); var fc1 = rng.Next(1, 3);
    var mcc1 = rng.Next(0, 3); var fcc1 = rng.Next(0, 3);
    var hs1 = mc1 + fc1 + mcc1 + fcc1;
    Execute(conn, @"
        INSERT INTO households VALUES (
            @id, @uid, @size,
            @mc, @fc, @mcc, @fcc, 0, 0, 0, 0,
            @on, 'عائلة مقيمة'
        );",
        ("@id", householdId1.ToString()), ("@uid", unitId1.ToString()),
        ("@size", hs1.ToString()), ("@mc", mc1.ToString()), ("@fc", fc1.ToString()),
        ("@mcc", mcc1.ToString()), ("@fcc", fcc1.ToString()),
        ("@on", occNature1.ToString()));

    var mc2 = rng.Next(1, 3); var fc2 = rng.Next(1, 3);
    var mcc2 = rng.Next(0, 2); var fcc2 = rng.Next(0, 2);
    var hs2 = mc2 + fc2 + mcc2 + fcc2;
    Execute(conn, @"
        INSERT INTO households VALUES (
            @id, @uid, @size,
            @mc, @fc, @mcc, @fcc, 0, 0, 0, 0,
            @on, 'عائلة نازحة'
        );",
        ("@id", householdId2.ToString()), ("@uid", unitId2.ToString()),
        ("@size", hs2.ToString()), ("@mc", mc2.ToString()), ("@fc", fc2.ToString()),
        ("@mcc", mcc2.ToString()), ("@fcc", fcc2.ToString()),
        ("@on", occNature2.ToString()));

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
            occupancy_type          INTEGER,
            has_evidence            INTEGER DEFAULT 0,
            notes                   TEXT
        );");

    // Person 1 owns Unit 1 (ownership relation)
    Execute(conn, @"
        INSERT INTO person_property_relations VALUES (
            @id, @pid, @uid, 1, @share, 1, 1, 'مالك بموجب سند ملكية'
        );",
        ("@id", relationId1.ToString()),
        ("@pid", personId1.ToString()),
        ("@uid", unitId1.ToString()),
        ("@share", ownershipShare.ToString()));

    // Person 3 has non-owner relation to Unit 2
    Execute(conn, @"
        INSERT INTO person_property_relations VALUES (
            @id, @pid, @uid, @rtype, NULL, 2, 0, 'علاقة إشغال'
        );",
        ("@id", relationId2.ToString()),
        ("@pid", personId3.ToString()),
        ("@uid", unitId2.ToString()),
        ("@rtype", relation2Type.ToString()));

    // ── 8. CLAIMS TABLE ────────────────────────────────────────────────────────
    // Columns aligned with StagingClaim entity
    // v1.5: primary_claimant_id is now NOT NULL (required)
    // v1.2: tenure_contract_type removed from .uhc schema
    // ClaimSource enum: FieldCollection=1, OfficeSubmission=2
    Execute(conn, @"
        CREATE TABLE claims (
            id                      TEXT PRIMARY KEY,
            property_unit_id        TEXT NOT NULL,
            claim_type              INTEGER NOT NULL,
            claim_source            INTEGER NOT NULL,
            primary_claimant_id     TEXT NOT NULL,
            originating_survey_id   TEXT,
            ownership_share         REAL,
            claim_description       TEXT
        );");

    // Claim 1: Ownership claim by P1 for Unit 1
    Execute(conn, @"
        INSERT INTO claims VALUES (
            @id, @uid, 1, 1, @pid, @sid, @share,
            'مطالبة ملكية بناءً على سند ملكية'
        );",
        ("@id", claimId1.ToString()),
        ("@uid", unitId1.ToString()),
        ("@pid", personId1.ToString()),
        ("@sid", surveyId1.ToString()),
        ("@share", ownershipShare.ToString()));

    // Claim 2: Occupancy claim by P3 for Unit 2
    Execute(conn, @"
        INSERT INTO claims VALUES (
            @id, @uid, 2, 1, @pid, @sid, NULL,
            'مطالبة إشغال'
        );",
        ("@id", claimId2.ToString()),
        ("@uid", unitId2.ToString()),
        ("@pid", personId3.ToString()),
        ("@sid", surveyId1.ToString()));

    // ── 9. EVIDENCES TABLE ─────────────────────────────────────────────────────
    // v1.7: person_id removed, evidence_type values 1,5 removed
    Execute(conn, @"
        CREATE TABLE evidences (
            id                          TEXT PRIMARY KEY,
            evidence_type               INTEGER NOT NULL,
            description                 TEXT,
            original_file_name          TEXT,
            file_path                   TEXT,
            file_size_bytes             INTEGER,
            person_property_relation_id TEXT,
            claim_id                    TEXT,
            file_hash                   TEXT,
            document_issued_date        TEXT,
            document_expiry_date        TEXT,
            issuing_authority           TEXT,
            document_reference_number   TEXT,
            notes                       TEXT
        );");

    // Tenure evidence — linked to ownership relation and claim (no person_id)
    Execute(conn, @"
        INSERT INTO evidences VALUES (
            @id, @etype,
            'وثيقة حيازة',
            'tenure_document.pdf',
            'documents/tenure_document.pdf',
            245760,
            @rid, @cid,
            'e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855',
            '2005-03-15', NULL,
            'السجل العقاري - حلب',
            '45678/2005',
            'نسخة مصورة عن الوثيقة'
        );",
        ("@id", evidenceId1.ToString()),
        ("@etype", evidenceType.ToString()),
        ("@rid", relationId1.ToString()),
        ("@cid", claimId1.ToString()));

    // ── 9b. EVIDENCE RELATIONS JUNCTION TABLE ───────────────────────────────────
    // v1.8: Many-to-many evidence-to-relation links
    var evidRelId1 = DeriveGuid(packageId, "evid_rel_1");
    var evidRelId2 = DeriveGuid(packageId, "evid_rel_2");

    Execute(conn, @"
        CREATE TABLE evidence_relations (
            id                          TEXT PRIMARY KEY,
            evidence_id                 TEXT NOT NULL REFERENCES evidences(id),
            person_property_relation_id TEXT NOT NULL REFERENCES person_property_relations(id)
        );");

    // Link evidence to both relations (owner + tenant) — tests many-to-many
    Execute(conn, @"INSERT INTO evidence_relations VALUES (@id, @eid, @rid);",
        ("@id", evidRelId1.ToString()),
        ("@eid", evidenceId1.ToString()),
        ("@rid", relationId1.ToString()));

    Execute(conn, @"INSERT INTO evidence_relations VALUES (@id, @eid, @rid);",
        ("@id", evidRelId2.ToString()),
        ("@eid", evidenceId1.ToString()),
        ("@rid", relationId2.ToString()));

    // ── 9c. IDENTIFICATION DOCUMENTS TABLE ─────────────────────────────────────
    // v1.7: NEW table for personal ID documents
    Execute(conn, @"
        CREATE TABLE identification_documents (
            id                          TEXT PRIMARY KEY,
            person_id                   TEXT NOT NULL,
            document_type               INTEGER NOT NULL,
            description                 TEXT NOT NULL,
            original_file_name          TEXT NOT NULL,
            file_path                   TEXT,
            file_size_bytes             INTEGER,
            file_hash                   TEXT,
            document_issued_date        TEXT,
            document_expiry_date        TEXT,
            issuing_authority           TEXT,
            document_reference_number   TEXT,
            notes                       TEXT
        );");

    // ID document for Person 1
    Execute(conn, @"
        INSERT INTO identification_documents VALUES (
            @id, @pid, @dtype,
            'وثيقة هوية شخصية',
            'id_document.jpg',
            'documents/id_document.jpg',
            128000,
            'a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2',
            NULL, NULL,
            'الأحوال المدنية',
            @natid,
            NULL
        );",
        ("@id", idDocId1.ToString()),
        ("@pid", personId1.ToString()),
        ("@dtype", docType.ToString()),
        ("@natid", p1NatId));

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

var relationTypeNames = new Dictionary<int, string> { {1,"Owner"}, {2,"Occupant"}, {3,"Tenant"}, {4,"Guest"}, {5,"Heir"} };
var statusNames = new Dictionary<int, string> { {1,"Draft"}, {3,"Finalized"}, {4,"Obstructed"} };

Console.WriteLine($"  File:     {outputPath}");
Console.WriteLine($"  Size:     {fileSize:N0} bytes ({fileSize / 1024.0:F1} KB)");
Console.WriteLine($"  Schema:   v1.8.0");
Console.WriteLine($"  PackageId: {packageId}");
Console.WriteLine($"  Checksum:  {contentChecksum}");
Console.WriteLine();
Console.WriteLine("=== Randomized Data ===");
Console.WriteLine($"  Building:   14-14-01-010-011-{buildingNumber} ({baseLat:F4}, {baseLon:F4})");
Console.WriteLine($"  Units:      {unitName1} (floor {floor1}), {unitName2} (floor {floor2})");
Console.WriteLine($"  Person 1:   {p1First} {p1Family} (ID: {p1NatId}, Gender: {p1Gender}) — Owner, ContactPerson");
Console.WriteLine($"  Person 2:   {p2First} {p2Family} (ID: {p2NatId}, Gender: {p2Gender}) — Spouse of P1");
Console.WriteLine($"  Person 3:   {p3First} {p3Family} (ID: {p3NatId}, Gender: {p3Gender}) — {relationTypeNames.GetValueOrDefault(relation2Type, "Other")}");
Console.WriteLine($"  Person 4:   {p4First} {p4Family} (Gender: {p4Gender}) — Spouse of P3");
Console.WriteLine($"  Claim 1:    OwnershipClaim (share: {ownershipShare}/2400) by P1");
Console.WriteLine($"  Claim 2:    OccupancyClaim by P3 (relation: {relationTypeNames.GetValueOrDefault(relation2Type, "Other")})");
Console.WriteLine($"  Evidence:   type={evidenceType} (tenure doc), linked to 2 relations via junction table");
Console.WriteLine($"  ID Doc:     type={docType} for P1");
Console.WriteLine($"  Survey:     status={statusNames.GetValueOrDefault(surveyStatus, "Unknown")} ({surveyStatus})");
Console.WriteLine();
Console.WriteLine("  Each run generates UNIQUE values. Run again for different test data.");

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
