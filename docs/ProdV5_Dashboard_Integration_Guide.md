# Dashboard Integration Guide — ProdV5

**Date:** October 17, 2026
**Backend branch:** `ProdV5` (on top of `main`, includes PRs #44, #45, #46)
**Audience:** Web dashboard (admin) frontend developers

Every change below is **additive**: no existing request or response field was removed, renamed or retyped. A dashboard built against `main` keeps working; the sections below say what to add to use the new features.

---

## Table of Contents

1. [Password management (admin reset, forgot-password queue, self change)](#1-password-management)
2. [Session expiry warning and renewal](#2-session-expiry-warning-and-renewal)
3. [Contact Point / Rights Claimant designations](#3-contact-point--rights-claimant-designations)
4. [Vocabulary management changes](#4-vocabulary-management-changes)
5. [Household "empty property" flag](#5-household-empty-property-flag)
6. [Import packages list — collector and buildings (PR #45)](#6-import-packages-list--collector-and-buildings-pr-45)
7. [Permissions and audit log](#7-permissions-and-audit-log)
8. [Bug fixes — role change, claims list, admin imports](#8-bug-fix--changing-a-users-role)
9. [Change summary](#9-change-summary)

---

## 1. Password management

### 1.1 Admin: reset a user's password

```
POST /api/v1/users/{id}/reset-password
Authorization: Bearer <admin token>        (permission Users_ResetPassword = 8004, Administrator)
```

Body is **optional**:

| Body | Behaviour |
|------|-----------|
| none / `{}` | Server generates a policy-compliant temporary password and returns it **once** |
| `{ "temporaryPassword": "Temp@2026-Aleppo" }` | Uses the admin's password (must satisfy the active password policy) |

Response `200`:

```json
{
  "userId": "bb63362a-66e3-4ef7-b4cd-fa449a02bd47",
  "username": "clerk01",
  "generatedTemporaryPassword": "q7#Rk2mWx9Zp",
  "mustChangePassword": true,
  "resetAtUtc": "2026-10-17T14:02:11Z"
}
```

- `generatedTemporaryPassword` is `null` when the admin supplied the password. It is never stored and cannot be fetched again: show it in a one-time dialog with a "copy" button and tell the admin to give it to the user out of band.
- The response is sent with `Cache-Control: no-store`.
- Effects: the user must change the password at next login (they get the restricted token flow); **all of the user's sessions end immediately**; the account is unlocked; any pending forgot-password request is resolved.

Errors:

| Status | When |
|--------|------|
| 400 | `temporaryPassword` violates the policy (`errors.TemporaryPassword[]`), or the admin targets their own account (use change-password) |
| 403 | Caller lacks `Users_ResetPassword` |
| 404 | User not found |

UI: add a **"Reset password"** action on the user detail/list screen, visible when the logged-in user has permission `8004`.

### 1.2 Admin: forgot-password request queue

Users who forgot their password submit a request from the login page (section 1.3). Admins see the queue:

```
GET /api/v1/users/password-reset-requests          (permission 8004)
```

```json
[
  {
    "userId": "bb63362a-66e3-4ef7-b4cd-fa449a02bd47",
    "username": "clerk01",
    "fullNameArabic": "موظف اختبار",
    "fullNameEnglish": "Test Clerk",
    "role": 3,
    "roleName": "OfficeClerk",
    "phoneNumber": "0999000000",
    "email": "clerk01@example.org",
    "isActive": true,
    "isLockedOut": false,
    "requestedAtUtc": "2026-10-17T13:55:40Z",
    "note": "ext 214"
  }
]
```

- Oldest first. Resolve a row with **Reset password** (1.1); the row disappears automatically.
- Reject a row without changing the password (e.g. the request was not made by the account owner):

```
DELETE /api/v1/users/{id}/password-reset-request    → 204   (404 if no pending request)
```

- `note` is free text typed by an anonymous user: **render it as text, never as HTML**.
- The same data is on the user record: `UserDto.passwordResetRequestedAtUtc` and `UserDto.passwordResetRequestNote` (null when nothing is pending). Use it for a badge on the users list and a counter on the admin menu.

### 1.3 Login page: "Forgot password?"

```
POST /api/v1/auth/forgot-password            (anonymous)
{ "username": "clerk01", "note": "Call me on extension 214" }
```

- Response is **always** `202 { "message": "..." }` (localized by `Accept-Language`), whether or not the username exists. Show the message as-is; do not tell the user whether the account exists.
- `400` when `username` is empty or longer than 50 characters, or `note` exceeds 500.
- `429` after 3 requests per 10 minutes from the same IP — show "Too many requests, try again later".
- A request that is already pending is not replaced by later submissions.

### 1.4 Profile: change my own password

```
POST /api/v1/auth/change-password            (or /api/v2/auth/change-password)
{ "currentPassword": "...", "newPassword": "...", "confirmPassword": "..." }
```

- `userId` in the body is now **optional** (defaults to the caller). If you still send it, it must be the caller's id (else `403`).
- **New response** (v1 and v2 are identical):

```json
{
  "success": true,
  "message": "Password changed successfully. Other sessions were signed out; this session continues with the returned tokens.",
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "bmV3IHJlZnJlc2ggdG9rZW4...",
  "accessTokenExpiry": "2026-10-17T15:45:00Z",
  "refreshTokenExpiry": "2026-10-24T15:15:00Z",
  "serverTimeUtc": "2026-10-17T15:15:00Z",
  "accessTokenExpiresInSeconds": 1800,
  "sessionExpiryWarningSeconds": 60
}
```

- **Replace the stored access and refresh tokens with the returned ones.** The old tokens stop working immediately (all other sessions of the user are signed out). If you ignore the new fields, the next call returns `401 SESSION_REVOKED` and the user simply logs in again (the old documented behaviour).
- First-login flow (`mustChangePassword = true`): the returned tokens are **full** tokens, so you may skip the re-login step.
- Validation errors (`400`) come from the active password policy (min length, character classes) — show `errors.NewPassword[]`.

---

## 2. Session expiry warning and renewal

Client requirement: warn a few seconds before the session expires with an option to renew; when it has expired, show the login screen.

### 2.1 Timing fields

`POST /api/v1/auth/login`, `POST /api/v1/auth/refresh` and change-password responses now include:

| Field | Meaning |
|-------|---------|
| `serverTimeUtc` | Server clock when the response was produced |
| `accessTokenExpiresInSeconds` | Seconds until the access token expires, **by the server clock** |
| `sessionExpiryWarningSeconds` | Show the warning this many seconds before expiry (server setting, default 60) |

`accessTokenExpiry` is unchanged. Do **not** compute the remaining time from the browser clock (`accessTokenExpiry - Date.now()`): office machines are often minutes off, which caused the offline-server login loop. Use `accessTokenExpiresInSeconds` and a local monotonic timer.

To re-sync after the tab was asleep:

```
GET /api/v1/auth/session
→ { "userId": "...", "username": "clerk01", "accessTokenExpiry": "...", "serverTimeUtc": "...",
    "accessTokenExpiresInSeconds": 528, "sessionExpiryWarningSeconds": 60, "mustChangePassword": false }
```

It does not extend the session.

### 2.2 Recommended flow

```ts
function scheduleSession(r: { accessTokenExpiresInSeconds: number; sessionExpiryWarningSeconds: number }) {
  clearTimeout(warnTimer); clearTimeout(expireTimer);
  const warnIn = Math.max(0, r.accessTokenExpiresInSeconds - r.sessionExpiryWarningSeconds) * 1000;
  warnTimer = setTimeout(showExpiryDialog, warnIn);          // "Your session expires in N s — Continue?"
  expireTimer = setTimeout(goToLogin, r.accessTokenExpiresInSeconds * 1000);
}

async function onContinueClicked() {          // OK in the dialog
  const r = await refreshOnce();              // POST /api/v1/auth/refresh { refreshToken }
  saveTokens(r); scheduleSession(r);          // new refresh token every time (rotation)
}
```

- Call `scheduleSession` after login, after every refresh and after change-password.
- **Single-flight refresh:** refresh tokens are single-use. If two requests refresh in parallel, the second one fails. Keep one shared in-flight refresh promise.
- On `visibilitychange` → visible, call `GET /auth/session` and reschedule.

### 2.3 Telling 401s apart

Every 401 from the auth pipeline now carries `errorCode` and headers:

| `errorCode` | Header | Meaning | Client action |
|-------------|--------|---------|---------------|
| `TOKEN_EXPIRED` | `Token-Expired: true` | Access token expired | Refresh once, retry the request; if refresh fails → login |
| `SESSION_REVOKED` | `WWW-Authenticate: Bearer error="invalid_token"…` | Password changed/reset, role changed or account deactivated | Go to login (do **not** refresh) |
| `TOKEN_INVALID` | same | Malformed / wrong signature | Go to login |
| `AUTHENTICATION_REQUIRED` | `WWW-Authenticate: Bearer` | No token sent | Go to login |

```json
{ "status": 401, "title": "Unauthorized", "message": "Your session has expired. Please login again.", "errorCode": "TOKEN_EXPIRED" }
```

`Token-Expired` and `WWW-Authenticate` are exposed through CORS, so `response.headers.get('Token-Expired')` works in the browser. When you redirect to login, keep the current route and return to it after login.

---

## 3. Contact Point / Rights Claimant designations

Persons now carry two independent designations (a person can hold both):

| Designation | Arabic | Meaning | Source |
|-------------|--------|---------|--------|
| Contact Point | جهة الاتصال | The person interviewed, to be contacted in the future | The survey's contact person |
| Rights Claimant | صاحب حق | Has a rights claim on the property | Has an active person–property relation on the unit |

Both are derived by the server; there is nothing to enter. Show them as badges wherever persons are listed.

### 3.1 Case detail — "who to call"

`GET /api/v1/cases/{id}` and `GET /api/v1/cases/by-property-unit/{propertyUnitId}` add `persons` — everyone on the case's unit, **contact points first**:

```json
"persons": [
  {
    "personId": "dc980fb3-...",
    "firstNameArabic": "ياسر", "fatherNameArabic": "حسن", "familyNameArabic": "النعيمي", "motherNameArabic": "زينب",
    "fullNameArabic": "ياسر حسن النعيمي",
    "nationalId": "22222222222",
    "mobileNumber": "+963-956-290348",
    "phoneNumber": null,
    "householdId": "98cfb13d-...",
    "isContactPoint": true,
    "isRightsClaimant": true,
    "relationType": 1,
    "claimType": 1,
    "relationId": "3f30ade0-...",
    "relations": [
      { "relationId": "3f30ade0-...", "relationType": 1, "claimType": 1 },
      { "relationId": "cbe70e40-...", "relationType": 2, "claimType": 2 }
    ]
  }
]
```

- `relationType`/`claimType` are the person's primary relation (ownership wins); `relations` lists all of them. Both are `null`/empty for a contact point who has no claim.
- A unit surveyed several times can have several contact points (one per survey).

### 3.2 Persons

`PersonDto` adds `isRightsClaimant`. `isContactPerson` keeps its name; on person endpoints (`/api/v1/persons…`) it means "contact point of any survey".

### 3.3 Claim detail

`GET /api/v1/claims/{id}` adds `contactPoint` (same shape as a `persons` entry above, or `null`) and `primaryClaimantIsContactPoint`. Show "Contact: name — phone" on the claim screen.

---

## 4. Vocabulary management changes

### 4.1 System / non-system

| Vocabulary | Arabic | Before | Now |
|------------|--------|--------|-----|
| `evidence_type` | نوع الوثيقة الداعمة للادعاء | system | **non-system** |
| `building_type` | نوع البناء | system | **non-system** |
| `occupancy_type` | نوع الإشغال | system | **non-system** |
| `relation_type` | نوع العلاقة | system | **non-system** |
| `survey_source` | مصدر الاستطلاع | non-system | **system** |

`isSystemVocabulary` in `VocabularyDto` reflects this after the server restarts. Admins can now add values to the four non-system vocabularies with a **major** version:

```
POST /api/v1/Vocabularies/{currentVersionId}/versions
{ "versionType": "major", "changeLog": "Add co-occupant",
  "values": [ ...all current values..., { "code": 7, "labelArabic": "شريك في السكن", "labelEnglish": "Co-occupant", "displayOrder": 7, "isDeprecated": false } ] }
```

The same call on `survey_source` (now system) returns `400`. Enable/disable the "add value" button from `isSystemVocabulary`.

Behaviour notes to show in the vocabulary admin UI as help text:

- A new **relation type** is treated as an **occupancy** claim (case stays open, no ownership share). Only Owner and Heir produce ownership claims.
- A major version means tablets must sync vocabularies before their next package is accepted (existing behaviour of the compatibility check).

### 4.2 Renamed dictionaries

| Code | Old name | New name |
|------|----------|----------|
| `document_type` | نوع الوثيقة / Document Type | **نوع وثيقة إثبات الشخصية / Personal Identification Document Type** |
| `evidence_type` | نوع الدليل / Evidence Type | **نوع الوثيقة الداعمة للادعاء / Claim Supporting Document Type** |

Existing databases are renamed by migration (only where the name was still the default; admin-edited names are kept). Always display `displayNameArabic`/`displayNameEnglish` from the API — never hard-code dictionary titles. Codes and versions are unchanged.

### 4.3 Labels

Render value labels from the vocabulary by **code**. Some DTOs also carry English enum names (e.g. `buildingTypeName`); for admin-added codes those show the number, so prefer the vocabulary label.

---

## 5. Household "empty property" flag

Households imported from tablets can be flagged as **found empty** during the field survey:

```json
{ "id": "...", "householdSize": 0, "isEmptyProperty": true,
  "emptyPropertyJustification": "الوحدة مغلقة والسكان نازحون منذ 2016", ... }
```

Present on every `HouseholdDto` (survey households, `/api/v1/Households`, office survey detail). Read-only — only tablets set it. Show "عقار فارغ" with the justification on household/case views. In the import review screen the staged household's `displayInfo` reads `Empty property (عقار فارغ): <justification>`.

---

## 6. Import packages list — collector and buildings (PR #45)

`GET /api/v1/import/packages` items add:

```json
"collectorName": "أحمد محمد",
"buildings": [
  { "buildingNumber": "00001", "governorateName": "حلب", "districtName": "جبل سمعان",
    "subDistrictName": "مركز جبل سمعان", "communityName": "حلب", "neighborhoodName": "العقبة" }
]
```

`page`/`pageSize` in the response are now the values actually used (page size is capped at 100).

---

## 7. Permissions and audit log

New permissions (synced to role defaults on server start; users must log in again to see them in their token-derived UI):

| Code | Name | Arabic | Default roles |
|------|------|--------|---------------|
| 8004 | `Users_ResetPassword` | إعادة تعيين كلمة مرور مستخدم | Administrator |
| 9021 | `MapReferenceData_Write` | تعديل بيانات الخريطة | Administrator, DataManager, OfficeClerk |
| 9022 | `MapReferenceData_Delete` | حذف بيانات الخريطة | Administrator, DataManager |

PR #46 also gives OfficeClerk `Buildings_Create` (4001) and `Buildings_Update` (4002). Landmark/street create/update/delete previously accepted any logged-in user; they now require 9021/9022.

New audit action types (see `docs/fromdashbord/AuditActionType-reference.md`):

| `actionType` | Name | Arabic |
|--------------|------|--------|
| 47 | PasswordReset | إعادة تعيين كلمة المرور |
| 48 | PasswordResetRequested | طلب إعادة تعيين كلمة المرور |

---

## 8. Bug fix — changing a user's role

**Reported:** an administrator changed a user's role from DataManager to Administrator; the dashboard showed success, but the user still had DataManager permissions.

**Root cause (backend):** `PUT /api/v1/users/{id}` with a new `role` returned `200` and a DTO showing the new role, but the database save failed silently and was rolled back. The new permission rows were saved as UPDATEs of non-existent rows, and the error was swallowed. Nothing — neither the role nor the permissions — was stored. Reloading the user (`GET /api/v1/users/{id}`) showed the old role. The bug is fixed on the backend; **no dashboard change is required for the save itself.** Role changes made before this fix were never applied: redo them.

**Behaviour after the fix:**

```
PUT /api/v1/users/{id}
{ "userId": "{id}", "role": 6 }
→ 200 UserDto { "role": 6, ... }        (now really persisted)
```

| Effect | Detail |
|--------|--------|
| Role and role permissions | Saved together. Permissions only in the old role are revoked, permissions only in the new role are granted, shared ones stay |
| User's sessions | **End immediately** (`401 SESSION_REVOKED` on their next call). The role is carried inside the token, so the user must log in again to get the new role and menus |
| Failure | Now an error status (`500`/`409`) instead of a fake `200` |

Role codes: FieldCollector 1, FieldSupervisor 2, OfficeClerk 3, DataManager 4, Analyst 5, Administrator 6.

**Dashboard actions:**

1. After a successful role change, **re-fetch the user** (`GET /api/v1/users/{id}`) and render from that response, not from the form state. That way any future save problem is visible immediately.
2. Show a hint after saving: "The user will be signed out and must log in again for the new role to apply."
3. If the administrator changes **their own** role, their session ends as well: handle the next `401 SESSION_REVOKED` by going to the login page.
4. Do not derive admin menus from a role cached at login if the user may have been changed while logged in; the session is revoked anyway, so the next login carries the right role.

**Related fix — claims list failed with `400` ("An item with the same key has already been added")**

`GET /api/v1/Claims` failed for every request as soon as the data contained a person with more than one relation on the same unit (for example owner **and** occupant). This was not new in ProdV5: such data already existed. The list now returns `200`.

The response contract is unchanged. These fields now follow one documented rule:

| Field | Meaning |
|-------|---------|
| `sourceRelationId` | The relation the claim belongs to: the claimant's active relation on the unit whose claim type equals the claim's `claimType` (ownership claim → Owner/Heir relation, occupancy claim → other relation types). If several match, the latest one created up to the claim's creation |
| `evidenceIds` / `hasEvidence` | Evidence of that relation |

So a person who owns and occupies a unit has **two claims, each with its own `sourceRelationId` and evidence**. The same rule is used by `GET /api/v1/Claims/{id}`, `GET /api/v1/Claims/summaries` and `PUT /api/v1/Claims/{id}`: editing the occupancy claim updates the occupancy relation, never the owner relation. List, detail and summaries always agree.

Also fixed: `GET /api/v1/Claims?PropertyUnitId=...` returned only the **first** claim of the unit. It now returns every claim on the unit (paged as usual).

**Related fix:** the bulk import endpoints `POST /api/v1/administrative-divisions/import`, `POST /api/v1/Buildings/import-bulk` and `POST /api/v1/Neighborhoods/import-bulk` required a role named "Admin" that does not exist, so they returned `403` for everyone. They are now available to **Administrator**.

---

## 9. Change summary

| Area | Endpoint / field | Change |
|------|------------------|--------|
| Users | `POST /users/{id}/reset-password` | New |
| Users | `GET /users/password-reset-requests`, `DELETE /users/{id}/password-reset-request` | New |
| Users | `UserDto.passwordResetRequestedAtUtc`, `.passwordResetRequestNote` | New fields |
| Auth | `POST /auth/forgot-password` | New (anonymous, 3 / 10 min / IP) |
| Auth | `POST /auth/change-password` (v1, v2) | `userId` optional; response adds tokens + timing |
| Auth | login / refresh | Add `serverTimeUtc`, `accessTokenExpiresInSeconds`, `sessionExpiryWarningSeconds` |
| Auth | `GET /auth/session` | New |
| Auth | all 401s | Add `errorCode`, `Token-Expired`, `WWW-Authenticate` |
| Auth | sessions | Ended immediately by password change/reset and deactivation |
| Cases | `CaseDto.persons` | New |
| Claims | `ClaimDto.contactPoint`, `.primaryClaimantIsContactPoint` | New (detail only) |
| Persons | `PersonDto.isRightsClaimant` | New |
| Vocabularies | 4 → non-system, `survey_source` → system, 2 renamed | Data change |
| Households | `isEmptyProperty`, `emptyPropertyJustification` | New fields |
| Import | packages list `collectorName`, `buildings` | New fields (PR #45) |
| Permissions | 8004, 9021, 9022 | New |
| Users | `PUT /users/{id}` role change | **Bug fixed** — now persisted; user's sessions end |
| Import | admin bulk imports (divisions, buildings, neighborhoods) | **Bug fixed** — reachable by Administrator |
| Claims | `GET /Claims` (list) | **Bug fixed** — no more 400 when a person has several relations on a unit; `sourceRelationId`/`evidenceIds` = the relation matching the claim type |
| Claims | `GET /Claims?PropertyUnitId=` | **Bug fixed** — returns all claims of the unit (was only the first) |
