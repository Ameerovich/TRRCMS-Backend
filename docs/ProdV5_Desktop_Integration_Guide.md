# Desktop Integration Guide — ProdV5

**Date:** October 17, 2026
**Backend branch:** `ProdV5` (on top of `main`, includes PRs #44, #45, #46)
**Audience:** Desktop application developers (office survey / claims / import screens) and the QGIS plugin developer

**Compatibility:** every API change is additive. No request field became required, no response field was removed, renamed or retyped, and no route changed. The current desktop build keeps working against ProdV5. Two behaviours changed for everyone (sections 2.4 and 9); everything else is opt-in.

---

## Table of Contents

1. [V2 Bug 1 — date of birth](#1-v2-bug-1--date-of-birth)
2. [Session expiry warning, renewal and login redirect](#2-session-expiry-warning-renewal-and-login-redirect)
3. [Change password / forgot password](#3-change-password--forgot-password)
4. [V2 Bug 2 — claim type dropdown](#4-v2-bug-2--claim-type-dropdown)
5. [V2 Bug 3 — description and notes after import](#5-v2-bug-3--description-and-notes-after-import)
6. [Contact Point / Rights Claimant designations](#6-contact-point--rights-claimant-designations)
7. [Household "empty property" (read-only)](#7-household-empty-property-read-only)
8. [Vocabularies](#8-vocabularies)
9. [Import packages screen (PR #45) and QGIS permissions (PR #46)](#9-import-packages-screen-pr-45-and-qgis-permissions-pr-46)
10. [QGIS plugin — session handling](#10-qgis-plugin--session-handling)
11. [Change summary](#11-change-summary)

---

## 1. V2 Bug 1 — date of birth

**Symptom:** after importing a tablet package, "تاريخ الميلاد" in claims → resume case → tenure claims → person info showed a different day/month than entered on mobile.

**Root cause (backend + mobile):** the `.uhc` package only carried `year_of_birth`, so every imported person was stored as 1 January of that year. The mobile app now sends `date_of_birth` (see the mobile guide); the server imports it exactly. Persons imported earlier still show 01/01 — the day and month were never received.

**What the desktop must do:**

- **Send** the date as a calendar date. Preferred: `"1990-05-12"`. Also accepted: `"1990-05-12T00:00:00Z"`, `"1990-05-12T00:00:00+03:00"`. The server normalises every form to `1990-05-12T00:00:00Z`.
- **Display** the date part of the value **without time-zone conversion**:

```csharp
// API value: "1990-05-12T00:00:00Z"
var dob = DateTime.Parse(json, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
dayPicker.Value = dob.Day; monthPicker.Value = dob.Month; yearPicker.Value = dob.Year;   // never .ToLocalTime()
```

Calling `.ToLocalTime()` on a UTC midnight value is fine in +03:00 but shifts the day for any zone west of UTC; reading the parts in UTC is always right.

Affects: `PersonDto.dateOfBirth` everywhere (persons, household persons, contact person, staged persons).

---

## 2. Session expiry warning, renewal and login redirect

Client requirement: sessions must not expire silently. Warn a few seconds before expiry with OK to renew; once expired, show the login screen.

### 2.1 Timing fields

`POST /api/v1/auth/login`, `POST /api/v1/auth/refresh` and change-password responses add:

```json
"serverTimeUtc": "2026-10-17T15:15:00Z",
"accessTokenExpiresInSeconds": 1800,
"sessionExpiryWarningSeconds": 60
```

Schedule timers from `accessTokenExpiresInSeconds` (server clock) — **not** from `accessTokenExpiry` minus the PC clock. Office PCs are often minutes off; that is what produced the "login succeeds but nothing happens" loop on the offline server.

`GET /api/v1/auth/session` returns the same timing for the current token (use after the PC wakes from sleep). It does not extend the session.

### 2.2 Flow

```csharp
void ScheduleSession(int expiresInSeconds, int warningSeconds)
{
    _warnTimer.Change(TimeSpan.FromSeconds(Math.Max(0, expiresInSeconds - warningSeconds)), Timeout.InfiniteTimeSpan);
    _expireTimer.Change(TimeSpan.FromSeconds(expiresInSeconds), Timeout.InfiniteTimeSpan);
}
// warn timer  → dialog: "ستنتهي الجلسة خلال {n} ثانية. هل تريد المتابعة؟"  [موافق] [تسجيل الخروج]
// OK          → POST /api/v1/auth/refresh { refreshToken } → store BOTH new tokens → ScheduleSession(...)
// expire timer / refresh fails → clear tokens → show login (return to the same screen afterwards)
```

- Refresh tokens are **single-use** (rotated on every refresh). Never run two refreshes at once: guard with one shared `Task`/semaphore.
- After every successful refresh store the **new** refresh token.

### 2.3 401 handling

401 responses now say why (`errorCode` in the body, plus headers):

| `errorCode` | Extra header | Action |
|-------------|--------------|--------|
| `TOKEN_EXPIRED` | `Token-Expired: true` | Refresh once and retry; if refresh fails → login |
| `SESSION_REVOKED` | `WWW-Authenticate: Bearer error="invalid_token", …` | Login directly (password changed/reset, role changed or account deactivated) |
| `TOKEN_INVALID` | same | Login |
| `AUTHENTICATION_REQUIRED` | `WWW-Authenticate: Bearer` | Login |

### 2.4 Behaviour change for every client

Changing or resetting a password, or deactivating an account, now **ends that user's existing sessions immediately** (`401 SESSION_REVOKED` on the next request). Before, old tokens kept working until they expired. Handle `SESSION_REVOKED` as above.

---

## 3. Change password / forgot password

### 3.1 Change my password (profile)

```
POST /api/v1/auth/change-password
{ "currentPassword": "...", "newPassword": "...", "confirmPassword": "..." }
```

- `userId` is optional now (defaults to the logged-in user).
- The response now includes a **fresh token pair** for this session (same fields as `/refresh` plus `success` and `message`). **Replace the stored tokens** and reschedule the session timers. Other devices of the user are signed out.
- Old behaviour still works: if the desktop ignores the new fields it gets `401 SESSION_REVOKED` on the next call and shows login.

### 3.2 Admin-reset users

When an administrator resets a user's password, login returns `mustChangePassword: true` with a restricted token. The desktop already handles this flow (change-password screen). The change-password response now carries full tokens, so you can go straight to the main screen instead of asking the user to log in again.

### 3.3 "Forgot password?" on the login screen

```
POST /api/v1/auth/forgot-password       (no token)
{ "username": "clerk01", "note": "هاتف المكتب 214" }
→ 202 { "message": "<localized: request sent to the administrator>" }
```

Always 202 with the same message (do not reveal whether the user exists). `429` after 3 requests per 10 minutes. The administrator resets the password from the dashboard and gives the temporary password to the user.

---

## 4. V2 Bug 2 — claim type dropdown

**Symptom:** "نوع الادعاء" in tenure claims → claim info listed relation types (مالك، ساكن، مستأجر، ضيف، وريث، أخرى) instead of the two claim types used on mobile, and the value did not survive import.

**Backend fixes:**

- Every relation response (`PersonPropertyRelationDto`) now has **`claimType`**: `1` = ملكية (Ownership), `2` = إشغال (Occupancy). It is derived from `relationType`: Owner/Heir → 1, everything else (including admin-added relation types) → 2.
- Import keeps the tablet's choice: when the tablet sends `relation_type = 0` (app 1.0.19) or a value that contradicts the claim, the relation is aligned to the claim type.

**What the desktop must do:**

1. Fill the dropdown from the **`claim_type`** vocabulary (`GET /api/v1/Vocabularies`, name `claim_type`): exactly two values.

   | code | labelArabic | labelEnglish |
   |------|-------------|--------------|
   | 1 | مطالبة ملكية | Ownership claim |
   | 2 | مطالبة إشغال | Occupancy claim |

2. Pre-select it from `relation.claimType`.
3. Save with `claimType` (supported since PR #44) — **do not send `relationType` from this dropdown**:

```
PATCH /api/v1/surveys/{surveyId}/relations/{relationId}
{ "claimType": 1, "ownershipShare": 1200 }
```

```
POST /api/v1/surveys/{surveyId}/property-units/{unitId}/link     (new relation)
{ "personId": "...", "claimType": 2, "notes": "..." }
```

| Sent | Stored `relationType` | Notes |
|------|-----------------------|-------|
| `claimType: 1` | Owner (1) — or kept if already Owner/Heir | `ownershipShare` required (1–2400) |
| `claimType: 2` | Occupant (2) — or kept if already an occupancy type | ownership share is cleared |
| `claimType` + inconsistent `relationType` | — | `400` |

Show "حصة الملكية" only when `claimType == 1`.

---

## 5. V2 Bug 3 — description and notes after import

**Symptom:** after importing a package, "الوصف" and "ملاحظات" in claim info were empty although entered on mobile.

**Mapping** (unchanged for the desktop — it already binds these):

| Desktop field | Relation field |
|---------------|----------------|
| الوصف (description) | `contractDetails` |
| ملاحظاتك (notes) | `notes` |

**Backend fix:** the tablet writes the description to its claim (`claims.claim_description`). Import now copies it into the relation's `contractDetails` (max 2000 characters), and a data migration filled `contractDetails` for relations imported earlier. Notes are imported from the relation's `notes` column; packages from mobile app 1.0.19 leave it empty — fixed on the mobile side.

No desktop change needed. After the migration, re-open the affected cases to verify.

---

### 5.1 Claims list fix — one claim per relation

`GET /api/v1/Claims` returned `400` ("An item with the same key has already been added") whenever a person had more than one relation on the same unit (e.g. owner and occupant). Fixed — no desktop change needed. The contract is unchanged; `sourceRelationId` / `evidenceIds` / `hasEvidence` now always refer to the claimant's relation whose claim type matches the claim (`claimType` 1 → Owner/Heir relation, 2 → other types). Such a person has two claims, each with its own relation and evidence, and `PUT /api/v1/Claims/{id}` edits only that claim's relation. List, detail (`GET /Claims/{id}`) and summaries return the same `sourceRelationId`. `GET /api/v1/Claims?PropertyUnitId=...` now returns every claim of the unit (it returned only the first one).

---

## 6. Contact Point / Rights Claimant designations

Client requirement: when several persons exist in a case, the desk operator must see who to call.

| Designation | Arabic | Rule |
|-------------|--------|------|
| Contact Point | جهة الاتصال | The survey's contact person |
| Rights Claimant | صاحب حق | Has an active relation (claim) on the unit |

Both are derived by the server; a person can hold both. Show two badges in every person list.

### 6.1 Fields

| Endpoint | New / changed |
|----------|---------------|
| `GET /surveys/{id}/households/{hid}/persons`, `GET /surveys/{id}/contact-person` | `isContactPerson` = contact of **this** survey; new `isRightsClaimant` = claim on **this** survey's unit |
| `GET /persons`, `GET /persons/{id}` | `isContactPerson` = contact of any survey; `isRightsClaimant` = any active relation |
| `GET /surveys/{id}/property-units/{unitId}/relations` | each relation adds `isContactPoint` and `claimType` |
| `GET /cases/{id}`, `GET /cases/by-property-unit/{unitId}` | new `persons[]`: name, national id, **mobile/phone**, `isContactPoint`, `isRightsClaimant`, primary `relationType`/`claimType`, all `relations[]` — contact points first |
| `GET /claims/{id}` | new `contactPoint` (same shape as a case person) and `primaryClaimantIsContactPoint` |

Use `CaseDto.persons` for a "Persons in this case" panel with a call button on the contact points.

### 6.2 Change the contact point (office survey)

```
POST /api/v1/surveys/{surveyId}/contact-point
{ "personId": "8b49a5c7-..." }
→ 200 PersonDto (isContactPerson = true)
```

- Draft surveys only (`400` otherwise).
- The person must belong to the survey: a household member on the survey's unit, a claimant on the unit, a contact of another survey on the unit, or a stand-alone contact person (created with `POST /contact-person`). Anyone else → `400`.
- The previous contact point stays in the survey as a normal person.

Existing calls are unchanged: `POST /surveys/{id}/contact-person` (create a new contact) and `isContactPerson: true` in `POST /surveys/{id}/households/{hid}/persons` still set the contact point and now also release the previous one correctly.

---

## 7. Household "empty property" (read-only)

Field collectors can now mark a unit as found empty during the survey (tablet only). Imported households carry:

```json
"isEmptyProperty": true,
"emptyPropertyJustification": "الوحدة مغلقة والسكان نازحون منذ 2016",
"householdSize": 0
```

Show it in **household info** (معلومات الأسرة) as a read-only notice: "عقار فارغ — {justification}". Office/desk surveys never set it (`false`). Do not send these fields in create/update household requests (they are ignored). An empty household can have `householdSize = 0`; if the household edit form round-trips the size, hide editing for empty households (the update validator still requires 1–50).

In the **import review** screen the staged household shows `Empty property (عقار فارغ): <justification>` in `displayInfo`, and a missing justification appears as a validation warning.

---

## 8. Vocabularies

- `evidence_type`, `building_type`, `occupancy_type`, `relation_type` are now **non-system**; admins may add values. `survey_source` is now **system**.
  - Never hard-code these lists or their labels; load them from `GET /api/v1/Vocabularies` and render labels by code.
  - English name fields such as `buildingTypeName` show the number for admin-added codes — use the vocabulary label.
  - A new relation type is an occupancy claim (claimType 2).
- Renamed dictionaries (titles come from `displayNameArabic`/`displayNameEnglish`):

  | Code | New name |
  |------|----------|
  | `document_type` | نوع وثيقة إثبات الشخصية — Personal Identification Document Type |
  | `evidence_type` | نوع الوثيقة الداعمة للادعاء — Claim Supporting Document Type |

  Update any screen label that hard-codes "نوع الوثيقة" / "نوع الدليل" (e.g. the ID document type and claim document type pickers).

---

## 9. Import packages screen (PR #45) and QGIS permissions (PR #46)

`GET /api/v1/import/packages` items add `collectorName` and `buildings[]` (`buildingNumber`, `governorateName`, `districtName`, `subDistrictName`, `communityName`, `neighborhoodName`) for a readable package list. `page`/`pageSize` in the response are the values actually applied (max page size 100).

**Behaviour change (PR #46):** landmark and street write endpoints were open to any logged-in user; they now require:

| Endpoint | Permission | Roles |
|----------|-----------|-------|
| `POST landmarks/register`, `bulk-register`, `PUT landmarks/{id}` (same for `streets`) | `MapReferenceData_Write` (9021) | Administrator, DataManager, OfficeClerk |
| `DELETE landmarks/{id}`, `DELETE streets/{id}` | `MapReferenceData_Delete` (9022) | Administrator, DataManager |

Other roles get `403`. OfficeClerk also gained `Buildings_Create` / `Buildings_Update` for the QGIS plugin. Hide delete buttons for OfficeClerk.

---

## 10. QGIS plugin — session handling

See the dedicated **`ProdV5_QGIS_Integration_Guide.md`** (session warning/renew/re-login with code for `api_client.py` and `plugin.py`, PR #46 permissions, test checklist).

Also relevant to the desktop: changing a user's **role** now ends that user's sessions (`401 SESSION_REVOKED`) so the next login carries the new role — handle it like a password reset (section 2.4).

---

## 11. Change summary

| # | Area | Desktop action |
|---|------|----------------|
| 1 | DOB | Send `yyyy-MM-dd`; display date parts in UTC |
| 2 | Sessions | Warning timer from `accessTokenExpiresInSeconds`; renew via `/refresh`; login on expiry; branch on `errorCode` |
| 2.4 | Sessions | Handle `SESSION_REVOKED` after password change/reset/deactivation |
| 3 | Passwords | Store tokens returned by change-password; add "Forgot password?" |
| 4 | Claim type | Dropdown from `claim_type` vocab; read `claimType`; save `claimType` |
| 5 | Description/notes | None (backend fix + migration) |
| 5.1 | Claims list | None — 400 fixed; `sourceRelationId` = relation matching the claim type |
| 6 | Designations | Badges; case persons panel; `POST /contact-point` |
| 7 | Empty property | Read-only notice in household info |
| 8 | Vocabularies | No hard-coded lists/titles; new dictionary names |
| 9 | Import list / QGIS | Optional new columns; handle 403 on map writes |
| 10 | QGIS | Expiry warning + `errorCode` handling |
