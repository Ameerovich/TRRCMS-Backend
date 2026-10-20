# Dashboard Integration Guide — Desktop v2 issues (19-10-2026)

**Date:** October 20, 2026
**Backend branch:** `feat/desktop-v2-empty-property-import-packages` (on top of `main`)
**Audience:** Dashboard (web) developers

**Compatibility:** nothing to change on the dashboard. These are backend changes made for the desktop. Two of them make data you already read richer or more common. No field was removed, renamed or retyped.

---

## 1. Households: `isEmptyProperty` can now come from the desktop

The ProdV5 dashboard guide (section 5) described `isEmptyProperty` / `emptyPropertyJustification` as *"Read-only — only tablets set it"*. That is no longer true. Office users can now mark a survey household as an empty unit (المقسم فارغ) from the desktop.

What this means for display:
- `isEmptyProperty: true` households now also come from office surveys, with `householdSize: 0` and all counts `0`.
- `emptyPropertyJustification` may be `null` on these, because the desktop does not ask for one. Show "عقار فارغ" without a justification in that case. Don't render an empty quote or a "null" label.
- Unticking on the desktop clears both the flag and the justification.

Nothing changed on `/api/v1/Households` (the generic endpoints), so the dashboard's own forms behave as before.

---

## 2. Import packages list: location names filled, new `assignedByName`

`GET /api/v1/import/packages`, `items[].buildings[]`:

- **Location names** (`governorateName` … `neighborhoodName`) are now filled for real tablet packages. They were always `null` before, because tablets don't write names into the package. The server now resolves them from the package's administrative codes. A level is still `null` when its code is unknown to the server.
- **New `assignedByName`** (`string?`): the supervisor who assigned that building to the package's collector (Arabic full name first). It is optional to display. `null` when no assignment matches.

If you already show a location for each package, it will start showing real values with no code change.
