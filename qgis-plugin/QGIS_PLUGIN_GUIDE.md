# TRRCMS Building Register — QGIS Plugin Quick Guide

## Installation

1. Open **QGIS Desktop** (3.28+)
2. Go to **Plugins → Manage and Install Plugins → Install from ZIP**
3. Browse to `trrcms_building_register.zip` and click **Install Plugin**
4. A green building icon appears in your toolbar

## Step 1: Login

1. Click the **TRRCMS Register Building** toolbar button
2. Enter:
   - **Server URL:** `http://localhost:8080` (default — change if your server is elsewhere)
   - **Username:** your TRRCMS username (e.g. `admin`)
   - **Password:** your password (e.g. `Admin@123`)
3. Click **Login** — you'll see "Login successful" and the registration dialog opens
4. Your session persists — you won't need to log in again until you close QGIS or the token expires

## Step 2: Select Administrative Location

Fill in the 5 cascading dropdowns in order — each one loads after the previous is selected:

1. **Governorate** (e.g. حلب)
2. **District** (e.g. جبل سمعان)
3. **Sub-District**
4. **Community**
5. **Neighborhood**

The **Building ID** preview at the bottom updates as you select (e.g. `01-01-01-003-002-00001`).

## Step 3: Enter Building Number

- Type a number up to 5 digits (e.g. `1` or `00042`)
- It will be zero-padded automatically to 5 digits

## Step 4: Capture Building Geometry

You have two options:

**Option A — Use an existing polygon:**
1. Select **"Use selected feature from active layer"** (default)
2. In the QGIS map canvas, click on a polygon feature to select it
3. Click **Capture Geometry**
4. Status shows "Geometry captured from selected feature"

**Option B — Draw a new polygon:**
1. Select **"Draw polygon on map"**
2. Click **Capture Geometry** — the dialog hides and the map tool activates
3. **Left-click** to place vertices of the building outline
4. **Right-click** or **double-click** to finish the polygon
5. The dialog reappears with "Polygon drawn and captured"

> The plugin automatically reprojects geometry to WGS84 (EPSG:4326) regardless of your project's CRS.

## Step 5: Register

1. Optionally add **Notes**
2. Click **Register Building**
3. On success: a message shows the new Building ID
4. The form resets (keeping admin hierarchy) so you can register the next building immediately

## Troubleshooting

| Issue | Solution |
|-------|----------|
| "Connection error" on login | Check the server URL and make sure the TRRCMS Docker container is running |
| "Session expired" | Close and reopen the plugin — it will prompt login again |
| Dropdowns not loading | Check your internet/network connection to the server |
| "Selected feature is not a polygon" | Make sure your active layer has polygon geometry and a feature is selected |
| Registration fails with 403 | Your account needs the **DataManager** or **Administrator** role |
