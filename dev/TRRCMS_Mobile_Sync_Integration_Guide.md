# TRRCMS Mobile Sync Integration Guide

**Version:** 1.6.1
**Last Updated:** April 1, 2026
**Audience:** Mobile (Flutter/Android) Development Team
**Backend Contact:** TRRCMS Backend Team
**System:** Tenure Rights Registration & Claims Management System (TRRCMS)

---

## Table of Contents

1. [Overview](#1-overview)
2. [Sync Protocol — Step by Step](#2-sync-protocol--step-by-step)
3. [The .uhc Package Format](#3-the-uhc-package-format)
4. [Content Checksum Algorithm](#4-content-checksum-algorithm)
5. [Building Assignment Payload (Step 3 Response)](#5-building-assignment-payload-step-3-response)
6. [Vocabulary Payload (Step 3 Response)](#6-vocabulary-payload-step-3-response)
7. [Foreign Key Relationships](#7-foreign-key-relationships)
8. [SQL Schema — Quick-Start Template](#8-sql-schema--quick-start-template)
9. [Package Assembly Workflow](#9-package-assembly-workflow)
10. [Error Handling & Edge Cases](#10-error-handling--edge-cases)
11. [Checklist Before First Sync](#11-checklist-before-first-sync)

---

## Changelog — v1.6.1 (April 1, 2026)

> **Mobile team: review all items marked with `>>> CHANGED v1.6.1` in this document.**
> Search for `>>> CHANGED v1.6.1` to find every updated section.

| Change | Section | Impact |
|--------|---------|--------|
| First-login password change blocks sync endpoints | 1, 2, 10, 11 | Mobile app must handle `mustChangePassword` flow before sync |

---

## Changelog — v1.6 (March 31, 2026)

> **Mobile team: review all items marked with `>>> CHANGED v1.6` in this document.**
> Search for `>>> CHANGED v1.6` to find every updated section.

| Change | Section | Impact |
|--------|---------|--------|
| `claims.claim_type` changed from TEXT to INTEGER | 3.2, 6, 8, 11 | **BREAKING** — use integer codes `1` (Ownership) or `2` (Occupancy) instead of strings |
| `claims` — added `originating_survey_id` column | 3.2, 7, 8 | New optional FK linking claim to its originating survey |
| `households` — removed `head_of_household_name`, `head_of_household_person_id` | 3.2, 7, 8 | **BREAKING** — these columns no longer exist |
| `households` — added `occupancy_type`, `occupancy_nature` columns | 3.2, 6, 8 | New optional vocabulary-coded fields |
| `person_property_relations` — added `occupancy_type`, `has_evidence` columns | 3.2, 6, 8 | New optional fields for relation classification |
| `surveys` — added `duration_minutes` column | 3.2, 8 | New optional integer field |
| `gender`, `nationality`, `relationship_to_head` now vocabulary-driven | 6 | Were hardcoded enums, now downloaded as vocabularies |
| `manifest.schema_version` updated to `1.6.0` | 3.1 | Update your manifest value |

---

## 1. Overview

The TRRCMS LAN Sync protocol allows Android field tablets to exchange survey data with the server over local Wi-Fi (no internet required). The protocol is a **4-step round-trip**:

| Step | Direction | Endpoint | Purpose |
|------|-----------|----------|---------|
| 1 | Tablet → Server | `POST /api/v1/sync/session` | Open a sync session |
| 2 | Tablet → Server | `POST /api/v1/sync/upload` | Upload `.uhc` survey package |
| 3 | Server → Tablet | `GET /api/v1/sync/assignments` | Download building assignments + vocabularies |
| 4 | Tablet → Server | `POST /api/v1/sync/assignments/ack` | Acknowledge receipt of assignments |

**Authorization:** All endpoints require a valid JWT Bearer token with the `CanSyncData` permission (permission code 9010). Roles with this permission: Field Collector, Field Supervisor, Administrator.

`>>> CHANGED v1.6.1` **First-Login Password Change:** When a user logs in for the first time after account creation, the server returns a **restricted token** (`mustChangePassword: true` in the login response). This restricted token is **blocked by all sync endpoints** (returns 403 Forbidden). The mobile app must detect `mustChangePassword` in the login response and redirect the user to change their password via `POST /api/v1/auth/change-password` before attempting any sync operation. After changing the password, the user must log in again to receive a full-access token. See Section 2 — Pre-Sync Authentication for details.

---

## 2. Sync Protocol — Step by Step

### `>>> CHANGED v1.6.1` Pre-Sync Authentication

Before starting the sync protocol, the mobile app must ensure the user has a **full-access token** (not a restricted password-change token).

**Login flow:**
1. Call `POST /api/v1/auth/login` with `username`, `password`, and `deviceId`.
2. Check the `mustChangePassword` field in the response:
   - If `false`: proceed to sync (Step 1). Store `accessToken` and `refreshToken`.
   - If `true`: the token is restricted. Sync endpoints will return **403 Forbidden**.
3. If `mustChangePassword` is `true`:
   - Display a password change screen to the user.
   - Call `POST /api/v1/auth/change-password` using the restricted token:
     ```json
     {
       "userId": "<userId from login response>",
       "currentPassword": "<the password they just logged in with>",
       "newPassword": "<new password>",
       "confirmPassword": "<new password>"
     }
     ```
   - Password requirements: min 8 chars, uppercase, lowercase, digit, special character.
   - On success: redirect to login. The user logs in again with the new password.
   - The second login will return `mustChangePassword: false` with a full-access token.
4. Note: when `mustChangePassword` is `true`, `refreshToken` is `null` — do not attempt token refresh.

**Restricted token properties:**
- Lifetime: 10 minutes (vs. normal 15 minutes)
- No refresh token issued
- Only works for: `POST /api/v1/auth/change-password`, `POST /api/v1/auth/logout`
- All other endpoints (including all sync endpoints) return 403:
  ```json
  {
    "error": "PasswordChangeRequired",
    "message": "You must change your password before accessing other resources."
  }
  ```

### Step 1 — Open Session

**`POST /api/v1/sync/session`**

```json
// Request Body (JSON)
{
  "fieldCollectorId": "550e8400-e29b-41d4-a716-446655440000",
  "deviceId": "TABLET-FIELD-001",
  "serverIpAddress": "192.168.1.100"
}
```

```json
// Response (201 Created)
{
  "id": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
  "fieldCollectorId": "550e8400-e29b-41d4-a716-446655440000",
  "deviceId": "TABLET-FIELD-001",
  "serverIpAddress": "192.168.1.100",
  "sessionStatus": 1,
  "startedAtUtc": "2026-02-23T10:30:00Z",
  "completedAtUtc": null,
  "packagesUploaded": 0,
  "packagesFailed": 0,
  "assignmentsDownloaded": 0,
  "assignmentsAcknowledged": 0,
  "vocabularyVersionsSent": null,
  "errorMessage": null
}
```

**Important:** Save the returned `id` — it is required by all subsequent steps.

Session statuses: `1` = InProgress, `2` = Completed, `3` = PartiallyCompleted, `4` = Failed.

---

### Step 2 — Upload .uhc Package

**`POST /api/v1/sync/upload`** (multipart/form-data)

| Form Field | Type | Required | Description |
|---|---|---|---|
| `file` | Binary | Yes | The `.uhc` file (must have `.uhc` extension) |
| `SyncSessionId` | UUID | Yes | Session ID from Step 1 |
| `Sha256Checksum` | String | No | Optional client-side content checksum for extra integrity verification (see Section 4) |


**Max file size:** 500 MB.

**Example cURL:**
```bash
curl -X POST http://server:5000/api/v1/sync/upload \
  -H "Authorization: Bearer JWT_TOKEN" \
  -F "file=@survey-2026-02-23.uhc;type=application/octet-stream" \
  -F "SyncSessionId=f47ac10b-58cc-4372-a567-0e02b2c3d479"
```

```json
// Success Response (200 OK)
{
  "accepted": true,
  "packageId": "aaaa0001-0001-0001-0001-000000000001",
  "isDuplicate": false,
  "message": "Package received and queued for import.",
  "importPackageId": "31045e66-6f32-492b-a010-de8d12c9ca63",
  "importError": null
}
```

| Response Field | Type | Description |
|---|---|---|
| `accepted` | boolean | `true` if the file was received and stored |
| `packageId` | UUID | PackageId extracted from the .uhc manifest |
| `isDuplicate` | boolean | `true` if this PackageId was already uploaded |
| `message` | string | Human-readable status message |
| `importPackageId` | UUID? | Server-side import tracking ID. `null` if import pipeline failed. |
| `importError` | string? | `null` on success. Non-null with error details if the import pipeline failed (file is still stored for manual re-import). |

**Idempotency:** Uploading the same package (same `package_id` in manifest) twice returns `accepted: true, isDuplicate: true`. Safe to retry.

**Checksum verification:** The server reads the `checksum` value from the .uhc manifest and compares it against a server-computed content checksum. If the optional `Sha256Checksum` form field is also provided, it is cross-verified as an additional integrity check. A checksum mismatch marks the session as Failed — no further uploads accepted on that session. Open a new session to retry.

---

### Step 3 — Download Assignments & Vocabularies

**`GET /api/v1/sync/assignments?sessionId={uuid}&modifiedSinceUtc={iso8601}`**

| Query Param | Required | Description |
|---|---|---|
| `sessionId` | Yes | Session ID from Step 1 |
| `modifiedSinceUtc` | No | ISO-8601 UTC timestamp for incremental sync. Omit for full download. |

```json
// Response (200 OK) — see Sections 5 & 6 for payload details
{
  "syncSessionId": "f47ac10b-...",
  "fieldCollectorId": "550e8400-...",
  "generatedAtUtc": "2026-02-23T10:30:45Z",
  "totalAssignments": 2,
  "assignments": [ /* SyncBuildingDto[] — see Section 5 */ ],
  "vocabularies": [ /* SyncVocabularyDto[] — see Section 6 */ ],
  "vocabularyVersionsSentJson": "{\"building_type\":\"1.0.0\", ...}"
}
```

**Tip:** Store `generatedAtUtc` and pass it as `modifiedSinceUtc` on the next sync to only receive new/changed assignments.

---

### Step 4 — Acknowledge Assignments

**`POST /api/v1/sync/assignments/ack`**

```json
// Request Body (JSON)
{
  "syncSessionId": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
  "assignmentIds": [
    "asgn-guid-0001",
    "asgn-guid-0002"
  ]
}
```

```json
// Response (200 OK)
{
  "acknowledgedCount": 2,
  "failedCount": 0,
  "failedAssignmentIds": [],
  "message": "2 assignments acknowledged. Sync session completed successfully."
}
```

**Idempotency:** Acknowledging an already-transferred assignment is a no-op — safe to retry.

---

## 3. The .uhc Package Format

A `.uhc` file is a **renamed SQLite 3 database** containing:

| Table | Required | Description |
|---|---|---|
| `manifest` | Yes | Package metadata (key-value pairs) |
| `buildings` | Yes | Building records |
| `building_documents` | No | Building-level document/photo metadata |
| `property_units` | Yes | Property units within buildings |
| `persons` | Yes | Person/individual records |
| `households` | Yes | Household occupancy profiles |
| `person_property_relations` | Yes | Person-to-property linkages |
| `claims` | Yes | Tenure rights claims |
| `surveys` | Yes | Survey session records |
| `evidences` | Yes | Evidence/document metadata |
| `attachments` | No | Binary blobs for evidence files and building documents |

**Tables may be empty** (zero rows) but must exist in the database. The server gracefully skips tables with no rows or missing optional tables.

**All IDs** (`id`, foreign keys) must be **UUID strings** (lowercase hex with dashes: `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`). Generate them client-side with `Uuid.v4()` or equivalent.

---

### 3.1 Manifest Table

The manifest is a simple key-value table:

```sql
CREATE TABLE manifest (
    key   TEXT PRIMARY KEY,
    value TEXT
);
```

**Required keys:**

| Key | Example Value | Description |
|---|---|---|
| `package_id` | `a1b2c3d4-...` | Unique UUID for this package (server extracts this automatically) |
| `schema_version` | `1.6.0` | Schema version of the .uhc format. `>>> CHANGED v1.6` — was `1.5.0` |
| `created_utc` | `2026-02-23T09:45:00Z` | ISO-8601 UTC creation timestamp |
| `device_id` | `TABLET-FIELD-001` | Device identifier |
| `app_version` | `1.0.0` | Tablet application version |
| `exported_by_user_id` | `550e8400-...` | UUID of the field collector |
| `exported_date_utc` | `2026-02-23T09:45:00Z` | ISO-8601 UTC export timestamp |
| `checksum` | `34d9f42c1305d4...` | **Content checksum** (see Section 4) |

**Record count keys** (informational, used for validation):

| Key | Description |
|---|---|
| `survey_count` | Number of rows in `surveys` table |
| `building_count` | Number of rows in `buildings` table |
| `building_document_count` | Number of rows in `building_documents` table |
| `property_unit_count` | Number of rows in `property_units` table |
| `person_count` | Number of rows in `persons` table |
| `household_count` | Number of rows in `households` table |
| `relation_count` | Number of rows in `person_property_relations` table |
| `claim_count` | Number of rows in `claims` table |
| `document_count` | Number of rows in `evidences` table |
| `total_attachment_size_bytes` | Sum of all attachment file sizes |

**Optional keys:**

| Key | Description |
|---|---|
| `digital_signature` | Digital signature for package integrity |
| `form_schema_version` | Survey form schema version |
| `vocab_versions` | JSON map of vocabulary versions (e.g., `{"building_type":"1.0.0","building_status":"1.0.0"}`) |

---

### 3.2 Data Table Schemas

#### `buildings`

| Column | SQLite Type | Required | Description |
|---|---|---|---|
| `id` | TEXT (UUID) | Yes | Primary key — generate with `Uuid.v4()` |
| `governorate_code` | TEXT | Yes | 2-digit code (e.g., `14`) |
| `district_code` | TEXT | Yes | 2-digit code (e.g., `14`) |
| `sub_district_code` | TEXT | Yes | 2-digit code (e.g., `01`) |
| `community_code` | TEXT | Yes | 3-digit code (e.g., `010`) |
| `neighborhood_code` | TEXT | Yes | 3-digit code (e.g., `011`) |
| `building_number` | TEXT | Yes | 5-digit code (e.g., `00001`) |
| `building_id` | TEXT | No | Full 17-digit code (auto-composed: `GGDDSSCCCCNNNBBBBB`). Optional — server recomputes it. |
| `building_type` | INTEGER | Yes | Vocabulary code from `building_type` |
| `building_status` | INTEGER | Yes | Vocabulary code from `building_status` |
| `number_of_property_units` | INTEGER | Yes | Total units in this building |
| `number_of_apartments` | INTEGER | Yes | Apartment count |
| `number_of_shops` | INTEGER | Yes | Shop count |
| `latitude` | REAL | No | GPS latitude (decimal degrees) |
| `longitude` | REAL | No | GPS longitude (decimal degrees) |
| `building_geometry_wkt` | TEXT | No | WKT geometry string |
| `notes` | TEXT | No | Additional notes |
| `governorate_name` | TEXT | No | Arabic name of governorate |
| `district_name` | TEXT | No | Arabic name of district |
| `sub_district_name` | TEXT | No | Arabic name of sub-district |
| `community_name` | TEXT | No | Arabic name of community |
| `neighborhood_name` | TEXT | No | Arabic name of neighborhood |

#### `building_documents`

Building-level photos and documents (e.g., front photo, damage photo). Linked to a building via `building_id`.

| Column | SQLite Type | Required | Description |
|---|---|---|---|
| `id` | TEXT (UUID) | Yes | Primary key |
| `building_id` | TEXT (UUID) | Yes | FK → `buildings.id` |
| `original_file_name` | TEXT | Yes | Original file name (e.g., `building_front.jpg`) |
| `file_size_bytes` | INTEGER | Yes | File size in bytes |
| `file_path` | TEXT | Yes | Relative file path (server may override if blob is embedded) |
| `file_hash` | TEXT | No | SHA-256 hash of the file content |
| `description` | TEXT | No | Document description |
| `notes` | TEXT | No | Additional notes |

> **Blob storage:** All file bytes **must** be embedded in the `attachments` table with `building_document_id` as the key (see `attachments` table below). The server extracts BLOBs to disk during import. Documents without a BLOB in the attachments table will be flagged as missing.

#### `property_units`

| Column | SQLite Type | Required | Description |
|---|---|---|---|
| `id` | TEXT (UUID) | Yes | Primary key |
| `building_id` | TEXT (UUID) | Yes | FK → `buildings.id` |
| `unit_identifier` | TEXT | Yes | Label within building (e.g., `شقة 1`, `محل 2`) |
| `unit_type` | INTEGER | Yes | Vocabulary code from `property_unit_type` |
| `status` | INTEGER | Yes | Vocabulary code from `property_unit_status` |
| `floor_number` | INTEGER | No | Floor number |
| `number_of_rooms` | INTEGER | No | Room count |
| `area_square_meters` | REAL | No | Unit area in m² |
| `description` | TEXT | No | Free-text description |

#### `persons`

| Column | SQLite Type | Required | Description |
|---|---|---|---|
| `id` | TEXT (UUID) | Yes | Primary key |
| `family_name_arabic` | TEXT | Yes | اسم العائلة |
| `first_name_arabic` | TEXT | Yes | الاسم الأول |
| `father_name_arabic` | TEXT | Yes | اسم الأب |
| `mother_name_arabic` | TEXT | No | اسم الأم |
| `national_id` | TEXT | No | National ID / document number |
| `year_of_birth` | INTEGER | No | Year of birth |
| `email` | TEXT | No | Email address |
| `mobile_number` | TEXT | No | Mobile phone |
| `phone_number` | TEXT | No | Landline phone |
| `gender` | INTEGER | No | Enum code: `1` = Male, `2` = Female |
| `nationality` | INTEGER | No | Enum code: `1` = Syrian, `2` = Palestinian, `3` = Iraqi, `99` = Other |
| `household_id` | TEXT (UUID) | No | FK → `households.id` |
| `relationship_to_head` | INTEGER | No | Enum code: `1` = Head, `2` = Spouse, `3` = Son, `4` = Daughter, `5` = Father, `6` = Mother, `7` = Brother, `8` = Sister, `99` = Other |
| `is_contact_person` | INTEGER | No | `1` if this person is the contact person for the survey, `0` otherwise (default: `0`) |

> **Contact Person:** When a survey is initiated, the first person added is typically marked as the contact person (`is_contact_person = 1`). The server links this person to the survey via the `contact_person_id` field on the `surveys` table. Only one person per survey should be marked as the contact person.

#### `households`

> `>>> CHANGED v1.6` — Removed `head_of_household_name` and `head_of_household_person_id` columns. The head of household is now identified by the person with `relationship_to_head = 1` (Head) in the `persons` table.

| Column | SQLite Type | Required | Description |
|---|---|---|---|
| `id` | TEXT (UUID) | Yes | Primary key |
| `property_unit_id` | TEXT (UUID) | Yes | FK → `property_units.id` |
| `household_size` | INTEGER | Yes | Total household members |
| `male_count` | INTEGER | No | Male members (default: 0) |
| `female_count` | INTEGER | No | Female members (default: 0) |
| `male_child_count` | INTEGER | No | Male children (default: 0) |
| `female_child_count` | INTEGER | No | Female children (default: 0) |
| `male_elderly_count` | INTEGER | No | Male elderly (default: 0) |
| `female_elderly_count` | INTEGER | No | Female elderly (default: 0) |
| `male_disabled_count` | INTEGER | No | Male disabled (default: 0) |
| `female_disabled_count` | INTEGER | No | Female disabled (default: 0) |
| `occupancy_type` | INTEGER | No | `>>> CHANGED v1.6` — Vocabulary code from `occupancy_type` |
| `occupancy_nature` | INTEGER | No | `>>> CHANGED v1.6` — Vocabulary code from `occupancy_nature` |
| `notes` | TEXT | No | Additional notes |

#### `person_property_relations`

| Column | SQLite Type | Required | Description |
|---|---|---|---|
| `id` | TEXT (UUID) | Yes | Primary key |
| `person_id` | TEXT (UUID) | Yes | FK → `persons.id` |
| `property_unit_id` | TEXT (UUID) | Yes | FK → `property_units.id` |
| `relation_type` | INTEGER | Yes | Vocabulary code from `relation_type` |
| `ownership_share` | REAL | No | Ownership percentage (0.0–100.0) |
| `occupancy_type` | INTEGER | No | `>>> CHANGED v1.6` — Vocabulary code from `occupancy_type` |
| `has_evidence` | INTEGER | No | `>>> CHANGED v1.6` — `1` if evidence documents are attached, `0` otherwise (default: `0`) |
| `notes` | TEXT | No | Additional notes |

#### `claims`

| Column | SQLite Type | Required | Description |
|---|---|---|---|
| `id` | TEXT (UUID) | Yes | Primary key |
| `property_unit_id` | TEXT (UUID) | Yes | FK → `property_units.id` |
| `claim_type` | INTEGER | Yes | `>>> CHANGED v1.6` — was TEXT, now INTEGER. Vocabulary code from `claim_type`: `1` = OwnershipClaim, `2` = OccupancyClaim |
| `claim_source` | INTEGER | Yes | Vocabulary code from `claim_source` |
| `primary_claimant_id` | TEXT (UUID) | Yes | FK → `persons.id` — the person making the claim. **Required.** Claims without a claimant are rejected during import. |
| `originating_survey_id` | TEXT (UUID) | No | `>>> CHANGED v1.6` — FK → `surveys.id`. Links the claim to the survey that originated it. |
| `ownership_share` | REAL | No | Ownership percentage (0.0–100.0) |
| `claim_description` | TEXT | No | Description of the claim |

#### `surveys`

| Column | SQLite Type | Required | Description |
|---|---|---|---|
| `id` | TEXT (UUID) | Yes | Primary key |
| `building_id` | TEXT (UUID) | Yes | FK → `buildings.id` |
| `survey_date` | TEXT | Yes | ISO-8601 datetime of survey |
| `property_unit_id` | TEXT (UUID) | No | FK → `property_units.id` (unit where interview took place) |
| `gps_coordinates` | TEXT | No | GPS coordinates string (e.g., `36.2021,37.1343`) |
| `duration_minutes` | INTEGER | No | `>>> CHANGED v1.6` — Duration of the survey in minutes |
| `notes` | TEXT | No | Survey notes |
| `field_collector_id` | TEXT (UUID) | No | UUID of the field collector (should match authenticated user) |
| `contact_person_id` | TEXT (UUID) | No | FK → `persons.id` — the person marked as `is_contact_person = 1`. **Required for survey finalization** — surveys without a contact person cannot be finalized. |
| `reference_code` | TEXT | No | Survey reference code. If omitted, server auto-generates one. |
| `type` | INTEGER | No | Vocabulary code from `survey_type` |
| `source` | INTEGER | No | Vocabulary code from `survey_source` |
| `status` | INTEGER | No | Vocabulary code from `survey_status` |

> **Contact Person Link:** The `contact_person_id` should reference the `id` of the person in the `persons` table who has `is_contact_person = 1`. This creates a bidirectional link: the person knows it is a contact person, and the survey knows which person is its contact. **Every survey must have a contact person** — the server rejects finalization if `contact_person_id` is null.

#### `evidences`

| Column | SQLite Type | Required | Description |
|---|---|---|---|
| `id` | TEXT (UUID) | Yes | Primary key |
| `evidence_type` | INTEGER | Yes | Vocabulary code from `evidence_type` |
| `description` | TEXT | Yes | Document description |
| `original_file_name` | TEXT | Yes | Original file name (e.g., `deed_photo.jpg`) |
| `file_path` | TEXT | No | Relative file path (server may override if blob is embedded) |
| `file_size_bytes` | INTEGER | No | File size in bytes |
| `file_hash` | TEXT | No | SHA-256 hash of the file content |
| `person_id` | TEXT (UUID) | No | FK → `persons.id` |
| `person_property_relation_id` | TEXT (UUID) | No | FK → `person_property_relations.id` |
| `claim_id` | TEXT (UUID) | No | FK → `claims.id` |
| `document_issued_date` | TEXT | No | ISO-8601 date |
| `document_expiry_date` | TEXT | No | ISO-8601 date |
| `issuing_authority` | TEXT | No | Authority that issued the document |
| `document_reference_number` | TEXT | No | Document reference number |
| `notes` | TEXT | No | Additional notes |

#### `attachments` (Optional)

Binary blobs for evidence files and building documents. The server extracts blobs from this table and stores them to disk.

```sql
CREATE TABLE attachments (
    evidence_id          TEXT,  -- FK → evidences.id (for evidence file blobs)
    building_document_id TEXT,  -- FK → building_documents.id (for building document blobs)
    data                 BLOB NOT NULL  -- Raw file bytes
);
```

Each row should have **either** `evidence_id` or `building_document_id` set (not both). The server queries this table by the appropriate ID to extract the file.

> **BLOB Required:** All evidence files and building document files **must** be embedded as BLOBs in this table. Tablet file paths (e.g., SD card paths) are not accessible on the server — records with no matching BLOB will result in missing files. The server logs a warning for any evidence or building document without a corresponding BLOB in the attachments table.

---

## 4. Content Checksum Algorithm

**Critical:** The tablet must compute the same checksum as the server and store it in `manifest.checksum`. The server verifies integrity by comparing the manifest checksum against a server-computed checksum. Optionally, the tablet can also send the checksum as the `Sha256Checksum` form field in Step 2 for an additional cross-verification layer.

### Algorithm

```
1. Open the .uhc SQLite file
2. List all tables (SELECT name FROM sqlite_master WHERE type='table' ORDER BY name)
3. Exclude: "manifest", "attachments", "sqlite_sequence", and any table starting with "sqlite_"
4. The remaining tables are already sorted alphabetically (ORDER BY name)
5. Initialize SHA-256 hash
6. For each table (in alphabetical order):
   a. Append "TABLE:{table_name}\n" to the hash
   b. Get column names via PRAGMA table_info("{table_name}"), sort alphabetically (Ordinal)
   c. SELECT * FROM "{table_name}" ORDER BY rowid
   d. For each row:
      - For each column (in alphabetical order):
        - If NULL → "column_name=\0"
        - If not NULL → "column_name={value}"
      - Join all column pairs with TAB character (\t)
      - Append the joined string + "\n" to the hash
7. Finalize SHA-256 → lowercase hex string (64 characters)
```

### Pseudocode (Dart/Flutter)

```dart
import 'dart:convert';
import 'package:crypto/crypto.dart';
import 'package:sqflite/sqflite.dart';

Future<String> computeContentChecksum(Database db) async {
  // 1. Get all tables, sorted alphabetically
  final excluded = {'manifest', 'attachments', 'sqlite_sequence'};
  final tablesResult = await db.rawQuery(
    "SELECT name FROM sqlite_master WHERE type='table' ORDER BY name"
  );
  final tables = tablesResult
      .map((r) => r['name'] as String)
      .where((n) => !n.startsWith('sqlite_') && !excluded.contains(n))
      .toList();

  // 2. Build hash
  final output = AccumulatorSink<Digest>();
  final input = sha256.startChunkedConversion(output);

  for (final table in tables) {
    // Table header
    input.add(utf8.encode('TABLE:$table\n'));

    // Column names sorted alphabetically
    final pragmaResult = await db.rawQuery('PRAGMA table_info("$table")');
    final columns = pragmaResult
        .map((r) => r['name'] as String)
        .toList()
      ..sort(); // Ordinal sort

    // Rows ordered by rowid
    final rows = await db.rawQuery('SELECT * FROM "$table" ORDER BY rowid');
    for (final row in rows) {
      final parts = <String>[];
      for (final col in columns) {
        final value = row[col];
        parts.add('$col=${value == null ? "\\0" : value.toString()}');
      }
      input.add(utf8.encode('${parts.join("\t")}\n'));
    }
  }

  input.close();
  return output.events.single.toString(); // lowercase hex
}
```

### Important Notes

- The checksum is computed **after** all data is inserted into the .uhc but **before** writing it to the manifest.
- The manifest table is excluded from the hash, so there is no circular dependency.
- NULL values are represented as the literal two-character string `\0` (backslash + zero), NOT an actual null byte.
- The `\n` and `\t` in the format are actual newline and tab characters (not literal backslash-n/t).
- String sort is **ordinal** (byte-by-byte), not locale-aware.
- Write the resulting hex string to `manifest.checksum`. Optionally, also send it as the `Sha256Checksum` form field in Step 2 for extra verification.

---

## 5. Building Assignment Payload (Step 3 Response)

Each element in the `assignments` array has this structure:

```json
{
  "assignmentId": "guid",
  "assignedDate": "2026-02-23T09:00:00Z",
  "targetCompletionDate": "2026-03-01T00:00:00Z",
  "priority": "Normal",
  "assignmentNotes": "Focus on ground floor units",
  "isRevisit": false,
  "unitsForRevisit": null,

  "buildingCode": "14140101001100001",
  "buildingCodeDisplay": "14-14-01-010-011-00001",

  "governorateCode": "14",
  "districtCode": "14",
  "subDistrictCode": "01",
  "communityCode": "010",
  "neighborhoodCode": "011",
  "buildingNumber": "00001",

  "governorateName": "حلب",
  "districtName": "حلب",
  "subDistrictName": "مركز حلب",
  "communityName": "حلب المدينة",
  "neighborhoodName": "الجميلية",

  "notes": null,

  "propertyUnits": [
    {
      "id": "unit-guid",
      "unitIdentifier": "شقة 1",
      "floorNumber": 1,
      "unitType": 1,
      "status": 1,
      "areaSquareMeters": 120.5
    }
  ]
}
```

### Using Assignment Data in the .uhc

When the tablet creates a `.uhc` package from collected survey data:

1. Use the `buildingCode` administrative codes (`governorateCode`, `districtCode`, etc.) to populate the `buildings` table columns.
2. Use the property unit details to pre-populate `property_units` rows.
3. The `assignmentId` is for acknowledgement only — it does **not** go into the `.uhc` file.
4. For `isRevisit: true`, `unitsForRevisit` contains a JSON array of unit UUIDs. Only survey those specific units.

---

## 6. Vocabulary Payload (Step 3 Response)

Each element in the `vocabularies` array has this structure:

```json
{
  "vocabularyName": "building_type",
  "displayNameArabic": "نوع البناء",
  "displayNameEnglish": "Building Type",
  "version": "1.0.0",
  "valuesJson": "[{\"code\":1,\"labelAr\":\"سكني\",\"labelEn\":\"Residential\",\"displayOrder\":1}, ...]"
}
```

### Vocabulary Domains Used in Data Tables

The following vocabulary names are used as integer codes in the `.uhc` data tables. Always populate drop-downs from these downloaded vocabularies:

| Vocabulary Name | Used In | Column(s) |
|---|---|---|
| `building_type` | `buildings` | `building_type` |
| `building_status` | `buildings` | `building_status` |
| `property_unit_type` | `property_units` | `unit_type` |
| `property_unit_status` | `property_units` | `status` |
| `relation_type` | `person_property_relations` | `relation_type` |
| `claim_source` | `claims` | `claim_source` |
| `evidence_type` | `evidences` | `evidence_type` |
| `survey_type` | `surveys` | `type` |
| `survey_source` | `surveys` | `source` |
| `survey_status` | `surveys` | `status` |
| `occupancy_type` | `households`, `person_property_relations` | `occupancy_type` | `>>> CHANGED v1.6` |
| `occupancy_nature` | `households` | `occupancy_nature` | `>>> CHANGED v1.6` |
| `claim_type` | `claims` | `claim_type` | `>>> CHANGED v1.6` — was TEXT, now INTEGER vocabulary code |
| `gender` | `persons` | `gender` | `>>> CHANGED v1.6` — now vocabulary-driven |
| `nationality` | `persons` | `nationality` | `>>> CHANGED v1.6` — now vocabulary-driven |
| `relationship_to_head` | `persons` | `relationship_to_head` | `>>> CHANGED v1.6` — now vocabulary-driven |

> **`>>> CHANGED v1.6`** — `gender`, `nationality`, and `relationship_to_head` are now **vocabulary-driven** (downloaded via sync like other vocabularies). `claim_type` is now an **INTEGER** vocabulary code, not a TEXT string. All integer codes come from the vocabulary values downloaded in Step 3.

### How to Use Vocabularies

1. **Download** vocabularies in Step 3.
2. **Parse** `valuesJson` — it is a JSON array of objects with at minimum `code` (integer), `labelAr` (Arabic), `labelEn` (English).
3. **Store locally** in the tablet's SQLite database, replacing any previous version.
4. **Populate drop-downs** using `labelAr` (or `labelEn`) as display text and `code` as the stored value.
5. **Write integer codes** to the `.uhc` data table columns — the server validates codes against the vocabulary.
6. **Include versions** in the manifest's `vocab_versions` key so the server can check compatibility.

### Version Compatibility

- **MAJOR version mismatch** (e.g., server has `2.0.0`, tablet has `1.x.x`): Import is blocked. Tablet must sync vocabularies first.
- **MINOR/PATCH mismatch**: Warning only — import proceeds.

---

## 7. Foreign Key Relationships

All relationships are enforced by UUID references within the `.uhc` file:

```
buildings
  ├── building_documents     (building_id → buildings.id)
  └── property_units         (building_id → buildings.id)
        ├── households        (property_unit_id → property_units.id)
        ├── claims            (property_unit_id → property_units.id)
        └── surveys           (property_unit_id → property_units.id)
                               (building_id → buildings.id)

persons
  ├── is_contact_person = 1   (marks this person as the survey contact)
  ├── households              (household_id → households.id — on persons table)
  ├── person_property_relations (person_id → persons.id)
  │                           (property_unit_id → property_units.id)
  ├── claims                  (primary_claimant_id → persons.id) [REQUIRED]
  │                           (originating_survey_id → surveys.id) [optional, >>> CHANGED v1.6]
  ├── surveys                 (contact_person_id → persons.id) [required for finalization]
  └── evidences               (person_id → persons.id)
                               (claim_id → claims.id)
                               (person_property_relation_id → person_property_relations.id)
```

**Important:** All UUID foreign keys must match existing `id` values within the same `.uhc` package. Generate all IDs client-side before inserting rows.

---

## 8. SQL Schema — Quick-Start Template

Copy-paste this into your local SQLite database creation code:

```sql
CREATE TABLE manifest (
    key   TEXT PRIMARY KEY,
    value TEXT
);

CREATE TABLE buildings (
    id                      TEXT PRIMARY KEY,
    governorate_code        TEXT NOT NULL,
    district_code           TEXT NOT NULL,
    sub_district_code       TEXT NOT NULL,
    community_code          TEXT NOT NULL,
    neighborhood_code       TEXT NOT NULL,
    building_number         TEXT NOT NULL,
    building_id             TEXT,
    building_type           INTEGER NOT NULL,
    building_status         INTEGER NOT NULL,
    number_of_property_units INTEGER NOT NULL DEFAULT 0,
    number_of_apartments    INTEGER NOT NULL DEFAULT 0,
    number_of_shops         INTEGER NOT NULL DEFAULT 0,
    latitude                REAL,
    longitude               REAL,
    building_geometry_wkt   TEXT,
    notes                   TEXT,
    governorate_name        TEXT,
    district_name           TEXT,
    sub_district_name       TEXT,
    community_name          TEXT,
    neighborhood_name       TEXT
);

CREATE TABLE building_documents (
    id                  TEXT PRIMARY KEY,
    building_id         TEXT NOT NULL REFERENCES buildings(id),
    original_file_name  TEXT NOT NULL,
    file_size_bytes     INTEGER NOT NULL DEFAULT 0,
    file_path           TEXT NOT NULL,
    file_hash           TEXT,
    description         TEXT,
    notes               TEXT
);

CREATE TABLE property_units (
    id                  TEXT PRIMARY KEY,
    building_id         TEXT NOT NULL REFERENCES buildings(id),
    unit_identifier     TEXT NOT NULL,
    unit_type           INTEGER NOT NULL,
    status              INTEGER NOT NULL,
    floor_number        INTEGER,
    number_of_rooms     INTEGER,
    area_square_meters  REAL,
    description         TEXT
);

CREATE TABLE persons (
    id                    TEXT PRIMARY KEY,
    family_name_arabic    TEXT NOT NULL,
    first_name_arabic     TEXT NOT NULL,
    father_name_arabic    TEXT NOT NULL,
    mother_name_arabic    TEXT,
    national_id           TEXT,
    year_of_birth         INTEGER,
    email                 TEXT,
    mobile_number         TEXT,
    phone_number          TEXT,
    gender                INTEGER,
    nationality           INTEGER,
    household_id          TEXT REFERENCES households(id),
    relationship_to_head  INTEGER,
    is_contact_person     INTEGER DEFAULT 0
);

-- >>> CHANGED v1.6: added occupancy_type, occupancy_nature; removed head_of_household_name, head_of_household_person_id
CREATE TABLE households (
    id                           TEXT PRIMARY KEY,
    property_unit_id             TEXT NOT NULL REFERENCES property_units(id),
    household_size               INTEGER NOT NULL,
    male_count                   INTEGER DEFAULT 0,
    female_count                 INTEGER DEFAULT 0,
    male_child_count             INTEGER DEFAULT 0,
    female_child_count           INTEGER DEFAULT 0,
    male_elderly_count           INTEGER DEFAULT 0,
    female_elderly_count         INTEGER DEFAULT 0,
    male_disabled_count          INTEGER DEFAULT 0,
    female_disabled_count        INTEGER DEFAULT 0,
    occupancy_type               INTEGER,          -- >>> CHANGED v1.6
    occupancy_nature             INTEGER,           -- >>> CHANGED v1.6
    notes                        TEXT
);

-- >>> CHANGED v1.6: added occupancy_type, has_evidence
CREATE TABLE person_property_relations (
    id                       TEXT PRIMARY KEY,
    person_id                TEXT NOT NULL REFERENCES persons(id),
    property_unit_id         TEXT NOT NULL REFERENCES property_units(id),
    relation_type            INTEGER NOT NULL,
    ownership_share          REAL,
    occupancy_type           INTEGER,              -- >>> CHANGED v1.6
    has_evidence             INTEGER DEFAULT 0,    -- >>> CHANGED v1.6
    notes                    TEXT
);

-- >>> CHANGED v1.6: claim_type TEXT→INTEGER, added originating_survey_id
CREATE TABLE claims (
    id                    TEXT PRIMARY KEY,
    property_unit_id      TEXT NOT NULL REFERENCES property_units(id),
    claim_type            INTEGER NOT NULL,        -- >>> CHANGED v1.6: was TEXT
    claim_source          INTEGER NOT NULL,
    primary_claimant_id   TEXT NOT NULL REFERENCES persons(id),
    originating_survey_id TEXT REFERENCES surveys(id), -- >>> CHANGED v1.6
    ownership_share       REAL,
    claim_description     TEXT
);

-- >>> CHANGED v1.6: added duration_minutes
CREATE TABLE surveys (
    id                       TEXT PRIMARY KEY,
    building_id              TEXT NOT NULL REFERENCES buildings(id),
    survey_date              TEXT NOT NULL,
    property_unit_id         TEXT REFERENCES property_units(id),
    gps_coordinates          TEXT,
    duration_minutes         INTEGER,              -- >>> CHANGED v1.6
    notes                    TEXT,
    field_collector_id       TEXT,
    contact_person_id        TEXT REFERENCES persons(id),
    reference_code           TEXT,
    type                     INTEGER,
    source                   INTEGER,
    status                   INTEGER
);

CREATE TABLE evidences (
    id                            TEXT PRIMARY KEY,
    evidence_type                 INTEGER NOT NULL,
    description                   TEXT NOT NULL,
    original_file_name            TEXT NOT NULL,
    file_path                     TEXT,
    file_size_bytes               INTEGER,
    file_hash                     TEXT,
    person_id                     TEXT REFERENCES persons(id),
    person_property_relation_id   TEXT REFERENCES person_property_relations(id),
    claim_id                      TEXT REFERENCES claims(id),
    document_issued_date          TEXT,
    document_expiry_date          TEXT,
    issuing_authority             TEXT,
    document_reference_number     TEXT,
    notes                         TEXT
);

-- Required when evidence or building document files exist
CREATE TABLE attachments (
    evidence_id          TEXT REFERENCES evidences(id),
    building_document_id TEXT REFERENCES building_documents(id),
    data                 BLOB NOT NULL
);
```

---

## 9. Package Assembly Workflow

When the tablet is ready to export collected data:

```
1. Create a new SQLite database file: "{package_id}.uhc"
2. Execute the CREATE TABLE statements from Section 8
3. Insert all collected data into the data tables
   - Generate all UUIDs client-side before inserting
   - Ensure FK consistency: all referenced IDs exist within the package
   - Every claim must have a `primary_claimant_id` pointing to a valid person
   - Mark exactly one person per survey as is_contact_person = 1
   - Set contact_person_id on the survey to that person's id
   - Embed all evidence and building document files as BLOBs in the attachments table
4. Compute the content checksum (Section 4) over the data tables
5. Insert manifest key-value pairs, including:
   - package_id = {newly generated UUID}
   - checksum = {computed content checksum}
   - survey_count, building_count, building_document_count, etc. = actual row counts
   - vocab_versions = JSON of vocabulary versions used
6. Close the database connection
7. The .uhc file is ready for upload in Step 2
```

---

## 10. Error Handling & Edge Cases

| Scenario | Behavior |
|---|---|
| Checksum mismatch | Session marked `Failed`. Open new session and re-upload. |
| Duplicate package upload | Returns `accepted: true, isDuplicate: true`. Safe to retry. |
| Session expired/closed | Returns error. Open a new session. |
| Missing table in .uhc | Server skips that entity type (zero records staged). |
| Claim without primary_claimant_id | Claim is **rejected** during commit — the person must exist in the package. |
| Evidence/document without BLOB | Warning logged; record is staged but the file will be missing on the server. |
| Unknown vocabulary code | Record staged with validation warning; may require manual review. |
| Empty tables | Allowed — server handles zero-row tables gracefully. |
| Upload network failure | Retry with same PackageId — idempotent. |
| Ack network failure | Retry — acknowledging already-transferred assignments is a no-op. |
| JWT token expired | Refresh the token and retry the request. |
| `>>> CHANGED v1.6.1` 403 `PasswordChangeRequired` | User must change password before sync. Redirect to password change screen, then re-login. |
| Major vocab version mismatch | Import blocked. Re-sync vocabularies before uploading. |

---

## 11. Checklist Before First Sync

- [ ] JWT authentication working with `CanSyncData` permission
- [ ] `>>> CHANGED v1.6.1` — Handle `mustChangePassword: true` in login response — redirect to password change screen, then re-login before sync
- [ ] `>>> CHANGED v1.6.1` — Handle `refreshToken: null` when `mustChangePassword` is true — do not attempt token refresh
- [ ] `>>> CHANGED v1.6.1` — Handle 403 response with `"error": "PasswordChangeRequired"` on any sync endpoint — redirect to password change
- [ ] Can create sync session (Step 1)
- [ ] SQLite database creation with all 11 tables (manifest + 9 data + attachments)
- [ ] Content checksum implementation matches server algorithm
- [ ] Vocabulary codes stored as integers in data tables
- [ ] `>>> CHANGED v1.6` — `gender`, `nationality`, `relationship_to_head` populated from downloaded vocabularies (no longer hardcoded)
- [ ] `>>> CHANGED v1.6` — `claim_type` uses INTEGER codes (`1` = OwnershipClaim, `2` = OccupancyClaim) — NOT text strings
- [ ] `>>> CHANGED v1.6` — `households` include `occupancy_type` and `occupancy_nature` vocabulary codes
- [ ] `>>> CHANGED v1.6` — `person_property_relations` include `occupancy_type` and `has_evidence` fields
- [ ] `>>> CHANGED v1.6` — `surveys` include `duration_minutes`
- [ ] `>>> CHANGED v1.6` — `claims` include `originating_survey_id` linking to the survey
- [ ] `>>> CHANGED v1.6` — `households` no longer have `head_of_household_name` or `head_of_household_person_id` — do NOT include these columns
- [ ] Every claim has a valid `primary_claimant_id` (required, not nullable)
- [ ] All UUID foreign keys are consistent within the package
- [ ] All evidence and building document files embedded as BLOBs in `attachments` table
- [ ] `.uhc` file extension on the upload
- [ ] Manifest table populated with all required keys (including `building_document_count`), `schema_version` = `1.6.0`
- [ ] `manifest.checksum` contains the correct content checksum (server verifies automatically)
- [ ] `is_contact_person` flag set on the correct person per survey
- [ ] `contact_person_id` on survey points to the person with `is_contact_person = 1`
- [ ] Building documents table populated with photo/document metadata
- [ ] Can parse assignment payload (building codes, property units)
- [ ] Can parse vocabulary payload and store locally
- [ ] Assignment acknowledgement working (Step 4)

---------------------------------------------------------------------------------------