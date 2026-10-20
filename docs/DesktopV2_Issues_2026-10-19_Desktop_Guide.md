# Desktop Integration Guide — Desktop v2 issues (19-10-2026)

**Date:** October 20, 2026
**Backend branch:** `feat/desktop-v2-empty-property-import-packages` (on top of `main`)
**Audience:** Desktop application developers
**Answers:** `BACKEND_CHANGES_2026-10-19.md` (Issues Report items 9A, 7 and 8)

**Compatibility:** every change is additive. No field was removed, renamed or retyped, and no route changed. Requests without the new fields behave exactly as before, so your current build and your new build both work against this backend, in either deploy order.

---

## 1. Empty property unit — household size 0 (item 9A)

Implemented as you specified it.

### Endpoints

- `POST /api/v1/Surveys/{surveyId}/households`
- `PUT  /api/v1/Surveys/{surveyId}/households/{householdId}`

### New request fields

| Field | POST | PUT |
|---|---|---|
| `isEmptyProperty` | `bool`, default `false` | `bool?`, where `null` or omitted keeps the stored value |
| `emptyPropertyJustification` | `string?`, max 2000 | `string?`, max 2000 |

### Validation

| Request | Result |
|---|---|
| `isEmptyProperty: true`, `householdSize: 0`, every count `0` (or omitted) | **201** on POST, **200** on PUT |
| `isEmptyProperty: true` with any non-zero size or count | **400** with `EmptyProperty_CountsMustBeZero` |
| `isEmptyProperty: false` or omitted, with `householdSize: 0` | **400** with `HouseholdSize_AtLeast1`, as before |
| Justification longer than 2000 characters | **400** with `EmptyPropertyJustification_MaxLength2000` |

The messages for `EmptyProperty_CountsMustBeZero`:
- EN: *When the property unit is marked empty, household size and all counts must be 0*
- AR: *عند تحديد المقسم كفارغ يجب أن تكون جميع الأعداد صفراً*

When the unit is marked empty, the gender and age sum checks are skipped, so the empty-unit message is the only error you get. When it is not empty, every existing rule applies unchanged.

On **PUT**, size and counts are checked only when you send them, the same as the rest of the partial update. The server does not zero stored counts for you. So:
- **When the box is ticked**, always send `householdSize: 0` and all six counts as `0`. Your payload already does this. If you omit them, the stored non-zero values stay.
- **When the box is unticked**, always send `householdSize >= 1` with matching counts. Otherwise a stored size of 0 stays.

### Justification round-trip (PUT)

| Sent | Stored justification after save |
|---|---|
| `isEmptyProperty: true`, `emptyPropertyJustification: null` or blank | **kept** (for example the tablet one) |
| `isEmptyProperty: true`, `emptyPropertyJustification: "text"` | replaced with `"text"` (trimmed) |
| `isEmptyProperty: false` | cleared (`null`) |
| `isEmptyProperty` omitted | unchanged |

Sending back what you received, or `null`, is therefore always safe.

### Response

The `HouseholdDto` already carried `isEmptyProperty` and `emptyPropertyJustification`. They now reflect the value you saved.

---

## 2. Import packages list — locations and "assigned by" (item 7)

`GET /api/v1/import/packages`: each `items[].buildings[]` entry now has:

| Field | Change |
|---|---|
| `governorateName`, `districtName`, `subDistrictName`, `communityName`, `neighborhoodName` | Now filled for real tablet packages. Those packages leave the `*_name` columns NULL, so the names are resolved on the server from the package's administrative codes (Arabic names). A name written in the package still wins. A level stays `null` when its code is not in the server's administrative reference data. |
| `assignedByName` (new, `string?`) | Display name of the supervisor who assigned the building to the package's collector: `fullNameArabic`, else `fullNameEnglish`, else `username`. Closed or completed assignments count. When there are several, the latest wins. `null` when no assignment matches or it has no assigner. |

Your reading logic needs no change: take the first non-empty of neighbourhood → community → sub-district → district → governorate, and show the distinct `assignedByName` values as `أسند المبنى: …` (or `—`).

The buildings come from the `.uhc` file of each package. A package whose file is missing or unreadable still returns `buildings: []` without failing the list.

---

## 3. Claim notes — `contractDetails: null` in PATCH (item 8, FYI)

**Confirmed: `contractDetails: null` is accepted and keeps the stored `ContractDetails`.** It does not clear it. The PATCH is a partial update, so `null` means "not sent".

`PATCH /api/v1/Surveys/{surveyId}/relations/{relationId}` with `notes: "<text>"` and `contractDetails: null` therefore saves the text in `Notes` and leaves the original text in `ContractDetails` too.

If you want the text to *move* rather than be copied, send `clearContractDetails: true`, a flag that already exists:

```json
{ "notes": "<text>", "clearContractDetails": true }
```

Either way the text is preserved in `Notes`.

---

## 4. Not changed

The generic `POST/PUT /api/v1/Households` endpoints (item 6, marked optional and not used by the desktop) were **not** changed. They are shared with other clients, so the change was held back for a separate decision. They still require `householdSize >= 1`.
