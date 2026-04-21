# TRRCMS Mobile Sync Integration Guide

**Version:** 1.9
**Last Updated:** April 10, 2026
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

## Changelog — v1.9 (April 10, 2026)

> **Mobile team: review all items marked with `>>> CHANGED v1.9` in this document.**
> Search for `>>> CHANGED v1.9` to find every updated section.

| Change | Section | Impact |
|--------|---------|--------|
| `households` field realignment — removed 6 gendered age/disability columns (`male_child_count`, `female_child_count`, `male_elderly_count`, `female_elderly_count`, `male_disabled_count`, `female_disabled_count`); added `adult_count`, `disabled_count`, `occupancy_start_date`; `male_count`/`female_count` now mean total of all ages (was adult-only) | 3.2, 8, 11 | **BREAKING** — mobile must ship v1.9 package format |
| `identification_documents` — `document_type` and `description` are now optional. Only `id`, `person_id`, and `original_file_name` are required. | 3.2, 8, 11 | Non-breaking — server accepts packages without these fields |
| `manifest.schema_version` updated to `1.9.0` | 3.1 | Update your manifest value |

---

## Changelog — v1.8 (April 5, 2026)

> **Mobile team: review all items marked with `>>> CHANGED v1.8` in this document.**
> Search for `>>> CHANGED v1.8` to find every updated section.

| Change | Section | Impact |
|--------|---------|--------|
| New `evidence_relations` junction table — many-to-many evidence-to-relation links | 3.2, 7, 8, 11 | One evidence can link to multiple person-property relations |
| `survey_status` value 4 renamed from `Interrupted` to `Obstructed` (معرقل) | 6 | Label change — integer code unchanged |
| Vocabulary seeder now marks removed enum values as `isDeprecated: true` | 6 | Filter deprecated values from dropdowns |
| `manifest.schema_version` updated to `1.8.0` | 3.1 | Update your manifest value |
| New `evidence_relation_count` manifest key | 3.1 | Count of rows in `evidence_relations` table |
| `evidences.person_property_relation_id` kept as backward-compatible fallback | 3.2 | Junction table takes priority if present; direct FK used as fallback |
| `claims.tenure_contract_type` — new optional column | 3.2, 8 | Vocabulary code for tenure arrangement type |
| `identification_documents` now fully staged and committed via import pipeline | 3.2, 8 | ID docs in .uhc are imported to production |
| `OwnershipShare` validated as 0–2400 (Syrian cadastral) across all endpoints | 3.2, 8 | Was inconsistent across validators |
| Backward-compat: `evidences.person_id`, `persons.relationship_to_head`, `households.occupancy_type` still accepted | 3.2 | Deprecated but not rejected — for older .uhc packages |

---

## Changelog — v1.7 (April 4, 2026)

> **Mobile team: review all items marked with `>>> CHANGED v1.7` in this document.**
> Search for `>>> CHANGED v1.7` to find every updated section.

| Change | Section | Impact |
|--------|---------|--------|
| New `identification_documents` table — ID docs are separate from evidence | 3.2, 7, 8, 11 | **BREAKING** — ID documents must go in the new table, not in `evidences` |
| `evidences.person_id` column removed | 3.2, 7, 8 | **BREAKING** — evidence no longer links directly to persons |
| `EvidenceType` values 1 (IdentificationDocument) and 5 (Photo) removed | 6 | **BREAKING** — do not use these codes in `evidences.evidence_type` |
| New `document_type` vocabulary for identification documents | 6 | Use codes: 1=PersonalIdPhoto, 2=FamilyRecord, 3=Photo |
| Audio files (.mp3, .wav, .ogg, .m4a) accepted in evidence attachments | 3.2, 11 | Evidence can now contain voice recordings |
| `survey_status` — value `4` is `Obstructed` (معرقل) — owner/occupant refused cooperation | 6 | Valid status code for surveys |
| `occupancy_type` removed from households, `relationship_to_head` removed from persons | 3.2, 8, 11 | **BREAKING** — do NOT include these columns in the .uhc package |
| `surveys.reference_code` — new format | 3.2, 8, 11 | Generate as `SRV-{DeviceId}-{YYYYMMDDHHmmss}` at survey creation time |
| `manifest.schema_version` updated to `1.7.0` | 3.1 | (now `1.8.0` — see v1.8 changelog above) |
| New `identification_document_count` manifest key | 3.1 | Count of rows in `identification_documents` table |
| `attachments` table now accepts `identification_document_id` | 3.2, 8 | New FK column for ID document blobs |

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
| `evidences` | Yes | Tenure evidence/document metadata |
| `identification_documents` | No | `>>> CHANGED v1.7` — Personal identification document metadata (separate from evidence) |
| `attachments` | No | Binary blobs for evidence, identification document, and building document files |

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
| `schema_version` | `1.9.0` | Schema version of the .uhc format. `>>> CHANGED v1.9` — was `1.8.0` |
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
| `identification_document_count` | `>>> CHANGED v1.7` — Number of rows in `identification_documents` table |
| `evidence_relation_count` | `>>> CHANGED v1.8` — Number of rows in `evidence_relations` table |
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
| `is_contact_person` | INTEGER | No | `1` if this person is the contact person for the survey, `0` otherwise (default: `0`) |

> `>>> CHANGED v1.7` — `relationship_to_head` column removed from the .uhc schema. The server ignores it if present. Desktop may still use it via API, but the mobile app should NOT include this column.

> **Contact Person:** When a survey is initiated, the first person added is typically marked as the contact person (`is_contact_person = 1`). The server links this person to the survey via the `contact_person_id` field on the `surveys` table. Only one person per survey should be marked as the contact person.

#### `households`

> `>>> CHANGED v1.6` — Removed `head_of_household_name` and `head_of_household_person_id` columns.
> `>>> CHANGED v1.9` — Removed 6 gendered age/disability columns. Added `adult_count`, `disabled_count`, `occupancy_start_date`. `male_count`/`female_count` now represent **total members of that gender across all ages** (was adult-only in v1.8).

| Column | SQLite Type | Required | Description |
|---|---|---|---|
| `id` | TEXT (UUID) | Yes | Primary key |
| `property_unit_id` | TEXT (UUID) | Yes | FK → `property_units.id` |
| `household_size` | INTEGER | Yes | Total household members |
| `male_count` | INTEGER | No | `>>> CHANGED v1.9` — Total males (all ages). Was adult-only in v1.8. |
| `female_count` | INTEGER | No | `>>> CHANGED v1.9` — Total females (all ages). Was adult-only in v1.8. |
| `adult_count` | INTEGER | No | `>>> NEW v1.9` — Number of adults |
| `child_count` | INTEGER | No | `>>> CHANGED v1.9` — Total children, ungendered (replaces `male_child_count` + `female_child_count`) |
| `elderly_count` | INTEGER | No | `>>> CHANGED v1.9` — Total elderly, ungendered (replaces `male_elderly_count` + `female_elderly_count`) |
| `disabled_count` | INTEGER | No | `>>> NEW v1.9` — Total persons with disabilities, ungendered (replaces `male_disabled_count` + `female_disabled_count`) |
| `occupancy_nature` | INTEGER | No | Vocabulary code from `occupancy_nature` |
| `occupancy_start_date` | TEXT | No | `>>> NEW v1.9` — ISO-8601 datetime (UTC) when the household started occupying this unit |
| `notes` | TEXT | No | Additional notes |

> `>>> CHANGED v1.9` — **Removed columns** (do NOT include in v1.9 packages): `male_child_count`, `female_child_count`, `male_elderly_count`, `female_elderly_count`, `male_disabled_count`, `female_disabled_count`.
> `>>> CHANGED v1.7` — `occupancy_type` removed. The server ignores it if present.

#### `person_property_relations`

| Column | SQLite Type | Required | Description |
|---|---|---|---|
| `id` | TEXT (UUID) | Yes | Primary key |
| `person_id` | TEXT (UUID) | Yes | FK → `persons.id` |
| `property_unit_id` | TEXT (UUID) | Yes | FK → `property_units.id` |
| `relation_type` | INTEGER | Yes | Vocabulary code from `relation_type` |
| `ownership_share` | REAL | No | Ownership share in Syrian cadastral units (0–2400). `>>> CHANGED v1.8` |
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
| `ownership_share` | REAL | No | Ownership share in Syrian cadastral units (0–2400). `>>> CHANGED v1.8` |
| `tenure_contract_type` | INTEGER | No | `>>> CHANGED v1.8` — Vocabulary code from `tenure_contract_type`. Optional — classifies the tenure arrangement. |
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
| `reference_code` | TEXT | Yes | `>>> CHANGED v1.7` — Survey reference code. Format: `SRV-{DeviceId}-{YYYYMMDDHHmmss}` (e.g., `SRV-T01-20260404091205`). Mobile must generate this at survey creation time using the tablet's device ID and UTC timestamp. Server uses it as-is if provided; generates a fallback `SRV-OFC-{timestamp}` if missing. |
| `type` | INTEGER | No | Vocabulary code from `survey_type` |
| `source` | INTEGER | No | Vocabulary code from `survey_source` |
| `status` | INTEGER | No | Vocabulary code from `survey_status` |

> **Contact Person Link:** The `contact_person_id` should reference the `id` of the person in the `persons` table who has `is_contact_person = 1`. This creates a bidirectional link: the person knows it is a contact person, and the survey knows which person is its contact. **Every survey must have a contact person** — the server rejects finalization if `contact_person_id` is null.

#### `evidences`

> `>>> CHANGED v1.7` — `person_id` column removed. Evidence is now for **tenure documents only** (linked to `person_property_relations`). Identification documents go in the new `identification_documents` table.
> `>>> CHANGED v1.7` — `evidence_type` values `1` (IdentificationDocument) and `5` (Photo) are removed. Do not use these codes.
> `>>> CHANGED v1.7` — Audio files (.mp3, .wav, .ogg, .m4a) are now accepted as evidence attachments (voice recordings).

| Column | SQLite Type | Required | Description |
|---|---|---|---|
| `id` | TEXT (UUID) | Yes | Primary key |
| `evidence_type` | INTEGER | Yes | `>>> CHANGED v1.7` — Vocabulary code from `evidence_type`. Valid codes: 2, 3, 4, 6, 7, 8, 9, 99. Values 1 and 5 are no longer valid. |
| `description` | TEXT | Yes | Document description |
| `original_file_name` | TEXT | Yes | Original file name (e.g., `deed_photo.jpg`, `testimony.mp3`) |
| `file_path` | TEXT | No | Relative file path (server may override if blob is embedded) |
| `file_size_bytes` | INTEGER | No | File size in bytes |
| `file_hash` | TEXT | No | SHA-256 hash of the file content |
| `person_property_relation_id` | TEXT (UUID) | No | FK → `person_property_relations.id`. `>>> CHANGED v1.8` — Legacy fallback; prefer `evidence_relations` junction table for many-to-many. |
| `claim_id` | TEXT (UUID) | No | FK → `claims.id` |
| `document_issued_date` | TEXT | No | ISO-8601 date |
| `document_expiry_date` | TEXT | No | ISO-8601 date |
| `issuing_authority` | TEXT | No | Authority that issued the document |
| `document_reference_number` | TEXT | No | Document reference number |
| `notes` | TEXT | No | Additional notes |

> `>>> CHANGED v1.8` **Backward compatibility note:** The server still accepts the following deprecated columns if present in the .uhc package (for older mobile app versions). New mobile builds should NOT include these columns:
> - `evidences.person_id` — removed in v1.7, ignored if sent
> - `persons.relationship_to_head` — removed in v1.7, ignored if sent
> - `households.occupancy_type` — removed in v1.7, ignored if sent

#### `>>> CHANGED v1.8` — `evidence_relations` (NEW)

Many-to-many junction table linking evidence to person-property relations. Use this when one evidence document (e.g., ownership deed) applies to multiple relations (e.g., father as Owner, son as Heir).

| Column | SQLite Type | Required | Description |
|---|---|---|---|
| `id` | TEXT (UUID) | Yes | Primary key |
| `evidence_id` | TEXT (UUID) | Yes | FK → `evidences.id` |
| `person_property_relation_id` | TEXT (UUID) | Yes | FK → `person_property_relations.id` |

**Priority logic on the server:**
- If `evidence_relations` rows exist for an evidence → server uses those (many-to-many)
- If no `evidence_relations` rows exist → server falls back to `evidences.person_property_relation_id` (legacy 1:1)
- Both can coexist in the same package — the junction table takes priority per evidence

#### `>>> CHANGED v1.7` — `identification_documents` (NEW) / `>>> CHANGED v1.9` — fields simplified

Personal identification documents linked to a Person. Separate from tenure evidence.

> `>>> CHANGED v1.9` — `document_type` and `description` are now **optional**. Only `id`, `person_id`, and `original_file_name` are required. The server accepts all other columns if present but does not require them.

| Column | SQLite Type | Required | Description |
|---|---|---|---|
| `id` | TEXT (UUID) | Yes | Primary key |
| `person_id` | TEXT (UUID) | Yes | FK → `persons.id` |
| `original_file_name` | TEXT | Yes | Original file name |
| `document_type` | INTEGER | No | `>>> CHANGED v1.9` — now optional. Vocabulary code from `document_type`: 1=PersonalIdPhoto, 2=FamilyRecord, 3=Photo |
| `description` | TEXT | No | `>>> CHANGED v1.9` — now optional. Document description |
| `file_path` | TEXT | No | Relative file path |
| `file_size_bytes` | INTEGER | No | File size in bytes |
| `file_hash` | TEXT | No | SHA-256 hash of the file content |
| `document_issued_date` | TEXT | No | ISO-8601 date |
| `document_expiry_date` | TEXT | No | ISO-8601 date |
| `issuing_authority` | TEXT | No | Authority that issued the document |
| `document_reference_number` | TEXT | No | Document reference number |
| `notes` | TEXT | No | Additional notes |

#### `attachments` (Optional)

Binary blobs for evidence, identification document, and building document files. The server extracts blobs from this table and stores them to disk.

```sql
CREATE TABLE attachments (
    evidence_id                TEXT,  -- FK → evidences.id (for tenure evidence blobs)
    identification_document_id TEXT,  -- >>> CHANGED v1.7 — FK → identification_documents.id (for ID document blobs)
    building_document_id       TEXT,  -- FK → building_documents.id (for building document blobs)
    data                       BLOB NOT NULL  -- Raw file bytes
);
```

Each row should have **one of** `evidence_id`, `identification_document_id`, or `building_document_id` set (not multiple). The server queries this table by the appropriate ID to extract the file.

> **BLOB Required:** All evidence files, identification document files, and building document files **must** be embedded as BLOBs in this table. Tablet file paths (e.g., SD card paths) are not accessible on the server — records with no matching BLOB will result in missing files.

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

  "buildingType": 1,
  "buildingStatus": 1,
  "numberOfPropertyUnits": 12,
  "numberOfApartments": 10,
  "numberOfShops": 2,
  "notes": null,

  "buildingGeometryWkt": "POLYGON ((37.15 36.20, 37.16 36.20, 37.16 36.21, 37.15 36.21, 37.15 36.20))",
  "latitude": 36.205,
  "longitude": 37.155,

  "propertyUnits": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "unitIdentifier": "شقة 1",
      "floorNumber": 2,
      "unitType": 1,
      "status": 1,
      "areaSquareMeters": 120.5
    },
    {
      "id": "7bc94a12-1234-4abc-beef-9d8e7f6a5b4c",
      "unitIdentifier": "محل 1",
      "floorNumber": 0,
      "unitType": 2,
      "status": 2,
      "areaSquareMeters": null
    }
  ]
}
```

#### Building Attribute Fields

| Field | Type | Description |
|---|---|---|
| `buildingType` | `int` | `1` Residential (سكني) · `2` Commercial (تجاري) · `3` Mixed-Use (مختلط) · `4` Industrial (صناعي) |
| `buildingStatus` | `int` | `1` Intact (سليم) · `2` Minor Damage (أضرار طفيفة) · `3` Moderate Damage (أضرار متوسطة) · `4` Major Damage (أضرار كبيرة) · `5` Severely Damaged (أضرار شديدة) · `6` Destroyed (مدمر) · `7` Under Construction (قيد الإنشاء) · `8` Abandoned (مهجور) · `99` Unknown (غير معروف) |
| `numberOfPropertyUnits` | `int` | Total registered units in the building — maps to `number_of_property_units` in mobile DB |
| `numberOfApartments` | `int` | Residential apartment count — maps to `number_of_apartments` in mobile DB |
| `numberOfShops` | `int` | Commercial shop count — maps to `number_of_shops` in mobile DB |
| `notes` | `string \| null` | General building notes / description |

#### Property Unit Fields

| Field | Type | Description |
|---|---|---|
| `id` | `guid` | Server-side unit ID — use this to correlate collected survey data in the `.uhc` upload |
| `unitIdentifier` | `string` | Human-readable label, e.g. `"شقة 1"`, `"محل 3"` |
| `floorNumber` | `int \| null` | Floor number; `null` if unknown. Ground floor = `0` |
| `unitType` | `int` | `1` Apartment (شقة سكنية) · `2` Shop (محل تجاري) · `3` Office (مكتب) · `4` Warehouse (مستودع) · `5` Other (أخرى) |
| `status` | `int` | `1` Occupied (مشغول) · `2` Vacant (شاغر) · `3` Damaged (متضرر) · `4` Under Renovation (قيد الترميم) · `5` Uninhabitable (غير صالح للسكن) · `6` Locked (مغلق) · `99` Unknown (غير معروف) |
| `areaSquareMeters` | `decimal \| null` | Unit area in m²; `null` if not yet measured |

> **Note:** `propertyUnits` is always an array — never `null`. It is `[]` when the building has no registered units yet.

#### Spatial Fields

| Field | Type | Description |
|---|---|---|
| `buildingGeometryWkt` | `string \| null` | Full building polygon (or point) in WKT format. Use this to render the building footprint on the map. `null` if no geometry has been registered yet. |
| `latitude` | `number \| null` | Centroid latitude (decimal degrees, WGS-84). For polygon buildings this is computed automatically from the polygon centroid; for point-only buildings it is the recorded GPS coordinate. |
| `longitude` | `number \| null` | Centroid longitude (decimal degrees, WGS-84). |

> **Map rendering guidance:** Use `buildingGeometryWkt` to draw the polygon footprint when available. Fall back to the `latitude`/`longitude` pin when only coordinates exist. When both are `null`, the building has no spatial data yet — show a placeholder or omit from the map layer.

### Using Assignment Data in the .uhc

When the tablet creates a `.uhc` package from collected survey data:

1. Use the `buildingCode` administrative codes (`governorateCode`, `districtCode`, etc.) to populate the `buildings` table columns.
2. Use the property unit details to pre-populate `property_units` rows.
3. The `assignmentId` is for acknowledgement only — it does **not** go into the `.uhc` file.
4. For `isRevisit: true`, `unitsForRevisit` contains a JSON array of unit UUIDs. Only survey those specific units.
5. `buildingGeometryWkt`, `latitude`, and `longitude` are read-only reference data for map display — they are **not** included in the `.uhc` upload.

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
| `evidence_type` | `evidences` | `evidence_type` | `>>> CHANGED v1.7` — values 1 and 5 removed |
| `document_type` | `identification_documents` | `document_type` | `>>> CHANGED v1.7` — new vocabulary for ID documents |
| `survey_type` | `surveys` | `type` |
| `survey_source` | `surveys` | `source` |
| `survey_status` | `surveys` | `status` | `>>> CHANGED v1.8` — value `4` renamed to `Obstructed` (معرقل, was Interrupted). Owner/occupant refused cooperation. |
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
  ├── identification_documents (person_id → persons.id) [>>> CHANGED v1.7 — new table]
  ├── person_property_relations (person_id → persons.id)
  │                           (property_unit_id → property_units.id)
  ├── claims                  (primary_claimant_id → persons.id) [REQUIRED]
  │                           (originating_survey_id → surveys.id) [optional, >>> CHANGED v1.6]
  ├── surveys                 (contact_person_id → persons.id) [required for finalization]
  └── evidences               (claim_id → claims.id) [>>> CHANGED v1.7 — person_id removed]
                               (person_property_relation_id → person_property_relations.id) [legacy fallback]

evidence_relations            [>>> CHANGED v1.8 — NEW many-to-many junction]
  ├── evidence_id              → evidences.id
  └── person_property_relation_id → person_property_relations.id
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
    -- >>> CHANGED v1.7: relationship_to_head removed from .uhc schema
    is_contact_person     INTEGER DEFAULT 0
);

-- >>> CHANGED v1.9: removed gendered age/disability columns; added adult_count, disabled_count, occupancy_start_date
-- >>> CHANGED v1.6: added occupancy_nature; removed head_of_household_name, head_of_household_person_id
CREATE TABLE households (
    id                    TEXT PRIMARY KEY,
    property_unit_id      TEXT NOT NULL REFERENCES property_units(id),
    household_size        INTEGER NOT NULL,
    male_count            INTEGER,          -- >>> CHANGED v1.9: total males all ages (was adult-only)
    female_count          INTEGER,          -- >>> CHANGED v1.9: total females all ages (was adult-only)
    adult_count           INTEGER,          -- >>> NEW v1.9
    child_count           INTEGER,          -- >>> CHANGED v1.9: ungendered total (replaces male/female_child_count)
    elderly_count         INTEGER,          -- >>> CHANGED v1.9: ungendered total (replaces male/female_elderly_count)
    disabled_count        INTEGER,          -- >>> NEW v1.9: ungendered total (replaces male/female_disabled_count)
    occupancy_nature      INTEGER,
    occupancy_start_date  TEXT,             -- >>> NEW v1.9: ISO-8601 UTC move-in date
    notes                 TEXT
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
    tenure_contract_type  INTEGER,                -- >>> CHANGED v1.8: vocabulary code from tenure_contract_type
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
    reference_code           TEXT,              -- >>> CHANGED v1.7: format SRV-{DeviceId}-{YYYYMMDDHHmmss}
    type                     INTEGER,
    source                   INTEGER,
    status                   INTEGER
);

-- >>> CHANGED v1.7: removed person_id column; evidence is for tenure documents only
CREATE TABLE evidences (
    id                            TEXT PRIMARY KEY,
    evidence_type                 INTEGER NOT NULL,  -- >>> CHANGED v1.7: values 1,5 removed
    description                   TEXT NOT NULL,
    original_file_name            TEXT NOT NULL,
    file_path                     TEXT,
    file_size_bytes               INTEGER,
    file_hash                     TEXT,
    person_property_relation_id   TEXT REFERENCES person_property_relations(id),
    claim_id                      TEXT REFERENCES claims(id),
    document_issued_date          TEXT,
    document_expiry_date          TEXT,
    issuing_authority             TEXT,
    document_reference_number     TEXT,
    notes                         TEXT
);

-- >>> CHANGED v1.8: NEW many-to-many junction for evidence-to-relation links
CREATE TABLE evidence_relations (
    id                          TEXT PRIMARY KEY,
    evidence_id                 TEXT NOT NULL REFERENCES evidences(id),
    person_property_relation_id TEXT NOT NULL REFERENCES person_property_relations(id)
);

-- >>> CHANGED v1.7: NEW table for personal identification documents
-- >>> CHANGED v1.9: document_type and description are now optional (NOT NULL removed)
CREATE TABLE identification_documents (
    id                            TEXT PRIMARY KEY,
    person_id                     TEXT NOT NULL REFERENCES persons(id),
    original_file_name            TEXT NOT NULL,
    document_type                 INTEGER,          -- optional: vocabulary: document_type (1=PersonalIdPhoto, 2=FamilyRecord, 3=Photo)
    description                   TEXT,             -- optional: document description
    file_path                     TEXT,
    file_size_bytes               INTEGER,
    file_hash                     TEXT,
    document_issued_date          TEXT,
    document_expiry_date          TEXT,
    issuing_authority             TEXT,
    document_reference_number     TEXT,
    notes                         TEXT
);

-- Required when evidence, identification document, or building document files exist
-- >>> CHANGED v1.7: added identification_document_id column
CREATE TABLE attachments (
    evidence_id                TEXT REFERENCES evidences(id),
    identification_document_id TEXT REFERENCES identification_documents(id),
    building_document_id       TEXT REFERENCES building_documents(id),
    data                       BLOB NOT NULL
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
   - `>>> CHANGED v1.8` — Populate `evidence_relations` table for evidence linked to multiple relations
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
- [ ] `>>> CHANGED v1.9` — `households` table uses new field shape: `adult_count`, `child_count`, `elderly_count`, `disabled_count`, `occupancy_start_date`; **no** gendered age/disability columns (`male_child_count` etc. are removed); `male_count`/`female_count` now total all ages
- [ ] `>>> CHANGED v1.8` — SQLite database creation with all 13 tables (manifest + 11 data + attachments) — includes `identification_documents` and `evidence_relations` tables
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
- [ ] `>>> CHANGED v1.7` — Identification documents go in `identification_documents` table (NOT in `evidences`)
- [ ] `>>> CHANGED v1.7` — `evidences` table does NOT have `person_id` column — evidence links only via `person_property_relation_id`
- [ ] `>>> CHANGED v1.7` — `evidence_type` codes 1 (IdentificationDocument) and 5 (Photo) are NOT used in `evidences`
- [ ] `>>> CHANGED v1.7` — `identification_documents` use `document_type` vocabulary codes (1=PersonalIdPhoto, 2=FamilyRecord, 3=Photo) — `>>> CHANGED v1.9` — now optional, do NOT treat as required
- [ ] `>>> CHANGED v1.7` — Audio files (.mp3, .wav, .ogg, .m4a) can be embedded as evidence BLOBs for voice recordings
- [ ] `>>> CHANGED v1.7` — `attachments` table has `identification_document_id` column for ID doc blobs
- [ ] `>>> CHANGED v1.7` — `occupancy_type` removed from households table, `relationship_to_head` removed from persons table — do NOT include these columns
- [ ] `>>> CHANGED v1.7` — `surveys.reference_code` generated at survey creation as `SRV-{DeviceId}-{YYYYMMDDHHmmss}` (e.g., `SRV-T01-20260404091205`)
- [ ] `>>> CHANGED v1.8` — `evidence_relations` junction table populated when one evidence links to multiple relations
- [ ] `>>> CHANGED v1.8` — `survey_status` value 4 is `Obstructed` (معرقل) — not Interrupted
- [ ] `>>> CHANGED v1.8` — Vocabulary entries with `isDeprecated: true` are hidden from new data entry dropdowns
- [ ] `>>> CHANGED v1.9` — `identification_documents` only requires `id`, `person_id`, `original_file_name` — `document_type` and `description` are optional, do NOT treat them as required
- [ ] All evidence, identification document, and building document files embedded as BLOBs in `attachments` table
- [ ] `.uhc` file extension on the upload
- [ ] Manifest table populated with all required keys (including `building_document_count`, `identification_document_count`, `evidence_relation_count`), `schema_version` = `1.9.0`
- [ ] `manifest.checksum` contains the correct content checksum (server verifies automatically)
- [ ] `is_contact_person` flag set on the correct person per survey
- [ ] `contact_person_id` on survey points to the person with `is_contact_person = 1`
- [ ] Building documents table populated with photo/document metadata
- [ ] Can parse assignment payload (building codes, property units)
- [ ] Can parse vocabulary payload and store locally
- [ ] Assignment acknowledgement working (Step 4)

---------------------------------------------------------------------------------------