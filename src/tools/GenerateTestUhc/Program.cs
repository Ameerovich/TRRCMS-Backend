// =============================================================================
// GenerateTestUhc — Creates a realistic .uhc test package for TRRCMS (v1.9)
//
// The .uhc file is a renamed SQLite database containing:
//   - manifest table (key-value metadata)
//   - 11 data tables: surveys, buildings, building_documents, property_units,
//     persons, households, person_property_relations, claims, evidences,
//     evidence_relations (M:N junction), identification_documents
//
// v1.9 changes (current):
//   - households: per-gender age/disability columns removed; adult_count,
//     disabled_count, occupancy_start_date added. MaleCount/FemaleCount now
//     mean total male/female across all ages (were adult-only).
//   - household_size is now computed from linked persons so
//     HouseholdStructureValidator stays clean.
//   - manifest.vocab_versions is now empty ({}) — the server treats a missing
//     dictionary as "no declared dependencies", removing the stale
//     version-drift warnings the test generator used to produce.
//
// v1.8 changes:
//   - evidence_relations junction table (M:N evidence ↔ person_property_relation)
//
// v1.7 changes:
//   - identification_documents table (personal ID docs separated from evidence)
//   - evidences: person_id removed, evidence_type values 1,5 removed
//   - persons: relationship_to_head removed
//   - SurveyStatus: Obstructed(4) replaces Interrupted
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
// Guarantee the two unit identifiers are unique within the same building —
// BuildingUnitCodeValidator (level 8) rejects duplicate unit identifiers per building.
var unitNum1        = rng.Next(1, 100);
int unitNum2;
do { unitNum2 = rng.Next(1, 100); } while (unitNum2 == unitNum1);
var unitName1       = $"شقة {unitNum1}";
var unitName2       = $"شقة {unitNum2}";
var baseLat         = 36.2021 + RandomLatOffset();
var baseLon         = 37.1343 + RandomLonOffset();

// Year-of-birth range: 1955–2005 → ages 21–71 in 2026 → can be adult or elderly (>=60)
// but never child. The generator never produces children, so child_count is always 0.
int RandomYob() => rng.Next(1955, 2006);
int AgeFromYob(int yob) => DateTime.UtcNow.Year - yob;
bool IsElderly(int yob) => AgeFromYob(yob) >= 60;

// Person 1 - Owner (contact person)
var p1First = RandomName(arabicFirstNames); var p1Family = RandomName(arabicFamilyNames);
var p1Father = RandomName(arabicFatherNames); var p1Mother = RandomName(arabicMotherNames);
var p1NatId = RandomNationalId(); var p1Gender = RandomGender(); var p1Yob = RandomYob();

// Person 2 - Spouse of P1
var p2First = RandomName(arabicFirstNames); var p2Family = RandomName(arabicFamilyNames);
var p2Father = RandomName(arabicFatherNames); var p2Mother = RandomName(arabicMotherNames);
var p2NatId = RandomNationalId(); var p2Gender = p1Gender == 1 ? 2 : 1; var p2Yob = RandomYob();

// Person 3 - Non-owner (tenant/occupant/heir)
var p3First = RandomName(arabicFirstNames); var p3Family = RandomName(arabicFamilyNames);
var p3Father = RandomName(arabicFatherNames); var p3Mother = RandomName(arabicMotherNames);
var p3NatId = RandomNationalId(); var p3Gender = RandomGender(); var p3Yob = RandomYob();

// Person 4 - Spouse of P3
var p4First = RandomName(arabicFirstNames); var p4Family = RandomName(arabicFamilyNames);
var p4Father = RandomName(arabicFatherNames); var p4Mother = RandomName(arabicMotherNames);
var p4Gender = p3Gender == 1 ? 2 : 1; var p4Yob = RandomYob();

// Household composition — derived from the 2 linked persons per household so
// HouseholdStructureValidator never fires a "declared size X but Y linked" warning
// and the upper-bound cross-field rules are guaranteed to pass.
int GenderCount(int[] genders, int target) => genders.Count(g => g == target);
int ElderlyCount(int[] yobs) => yobs.Count(IsElderly);

var hh1Genders = new[] { p1Gender, p2Gender };
var hh1Yobs    = new[] { p1Yob, p2Yob };
var hh1Size    = 2;
var hh1Male    = GenderCount(hh1Genders, 1);
var hh1Female  = GenderCount(hh1Genders, 2);
var hh1Elderly = ElderlyCount(hh1Yobs);
var hh1Adult   = hh1Size - hh1Elderly; // no children in generator
var hh1Disabled = rng.Next(0, 2);       // 0 or 1

var hh2Genders = new[] { p3Gender, p4Gender };
var hh2Yobs    = new[] { p3Yob, p4Yob };
var hh2Size    = 2;
var hh2Male    = GenderCount(hh2Genders, 1);
var hh2Female  = GenderCount(hh2Genders, 2);
var hh2Elderly = ElderlyCount(hh2Yobs);
var hh2Adult   = hh2Size - hh2Elderly;
var hh2Disabled = rng.Next(0, 2);

// Occupancy start dates — random date 1-5 years in the past (UTC, ISO-8601)
string RandomOccupancyStartDate() =>
    DateTime.UtcNow.AddDays(-rng.Next(365, 365 * 5)).ToString("o");
var hh1OccupancyStart = RandomOccupancyStartDate();
var hh2OccupancyStart = RandomOccupancyStartDate();

// Relation types: P1=Owner(1), P3=random non-owner
var relation2Type = new[] { 2, 3, 4, 5 }[rng.Next(4)]; // Occupant, Tenant, Guest, Heir
var ownershipShare = rng.Next(100, 2401); // 100-2400 (out of 2400 — Islamic inheritance shares)
var surveyStatus = RandomSurveyStatus();
var evidenceType = RandomEvidenceType();
var docType = RandomDocumentType();
var occNature1 = RandomOccupancyNature();
var occNature2 = RandomOccupancyNature();

var floor1 = 0; var floor2 = 0; // set inside using block

var collectorUserId = Guid.Parse("00000000-0000-0000-0000-000000000000");

var outputPath = Path.Combine(Directory.GetCurrentDirectory(), "test-package.uhc");
if (File.Exists(outputPath)) File.Delete(outputPath);

Console.WriteLine("=== TRRCMS Test .uhc Package Generator (v1.9) ===\n");

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

    // Empty vocab_versions — the test generator intentionally declares no pinned
    // vocabulary versions so it never produces version-drift warnings when the
    // server's vocabularies get bumped. Real mobile clients pin their vocabs here
    // so the server can detect when they're out of date.
    var vocabVersions = "{}";

    // Insert manifest with placeholder checksum — will be updated after data tables are populated
    var manifest = new Dictionary<string, string>
    {
        ["package_id"]                = packageId.ToString(),
        ["schema_version"]            = "1.9.0",
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
        ("@yob", p1Yob.ToString()), ("@phone", $"+963-9{rng.Next(10,100)}-{rng.Next(100000,999999)}"),
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
        ("@yob", p2Yob.ToString()), ("@phone", $"+963-9{rng.Next(10,100)}-{rng.Next(100000,999999)}"),
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
        ("@yob", p3Yob.ToString()), ("@phone", $"+963-9{rng.Next(10,100)}-{rng.Next(100000,999999)}"),
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
        ("@yob", p4Yob.ToString()), ("@phone", $"+963-9{rng.Next(10,100)}-{rng.Next(100000,999999)}"),
        ("@gender", p4Gender.ToString()), ("@nat", RandomNationality().ToString()),
        ("@hid", householdId2.ToString()));

    // ── 6. HOUSEHOLDS TABLE ────────────────────────────────────────────────────
    // v1.9 shape:
    //   - Dropped: occupancy_type, male_child_count, female_child_count,
    //     male_elderly_count, female_elderly_count, male_disabled_count,
    //     female_disabled_count
    //   - Added:   adult_count, disabled_count, occupancy_start_date
    //   - Renamed semantic: male_count/female_count now mean totals across all
    //     ages (they were adult-only before v1.9)
    Execute(conn, @"
        CREATE TABLE households (
            id                          TEXT PRIMARY KEY,
            property_unit_id            TEXT NOT NULL,
            household_size              INTEGER NOT NULL,
            male_count                  INTEGER,
            female_count                INTEGER,
            adult_count                 INTEGER,
            child_count                 INTEGER,
            elderly_count               INTEGER,
            disabled_count              INTEGER,
            occupancy_nature            INTEGER,
            occupancy_start_date        TEXT,
            notes                       TEXT
        );");

    // Household 1 — derived from linked persons P1 + P2 (always size=2, 1M+1F)
    Execute(conn, @"
        INSERT INTO households VALUES (
            @id, @uid, @size,
            @mc, @fc, @adult, @child, @elderly, @disabled,
            @on, @osd, 'عائلة مقيمة'
        );",
        ("@id", householdId1.ToString()), ("@uid", unitId1.ToString()),
        ("@size", hh1Size.ToString()),
        ("@mc", hh1Male.ToString()), ("@fc", hh1Female.ToString()),
        ("@adult", hh1Adult.ToString()), ("@child", "0"),
        ("@elderly", hh1Elderly.ToString()), ("@disabled", hh1Disabled.ToString()),
        ("@on", occNature1.ToString()), ("@osd", hh1OccupancyStart));

    // Household 2 — derived from linked persons P3 + P4 (always size=2, 1M+1F)
    Execute(conn, @"
        INSERT INTO households VALUES (
            @id, @uid, @size,
            @mc, @fc, @adult, @child, @elderly, @disabled,
            @on, @osd, 'عائلة نازحة'
        );",
        ("@id", householdId2.ToString()), ("@uid", unitId2.ToString()),
        ("@size", hh2Size.ToString()),
        ("@mc", hh2Male.ToString()), ("@fc", hh2Female.ToString()),
        ("@adult", hh2Adult.ToString()), ("@child", "0"),
        ("@elderly", hh2Elderly.ToString()), ("@disabled", hh2Disabled.ToString()),
        ("@on", occNature2.ToString()), ("@osd", hh2OccupancyStart));

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
Console.WriteLine($"  Schema:   v1.9.0");
Console.WriteLine($"  PackageId: {packageId}");
Console.WriteLine($"  Checksum:  {contentChecksum}");
Console.WriteLine();
Console.WriteLine("=== Randomized Data ===");
Console.WriteLine($"  Building:   14-14-01-010-011-{buildingNumber} ({baseLat:F4}, {baseLon:F4})");
Console.WriteLine($"  Units:      {unitName1} (floor {floor1}), {unitName2} (floor {floor2})");
Console.WriteLine($"  Person 1:   {p1First} {p1Family} (ID: {p1NatId}, Gender: {p1Gender}, Age: {AgeFromYob(p1Yob)}) — Owner, ContactPerson");
Console.WriteLine($"  Person 2:   {p2First} {p2Family} (ID: {p2NatId}, Gender: {p2Gender}, Age: {AgeFromYob(p2Yob)}) — Spouse of P1");
Console.WriteLine($"  Person 3:   {p3First} {p3Family} (ID: {p3NatId}, Gender: {p3Gender}, Age: {AgeFromYob(p3Yob)}) — {relationTypeNames.GetValueOrDefault(relation2Type, "Other")}");
Console.WriteLine($"  Person 4:   {p4First} {p4Family} (Gender: {p4Gender}, Age: {AgeFromYob(p4Yob)}) — Spouse of P3");
Console.WriteLine($"  Household 1: size={hh1Size}, M={hh1Male}/F={hh1Female}, adult={hh1Adult}/elderly={hh1Elderly}/disabled={hh1Disabled}");
Console.WriteLine($"  Household 2: size={hh2Size}, M={hh2Male}/F={hh2Female}, adult={hh2Adult}/elderly={hh2Elderly}/disabled={hh2Disabled}");
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
