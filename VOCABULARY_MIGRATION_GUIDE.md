# Vocabulary Migration Guide — PyQt5 Frontend

## What Changed on the API

The TRRCMS backend now returns **all enum fields as integers** instead of strings.
A new **public Vocabularies API** provides Arabic/English labels for every enum value.

### Before vs After

| Field Example | Before (string) | After (integer) |
|--------------|-----------------|-----------------|
| `gender` | `"Male"` | `1` |
| `nationality` | `"Syrian"` | `1` |
| `status` (survey) | `"Draft"` | `1` |
| `surveyType` | `"Office"` | `2` |
| `relationType` | `"Owner"` | `1` |
| `occupancyType` | `"OwnerOccupied"` | `1` |
| `occupancyNature` | `"LegalFormal"` | `1` |
| `evidenceType` | `"OwnershipDeed"` | `2` |
| `buildingType` | `"Residential"` | `1` |
| `buildingStatus` | `"Intact"` | `1` |
| `damageLevel` | `"Minor"` | `1` |
| `unitType` | `"Apartment"` | `1` |
| `claimStatus` | `"Draft"` | `1` |
| `transferStatus` | `"Transferred"` | `3` |

**This affects ALL API responses.** Requests that were already sending integers remain unchanged.

---

## Part A — DevOps / Docker Deployment

### Step 1: Rebuild & redeploy

```bash
git pull origin main
docker-compose build api
docker-compose up -d
```

Everything else is automatic:
- Database migration runs on startup (`MigrateAsync()`) — creates the `Vocabularies` table
- Seed data runs on startup — populates 24 vocabularies with Arabic/English labels
- No config file changes needed

### Step 2: Verify

```bash
# Health check
curl http://localhost:8080/health

# Test vocabularies endpoint (public, no auth)
curl http://localhost:8080/api/v1/vocabularies
```

---

## Part B — PyQt5 Frontend Migration Steps

### Step 1: Fetch and cache vocabularies on app startup

Call the new public endpoint **once on startup** (no authentication required):

```
GET {BASE_URL}/api/v1/vocabularies
```

Response is a JSON array of 24 vocabulary objects. Each object has:

```json
{
  "vocabularyName": "gender",
  "displayNameArabic": "الجنس",
  "displayNameEnglish": "Gender",
  "version": "1.0.0",
  "category": "Demographics",
  "values": [
    { "code": 1, "labelArabic": "ذكر", "labelEnglish": "Male", "displayOrder": 0 },
    { "code": 2, "labelArabic": "أنثى", "labelEnglish": "Female", "displayOrder": 1 }
  ]
}
```

**What to do:**
- Make a GET request to `/api/v1/vocabularies` during application initialization
- Store the result in memory (a Python dictionary) — this data rarely changes
- Build a lookup structure: `vocab_name -> code -> {ar, en}` for quick label resolution
- Optionally filter by category: `GET /api/v1/vocabularies?category=Demographics`

**Available categories:** `Demographics`, `Property`, `Relations`, `Legal`, `Claims`, `Survey`, `Operations`

---

### Step 2: Create a vocabulary service/helper module

Create a single module (e.g., `vocab_service.py`) that the rest of the app uses. It should provide:

1. **`get_label(vocab_name, code, lang="ar")`** — Returns the display label for a given integer code
   - Example: `get_label("gender", 1, "ar")` returns `"ذكر"`
   - Example: `get_label("gender", 1, "en")` returns `"Male"`
   - Should return a fallback (e.g., `str(code)`) if the code is not found

2. **`get_options(vocab_name, lang="ar")`** — Returns a list of `(code, label)` tuples for populating QComboBox dropdowns
   - Example: `get_options("gender", "ar")` returns `[(1, "ذكر"), (2, "أنثى")]`
   - Sorted by `displayOrder`

3. **`get_all_vocabularies()`** — Returns the raw cached data if needed

---

### Step 3: Update all QComboBox / dropdown population

Everywhere you populate a QComboBox with enum values, change from hardcoded items to vocabulary-driven items.

**Pattern to apply in every form that has enum dropdowns:**
- Instead of `combo.addItem("ذكر", "Male")`, use the vocabulary service to get options
- The `userData` (data role) for each item must be the **integer code** (e.g., `1`), not the string name
- When reading the selected value to send to the API, read the integer code from the data role

**Affected dropdowns across the app (search for these field names):**

| Vocabulary Name | Fields / Forms That Use It |
|----------------|---------------------------|
| `gender` | Person forms (create/edit person) |
| `nationality` | Person forms (create/edit person) |
| `relationship_to_head` | Person forms (relationship to household head) |
| `building_type` | Building forms |
| `building_status` | Building forms |
| `damage_level` | Building forms |
| `occupancy_type` | Household forms, Relation forms |
| `occupancy_nature` | Household forms |
| `relation_type` | Person-Property relation forms |
| `evidence_type` | Evidence upload forms (tenure documents) |
| `survey_type` | Survey creation (Field=1, Office=2) |
| `survey_status` | Survey filtering/display |
| `claim_status` | Claim display/filtering |
| `tenure_contract_type` | Relation forms |

---

### Step 4: Update all display/table cells that show enum values

Everywhere the app displays an enum value from an API response (in QTableWidget cells, QLabel text, detail views, etc.), the value is now an **integer**. You need to resolve it to a label.

**Pattern to apply:**
- Wherever you display `response["gender"]`, it used to be `"Male"` — now it's `1`
- Replace direct display with: `get_label("gender", response["gender"], "ar")`
- Apply this to every field listed in the mapping table below

**Common places to check:**
- Survey detail views (status, surveyType)
- Person detail views / tables (gender, nationality, relationshipToHead)
- Building detail views / tables (buildingType, status, damageLevel)
- Household detail views (occupancyType, occupancyNature)
- Relation tables (relationType, occupancyType)
- Evidence lists (evidenceType)
- Claim views (claimStatus, claimSource, casePriority)
- Property unit views (unitType, status)

---

### Step 5: Update request payloads (if sending strings)

Check every API request your app makes. If any request payload sends a **string** for an enum field, change it to the **integer code**.

Most requests were likely already sending integers, but verify these specifically:
- `gender`, `nationality`, `relationshipToHead` in person creation/update
- `occupancyType`, `occupancyNature` in household creation/update
- `relationType`, `occupancyType` in relation creation/update
- `evidenceType` in tenure document upload
- `unitType`, `status` in property unit creation/update
- `buildingType`, `status`, `damageLevel` in building creation/update

---

### Step 6: Handle PropertyUnit enums (not in vocabularies)

**Important:** `PropertyUnitType` (Apartment=1, Shop=2, Office=3, Warehouse=4, Other=5) and `PropertyUnitStatus` (Occupied=1, Vacant=2, Damaged=3, UnderRenovation=4, Uninhabitable=5, Locked=6, Unknown=99) are **NOT** in the vocabularies API — they are system/internal enums.

These must remain hardcoded in the frontend or stored in a local config. The integer codes are fixed and listed above.

---

## Complete Vocabulary Name to API Field Mapping

Use this table to know which `vocabularyName` to use when resolving labels for each API response field.

| Vocabulary Name | API Response Field(s) | DTO(s) | Category |
|----------------|----------------------|--------|----------|
| `gender` | `gender` | PersonDto | Demographics |
| `nationality` | `nationality` | PersonDto | Demographics |
| `age_category` | `ageCategory` | PersonDto | Demographics |
| `relationship_to_head` | `relationshipToHead` | PersonDto | Demographics |
| `building_type` | `buildingType` | BuildingDto | Property |
| `building_status` | `status` | BuildingDto | Property |
| `damage_level` | `damageLevel` | BuildingDto | Property |
| `occupancy_type` | `occupancyType` | HouseholdDto, PersonPropertyRelationDto | Property |
| `occupancy_nature` | `occupancyNature` | HouseholdDto | Property |
| `tenure_contract_type` | `tenureContractType` | PersonPropertyRelationDto | Property |
| `relation_type` | `relationType` | PersonPropertyRelationDto, CreatedClaimSummaryDto | Relations |
| `evidence_type` | `evidenceType` | EvidenceDto | Legal |
| `document_type` | `documentType` | DocumentDto | Legal |
| `verification_status` | `verificationStatus` | DocumentDto | Legal |
| `claim_status` | `claimStatus` | ClaimDto, FieldSurveyDetailDto | Claims |
| `claim_source` | `claimSource` | ClaimDto, CreatedClaimSummaryDto | Claims |
| `case_priority` | `casePriority` | ClaimDto, CreatedClaimSummaryDto | Claims |
| `lifecycle_stage` | `lifecycleStage` | ClaimDto | Claims |
| `certificate_status` | `certificateStatus` | CertificateDto | Claims |
| `survey_type` | `surveyType` | SurveyDto, FieldSurveyDetailDto, OfficeSurveyDetailDto | Survey |
| `survey_status` | `status` | SurveyDto, FieldSurveyDetailDto, OfficeSurveyDetailDto | Survey |
| `survey_source` | `surveySource` | SurveyDto | Survey |
| `transfer_status` | `transferStatus` | BuildingAssignmentDto | Operations |
| `referral_role` | `referralRole` | ReferralDto | Operations |

---

## Testing Checklist

After applying all changes, test these workflows:

1. **App startup** — Vocabularies fetch successfully, dropdowns populate with Arabic labels
2. **Create field survey** — All dropdowns work, request sends integers, response displays labels
3. **Create office survey** — Same as above
4. **Add person to household** — Gender, nationality, relationshipToHead dropdowns and display
5. **Create/edit household** — OccupancyType, occupancyNature dropdowns and display
6. **Link person to property** — RelationType, occupancyType dropdowns and display
7. **Upload evidence** — EvidenceType selection and display
8. **Create/edit property unit** — UnitType, status dropdowns and display (hardcoded, not from vocabularies)
9. **Create/edit building** — BuildingType, status, damageLevel dropdowns and display
10. **View survey details** — Status and surveyType display correctly as labels
11. **View claims** — ClaimStatus, claimSource, casePriority display correctly
12. **List views / tables** — All enum columns show labels instead of raw integers
