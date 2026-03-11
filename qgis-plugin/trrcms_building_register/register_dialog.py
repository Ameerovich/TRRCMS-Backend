"""
Building registration dialog with cascading admin hierarchy and polygon capture.
"""

from qgis.PyQt.QtWidgets import (
    QDialog, QVBoxLayout, QHBoxLayout, QLabel, QLineEdit,
    QPushButton, QComboBox, QTextEdit, QFormLayout, QGroupBox,
    QRadioButton, QButtonGroup, QMessageBox, QApplication
)
from qgis.PyQt.QtCore import Qt
from qgis.PyQt.QtGui import QColor
from qgis.core import (
    QgsCoordinateReferenceSystem, QgsCoordinateTransform,
    QgsProject, QgsGeometry, QgsWkbTypes, QgsPointXY
)
from qgis.gui import QgsMapToolEmitPoint, QgsRubberBand


class PolygonMapTool(QgsMapToolEmitPoint):
    """Map tool for drawing a polygon by clicking points. Right-click or double-click to finish."""

    def __init__(self, canvas, callback):
        super().__init__(canvas)
        self.canvas = canvas
        self.callback = callback
        self.points = []
        self.rubber_band = QgsRubberBand(canvas, QgsWkbTypes.PolygonGeometry)
        self.rubber_band.setColor(QColor(255, 0, 0, 100))
        self.rubber_band.setStrokeColor(QColor(255, 0, 0))
        self.rubber_band.setWidth(2)
        self.temp_rubber_band = QgsRubberBand(canvas, QgsWkbTypes.LineGeometry)
        self.temp_rubber_band.setColor(QColor(255, 0, 0, 150))
        self.temp_rubber_band.setWidth(1)

    def canvasPressEvent(self, event):
        if event.button() == Qt.RightButton:
            self._finish()
            return
        if event.button() == Qt.LeftButton:
            point = self.toMapCoordinates(event.pos())
            self.points.append(point)
            self._update_rubber_band()

    def canvasDoubleClickEvent(self, event):
        self._finish()

    def canvasMoveEvent(self, event):
        if self.points:
            point = self.toMapCoordinates(event.pos())
            self.temp_rubber_band.reset(QgsWkbTypes.LineGeometry)
            self.temp_rubber_band.addPoint(self.points[-1])
            self.temp_rubber_band.addPoint(point)

    def _update_rubber_band(self):
        self.rubber_band.reset(QgsWkbTypes.PolygonGeometry)
        for pt in self.points:
            self.rubber_band.addPoint(pt)

    def _finish(self):
        if len(self.points) >= 3:
            # Close the polygon
            self.points.append(self.points[0])
            geometry = QgsGeometry.fromPolygonXY([self.points])
            self.callback(geometry)
        else:
            self.callback(None)
        self.reset()

    def reset(self):
        self.points = []
        self.rubber_band.reset(QgsWkbTypes.PolygonGeometry)
        self.temp_rubber_band.reset(QgsWkbTypes.LineGeometry)

    def deactivate(self):
        self.reset()
        super().deactivate()


class RegisterBuildingDialog(QDialog):
    """Dialog for registering a building on the TRRCMS server."""

    def __init__(self, iface, api_client, parent=None):
        super().__init__(parent)
        self.iface = iface
        self.api_client = api_client
        self.captured_geometry = None
        self.map_tool = None
        self.previous_map_tool = None

        self.setWindowTitle("Register Building - TRRCMS")
        self.setMinimumWidth(500)
        self.setMinimumHeight(600)
        self.setWindowFlags(self.windowFlags() & ~Qt.WindowContextHelpButtonHint)
        self._setup_ui()
        self._load_governorates()

    def _setup_ui(self):
        layout = QVBoxLayout(self)

        # ==================== Admin Hierarchy ====================
        admin_group = QGroupBox("Administrative Location")
        admin_layout = QFormLayout()

        self.governorate_combo = QComboBox()
        self.governorate_combo.currentIndexChanged.connect(self._on_governorate_changed)
        admin_layout.addRow("Governorate:", self.governorate_combo)

        self.district_combo = QComboBox()
        self.district_combo.currentIndexChanged.connect(self._on_district_changed)
        admin_layout.addRow("District:", self.district_combo)

        self.sub_district_combo = QComboBox()
        self.sub_district_combo.currentIndexChanged.connect(self._on_sub_district_changed)
        admin_layout.addRow("Sub-District:", self.sub_district_combo)

        self.community_combo = QComboBox()
        self.community_combo.currentIndexChanged.connect(self._on_community_changed)
        admin_layout.addRow("Community:", self.community_combo)

        self.neighborhood_combo = QComboBox()
        self.neighborhood_combo.currentIndexChanged.connect(self._update_building_id_preview)
        admin_layout.addRow("Neighborhood:", self.neighborhood_combo)

        admin_group.setLayout(admin_layout)
        layout.addWidget(admin_group)

        # ==================== Building Info ====================
        building_group = QGroupBox("Building Information")
        building_layout = QFormLayout()

        self.building_number_input = QLineEdit()
        self.building_number_input.setPlaceholderText("5-digit number (e.g. 00001)")
        self.building_number_input.setMaxLength(5)
        self.building_number_input.textChanged.connect(self._update_building_id_preview)
        building_layout.addRow("Building Number:", self.building_number_input)

        self.building_id_preview = QLabel("")
        self.building_id_preview.setStyleSheet("font-weight: bold; color: #333; font-size: 13px;")
        building_layout.addRow("Building ID:", self.building_id_preview)

        self.notes_input = QTextEdit()
        self.notes_input.setMaximumHeight(60)
        self.notes_input.setPlaceholderText("Optional notes...")
        building_layout.addRow("Notes:", self.notes_input)

        building_group.setLayout(building_layout)
        layout.addWidget(building_group)

        # ==================== Geometry Source ====================
        geom_group = QGroupBox("Building Geometry")
        geom_layout = QVBoxLayout()

        self.geom_btn_group = QButtonGroup(self)
        self.radio_selected = QRadioButton("Use selected feature from active layer")
        self.radio_draw = QRadioButton("Draw polygon on map")
        self.radio_selected.setChecked(True)
        self.geom_btn_group.addButton(self.radio_selected, 1)
        self.geom_btn_group.addButton(self.radio_draw, 2)
        geom_layout.addWidget(self.radio_selected)
        geom_layout.addWidget(self.radio_draw)

        btn_row = QHBoxLayout()
        self.capture_btn = QPushButton("Capture Geometry")
        self.capture_btn.clicked.connect(self._on_capture_geometry)
        btn_row.addWidget(self.capture_btn)

        self.clear_geom_btn = QPushButton("Clear")
        self.clear_geom_btn.clicked.connect(self._on_clear_geometry)
        btn_row.addWidget(self.clear_geom_btn)
        geom_layout.addLayout(btn_row)

        self.geom_status_label = QLabel("No geometry captured")
        self.geom_status_label.setStyleSheet("color: gray;")
        self.geom_status_label.setWordWrap(True)
        geom_layout.addWidget(self.geom_status_label)

        geom_group.setLayout(geom_layout)
        layout.addWidget(geom_group)

        # ==================== Action Buttons ====================
        self.status_label = QLabel("")
        self.status_label.setAlignment(Qt.AlignCenter)
        self.status_label.setWordWrap(True)
        layout.addWidget(self.status_label)

        action_layout = QHBoxLayout()
        self.register_btn = QPushButton("Register Building")
        self.register_btn.setStyleSheet("font-weight: bold; padding: 8px;")
        self.register_btn.clicked.connect(self._on_register)
        action_layout.addWidget(self.register_btn)

        close_btn = QPushButton("Close")
        close_btn.clicked.connect(self.reject)
        action_layout.addWidget(close_btn)

        layout.addLayout(action_layout)

    # ==================== Cascading Admin Hierarchy ====================

    def _load_governorates(self):
        try:
            self.governorate_combo.blockSignals(True)
            self.governorate_combo.clear()
            self.governorate_combo.addItem("-- Select Governorate --", None)
            items = self.api_client.get_governorates()
            if isinstance(items, list):
                for item in items:
                    label = f"{item.get('nameArabic', '')} ({item.get('code', '')})"
                    self.governorate_combo.addItem(label, item)
            self.governorate_combo.blockSignals(False)
        except Exception as e:
            self.governorate_combo.blockSignals(False)
            self._show_error(f"Failed to load governorates: {e}")

    def _on_governorate_changed(self, index):
        self.district_combo.clear()
        self.sub_district_combo.clear()
        self.community_combo.clear()
        self.neighborhood_combo.clear()
        self._update_building_id_preview()

        data = self.governorate_combo.currentData()
        if not data:
            return
        try:
            self.district_combo.addItem("-- Select District --", None)
            items = self.api_client.get_districts(data["code"])
            if isinstance(items, list):
                for item in items:
                    label = f"{item.get('nameArabic', '')} ({item.get('code', '')})"
                    self.district_combo.addItem(label, item)
        except Exception as e:
            self._show_error(f"Failed to load districts: {e}")

    def _on_district_changed(self, index):
        self.sub_district_combo.clear()
        self.community_combo.clear()
        self.neighborhood_combo.clear()
        self._update_building_id_preview()

        gov_data = self.governorate_combo.currentData()
        dist_data = self.district_combo.currentData()
        if not gov_data or not dist_data:
            return
        try:
            self.sub_district_combo.addItem("-- Select Sub-District --", None)
            items = self.api_client.get_sub_districts(gov_data["code"], dist_data["code"])
            if isinstance(items, list):
                for item in items:
                    label = f"{item.get('nameArabic', '')} ({item.get('code', '')})"
                    self.sub_district_combo.addItem(label, item)
        except Exception as e:
            self._show_error(f"Failed to load sub-districts: {e}")

    def _on_sub_district_changed(self, index):
        self.community_combo.clear()
        self.neighborhood_combo.clear()
        self._update_building_id_preview()

        gov_data = self.governorate_combo.currentData()
        dist_data = self.district_combo.currentData()
        sd_data = self.sub_district_combo.currentData()
        if not gov_data or not dist_data or not sd_data:
            return
        try:
            self.community_combo.addItem("-- Select Community --", None)
            items = self.api_client.get_communities(gov_data["code"], dist_data["code"], sd_data["code"])
            if isinstance(items, list):
                for item in items:
                    label = f"{item.get('nameArabic', '')} ({item.get('code', '')})"
                    self.community_combo.addItem(label, item)
        except Exception as e:
            self._show_error(f"Failed to load communities: {e}")

    def _on_community_changed(self, index):
        self.neighborhood_combo.clear()
        self._update_building_id_preview()

        gov_data = self.governorate_combo.currentData()
        dist_data = self.district_combo.currentData()
        sd_data = self.sub_district_combo.currentData()
        comm_data = self.community_combo.currentData()
        if not gov_data or not dist_data or not sd_data or not comm_data:
            return
        try:
            self.neighborhood_combo.addItem("-- Select Neighborhood --", None)
            items = self.api_client.get_neighborhoods(
                gov_data["code"], dist_data["code"], sd_data["code"], comm_data["code"]
            )
            if isinstance(items, list):
                for item in items:
                    label = f"{item.get('nameArabic', '')} ({item.get('code', '')})"
                    self.neighborhood_combo.addItem(label, item)
        except Exception as e:
            self._show_error(f"Failed to load neighborhoods: {e}")

    def _update_building_id_preview(self):
        gov = self.governorate_combo.currentData()
        dist = self.district_combo.currentData()
        sd = self.sub_district_combo.currentData()
        comm = self.community_combo.currentData()
        neigh = self.neighborhood_combo.currentData()
        bnum = self.building_number_input.text().strip()

        parts = []
        parts.append(gov["code"] if gov else "GG")
        parts.append(dist["code"] if dist else "DD")
        parts.append(sd["code"] if sd else "SS")
        parts.append(comm["code"] if comm else "CCC")
        parts.append((neigh.get("code") or neigh.get("neighborhoodCode") or "NNN") if neigh else "NNN")
        parts.append(bnum.zfill(5) if bnum else "BBBBB")

        self.building_id_preview.setText("-".join(parts))

    # ==================== Geometry Capture ====================

    def _on_capture_geometry(self):
        if self.radio_selected.isChecked():
            self._capture_from_selection()
        else:
            self._capture_by_drawing()

    def _capture_from_selection(self):
        layer = self.iface.activeLayer()
        if not layer:
            self._show_error("No active layer. Please select a vector layer with polygon features.")
            return

        selected = layer.selectedFeatures()
        if not selected:
            self._show_error("No features selected. Please select a polygon feature on the active layer.")
            return

        feature = selected[0]
        geom = feature.geometry()

        if geom.isNull() or geom.isEmpty():
            self._show_error("Selected feature has no geometry.")
            return

        if geom.type() != QgsWkbTypes.PolygonGeometry:
            self._show_error("Selected feature is not a polygon. Please select a polygon feature.")
            return

        # Reproject to EPSG:4326
        geom_4326 = self._reproject_to_4326(geom, layer.crs())
        self.captured_geometry = geom_4326
        self.geom_status_label.setText(f"Geometry captured from selected feature (reprojected to WGS84)")
        self.geom_status_label.setStyleSheet("color: green;")

    def _capture_by_drawing(self):
        self.hide()
        self.previous_map_tool = self.iface.mapCanvas().mapTool()
        self.map_tool = PolygonMapTool(self.iface.mapCanvas(), self._on_polygon_drawn)
        self.iface.mapCanvas().setMapTool(self.map_tool)
        self.iface.messageBar().pushInfo(
            "TRRCMS",
            "Click to draw building polygon. Right-click or double-click to finish."
        )

    def _on_polygon_drawn(self, geometry):
        # Restore previous map tool
        if self.previous_map_tool:
            self.iface.mapCanvas().setMapTool(self.previous_map_tool)

        if geometry is None or geometry.isNull():
            self.geom_status_label.setText("Drawing cancelled (need at least 3 points)")
            self.geom_status_label.setStyleSheet("color: red;")
        else:
            canvas_crs = self.iface.mapCanvas().mapSettings().destinationCrs()
            geom_4326 = self._reproject_to_4326(geometry, canvas_crs)
            self.captured_geometry = geom_4326
            self.geom_status_label.setText("Polygon drawn and captured (reprojected to WGS84)")
            self.geom_status_label.setStyleSheet("color: green;")

        self.show()
        self.raise_()
        self.activateWindow()

    def _on_clear_geometry(self):
        self.captured_geometry = None
        self.geom_status_label.setText("No geometry captured")
        self.geom_status_label.setStyleSheet("color: gray;")

    def _reproject_to_4326(self, geometry, source_crs):
        """Reproject geometry to EPSG:4326 if needed."""
        target_crs = QgsCoordinateReferenceSystem("EPSG:4326")
        if source_crs != target_crs:
            transform = QgsCoordinateTransform(source_crs, target_crs, QgsProject.instance())
            geom = QgsGeometry(geometry)
            geom.transform(transform)
            return geom
        return QgsGeometry(geometry)

    # ==================== Registration ====================

    def _on_register(self):
        # Validate admin hierarchy
        gov = self.governorate_combo.currentData()
        dist = self.district_combo.currentData()
        sd = self.sub_district_combo.currentData()
        comm = self.community_combo.currentData()
        neigh = self.neighborhood_combo.currentData()

        if not all([gov, dist, sd, comm, neigh]):
            self._show_error("Please select all administrative levels (Governorate through Neighborhood).")
            return

        # Validate building number
        bnum = self.building_number_input.text().strip()
        if not bnum or not bnum.isdigit() or len(bnum) > 5:
            self._show_error("Building number must be a numeric value (up to 5 digits).")
            return

        # Validate geometry
        if self.captured_geometry is None:
            self._show_error("Please capture building geometry first (select a feature or draw a polygon).")
            return

        wkt = self.captured_geometry.asWkt()

        # Build request payload
        payload = {
            "governorateCode": gov["code"],
            "districtCode": dist["code"],
            "subDistrictCode": sd["code"],
            "communityCode": comm["code"],
            "neighborhoodCode": neigh.get("code") or neigh.get("neighborhoodCode"),
            "buildingNumber": bnum.zfill(5),
            "buildingGeometryWkt": wkt,
        }

        notes = self.notes_input.toPlainText().strip()
        if notes:
            payload["notes"] = notes

        # Send request
        self.register_btn.setEnabled(False)
        self.status_label.setText("Registering building...")
        self.status_label.setStyleSheet("color: gray;")
        QApplication.processEvents()

        try:
            result = self.api_client.register_building(payload)

            building_id = ""
            if isinstance(result, dict):
                building_id = result.get("buildingId", result.get("id", ""))

            self.status_label.setText(f"Building registered successfully! ID: {building_id}")
            self.status_label.setStyleSheet("color: green; font-weight: bold;")

            QMessageBox.information(
                self,
                "Success",
                f"Building registered successfully!\n\nBuilding ID: {building_id}"
            )

            # Reset form for next registration
            self._reset_form()

        except Exception as e:
            self.status_label.setText(f"Registration failed: {e}")
            self.status_label.setStyleSheet("color: red;")
        finally:
            self.register_btn.setEnabled(True)

    def _reset_form(self):
        """Reset form fields for next registration (keep admin hierarchy)."""
        self.building_number_input.clear()
        self.notes_input.clear()
        self.captured_geometry = None
        self.geom_status_label.setText("No geometry captured")
        self.geom_status_label.setStyleSheet("color: gray;")
        self.status_label.setText("")

    def _show_error(self, message):
        self.status_label.setText(message)
        self.status_label.setStyleSheet("color: red;")

    def closeEvent(self, event):
        # Clean up map tool if active
        if self.map_tool:
            self.map_tool.reset()
        super().closeEvent(event)
