# Dashboard APIs - Frontend Integration Guide

Base URL: `GET /api/v1/dashboard/...`

All endpoints require JWT Bearer token and `Dashboard_View` permission.
Authorized roles: **Administrator, DataManager, FieldSupervisor, Analyst**.

HTTP errors: `401` (not logged in), `403` (no permission), `500` (server error).

---

## 1. Summary - `GET /summary`

Quick overview counts for the main dashboard landing page.

### Response

```json
{
  "claims": {
    "totalClaims": 142,
    "byStatus": { "Open": 95, "Closed": 47 },
    "byLifecycleStage": { "Submitted": 30, "UnderReview": 25, "Approved": 40, "Rejected": 7 },
    "overdueCount": 3,
    "withConflictsCount": 5,
    "awaitingDocumentsCount": 12,
    "pendingVerificationCount": 8
  },
  "surveys": {
    "totalSurveys": 310,
    "byStatus": { "Draft": 45, "Completed": 250, "Archived": 15 },
    "fieldSurveyCount": 200,
    "officeSurveyCount": 110,
    "completedLast7Days": 18,
    "completedLast30Days": 62
  },
  "imports": {
    "totalPackages": 25,
    "byStatus": { "Pending": 2, "Completed": 20, "Failed": 3 },
    "activeCount": 2,
    "withUnresolvedConflicts": 1,
    "totalSurveysImported": 180,
    "totalBuildingsImported": 90,
    "totalPersonsImported": 350
  },
  "buildings": {
    "totalBuildings": 500,
    "totalPropertyUnits": 1200,
    "byStatus": { "Occupied": 300, "Damaged": 80, "Destroyed": 20 },
    "byDamageLevel": { "None": 250, "Minor": 100, "Major": 80, "Destroyed": 20 },
    "averageUnitsPerBuilding": 2.4
  },
  "generatedAtUtc": "2026-03-11T14:30:00Z"
}
```

### Suggested UI

Top-row KPI cards showing totals: Total Claims, Total Surveys, Total Buildings, Active Imports. Each card can show a mini breakdown (e.g. Open/Closed split, Field/Office split). Optionally add a "recent activity" badge for `completedLast7Days`.

---

## 2. Trends - `GET /trends?from=&to=`

Monthly time-series data for line/bar charts.

### Query Parameters

| Param  | Type       | Required | Description                        |
|--------|------------|----------|------------------------------------|
| `from` | `DateTime` | No       | Start date filter (inclusive)      |
| `to`   | `DateTime` | No       | End date filter (inclusive)        |

If omitted, returns all-time data.

### Response

```json
{
  "claims": [
    { "year": 2026, "month": 1, "label": "2026-01", "count": 25 },
    { "year": 2026, "month": 2, "label": "2026-02", "count": 38 },
    { "year": 2026, "month": 3, "label": "2026-03", "count": 12 }
  ],
  "surveys": [
    { "year": 2026, "month": 1, "label": "2026-01", "count": 50 },
    { "year": 2026, "month": 2, "label": "2026-02", "count": 72 }
  ],
  "buildings": [ ... ],
  "persons": [ ... ],
  "imports": [ ... ],
  "generatedAtUtc": "2026-03-11T14:30:00Z"
}
```

### Suggested UI

**Multi-line chart** with 5 series (Claims, Surveys, Buildings, Persons, Imports) plotted by month. Use the `label` field as X-axis labels. Add a date range picker that maps to `from`/`to` query params. Allow toggling individual series on/off. A stacked bar chart also works well for comparing relative volumes.

---

## 3. Registration Coverage - `GET /registration-coverage`

Snapshot of all registration data completeness.

### Response

```json
{
  "totalPersons": 850,
  "totalHouseholds": 220,
  "personsByGender": { "Male": 440, "Female": 410 },
  "personsWithNationalId": 620,
  "personsWithIdentificationDocument": 0,
  "totalPersonPropertyRelations": 380,
  "relationsByType": { "Owner": 180, "Occupant": 90, "Tenant": 60, "Heir": 30, "Guest": 10, "Other": 10 },
  "relationsWithEvidence": 250,
  "claimsOpen": 95,
  "claimsClosed": 47,
  "claimsByType": { "OwnershipClaim": 90, "OccupancyClaim": 52 },
  "claimsWithAllDocuments": 80,
  "claimsMissingDocuments": 62,
  "totalEvidenceItems": 1200,
  "generatedAtUtc": "2026-03-11T14:30:00Z"
}
```

### Suggested UI

Split into 4 sections/cards:

**1. People & Households**
- KPI cards: Total Persons, Total Households
- Donut/pie chart for gender distribution (`personsByGender`)
- Progress bar: "With National ID" = `personsWithNationalId / totalPersons`

**2. Person-Property Relations**
- KPI card: Total Relations
- Horizontal bar chart for `relationsByType` (Owner, Occupant, Tenant, Heir, etc.)
- Progress bar: "With Evidence" = `relationsWithEvidence / totalPersonPropertyRelations`

**3. Claims**
- Two big numbers: Open vs Closed (or a simple donut)
- Bar chart for `claimsByType` (Ownership vs Occupancy)
- Progress bar: "Document Complete" = `claimsWithAllDocuments / (claimsWithAllDocuments + claimsMissingDocuments)`

**4. Evidence**
- Single KPI card: Total Evidence Items

---

## 4. Geographic Coverage - `GET /geographic`

Building and property unit coverage per neighborhood - for map or table views.

### Response

```json
{
  "totalNeighborhoods": 45,
  "neighborhoodsWithBuildings": 32,
  "neighborhoods": [
    {
      "code": "SY-AL-01-001-002-N001",
      "nameArabic": "حي السريان",
      "nameEnglish": "Al-Suryan",
      "buildingCount": 85,
      "propertyUnitCount": 210
    },
    {
      "code": "SY-AL-01-001-002-N002",
      "nameArabic": "حي الجميلية",
      "nameEnglish": "Al-Jamiliyah",
      "buildingCount": 120,
      "propertyUnitCount": 340
    }
  ],
  "generatedAtUtc": "2026-03-11T14:30:00Z"
}
```

### Suggested UI

**Option A - Data Table:** Sortable table with columns: Neighborhood (Arabic name + code), Buildings, Property Units. Add a coverage progress indicator at the top: "32 of 45 neighborhoods have buildings". Highlight rows with 0 buildings in a different color.

**Option B - Choropleth Map:** If you have neighborhood boundary GeoJSON, render a map where color intensity = `buildingCount`. Tooltip on hover shows the full stats. The `code` field matches the neighborhood's `FullCode` in the system.

**Option C - Both:** Table below map for detailed drill-down.

Top-level KPIs: Total Neighborhoods, Neighborhoods with Buildings, Coverage % (`neighborhoodsWithBuildings / totalNeighborhoods * 100`).

---

## 5. Personnel Workload - `GET /personnel?from=&to=`

Staff performance and workload distribution.

### Query Parameters

| Param  | Type       | Required | Description                        |
|--------|------------|----------|------------------------------------|
| `from` | `DateTime` | No       | Start date filter (inclusive)      |
| `to`   | `DateTime` | No       | End date filter (inclusive)        |

If omitted, returns all-time data.

### Response

```json
{
  "fieldCollectors": [
    {
      "userId": "a1b2c3d4-...",
      "username": "fc_ahmad",
      "fullName": "أحمد محمد",
      "surveysCompleted": 45,
      "surveysDraft": 3,
      "totalSurveys": 48,
      "assignedBuildings": 60,
      "completedBuildings": 45
    }
  ],
  "officeClerks": [
    {
      "userId": "e5f6g7h8-...",
      "username": "oc_fatima",
      "fullName": "فاطمة علي",
      "surveysCompleted": 30,
      "surveysDraft": 5,
      "totalSurveys": 35,
      "assignedBuildings": 0,
      "completedBuildings": 0
    }
  ],
  "generatedAtUtc": "2026-03-11T14:30:00Z"
}
```

`assignedBuildings` / `completedBuildings` are only relevant for field collectors (always 0 for office clerks).

### Suggested UI

Two sections with a date range picker at the top:

**Field Collectors Table:**
| Name | Surveys (Completed/Draft/Total) | Buildings (Completed/Assigned) | Progress |
|------|------|------|------|
| ... | 45 / 3 / 48 | 45 / 60 | 75% bar |

- Progress = `completedBuildings / assignedBuildings * 100`
- Sort by total surveys or completion rate
- Stacked bar chart showing each collector's completed vs draft surveys

**Office Clerks Table:**
| Name | Surveys (Completed/Draft/Total) |
|------|------|
| ... | 30 / 5 / 35 |

- Simpler table since clerks don't have building assignments
- Bar chart comparing clerk productivity

---

## Recommended Dashboard Page Layout

```
+-------------------------------------------------------+
|  HEADER: Dashboard            [Date Range Picker]     |
+-------------------------------------------------------+
|                                                       |
|  [Summary KPI Cards Row]                              |
|  Claims: 142  |  Surveys: 310  |  Buildings: 500     |
|                                                       |
+-------------------------------------------------------+
|                                                       |
|  [Trends Chart - Line/Bar]            [Coverage Pie]  |
|  Monthly creation trends               Open/Closed    |
|                                                       |
+---------------------------+---------------------------+
|                           |                           |
|  [Geographic Table/Map]   |  [Registration Coverage]  |
|  Neighborhood coverage    |  People, Relations, Docs  |
|                           |                           |
+---------------------------+---------------------------+
|                                                       |
|  [Personnel Workload]                                 |
|  Field Collectors tab | Office Clerks tab             |
|                                                       |
+-------------------------------------------------------+
```

### Data Loading Strategy

All 5 endpoints are independent. Call them in parallel on page load:

```typescript
const [summary, trends, coverage, geographic, personnel] = await Promise.all([
  api.get('/api/v1/dashboard/summary'),
  api.get('/api/v1/dashboard/trends'),        // add ?from=&to= if date filtered
  api.get('/api/v1/dashboard/registration-coverage'),
  api.get('/api/v1/dashboard/geographic'),
  api.get('/api/v1/dashboard/personnel'),      // add ?from=&to= if date filtered
]);
```

Use `generatedAtUtc` from any response to show "Last updated: ..." timestamp.

### Error Handling

| Status | Meaning | UI Action |
|--------|---------|-----------|
| 200 | Success | Render data |
| 401 | Not authenticated | Redirect to login |
| 403 | No `Dashboard_View` permission | Show "Access Denied" message |
| 500 | Server error | Show retry button with error message |

### Charting Libraries

Recommended: **Recharts** (React), **Chart.js**, or **ApexCharts**. All handle the response format directly with minimal transformation.
