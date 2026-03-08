# Postman Collection Testing Guide

Complete guide to test the TRRCMS API flows using the Postman collections.

---

## Setup Prerequisites

### 1. Import Files into Postman

1. **Open Postman**
2. **Import Collections** (6 domain-specific files):
   - Click "Import" → "Upload Files"
   - Select the collections you need:

   | Collection File | Audience | Requests |
   |----------------|----------|----------|
   | `TRRCMS_01_Auth.postman_collection.json` | Everyone (import first) | 6 |
   | `TRRCMS_02_Buildings.postman_collection.json` | Admin, Data Manager, Desktop | 24 |
   | `TRRCMS_03_Mobile_Survey.postman_collection.json` | Mobile Team | 27 |
   | `TRRCMS_04_Desktop_Survey.postman_collection.json` | Desktop Team | 17 |
   | `TRRCMS_05_Sync_Import.postman_collection.json` | Mobile, Data Manager, Backend | 29 |
   | `TRRCMS_06_Admin.postman_collection.json` | Administrators | 12 |

   **Always import `01_Auth` first** — it populates the `accessToken` used by all others.

3. **Import Environment:**
   - Click "Import" → "Upload Files"
   - Select: `TRRCMS_Development.postman_environment.json`

### 2. Start the API Server

```bash
cd e:\Work\UN\Project\My Solution\TRRCMS\src
dotnet run --project TRRCMS.WebAPI
```

**API starts at:** `https://localhost:7204/swagger`

### 3. Select Environment

In Postman, top-right dropdown: Select **"TRRCMS Development"** environment.

**Verify:** `{{baseUrl}}` should resolve to `https://localhost:7204/api/v1`

---

## Test Workflow: Authentication (01_Auth)

**Always run first to get tokens.**

### Step 1: Login

**Collection:** `TRRCMS — 01. Authentication`

**Request:** `0. Authentication` → `Login (Admin)`

**Body:**
```json
{
  "username": "admin",
  "password": "Admin@123"
}
```

**Expected Response (200 OK):**
```json
{
  "id": "user-id-guid",
  "username": "admin",
  "email": "admin@example.com",
  "roles": ["Administrator"],
  "accessToken": "eyJ0eXAi...",
  "refreshToken": "refresh-token-guid"
}
```

**Auto-saved variables:**
- `currentUserId` — User GUID
- `currentUserName` — "admin"
- `currentUserRole` — "Administrator"
- `accessToken` — JWT token (auto-added to Authorization header)

---

## .uhc Package Schema v1.1 Changes

Before testing sync/import flows, note the latest schema changes:

| Change | Details |
|--------|---------|
| **New table:** `building_documents` | Building photos/scans with FK to `buildings.id` |
| **New column:** `persons.is_contact_person` | INTEGER (0/1), marks the survey contact person |
| **New column:** `surveys.contact_person_id` | UUID FK to `persons.id`, bidirectional link |
| **New manifest key:** `building_document_count` | Record count for building_documents table |
| **Removed:** `buildings.location_description` | No longer read by the staging pipeline |
| **Person API:** `isContactPerson` field | Available on Add Person to Household endpoint |

Use the `GenerateTestUhc` tool (`src/tools/GenerateTestUhc`) to create valid test `.uhc` packages with all v1.1 changes included.

---

## Test Workflow 1: Sync Protocol (05_Sync_Import)

**Collection:** `TRRCMS — 05. Sync, Import & Conflict Resolution`

**Scenario:** Tablet syncs field-collected survey data.

### Step 1: Create Sync Session

**Request:** `7. Sync Protocol` → `Step 1 - Create Sync Session`

**Body (pre-filled):**
```json
{
  "fieldCollectorId": "{{currentUserId}}",
  "deviceId": "TABLET-FIELD-001",
  "serverIpAddress": "192.168.1.100"
}
```

**Run:** Click "Send"

**Expected Response (201 Created):**
```json
{
  "id": "sync-session-guid",
  "fieldCollectorId": "user-id",
  "deviceId": "TABLET-FIELD-001",
  "sessionStatus": 1,
  "createdAtUtc": "2026-03-03T10:00:00Z"
}
```

**Auto-saved:** `syncSessionId` environment variable

---

### Step 2: Upload .uhc Package

**Request:** `7. Sync Protocol` → `Step 2 - Upload .uhc Package`

**Setup:**
1. Click `file` field → Select a `.uhc` test file (SQLite3 database)
   - If no test file, use any binary file renamed to `.uhc` for testing
2. Other fields auto-populated with test values

**Run:** Click "Send"

**Expected Response (200 OK):**
```json
{
  "packageId": "aaaa0001-0001-0001-0001-000000000001",
  "accepted": true,
  "isDuplicate": false,
  "message": "Package accepted and stored"
}
```

**Key Notes:**
- **Idempotent:** Upload same file twice → `isDuplicate: true`
- **Checksum validation:** SHA-256 must match data table contents (excludes manifest and attachments)
- **Max size:** 500 MB
- **Package tables (v1.1):** buildings, building_documents, property_units, persons, households, person_property_relations, surveys, claims, evidences, attachments

---

### Step 3: Download Assignments & Vocabularies

**Request:** `7. Sync Protocol` → `Step 3 - Download Assignments & Vocabularies`

**URL auto-filled:** `{{baseUrl}}/sync/assignments?sessionId={{syncSessionId}}`

**Run:** Click "Send"

**Expected Response (200 OK):**
```json
{
  "assignmentCount": 5,
  "payloadAssembledAtUtc": "2026-03-03T10:15:00Z",
  "assignments": [
    {
      "assignmentId": "assign-guid-1",
      "buildingCode": "BLD-001",
      "transferStatus": 1,
      "assignedDate": "2026-03-01T00:00:00Z"
    }
  ],
  "vocabularies": [
    {
      "name": "building_type",
      "version": "1.0.0",
      "values": [
        {
          "code": 1,
          "labelArabic": "سكني",
          "labelEnglish": "Residential"
        }
      ]
    }
  ]
}
```

**For Incremental Sync:**
- Add query param: `modifiedSinceUtc=2026-03-01T10:00:00Z`
- Returns only new/changed assignments since that time

---

### Step 4: Acknowledge Assignments

**Request:** `7. Sync Protocol` → `Step 4 - Acknowledge Assignments`

**Body (pre-filled):**
```json
{
  "syncSessionId": "{{syncSessionId}}",
  "assignmentIds": [
    "assignment-guid-001",
    "assignment-guid-002"
  ]
}
```

**Replace `assignmentIds`** with values from Step 3 response.

**Run:** Click "Send"

**Expected Response (200 OK):**
```json
{
  "acknowledgedCount": 2,
  "failedCount": 0,
  "message": "Assignments acknowledged successfully"
}
```

**Session Status After:** Completed (2) or PartiallyCompleted (3)

---

## Test Workflow 2: Import Pipeline (05_Sync_Import)

**Collection:** `TRRCMS — 05. Sync, Import & Conflict Resolution`

**Scenario:** Import survey package from tablet/external source.

### Step 1: Upload .uhc Package

**Request:** `8. Import Pipeline` → `Step 1 - Upload .uhc Package`

**Setup:**
1. Click `file` field → Select `.uhc` test package
2. Click "Send"

**Expected Response (201 Created):**
```json
{
  "package": {
    "id": "import-pkg-guid",
    "fileName": "survey_aleppo_202603.uhc",
    "status": "Pending",
    "createdAtUtc": "2026-03-03T10:30:00Z",
    "fileSize": 25600000,
    "buildingCount": 0,
    "personCount": 0,
    "surveyCount": 0
  },
  "isDuplicatePackage": false
}
```

**Auto-saved:** `importPackageId` environment variable

---

### Step 2: Stage Package

**Request:** `8. Import Pipeline` → `Step 2 - Stage Package`

**URL auto-filled:** `{{baseUrl}}/import/packages/{{importPackageId}}/stage`

**Run:** Click "Send"

**Expected Response (200 OK):**
```json
{
  "importPackageId": "import-pkg-guid",
  "validRowCount": 145,
  "validationErrors": [
    {
      "table": "persons",
      "row": 3,
      "field": "national_id",
      "error": "Invalid national ID format"
    }
  ],
  "stagedBuildingCount": 12,
  "stagedBuildingDocumentCount": 5,
  "stagedPersonCount": 45,
  "stagedSurveyCount": 8
}
```

**Next:** Review validation errors. If critical, cancel package. Otherwise proceed.

---

### Step 3: Get Validation Report

**Request:** `8. Import Pipeline` → `Get Validation Report`

**Run:** Click "Send"

**Verify:** All entities staged correctly, errors noted.

---

### Step 4: Get Staged Entities

**Request:** `8. Import Pipeline` → `Get Staged Entities`

**Optional query param:** `entityType=Building` (or BuildingDocument, Person, PropertyUnit, Survey, Claim, etc.)

**Expected Response:**
```json
{
  "Building": [
    {
      "id": "staging-building-guid",
      "sourceId": "BLD-001",
      "buildingCode": "BLD-001",
      "status": "Staged"
    }
  ],
  "BuildingDocument": [
    {
      "id": "staging-doc-guid",
      "buildingId": "staging-building-guid",
      "originalFileName": "building_front.jpg",
      "status": "Staged"
    }
  ],
  "Person": [
    {
      "id": "staging-person-guid",
      "sourceId": "PER-001",
      "nationalId": "123456789",
      "isContactPerson": true,
      "status": "Staged"
    }
  ]
}
```

---

### Step 5: Detect Duplicates

**Request:** `8. Import Pipeline` → `Step 3 - Detect Duplicates`

**Run:** Click "Send"

**Expected Response (200 OK):**
```json
{
  "importPackageId": "import-pkg-guid",
  "conflictCount": 3,
  "personConflicts": 2,
  "propertyConflicts": 1,
  "detectionCompletedAtUtc": "2026-03-03T10:45:00Z"
}
```

**If conflicts found:** Go to **Conflict Resolution (Section 9)** workflow.

---

### Step 6: Approve for Commit

**Request:** `8. Import Pipeline` → `Step 4 - Approve for Commit`

**Body (pre-filled):**
```json
{
  "approveAllRecords": true,
  "justification": "All conflicts resolved, ready for commit"
}
```

**Note:** If conflicts exist, resolve them first (see Conflict Resolution below).

**Run:** Click "Send"

**Expected Response (200 OK):**
```json
{
  "id": "import-pkg-guid",
  "status": "ReadyToCommit",
  "approvedAt": "2026-03-03T11:00:00Z"
}
```

---

### Step 7: Commit Package

**Request:** `8. Import Pipeline` → `Step 5 - Commit Package`

**Body (pre-filled):**
```json
{
  "commitAllApproved": true
}
```

**Run:** Click "Send"

**Expected Response (200 OK):**
```json
{
  "importPackageId": "import-pkg-guid",
  "status": "Completed",
  "recordsCommitted": 65,
  "buildingsCommitted": 12,
  "buildingDocumentsCommitted": 5,
  "personsCommitted": 45,
  "surveysCommitted": 8,
  "attachmentsProcessed": 23,
  "committedAtUtc": "2026-03-03T11:15:00Z"
}
```

**All records now in production!**

---

### Step 8: Get Commit Report

**Request:** `8. Import Pipeline` → `Get Commit Report`

**Run:** Click "Send"

**Verify:** Detailed report of committed records, failures (if any), record IDs generated.

---

## Test Workflow 3: Conflict Resolution (05_Sync_Import)

**Collection:** `TRRCMS — 05. Sync, Import & Conflict Resolution`

**Scenario:** Review and resolve duplicate entities detected during import.

### Step 1: Get Conflict Summary

**Request:** `9. Conflict Resolution` → `Conflict Summary`

**Run:** Click "Send"

**Expected Response (200 OK):**
```json
{
  "totalConflicts": 5,
  "pendingCount": 3,
  "resolvedCount": 2,
  "escalatedCount": 0
}
```

---

### Step 2: List Conflict Queue

**Request:** `9. Conflict Resolution` → `Conflict Queue`

**Query params (optional):**
- `conflictType=person` (or property, claim)
- `status=PendingReview`
- `importPackageId={{importPackageId}}`

**Run:** Click "Send"

**Expected Response:**
```json
{
  "items": [
    {
      "id": "conflict-guid",
      "conflictType": "person",
      "status": "PendingReview",
      "priority": "Normal",
      "importPackageId": "import-pkg-guid",
      "createdAtUtc": "2026-03-03T10:45:00Z"
    }
  ],
  "totalCount": 3,
  "page": 1,
  "pageSize": 20
}
```

**Auto-save first conflict ID as `conflictId`** (or click on conflict to view details).

---

### Step 3: View Property Duplicates (UC-007)

**Request:** `9. Conflict Resolution` → `Property Duplicates`

**Expected:** Buildings/units flagged as potential duplicates based on:
- Same `building_id`
- Same composite key (`building_id + unit_code`)

---

### Step 4: View Person Duplicates (UC-008)

**Request:** `9. Conflict Resolution` → `Person Duplicates`

**Expected:** Persons sharing same `national_id` value.

---

### Step 5: Get Conflict Details

**Request:** `9. Conflict Resolution` → `Get Conflict Details`

**URL auto-filled:** `{{baseUrl}}/conflicts/{{conflictId}}/details`

**Run:** Click "Send"

**Expected Response:**
```json
{
  "id": "conflict-guid",
  "conflictType": "person",
  "status": "PendingReview",
  "leftEntity": {
    "id": "person-staging-guid",
    "nationalId": "123456789",
    "firstName": "محمد",
    "lastName": "علي",
    "dateOfBirth": "1980-01-15"
  },
  "rightEntity": {
    "id": "person-production-guid",
    "nationalId": "123456789",
    "firstName": "محمد",
    "lastName": "علي",
    "dateOfBirth": "1980-01-15"
  }
}
```

---

### Step 6: Get Document Comparison (UC-008 S04)

**Request:** `9. Conflict Resolution` → `Get Document Comparison`

**Run:** Click "Send"

**Expected:** Evidence/document comparison:
```json
{
  "leftEntity": {
    "documents": [
      {
        "id": "doc-guid",
        "fileName": "national_id_scan.pdf",
        "uploadedAt": "2026-03-01T00:00:00Z"
      }
    ]
  },
  "rightEntity": {
    "documents": [
      {
        "id": "doc-guid",
        "fileName": "passport_scan.jpg",
        "uploadedAt": "2026-02-28T00:00:00Z"
      }
    ]
  }
}
```

---

### Step 7a: Merge Conflict

**Request:** `9. Conflict Resolution` → `Merge Conflict`

**Body (update with your chosen master):**
```json
{
  "masterEntityId": "person-production-guid",
  "justification": "Production record is more complete with recent documents"
}
```

**Run:** Click "Send"

**Expected Response (200 OK):**
```json
{
  "id": "conflict-guid",
  "status": "Resolved",
  "resolutionAction": "Merge",
  "resolvedAtUtc": "2026-03-03T11:30:00Z"
}
```

**Effect:**
- Staging entity soft-deleted
- Foreign keys updated to master entity
- Audit trail recorded

---

### Step 7b: Keep Separate (Alternative)

**Request:** `9. Conflict Resolution` → `Keep Separate`

**Body (pre-filled):**
```json
{
  "justification": "Records are distinct individuals, not duplicates"
}
```

**Run:** Click "Send"

**Expected Response (200 OK):**
```json
{
  "id": "conflict-guid",
  "status": "Resolved",
  "resolutionAction": "KeepSeparate",
  "resolvedAtUtc": "2026-03-03T11:30:00Z"
}
```

**Effect:**
- Both records preserved
- Conflict marked resolved
- Prevents re-detection

---

### Step 8: Escalate Conflict (Optional)

**Request:** `9. Conflict Resolution` → `Escalate Conflict`

**Body:**
```json
{
  "reason": "Conflicting documents. National ID shows different name than passport. Requires supervisor investigation."
}
```

**Run:** Click "Send"

**Expected Response (200 OK):**
```json
{
  "id": "conflict-guid",
  "status": "Escalated",
  "priority": "High",
  "escalatedAtUtc": "2026-03-03T11:35:00Z"
}
```

**Now in:** Senior review queue (UC-007 S05a / UC-008 S05a)

---

## Test Workflow 4: Dashboard (06_Admin)

**Collection:** `TRRCMS — 06. Admin (Users & Dashboard)`

**Scenario:** Desktop app requests dashboard statistics.

### Get Dashboard Summary

**Request:** `10. Dashboard (FR-D-12)` → `Get Dashboard Summary`

**URL:** `{{baseUrl}}/dashboard/summary`

**Run:** Click "Send"

**Expected Response (200 OK):**
```json
{
  "claims": {
    "totalClaims": 42,
    "byStatus": {
      "Draft": 5,
      "Submitted": 8,
      "UnderReview": 12,
      "Verified": 10,
      "Approved": 7
    },
    "byLifecycleStage": {
      "Submitted": 8,
      "UnderReview": 12,
      "PendingVerification": 6
    },
    "overdueCount": 2,
    "withConflictsCount": 1,
    "awaitingDocumentsCount": 3,
    "pendingVerificationCount": 6
  },
  "surveys": {
    "totalSurveys": 28,
    "byStatus": {
      "Draft": 4,
      "InProgress": 8,
      "Completed": 16
    },
    "fieldSurveyCount": 15,
    "officeSurveyCount": 13,
    "completedLast7Days": 6,
    "completedLast30Days": 18
  },
  "imports": {
    "totalPackages": 9,
    "byStatus": {
      "Pending": 1,
      "Completed": 7,
      "Failed": 1
    },
    "activeCount": 1,
    "withUnresolvedConflicts": 0,
    "totalSurveysImported": 156,
    "totalBuildingsImported": 45,
    "totalPersonsImported": 289
  },
  "buildings": {
    "totalBuildings": 156,
    "totalPropertyUnits": 789,
    "byStatus": {
      "Occupied": 140,
      "Damaged": 12,
      "Destroyed": 4
    },
    "byDamageLevel": {
      "None": 140,
      "Minor": 10,
      "Major": 5,
      "Destroyed": 1
    },
    "averageUnitsPerBuilding": 5.06
  },
  "generatedAtUtc": "2026-03-03T11:45:00Z"
}
```

**Usage:** Desktop app displays these stats on dashboard home screen.

---

## Testing Checklist

- [ ] **Authentication:** Login successful, tokens saved
- [ ] **Sync Protocol:** Session → Upload → Download → Acknowledge (all 4 steps)
- [ ] **Import Pipeline:** Upload → Stage → Validate → Detect → Approve → Commit
- [ ] **Import - building_documents:** Staged and committed correctly (check `buildingDocumentCount`)
- [ ] **Import - Contact Person:** `is_contact_person` staged on persons, `contact_person_id` linked on surveys
- [ ] **Person API:** `isContactPerson` field accepted on Add Person to Household endpoint
- [ ] **Conflict Resolution:** View queue → Resolve (merge or keep-separate) → Verify resolved
- [ ] **Dashboard:** Summary endpoint returns all 4 statistics sections
- [ ] **Environment Variables:** All auto-populated correctly:
  - `currentUserId`, `currentUserName`, `currentUserRole`
  - `syncSessionId`, `importPackageId`, `conflictId`
- [ ] **Test Responses:** Status codes match (201 for create, 200 for success, 404 for not found)
- [ ] **Console Logs:** Postman test scripts print values to console for verification

---

## Common Issues & Troubleshooting

### Issue: 401 Unauthorized
**Cause:** Access token expired or missing
**Solution:** Run `01_Auth` → `0. Authentication` → `Login (Admin)` again

### Issue: 404 Conflict Not Found
**Cause:** Using wrong `conflictId` environment variable
**Solution:** Run `05_Sync_Import` → `9. Conflict Resolution` → `Conflict Queue`, copy ID from response, manually set `conflictId` in environment

### Issue: 409 Package Not in ReadyToCommit Status
**Cause:** Skipped approval step before commit
**Solution:** Always run `Step 4 - Approve for Commit` before `Step 5 - Commit Package` (both in `05_Sync_Import`)

### Issue: File Upload Fails (multipart)
**Cause:** `.uhc` file not selected
**Solution:** Click file field, select actual binary file, ensure filename ends with `.uhc`

### Issue: Dashboard Returns Empty Statistics
**Cause:** No data committed to production yet
**Solution:** Complete at least one full import pipeline first

### Issue: Validation Shows Errors
**Cause:** .uhc file has invalid row data
**Solution:** Use properly formatted test .uhc file with valid record structure

---

## Notes

- **Always import `01_Auth` first** — all other collections depend on the `accessToken` it populates.
- **All workflows are sequential.** Start with Authentication, then Sync/Import/Conflicts, then Dashboard.
- **All 6 collections share one environment** — `TRRCMS_Development.postman_environment.json`.
- **Test scripts in Postman** automatically save environment variables (see "Tests" tab in each request).
- **Conflicts only appear after** duplicate detection completes.
- **Sync and Import are independent** — can test either without the other.
- **Dashboard queries production database** — requires completed imports to show meaningful stats.
