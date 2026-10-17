# QGIS Plugin Integration Guide — ProdV5

**Date:** October 17, 2026
**Backend branch:** `ProdV5`
**Audience:** Developer of the QGIS plugin `qgis-plugin/trrcms_building_register` (`api_client.py`, `plugin.py`, `login_dialog.py`)

**Compatibility:** the endpoints the plugin calls (`auth/login`, `auth/refresh`, administrative divisions, neighborhoods, `buildings/register`, landmarks, streets) keep their routes and payloads. Login/refresh responses only **gain** fields. Two behaviour changes need plugin work: session handling (section 1) and permissions (section 2).

---

## Table of Contents

1. [Session expiry — warning, renew, re-login](#1-session-expiry--warning-renew-re-login)
2. [Permissions for map data (PR #46)](#2-permissions-for-map-data-pr-46)
3. [Password changes and role changes](#3-password-changes-and-role-changes)
4. [Vocabularies](#4-vocabularies)
5. [Test checklist](#5-test-checklist)

---

## 1. Session expiry — warning, renew, re-login

Client requirement: in the Desktop, web and **QGIS** parts, sessions expire silently. Show a warning a few seconds before expiry with an option to renew (OK); when the session has expired, show the login dialog.

### 1.1 What the server now provides

`POST /api/v1/auth/login` and `POST /api/v1/auth/refresh` responses add:

```json
{
  "accessToken": "...",
  "refreshToken": "...",
  "accessTokenExpiry": "2026-10-17T15:45:00Z",
  "serverTimeUtc": "2026-10-17T15:15:00Z",
  "accessTokenExpiresInSeconds": 1800,
  "sessionExpiryWarningSeconds": 60
}
```

- Use `accessTokenExpiresInSeconds` (server clock). Do not compute `accessTokenExpiry - datetime.utcnow()`: field laptops are often minutes off.
- `GET /api/v1/auth/session` returns the same timing for the current token (call it after the laptop wakes from sleep). It does not extend the session.

Every 401 now has a machine-readable `errorCode`:

| `errorCode` | Header | Meaning | Plugin action |
|-------------|--------|---------|---------------|
| `TOKEN_EXPIRED` | `Token-Expired: true` | Access token expired | Refresh once and retry |
| `SESSION_REVOKED` | `WWW-Authenticate: Bearer error="invalid_token"…` | Password changed/reset, role changed or account deactivated | **Do not refresh** (it will fail) — clear tokens, show login |
| `TOKEN_INVALID` | same | Malformed token | Clear tokens, show login |
| `AUTHENTICATION_REQUIRED` | `WWW-Authenticate: Bearer` | No token sent | Show login |

### 1.2 Current plugin behaviour (problem)

`_request` refreshes on **every** 401. When the refresh fails it raises `"Session expired. Please log in again."` but keeps `access_token` / `refresh_token_value`, so `_ensure_authenticated()` still thinks the user is logged in and never reopens the login dialog — the user is stuck until QGIS restarts. This is the "silent expiry" the client reported.

### 1.3 `api_client.py` changes

```python
class SessionExpiredError(Exception):
    """Raised when the user must log in again."""


class ApiClient:
    def __init__(self, base_url="http://localhost:8080"):
        ...
        self.expires_in = None          # seconds, server clock
        self.warning_seconds = 60
        self.on_session_changed = None  # callback(expires_in, warning_seconds), set by plugin.py

    def _store_session(self, response):
        self.access_token = response.get("accessToken")
        self.refresh_token_value = response.get("refreshToken")      # single-use: always replace
        self.expires_in = response.get("accessTokenExpiresInSeconds")
        self.warning_seconds = response.get("sessionExpiryWarningSeconds") or 60
        if self.on_session_changed and self.expires_in is not None:
            self.on_session_changed(self.expires_in, self.warning_seconds)

    def login(self, username, password):
        response = self._request("POST", "/api/v1/auth/login",
                                 json_data={"username": username, "password": password}, auth=False)
        self._store_session(response)
        return response

    def refresh_token(self):
        if not self.refresh_token_value:
            raise SessionExpiredError("No refresh token")
        response = self._request("POST", "/api/v1/auth/refresh",
                                 json_data={"refreshToken": self.refresh_token_value}, auth=False)
        self._store_session(response)
        return response

    def clear_session(self):
        self.access_token = None
        self.refresh_token_value = None
        self.expires_in = None
```

In `_request`, branch on the 401 reason instead of always refreshing:

```python
        except urllib.error.HTTPError as e:
            raw = e.read().decode("utf-8") if e.fp else ""
            try:
                err = json.loads(raw) if raw else {}
            except ValueError:
                err = {}

            if e.code == 401 and auth:
                if err.get("errorCode") == "TOKEN_EXPIRED" and self.refresh_token_value:
                    try:
                        self.refresh_token()
                    except Exception:
                        self.clear_session()
                        raise SessionExpiredError("Session expired. Please log in again.")
                    return self._request(method, path, json_data, params, auth)   # retry once
                self.clear_session()                                                # revoked / invalid
                raise SessionExpiredError(err.get("message") or "Please log in again.")

            if e.code == 403:
                raise Exception(err.get("message") or "Not permitted for your role.")
            msg = err.get("detail") or err.get("title") or err.get("message") or raw or e.reason
            raise Exception(f"API Error ({e.code}): {msg}")
```

(Guard the retry so it happens only once, e.g. with a `_retried` flag argument.)

### 1.4 `plugin.py` — warning timer and re-login

```python
from qgis.PyQt.QtCore import QTimer
from qgis.PyQt.QtWidgets import QMessageBox

class TrrcmsPlugin:
    def __init__(self, iface):
        ...
        self._warn_timer = QTimer(); self._warn_timer.setSingleShot(True)
        self._expire_timer = QTimer(); self._expire_timer.setSingleShot(True)
        self._warn_timer.timeout.connect(self._on_session_warning)
        self._expire_timer.timeout.connect(self._on_session_expired)

    def _attach_client(self, client):
        self.api_client = client
        client.on_session_changed = self._schedule_session

    def _schedule_session(self, expires_in, warning_seconds):
        self._warn_timer.start(max(0, expires_in - warning_seconds) * 1000)
        self._expire_timer.start(expires_in * 1000)

    def _on_session_warning(self):
        answer = QMessageBox.question(
            self.iface.mainWindow(), "TRRCMS",
            "ستنتهي الجلسة قريباً. هل تريد تمديدها؟\nYour session is about to expire. Renew it?",
            QMessageBox.Ok | QMessageBox.Cancel)
        if answer == QMessageBox.Ok:
            try:
                self.api_client.refresh_token()          # reschedules via on_session_changed
            except Exception:
                self._on_session_expired()

    def _on_session_expired(self):
        self._warn_timer.stop(); self._expire_timer.stop()
        self.api_client.clear_session()
        self._ensure_authenticated()                     # opens LoginDialog

    def _ensure_authenticated(self):
        if not self.api_client.is_authenticated():       # must return False after clear_session()
            dlg = LoginDialog(self.iface.mainWindow(), self.api_client)
            if dlg.exec_() != LoginDialog.Accepted:
                return False
            self._attach_client(dlg.get_api_client())
        return True
```

Wrap every action (`_run_building`, `_run_landmark`, `_run_street`) so that a `SessionExpiredError` calls `_on_session_expired()` and then retries the action after a successful login, instead of only showing the error text. Stop both timers in `unload()`.

Make sure `is_authenticated()` returns `False` when `access_token` is `None` (it currently checks the token's presence).

---

## 2. Permissions for map data (PR #46)

Until now, any logged-in user could create, edit and delete landmarks and streets. Now:

| Plugin call | Endpoint | Required permission | Roles with it by default |
|-------------|----------|---------------------|--------------------------|
| `register_building` | `POST /api/v1/buildings/register` | `Buildings_Create` (4001) | Administrator, DataManager, **OfficeClerk (new)** |
| `register_landmark`, `update_landmark` | `POST /landmarks/register`, `PUT /landmarks/{id}` | `MapReferenceData_Write` (9021) | Administrator, DataManager, OfficeClerk |
| `delete_landmark` | `DELETE /landmarks/{id}` | `MapReferenceData_Delete` (9022) | Administrator, DataManager |
| `register_street`, `update_street` | `POST /streets/register`, `PUT /streets/{id}` | `MapReferenceData_Write` (9021) | Administrator, DataManager, OfficeClerk |
| `delete_street` | `DELETE /streets/{id}` | `MapReferenceData_Delete` (9022) | Administrator, DataManager |

FieldSupervisor, Analyst and FieldCollector accounts now get **403** on these writes. Show a clear message ("غير مسموح لدورك" / "Not permitted for your role") instead of a generic API error, and hide the delete buttons for OfficeClerk (the login response has `role` / `roleName`).

Role defaults are re-applied on every server start, so an existing OfficeClerk receives the new permissions without any admin action.

---

## 3. Password changes and role changes

- When an administrator **resets** a user's password, **changes the user's role**, or deactivates the account, the user's sessions end immediately: the plugin's next call returns `401 SESSION_REVOKED`. With section 1.3 this opens the login dialog.
- After an admin password reset, login returns `mustChangePassword: true` with a restricted token; every other call returns `403 PasswordChangeRequired`. The plugin has no change-password UI: show "Please change your password in the TRRCMS desktop/dashboard first".
- A role change takes effect at the next login (the role is inside the token).

---

## 4. Vocabularies

`building_type` is now an **extensible** vocabulary (administrators can add values). If the building form has a type dropdown, load it from `GET /api/v1/Vocabularies` (vocabulary `building_type`, non-deprecated values) instead of a hard-coded list, and send the selected `code`.

---

## 5. Test checklist

- [ ] Login → warning dialog appears `sessionExpiryWarningSeconds` before expiry (temporarily set the server's session timeout to 5 minutes in security settings to test)
- [ ] OK in the dialog renews the session; the next warning is scheduled from the new response
- [ ] Ignoring the dialog → at expiry the login dialog opens; after login the action can be repeated
- [ ] Admin resets the user's password while the plugin is open → next action opens the login dialog (not a stuck "Session expired" error)
- [ ] OfficeClerk: create/edit landmark and street works; delete shows "not permitted"
- [ ] Analyst / FieldSupervisor: landmark/street writes show "not permitted"
- [ ] OfficeClerk can register buildings
