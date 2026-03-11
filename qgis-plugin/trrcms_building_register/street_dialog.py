"""
Street registration dialog with line capture (draw on map or selected feature).
"""

from qgis.PyQt.QtWidgets import (
    QDialog, QVBoxLayout, QHBoxLayout, QLabel, QLineEdit,
    QPushButton, QFormLayout, QGroupBox,
    QRadioButton, QButtonGroup, QMessageBox, QApplication
)
from qgis.PyQt.QtCore import Qt
from qgis.PyQt.QtGui import QColor
from qgis.core import (
    QgsCoordinateReferenceSystem, QgsCoordinateTransform,
    QgsProject, QgsGeometry, QgsWkbTypes, QgsPointXY
)
from qgis.gui import QgsMapToolEmitPoint, QgsRubberBand


class LineMapTool(QgsMapToolEmitPoint):
    """Map tool for drawing a line by clicking points. Right-click or double-click to finish."""

    def __init__(self, canvas, callback):
        super().__init__(canvas)
        self.canvas = canvas
        self.callback = callback
        self.points = []
        self.rubber_band = QgsRubberBand(canvas, QgsWkbTypes.LineGeometry)
        self.rubber_band.setColor(QColor(0, 100, 255, 200))
        self.rubber_band.setStrokeColor(QColor(0, 100, 255))
        self.rubber_band.setWidth(3)
        self.temp_rubber_band = QgsRubberBand(canvas, QgsWkbTypes.LineGeometry)
        self.temp_rubber_band.setColor(QColor(0, 100, 255, 100))
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
        self.rubber_band.reset(QgsWkbTypes.LineGeometry)
        for pt in self.points:
            self.rubber_band.addPoint(pt)

    def _finish(self):
        if len(self.points) >= 2:
            geometry = QgsGeometry.fromPolylineXY(self.points)
            self.callback(geometry)
        else:
            self.callback(None)
        self.reset()

    def reset(self):
        self.points = []
        self.rubber_band.reset(QgsWkbTypes.LineGeometry)
        self.temp_rubber_band.reset(QgsWkbTypes.LineGeometry)

    def deactivate(self):
        self.reset()
        super().deactivate()


class RegisterStreetDialog(QDialog):
    """Dialog for registering a street on the TRRCMS server."""

    def __init__(self, iface, api_client, parent=None):
        super().__init__(parent)
        self.iface = iface
        self.api_client = api_client
        self.captured_geometry = None
        self.map_tool = None
        self.previous_map_tool = None

        self.setWindowTitle("Register Street - TRRCMS")
        self.setMinimumWidth(450)
        self.setMinimumHeight(380)
        self.setWindowFlags(self.windowFlags() & ~Qt.WindowContextHelpButtonHint)
        self._setup_ui()

    def _setup_ui(self):
        layout = QVBoxLayout(self)

        # ==================== Street Info ====================
        info_group = QGroupBox("Street Information")
        info_layout = QFormLayout()

        self.identifier_input = QLineEdit()
        self.identifier_input.setPlaceholderText("Sequential number (e.g. 1, 2, 3...)")
        info_layout.addRow("Identifier:", self.identifier_input)

        self.name_input = QLineEdit()
        self.name_input.setPlaceholderText("اسم الشارع (e.g. شارع النصر)")
        info_layout.addRow("Name:", self.name_input)

        info_group.setLayout(info_layout)
        layout.addWidget(info_group)

        # ==================== Geometry Source ====================
        geom_group = QGroupBox("Street Geometry (Line)")
        geom_layout = QVBoxLayout()

        self.geom_btn_group = QButtonGroup(self)
        self.radio_selected = QRadioButton("Use selected line feature from active layer")
        self.radio_draw = QRadioButton("Draw line on map")
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
        self.register_btn = QPushButton("Register Street")
        self.register_btn.setStyleSheet("font-weight: bold; padding: 8px;")
        self.register_btn.clicked.connect(self._on_register)
        action_layout.addWidget(self.register_btn)

        close_btn = QPushButton("Close")
        close_btn.clicked.connect(self.reject)
        action_layout.addWidget(close_btn)

        layout.addLayout(action_layout)

    # ==================== Geometry Capture ====================

    def _on_capture_geometry(self):
        if self.radio_selected.isChecked():
            self._capture_from_selection()
        else:
            self._capture_by_drawing()

    def _capture_from_selection(self):
        layer = self.iface.activeLayer()
        if not layer:
            self._show_error("No active layer. Please select a vector layer with line features.")
            return

        selected = layer.selectedFeatures()
        if not selected:
            self._show_error("No features selected. Please select a line feature on the active layer.")
            return

        feature = selected[0]
        geom = feature.geometry()

        if geom.isNull() or geom.isEmpty():
            self._show_error("Selected feature has no geometry.")
            return

        if geom.type() != QgsWkbTypes.LineGeometry:
            self._show_error("Selected feature is not a line. Please select a line feature.")
            return

        geom_4326 = self._reproject_to_4326(geom, layer.crs())
        self.captured_geometry = geom_4326
        self.geom_status_label.setText("Line captured from selected feature (reprojected to WGS84)")
        self.geom_status_label.setStyleSheet("color: green;")

    def _capture_by_drawing(self):
        self.hide()
        self.previous_map_tool = self.iface.mapCanvas().mapTool()
        self.map_tool = LineMapTool(self.iface.mapCanvas(), self._on_line_drawn)
        self.iface.mapCanvas().setMapTool(self.map_tool)
        self.iface.messageBar().pushInfo(
            "TRRCMS",
            "Click to draw street line. Right-click or double-click to finish."
        )

    def _on_line_drawn(self, geometry):
        if self.previous_map_tool:
            self.iface.mapCanvas().setMapTool(self.previous_map_tool)

        if geometry is None or geometry.isNull():
            self.geom_status_label.setText("Drawing cancelled (need at least 2 points)")
            self.geom_status_label.setStyleSheet("color: red;")
        else:
            canvas_crs = self.iface.mapCanvas().mapSettings().destinationCrs()
            geom_4326 = self._reproject_to_4326(geometry, canvas_crs)
            self.captured_geometry = geom_4326
            self.geom_status_label.setText("Line drawn and captured (reprojected to WGS84)")
            self.geom_status_label.setStyleSheet("color: green;")

        self.show()
        self.raise_()
        self.activateWindow()

    def _on_clear_geometry(self):
        self.captured_geometry = None
        self.geom_status_label.setText("No geometry captured")
        self.geom_status_label.setStyleSheet("color: gray;")

    def _reproject_to_4326(self, geometry, source_crs):
        target_crs = QgsCoordinateReferenceSystem("EPSG:4326")
        if source_crs != target_crs:
            transform = QgsCoordinateTransform(source_crs, target_crs, QgsProject.instance())
            geom = QgsGeometry(geometry)
            geom.transform(transform)
            return geom
        return QgsGeometry(geometry)

    # ==================== Registration ====================

    def _on_register(self):
        # Validate identifier
        identifier_text = self.identifier_input.text().strip()
        if not identifier_text or not identifier_text.isdigit():
            self._show_error("Identifier must be a positive integer.")
            return

        # Validate name
        name = self.name_input.text().strip()
        if not name:
            self._show_error("Street name is required.")
            return

        # Validate geometry
        if self.captured_geometry is None:
            self._show_error("Please capture the street geometry first.")
            return

        wkt = self.captured_geometry.asWkt()

        payload = {
            "identifier": int(identifier_text),
            "name": name,
            "geometryWkt": wkt,
        }

        self.register_btn.setEnabled(False)
        self.status_label.setText("Registering street...")
        self.status_label.setStyleSheet("color: gray;")
        QApplication.processEvents()

        try:
            result = self.api_client.register_street(payload)

            result_name = ""
            if isinstance(result, dict):
                result_name = result.get("name", result.get("id", ""))

            self.status_label.setText(f"Street registered successfully! ({result_name})")
            self.status_label.setStyleSheet("color: green; font-weight: bold;")

            QMessageBox.information(
                self,
                "Success",
                f"Street registered successfully!\n\nName: {result_name}"
            )

            self._reset_form()

        except Exception as e:
            self.status_label.setText(f"Registration failed: {e}")
            self.status_label.setStyleSheet("color: red;")
        finally:
            self.register_btn.setEnabled(True)

    def _reset_form(self):
        self.identifier_input.clear()
        self.name_input.clear()
        self.captured_geometry = None
        self.geom_status_label.setText("No geometry captured")
        self.geom_status_label.setStyleSheet("color: gray;")
        self.status_label.setText("")

    def _show_error(self, message):
        self.status_label.setText(message)
        self.status_label.setStyleSheet("color: red;")

    def closeEvent(self, event):
        if self.map_tool:
            self.map_tool.reset()
        super().closeEvent(event)
