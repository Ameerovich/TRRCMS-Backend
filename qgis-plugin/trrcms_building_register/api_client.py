"""
HTTP client for the TRRCMS API.
Uses urllib.request (stdlib) to avoid external dependency issues in QGIS Python.
"""

import json
import urllib.request
import urllib.error
import urllib.parse


class TRRCMSApiClient:
    """Client for TRRCMS REST API with JWT authentication."""

    def __init__(self, base_url="http://localhost:8080"):
        self.base_url = base_url.rstrip("/")
        self.access_token = None
        self.refresh_token_value = None
        self.user_info = None

    def login(self, username, password):
        """Authenticate and store tokens. Returns user info dict."""
        data = {"username": username, "password": password}
        response = self._request("POST", "/api/v1/auth/login", json_data=data, auth=False)
        self.access_token = response.get("accessToken")
        self.refresh_token_value = response.get("refreshToken")
        self.user_info = response
        return response

    def refresh_token(self):
        """Refresh the access token using the stored refresh token."""
        if not self.refresh_token_value:
            raise Exception("No refresh token available. Please log in again.")
        data = {"refreshToken": self.refresh_token_value}
        response = self._request("POST", "/api/v1/auth/refresh", json_data=data, auth=False)
        self.access_token = response.get("accessToken")
        self.refresh_token_value = response.get("refreshToken")
        return response

    def is_authenticated(self):
        """Check if the client has an active access token."""
        return self.access_token is not None

    # ==================== Administrative Hierarchy ====================

    def get_governorates(self):
        """Get list of governorates."""
        return self._request("GET", "/api/v1/administrative-divisions/governorates")

    def get_districts(self, governorate_code):
        """Get districts for a governorate."""
        params = {"governorateCode": governorate_code}
        return self._request("GET", "/api/v1/administrative-divisions/districts", params=params)

    def get_sub_districts(self, governorate_code, district_code):
        """Get sub-districts for a district."""
        params = {
            "governorateCode": governorate_code,
            "districtCode": district_code,
        }
        return self._request("GET", "/api/v1/administrative-divisions/sub-districts", params=params)

    def get_communities(self, governorate_code, district_code, sub_district_code):
        """Get communities for a sub-district."""
        params = {
            "governorateCode": governorate_code,
            "districtCode": district_code,
            "subDistrictCode": sub_district_code,
        }
        return self._request("GET", "/api/v1/administrative-divisions/communities", params=params)

    def get_neighborhoods(self, governorate_code, district_code, sub_district_code, community_code):
        """Get neighborhoods for a community."""
        params = {
            "governorateCode": governorate_code,
            "districtCode": district_code,
            "subDistrictCode": sub_district_code,
            "communityCode": community_code,
        }
        return self._request("GET", "/api/v1/neighborhoods", params=params)

    # ==================== Building Registration ====================

    def register_building(self, data):
        """Register a new building. Returns the created BuildingDto."""
        return self._request("POST", "/api/v1/buildings/register", json_data=data)

    # ==================== Landmark Registration ====================

    def register_landmark(self, data):
        """Register a new landmark. Returns the created LandmarkDto."""
        return self._request("POST", "/api/v1/landmarks/register", json_data=data)

    def update_landmark(self, landmark_id, data):
        """Update an existing landmark. Returns the updated LandmarkDto."""
        return self._request("PUT", f"/api/v1/landmarks/{landmark_id}", json_data=data)

    def delete_landmark(self, landmark_id):
        """Delete a landmark (soft delete)."""
        return self._request("DELETE", f"/api/v1/landmarks/{landmark_id}")

    # ==================== Street Registration ====================

    def register_street(self, data):
        """Register a new street. Returns the created StreetDto."""
        return self._request("POST", "/api/v1/streets/register", json_data=data)

    def update_street(self, street_id, data):
        """Update an existing street. Returns the updated StreetDto."""
        return self._request("PUT", f"/api/v1/streets/{street_id}", json_data=data)

    def delete_street(self, street_id):
        """Delete a street (soft delete)."""
        return self._request("DELETE", f"/api/v1/streets/{street_id}")

    # ==================== Internal ====================

    def _request(self, method, path, json_data=None, params=None, auth=True):
        """
        Make an HTTP request. Adds Authorization header if auth=True.
        Auto-refreshes token on 401 and retries once.
        """
        url = self.base_url + path
        if params:
            query = urllib.parse.urlencode(params)
            url = f"{url}?{query}"

        body = None
        if json_data is not None:
            body = json.dumps(json_data).encode("utf-8")

        headers = {"Content-Type": "application/json"}
        if auth and self.access_token:
            headers["Authorization"] = f"Bearer {self.access_token}"

        req = urllib.request.Request(url, data=body, headers=headers, method=method)

        try:
            with urllib.request.urlopen(req, timeout=30) as resp:
                resp_body = resp.read().decode("utf-8")
                if resp_body:
                    return json.loads(resp_body)
                return None
        except urllib.error.HTTPError as e:
            # Auto-refresh on 401 and retry once
            if e.code == 401 and auth and self.refresh_token_value:
                try:
                    self.refresh_token()
                    # Retry with new token
                    headers["Authorization"] = f"Bearer {self.access_token}"
                    req = urllib.request.Request(url, data=body, headers=headers, method=method)
                    with urllib.request.urlopen(req, timeout=30) as resp:
                        resp_body = resp.read().decode("utf-8")
                        if resp_body:
                            return json.loads(resp_body)
                        return None
                except Exception:
                    raise Exception("Session expired. Please log in again.")

            # Parse error response body
            error_body = ""
            try:
                error_body = e.read().decode("utf-8")
                error_json = json.loads(error_body)
                if isinstance(error_json, dict):
                    msg = error_json.get("detail") or error_json.get("title") or error_json.get("message") or str(error_json)
                    raise Exception(f"API Error ({e.code}): {msg}")
            except (json.JSONDecodeError, ValueError):
                pass
            raise Exception(f"API Error ({e.code}): {error_body or e.reason}")
        except urllib.error.URLError as e:
            raise Exception(f"Connection error: {e.reason}")
