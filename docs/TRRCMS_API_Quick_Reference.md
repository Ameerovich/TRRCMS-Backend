# TRRCMS API — Quick Reference for Mobile Team

**Version:** 1.0 | **Date:** 2026-03-14 | **Server:** ASP.NET Core 8

---

## Accessing the API Documentation

### Swagger UI (Interactive)
```
https://localhost:7001/swagger
```
Browse all endpoints, view request/response schemas, and test calls directly from the browser.

### OpenAPI JSON (Machine-readable)
```
https://localhost:7001/swagger/v1/swagger.json
```
Import this URL into Postman, Insomnia, or code generators to auto-generate client code.

> **Docker deployment:** Replace `localhost:7001` with `localhost:8080` (HTTP).

---

## Authentication

All sync endpoints require a JWT Bearer token.

```
POST /api/v1/Auth/login
Content-Type: application/json

{
  "username": "fieldcollector1",
  "password": "Test@123",
  "deviceId": "TABLET-FIELD-001"
}
```

**Response:**
```json
{
  "accessToken": "eyJhbGciOi...",
  "refreshToken": "a1b2c3...",
  "accessTokenExpiresAt": "2026-03-14T10:15:00Z",
  "userId": "guid",
  "fullNameArabic": "جامع بيانات 1",
  "role": "FieldCollector"
}
```

Use the `accessToken` as Bearer token in all subsequent requests:
```
Authorization: Bearer eyJhbGciOi...
```

Token expires in **15 minutes**. Refresh with:
```
POST /api/v1/Auth/refresh
{ "refreshToken": "a1b2c3..." }
```

---

## Sync Protocol Endpoints

| Step | Method | Endpoint | Auth | Description |
|------|--------|----------|------|-------------|
| 1 | POST | `/api/v1/sync/session` | Yes | Open sync session |
| 2 | POST | `/api/v1/sync/upload` | Yes | Upload .uhc package (multipart/form-data) |
| 3 | GET | `/api/v1/sync/assignments?sessionId={id}` | Yes | Download assignments + vocabularies |
| 4 | POST | `/api/v1/sync/assignments/ack` | Yes | Acknowledge assignment receipt |

**Required permission:** `CanSyncData` (System_Sync 9010)
**Allowed roles:** FieldCollector, FieldSupervisor, Administrator

### Step 1 — Create Session
```
POST /api/v1/sync/session
Content-Type: application/json

{
  "fieldCollectorId": "user-guid",
  "deviceId": "TABLET-FIELD-001",
  "serverIpAddress": "192.168.1.100"
}
```
Response: `201 Created` with `SyncSessionDto` (save `id` as sessionId)

### Step 2 — Upload Package
```
POST /api/v1/sync/upload
Content-Type: multipart/form-data

Form fields:
  file:               <.uhc file>
  SyncSessionId:      <session-id from Step 1>
  PackageId:          <UUID from .uhc manifest>
  DeviceId:           TABLET-FIELD-001
  CreatedUtc:         2026-03-14T09:00:00Z
  SchemaVersion:      1.0.0
  AppVersion:         1.0.0
  Sha256Checksum:     <SHA-256 hex string>
  VocabVersionsJson:  {"building_type":"1.0.0","gender":"1.0.0"}  (optional)
  FormSchemaVersion:  1.0.0  (optional)
```
Response: `200 OK` with `UploadSyncPackageResultDto`

**Max file size:** 500 MB
**Idempotent:** Same PackageId → `accepted=true, isDuplicate=true`

### Step 3 — Download Assignments
```
GET /api/v1/sync/assignments?sessionId=<session-id>&modifiedSinceUtc=<optional>
```
Response: `200 OK` with `SyncAssignmentPayloadDto`

Returns building assignments + full vocabulary snapshot.
Save `generatedAtUtc` and pass as `modifiedSinceUtc` on next sync for incremental download.

### Step 4 — Acknowledge
```
POST /api/v1/sync/assignments/ack
Content-Type: application/json

{
  "syncSessionId": "<session-id>",
  "assignmentIds": ["guid-1", "guid-2"]
}
```
Response: `200 OK` with `SyncAckResultDto`

**Idempotent:** Already-transferred assignments are silently skipped.
Closes the session (status → Completed or PartiallyCompleted).

---

## Vocabulary Endpoint

```
GET /api/v1/vocabularies
```

**Public — no authentication required.**

Returns all active vocabularies with bilingual labels (Arabic + English).

```json
[
  {
    "id": "guid",
    "name": "building_type",
    "version": "1.0.0",
    "category": "Building",
    "values": [
      { "code": 1, "labelArabic": "سكني", "labelEnglish": "Residential" },
      { "code": 2, "labelArabic": "تجاري", "labelEnglish": "Commercial" }
    ]
  }
]
```

**Vocabulary domains:** building_type, building_usage, building_condition, property_unit_type, floor_location, gender, nationality, relationship_to_head, relation_type, evidence_type, evidence_source_type, evidence_condition, survey_type

---

## Error Handling

| HTTP Status | Meaning | Action |
|-------------|---------|--------|
| 200 | Success | Process response |
| 201 | Created | Resource created (Step 1) |
| 400 | Validation error | Fix request body per `errors` array |
| 401 | Unauthorized | Token expired → refresh or re-login |
| 403 | Forbidden | User lacks CanSyncData permission |
| 404 | Not found | Resource doesn't exist |
| 409 | Conflict | Duplicate or state conflict |
| 413 | Payload too large | File exceeds 500 MB |
| 500 | Server error | Retry or report |

**Standard error response:**
```json
{
  "type": "ValidationException",
  "title": "Validation Failed",
  "status": 400,
  "errors": {
    "Sha256Checksum": ["Checksum mismatch: expected abc123, got def456"]
  }
}
```

---

## Health Check

```
GET /health
```
Returns `200 Healthy` when API and database are operational.

---

## Handoff Package Contents

| # | File | Purpose |
|---|------|---------|
| 1 | `TRRCMS_Mobile_Sync_Integration_Guide.md` (v1.3) | Full protocol spec, .uhc SQLite format, checksum algorithm, FK rules, Dart pseudocode |
| 2 | `TRRCMS_Mobile_Sync_Flow.postman_collection.json` | Ready-to-run Postman collection (auth → sync → vocab) |
| 3 | `TRRCMS_API_Quick_Reference.md` (this file) | API endpoints, auth, error codes |
| 4 | `TRRCMS_Development.postman_environment.json` | Postman environment variables |
| 5 | Swagger UI at `/swagger` | Live interactive API documentation |
| 6 | OpenAPI JSON at `/swagger/v1/swagger.json` | Import into code generators (Dart, Kotlin, Swift) |

### Code Generation from OpenAPI

Mobile team can auto-generate HTTP client code from the OpenAPI spec:

**Dart/Flutter:**
```bash
dart run build_runner build  # with openapi_generator package
# or
npx openapi-generator-cli generate -i swagger.json -g dart -o ./api_client
```

**Kotlin (Android):**
```bash
npx openapi-generator-cli generate -i swagger.json -g kotlin -o ./api_client
```
