# Mobile (Tablet) Integration Guide — ProdV5

**Date:** October 17, 2026
**Backend branch:** `ProdV5`
**Audience:** Mobile / tablet app developers (field data collection, `.uhc` export, LAN sync)
**Spec:** the column tables in `dev/TRRCMS_Mobile_Sync_Integration_Guide.md` (changelog v2.2) are updated to match this guide.

**Compatibility:** all `.uhc` changes are **new optional columns**. Packages produced by the current app (schema 1.9 / app 1.0.19) still upload, stage, validate and commit unchanged, and in addition benefit from the server-side fixes in sections 3 and 4. To get the new behaviour, the app must send the new columns.

Recommended: set `manifest.schema_version = "1.10.0"` when the app writes the new columns (informational — the server reads columns by name and does not reject older versions).

---

## Table of Contents

1. [V2 Bug 1 — `persons.date_of_birth`](#1-v2-bug-1--personsdate_of_birth)
2. [Empty property — `households.is_empty_property`](#2-empty-property--householdsis_empty_property)
3. [V2 Bug 2 — relation type must agree with the claim type](#3-v2-bug-2--relation-type-must-agree-with-the-claim-type)
4. [V2 Bug 3 — description and notes](#4-v2-bug-3--description-and-notes)
5. [Contact Point / Rights Claimant](#5-contact-point--rights-claimant)
6. [Vocabularies](#6-vocabularies)
7. [Authentication and sessions on the tablet](#7-authentication-and-sessions-on-the-tablet)
8. [Updated DDL and test package](#8-updated-ddl-and-test-package)
9. [Checklist](#9-checklist)

---

## 1. V2 Bug 1 — `persons.date_of_birth`

**Bug:** day and month of birth were different on the desktop after import. The package only carried `year_of_birth`, so the server stored 1 January.

**New column** on `persons`:

| Column | Type | Required | Format |
|--------|------|----------|--------|
| `date_of_birth` | TEXT | No | `yyyy-MM-dd` (e.g. `1983-07-19`) — a calendar date, **no time and no time zone** |

- Keep sending `year_of_birth` as well (older servers read only that).
- The server uses `date_of_birth` when present and valid. An ISO date-time (`1983-07-19T00:00:00Z`) is accepted too, but write the plain date: converting a local midnight to UTC (`1983-07-18T21:00:00Z` in Syria) is exactly what shifts days.
- Unparseable value → the server logs a warning and falls back to `year_of_birth`.
- Only year known → leave `date_of_birth` NULL.

```dart
final dob = person.dateOfBirth;                          // DateTime picked in the form
final text = dob == null ? null
  : '${dob.year.toString().padLeft(4, '0')}-${dob.month.toString().padLeft(2, '0')}-${dob.day.toString().padLeft(2, '0')}';
// INSERT INTO persons (..., year_of_birth, date_of_birth) VALUES (..., dob?.year, text)
```

---

## 2. Empty property — `households.is_empty_property`

New, **tablet-only** feature: during a field survey the collector can record that the property unit was found **empty** (عقار فارغ), with a justification. The desktop shows it read-only in the household info.

| Column | Type | Required | Description |
|--------|------|----------|-------------|
| `is_empty_property` | INTEGER | No | `1` = unit found empty, `0` / NULL = not empty (default) |
| `empty_property_justification` | TEXT | No | Why the unit is empty (max 2000 characters) |

**App UI:** a checkbox "العقار فارغ" in the occupancy / household step. When checked, make "مبرر كون العقار فارغاً" **required** and allow zero members.

**Import validation rules:**

| Package content | Result |
|-----------------|--------|
| `is_empty_property = 1`, justification given, `household_size = 0` | Valid |
| `is_empty_property = 1`, justification empty | **Warning** "Property unit reported empty without a justification" (still imported — enforce it in the app) |
| `is_empty_property = 1`, `household_size > 0` | Warning |
| `is_empty_property = 0`, justification given | Warning (justification kept) |
| justification > 2000 characters | **Error** (household rejected) |
| `household_size = 0` and not empty | **Error** "HouseholdSize must be > 0" (unchanged) |

Write one `households` row for the empty unit (it carries the flag) with `household_size = 0` and no persons. The survey, its contact person and the unit are exported as usual.

---

## 3. V2 Bug 2 — relation type must agree with the claim type

**Bug:** the claim type chosen on the tablet (ملكية / إشغال) did not survive import. App 1.0.19 writes the choice to `claims.claim_type` but sends `person_property_relations.relation_type = 0`, which is not a valid code. Such relations were rejected by import validation.

**Server fix (already active for current packages):** for each relation the server looks up the claim with the same `primary_claimant_id` + `property_unit_id`. When `relation_type` is missing, invalid or contradicts the claim type, it is replaced:

| `claims.claim_type` | `relation_type` stored |
|---------------------|------------------------|
| 1 (Ownership — ملكية) | Owner (1) — Heir (5) is kept if sent. When the type is replaced, the relation takes `claims.ownership_share` if it has none. |
| 2 (Occupancy — إشغال) | Occupant (2) — Tenant (3) / Guest (4) / Other (99) / admin-added codes are kept if sent. When the type is replaced, `ownership_share` is cleared. |
| anything else (0, unknown) | `relation_type` left as sent; the claim is reported invalid by validation |

**App change:** still send a valid, consistent `relation_type`. If the UI only asks for the claim type, write `relation_type = 1` for ownership and `2` for occupancy. `claims.claim_type` must be `1` or `2`.

---

## 4. V2 Bug 3 — description and notes

**Bug:** "الوصف" and "ملاحظات" in the desktop claim info were empty after import.

| Claim form field | Write to | Desktop shows it as |
|------------------|----------|---------------------|
| الوصف (description) | `claims.claim_description` (as today) — optionally also `person_property_relations.contract_details` | relation `contractDetails` |
| ملاحظاتك (notes) | **`person_property_relations.notes`** | relation `notes` |

- The server now copies `claims.claim_description` into the relation's description when `contract_details` is absent or blank (truncated to 2000 characters for the relation; the claim keeps the full text).
- **App 1.0.19 leaves `person_property_relations.notes` empty** — the notes typed in the claim form never reach the server. Write them to that column. (Survey notes stay in `surveys.notes`; household notes in `households.notes`; document notes in `evidences.notes`.)

Optional new column on `person_property_relations`:

| Column | Type | Required | Description |
|--------|------|----------|-------------|
| `contract_details` | TEXT | No | Claim description, max 2000 characters. Wins over `claims.claim_description` when not blank. |

---

## 5. Contact Point / Rights Claimant

The system now shows two person designations everywhere:

| Designation | Arabic | How the server determines it |
|-------------|--------|------------------------------|
| Contact Point | جهة الاتصال | `surveys.contact_person_id` |
| Rights Claimant | صاحب حق | The person has a row in `person_property_relations` (a claim) for the unit |

**No new columns.** Keep doing what the spec already requires:

- exactly one contact person per survey: `persons.is_contact_person = 1` and `surveys.contact_person_id` = that person's id;
- one `person_property_relations` row (+ claim) per rights claimant.

The server treats `surveys.contact_person_id` as authoritative and sets the person's contact flag from it even if `is_contact_person` was forgotten. A person can be both (the interviewee who is also an owner).

---

## 6. Vocabularies

| Vocabulary | Change | App impact |
|------------|--------|-----------|
| `relation_type`, `occupancy_type`, `evidence_type`, `building_type` | Now **extensible**: administrators can add codes | Never hard-code these lists or labels — always use the synced vocabulary payload. Accept codes you have not seen before. |
| `survey_source` | Now **system** (fixed codes) | None |
| `document_type` | Renamed "نوع وثيقة إثبات الشخصية" (Personal Identification Document Type) | Use `displayNameArabic` from the payload if you show dictionary titles |
| `evidence_type` | Renamed "نوع الوثيقة الداعمة للادعاء" (Claim Supporting Document Type) | Same |

- Adding codes creates a **major** vocabulary version. As today, a package built with an older major version is rejected by the compatibility check — sync vocabularies before collecting new data.
- The renames did **not** change vocabulary versions. If the app caches dictionary titles by version, refresh the titles on every sync.
- A new relation type added by an admin is treated as an **occupancy** claim. Send `claims.claim_type = 2` with it.

---

## 7. Authentication and sessions on the tablet

The login / refresh contracts are unchanged; responses gain fields:

| Field | Meaning |
|-------|---------|
| `serverTimeUtc` | Server clock at response time |
| `accessTokenExpiresInSeconds` | Seconds left on the access token, by the server clock |
| `sessionExpiryWarningSeconds` | Server's suggested warning lead time (default 60) |

Use `accessTokenExpiresInSeconds` instead of comparing `accessTokenExpiry` with the tablet clock (tablets are often skewed, especially offline).

**401 responses now carry `errorCode`:**

| `errorCode` | Action |
|-------------|--------|
| `TOKEN_EXPIRED` (also header `Token-Expired: true`) | Refresh once (`POST /api/v1/auth/refresh`), retry |
| `SESSION_REVOKED` | Password was changed/reset, the role was changed, or the account was deactivated → go to login. **Do not delete unsynced survey data.** |
| `TOKEN_INVALID`, `AUTHENTICATION_REQUIRED` | Login |

Other changes:

- Changing or resetting a password now ends existing sessions immediately. A collector whose password the admin resets is asked to log in (and then to set a new password: `mustChangePassword = true`).
- `POST /api/v1/auth/change-password` returns a fresh token pair (`accessToken`, `refreshToken`, timing fields). Store it and continue; ignoring it just means logging in again.
- Optional: a "Forgot password?" link can call `POST /api/v1/auth/forgot-password { "username": "...", "note": "..." }` (anonymous, always `202`, max 3 requests / 10 minutes). The admin then resets the password from the dashboard.
- Refresh tokens are single-use; never refresh twice concurrently, and always store the new refresh token.

---

## 8. Updated DDL and test package

Add to the quick-start schema (all nullable / defaulted):

```sql
ALTER TABLE persons                   ADD COLUMN date_of_birth TEXT;                 -- 'yyyy-MM-dd'
ALTER TABLE households                ADD COLUMN is_empty_property INTEGER DEFAULT 0;
ALTER TABLE households                ADD COLUMN empty_property_justification TEXT;
ALTER TABLE person_property_relations ADD COLUMN contract_details TEXT;              -- optional
```

Example rows:

```sql
INSERT INTO persons (id, family_name_arabic, first_name_arabic, father_name_arabic, national_id,
                     year_of_birth, date_of_birth, is_contact_person, household_id)
VALUES ('40a001be-...', 'الحلبي', 'أحمد', 'وليد', '60383227561', 1983, '1983-07-19', 1, 'daf6b19e-...');

-- unit found empty
INSERT INTO households (id, property_unit_id, household_size, is_empty_property, empty_property_justification)
VALUES ('8eab9828-...', 'cc4c5540-...', 0, 1, 'الوحدة مغلقة والسكان نازحون منذ 2016');

-- ownership claim: relation_type consistent with claim_type, notes on the relation
INSERT INTO person_property_relations (id, person_id, property_unit_id, relation_type, ownership_share, has_evidence, notes)
VALUES ('1cc4ff63-...', '40a001be-...', 'c27eda93-...', 1, 1200, 1, 'ملاحظات المستخدم');
INSERT INTO claims (id, property_unit_id, claim_type, claim_source, primary_claimant_id, originating_survey_id, ownership_share, claim_description)
VALUES ('313f5d2e-...', 'c27eda93-...', 1, 1, '40a001be-...', '1af65aa4-...', 1200, 'وصف الحالة');
```

The new columns are included in the content checksum like any other column (algorithm unchanged — columns are sorted by name).

Verified on the server with: an app 1.0.19 package (relation_type = 0 → staged as a valid Owner relation), cloned 1.9 packages with and without the new columns, empty-property households with and without justification, `claim_type = 0`, and descriptions longer than 2000 characters.

---

## 9. Checklist

- [ ] `persons.date_of_birth` written as `yyyy-MM-dd` (plus `year_of_birth`)
- [ ] Empty-property checkbox + required justification; household row with `is_empty_property = 1`, `household_size = 0`
- [ ] `person_property_relations.relation_type` valid and consistent with `claims.claim_type` (1 ↔ Owner/Heir, 2 ↔ others)
- [ ] `claims.claim_type` is 1 or 2
- [ ] Claim-form notes in `person_property_relations.notes`; description in `claims.claim_description` (optionally `contract_details`)
- [ ] One contact person per survey (`is_contact_person` + `contact_person_id`)
- [ ] No hard-coded `relation_type` / `occupancy_type` / `evidence_type` / `building_type` lists
- [ ] 401 handling by `errorCode`; keep unsynced data on `SESSION_REVOKED`
- [ ] `manifest.schema_version = "1.10.0"`
