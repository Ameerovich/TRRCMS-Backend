# Property Units Filtering & Grouping - Test Guide

## Overview

This guide demonstrates how to test the enhanced `GetAllPropertyUnits` API endpoint with filtering and grouping capabilities.

**API Endpoint:** `GET /api/v1/PropertyUnits`

**Swagger UI:** `https://localhost:7204/swagger` (or `http://localhost:8080/swagger` in Docker)

---

## Test Scenarios

### Scenario 1: Get All Units Grouped by Building (Default)

**Request:**
```http
GET /api/v1/PropertyUnits
```

**Expected Response:**
```json
{
  "groupedByBuilding": [
    {
      "buildingId": "building-guid-1",
      "buildingNumber": "01-02-03-001-002-00001",
      "unitCount": 3,
      "propertyUnits": [
        {
          "id": "unit-guid-1",
          "buildingId": "building-guid-1",
          "buildingNumber": "01-02-03-001-002-00001",
          "unitIdentifier": "1A",
          "floorNumber": 1,
          "unitType": 1,
          "status": 1,
          "areaSquareMeters": 85.5,
          "numberOfRooms": 3
        }
      ]
    }
  ],
  "totalUnits": 15,
  "totalBuildings": 5
}
```

**Validation:**
- ✅ Results are grouped by building
- ✅ Each building shows `unitCount`
- ✅ Units within building ordered by `floorNumber`, then `unitIdentifier`
- ✅ `totalUnits` and `totalBuildings` are accurate

---

### Scenario 2: Filter by Building ID

**Request:**
```http
GET /api/v1/PropertyUnits?buildingId={your-building-guid}
```

**Expected Behavior:**
- Only units from the specified building are returned
- Response still grouped (single building in array)
- `totalBuildings` = 1

**Validation:**
- ✅ All units have matching `buildingId`
- ✅ No units from other buildings

---

### Scenario 3: Filter by Unit Type (Apartments Only)

**Request:**
```http
GET /api/v1/PropertyUnits?unitType=1
```

**Unit Type Values:**
- 1 = Apartment
- 2 = Shop
- 3 = Office
- 4 = Warehouse
- 5 = Other

**Expected Behavior:**
- Only apartments (unitType=1) are returned
- Grouped by buildings that contain apartments
- Buildings with no apartments are excluded

**Validation:**
- ✅ All returned units have `unitType: 1`
- ✅ `totalUnits` matches count of apartments

---

### Scenario 4: Filter by Status (Vacant Units Only)

**Request:**
```http
GET /api/v1/PropertyUnits?status=2
```

**Status Values:**
- 1 = Occupied
- 2 = Vacant
- 3 = Damaged
- 4 = UnderRenovation
- 5 = Uninhabitable
- 6 = Locked
- 99 = Unknown

**Expected Behavior:**
- Only vacant units returned
- Grouped by buildings containing vacant units

**Validation:**
- ✅ All returned units have `status: 2`

---

### Scenario 5: Combined Filters (Vacant Apartments in Specific Building)

**Request:**
```http
GET /api/v1/PropertyUnits?buildingId={guid}&unitType=1&status=2
```

**Expected Behavior:**
- Only vacant apartments from the specified building
- Filters are AND-combined (all must match)

**Validation:**
- ✅ All units match ALL filter criteria
- ✅ `buildingId` matches
- ✅ `unitType` = 1 (Apartment)
- ✅ `status` = 2 (Vacant)

---

### Scenario 6: Ungrouped Flat List

**Request:**
```http
GET /api/v1/PropertyUnits?groupByBuilding=false
```

**Expected Response:**
```json
{
  "groupedByBuilding": [
    {
      "buildingId": "00000000-0000-0000-0000-000000000000",
      "buildingNumber": "All Units",
      "unitCount": 15,
      "propertyUnits": [
        /* all units in flat list */
      ]
    }
  ],
  "totalUnits": 15,
  "totalBuildings": 5
}
```

**Validation:**
- ✅ Single "building" entry with `buildingId` = `00000000-0000-0000-0000-000000000000`
- ✅ `buildingNumber` = "All Units"
- ✅ `unitCount` equals `totalUnits`
- ✅ `totalBuildings` shows actual count of buildings

---

### Scenario 7: Filter Validation (Invalid Values)

**Request 1: Invalid Unit Type**
```http
GET /api/v1/PropertyUnits?unitType=999
```

**Expected Response:**
```json
HTTP 400 Bad Request
{
  "errors": {
    "UnitType": ["Invalid property unit type value"]
  }
}
```

**Request 2: Invalid Status**
```http
GET /api/v1/PropertyUnits?status=888
```

**Expected Response:**
```json
HTTP 400 Bad Request
{
  "errors": {
    "Status": ["Invalid property unit status value"]
  }
}
```

**Validation:**
- ✅ FluentValidation catches invalid enum values
- ✅ Returns 400 Bad Request
- ✅ Error messages are clear

---

## Swagger UI Testing Steps

### Step 1: Start the API

**Option A: Docker**
```bash
cd "/e/Work/UN/Project/My Solution/TRRCMS"
docker compose up
```
Access: http://localhost:8080/swagger

**Option B: Local**
```bash
cd "/e/Work/UN/Project/My Solution/TRRCMS"
dotnet run --project src/TRRCMS.WebAPI
```
Access: https://localhost:7204/swagger

---

### Step 2: Authenticate

1. Click **Authorize** button (top right)
2. Login via `POST /api/Auth/login`:
   ```json
   {
     "username": "admin",
     "password": "Admin@123"
   }
   ```
3. Copy the `token` from response
4. Paste token in **Authorize** dialog
5. Click **Authorize**

---

### Step 3: Test GetAllPropertyUnits Endpoint

1. Navigate to **PropertyUnits** section
2. Find `GET /api/v1/PropertyUnits`
3. Click **Try it out**
4. Test each scenario:

#### Test 1: Default Grouped
- Leave all parameters empty
- Click **Execute**
- Verify grouping structure

#### Test 2: Filter by Type
- Set `unitType` = 1
- Click **Execute**
- Verify only apartments returned

#### Test 3: Filter by Status
- Set `status` = 2
- Click **Execute**
- Verify only vacant units returned

#### Test 4: Combined Filters
- Set `buildingId` = (copy a GUID from previous response)
- Set `unitType` = 1
- Set `status` = 1
- Click **Execute**
- Verify all filters applied

#### Test 5: Ungrouped
- Set `groupByBuilding` = false
- Click **Execute**
- Verify flat list structure

#### Test 6: Invalid Values
- Set `unitType` = 999
- Click **Execute**
- Verify 400 Bad Request with validation error

---

## Expected Performance

**Query Performance:**
- Single database query with filters applied at DB level
- No N+1 queries for building data (batch loading)
- Typical response time: < 200ms for 100 units

**Database Query:**
```sql
SELECT * FROM "PropertyUnits"
WHERE "IsDeleted" = false
  AND ("BuildingId" = @buildingId OR @buildingId IS NULL)
  AND ("UnitType" = @unitType OR @unitType IS NULL)
  AND ("Status" = @status OR @status IS NULL)
ORDER BY "BuildingId", "FloorNumber", "UnitIdentifier"
```

---

## Common Issues & Troubleshooting

### Issue 1: 401 Unauthorized
**Solution:** Ensure you've authenticated with a valid JWT token

### Issue 2: 403 Forbidden
**Solution:** User needs `PropertyUnits_View` (6000) permission

### Issue 3: Empty Results
**Solution:**
- Check if database has property units
- Try without filters first to verify data exists
- Verify filter values match existing data

### Issue 4: Validation Errors
**Solution:**
- Check enum values are valid (see scenario 7)
- Ensure BuildingId is valid GUID format
- Review Swagger UI for allowed values

---

## Success Criteria

✅ **All scenarios return expected responses**
✅ **Filtering works correctly (AND-combined)**
✅ **Grouping produces correct hierarchy**
✅ **Statistics (totalUnits, totalBuildings, unitCount) are accurate**
✅ **Validation rejects invalid enum values**
✅ **Performance is acceptable (< 200ms)**
✅ **No N+1 query issues**
✅ **Swagger documentation is clear and helpful**

---

## Test Checklist

- [ ] Scenario 1: Default grouped response works
- [ ] Scenario 2: Filter by buildingId works
- [ ] Scenario 3: Filter by unitType works
- [ ] Scenario 4: Filter by status works
- [ ] Scenario 5: Combined filters work (AND logic)
- [ ] Scenario 6: Ungrouped flat list works
- [ ] Scenario 7: Validation rejects invalid values
- [ ] Response structure matches documentation
- [ ] Statistics are accurate
- [ ] Performance is acceptable
- [ ] Swagger UI documentation is clear

---

**Testing Complete!** 🎉

For questions or issues, check:
- README.md - Project overview
- SETUP_GUIDE.md - Local development setup
- Swagger UI - Interactive API documentation
