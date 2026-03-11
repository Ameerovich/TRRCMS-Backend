"""
Landmark registration dialog with point capture (click on map or selected feature).
"""

from qgis.PyQt.QtWidgets import (
    QDialog, QVBoxLayout, QHBoxLayout, QLabel, QLineEdit,
    QPushButton, QComboBox, QFormLayout, QGroupBox,
    QRadioButton, QButtonGroup, QMessageBox, QApplication
)
from qgis.PyQt.QtCore import Qt
from qgis.core import (
    QgsCoordinateReferenceSystem, QgsCoordinateTransform,
    QgsProject, QgsGeometry, QgsWkbTypes
)
from qgis.gui import QgsMapToolEmitPoint


LANDMARK_TYPES = [
    (1, "Police Station", "مركز شرطة"),
    (2, "Mosque", "مسجد"),
    (3, "Public Building", "مبنى عام"),
    (4, "Shop", "محل تجاري"),
    (5, "School", "مدرسة"),
    (6, "Clinic", "عيادة"),
    (7, "Water Tank", "خزان مياه"),
    (8, "Fuel Station", "محطة وقود"),
]


class RegisterLandmarkDialog(QDialog):
    """Dialog for registering a landmark on the TRRCMS server."""

    def __init__(self, iface, api_client, parent=None):
        super().__init__(parent)
        self.iface = iface
        self.api_client = api_client
        self.captured_geometry = None
        self.previous_map_tool = None

        self.setWindowTitle("Register Landmark - TRRCMS")
        self.setMinimumWidth(450)
        self.setMinimumHeight(400)
        self.setWindowFlags(self.windowFlags() & ~Qt.WindowContextHelpButtonHint)
        self._setup_ui()

    def _setup_ui(self):
        layout = QVBoxLayout(self)

        # ==================== Landmark Info ====================
        info_group = QGroupBox("Landmark Information")
        info_layout = QFormLayout()

        self.identifier_input = QLineEdit()
        self.identifier_input.setPlaceholderText("Sequential number (e.g. 1, 2, 3...)")
        info_layout.addRow("Identifier:", self.identifier_input)

        self.name_input = QLineEdit()
        self.name_input.setPlaceholderText("اسم المعلم (e.g. جامع الأموي)")
        info_layout.addRow("Name:", self.name_input)

        self.type_combo = QComboBox()
        for type_id, name_en, name_ar in LANDMARK_TYPES:
            self.type_combo.addItem(f"{name_ar} - {name_en}", type_id)
        info_layout.addRow("Type:", self.type_combo)

        info_group.setLayout(info_layout)
        layout.addWidget(info_group)

        # ==================== Geometry Source ====================
        geom_group = QGroupBox("Landmark Location (Point)")
        geom_layout = QVBoxLayout()

        self.geom_btn_group = QButtonGroup(self)
        self.radio_selected = QRadioButton("Use selected point feature from active layer")
        self.radio_click = QRadioButton("Click on map to place landmark")
        self.radio_selected.setChecked(True)
        self.geom_btn_group.addButton(self.radio_selected, 1)
        self.geom_btn_group.addButton(self.radio_click, 2)
        geom_layout.addWidget(self.radio_selected)
        geom_layout.addWidget(self.radio_click)

        btn_row = QHBoxLayout()
        self.capture_btn = QPushButton("Capture Location")
        self.capture_btn.clicked.connect(self._on_capture_geometry)
        btn_row.addWidget(self.capture_btn)

        self.clear_geom_btn = QPushButton("Clear")
        self.clear_geom_btn.clicked.connect(self._on_clear_geometry)
        btn_row.addWidget(self.clear_geom_btn)
        geom_layout.addLayout(btn_row)

        self.geom_status_label = QLabel("No location captured")
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
        self.register_btn = QPushButton("Register Landmark")
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
            self._capture_by_click()

    def _capture_from_selection(self):
        layer = self.iface.activeLayer()
        if not layer:
            self._show_error("No active layer. Please select a vector layer with point features.")
            return

        selected = layer.selectedFeatures()
        if not selected:
            self._show_error("No features selected. Please select a point feature on the active layer.")
            return

        feature = selected[0]
        geom = feature.geometry()

        if geom.isNull() or geom.isEmpty():
            self._show_error("Selected feature has no geometry.")
            return

        if geom.type() != QgsWkbTypes.PointGeometry:
            self._show_error("Selected feature is not a point. Please select a point feature.")
            return

        geom_4326 = self._reproject_to_4326(geom, layer.crs())
        self.captured_geometry = geom_4326
        point = geom_4326.asPoint()
        self.geom_status_label.setText(
            f"Point captured: ({point.x():.6f}, {point.y():.6f}) WGS84")
        self.geom_status_label.setStyleSheet("color: green;")

    def _capture_by_click(self):
        self.hide()
        self.previous_map_tool = self.iface.mapCanvas().mapTool()
        self._click_tool = QgsMapToolEmitPoint(self.iface.mapCanvas())
        self._click_tool.canvasClicked.connect(self._on_point_clicked)
        self.iface.mapCanvas().setMapTool(self._click_tool)
        self.iface.messageBar().pushInfo("TRRCMS", "Click on the map to place the landmark.")

    def _on_point_clicked(self, point, button):
        if self.previous_map_tool:
            self.iface.mapCanvas().setMapTool(self.previous_map_tool)

        canvas_crs = self.iface.mapCanvas().mapSettings().destinationCrs()
        geom = QgsGeometry.fromPointXY(point)
        geom_4326 = self._reproject_to_4326(geom, canvas_crs)
        self.captured_geometry = geom_4326
        pt = geom_4326.asPoint()
        self.geom_status_label.setText(
            f"Point captured: ({pt.x():.6f}, {pt.y():.6f}) WGS84")
        self.geom_status_label.setStyleSheet("color: green;")

        self.show()
        self.raise_()
        self.activateWindow()

    def _on_clear_geometry(self):
        self.captured_geometry = None
        self.geom_status_label.setText("No location captured")
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
            self._show_error("Landmark name is required.")
            return

        # Validate geometry
        if self.captured_geometry is None:
            self._show_error("Please capture the landmark location first.")
            return

        wkt = self.captured_geometry.asWkt()
        landmark_type = self.type_combo.currentData()

        payload = {
            "identifier": int(identifier_text),
            "name": name,
            "type": landmark_type,
            "locationWkt": wkt,
        }

        self.register_btn.setEnabled(False)
        self.status_label.setText("Registering landmark...")
        self.status_label.setStyleSheet("color: gray;")
        QApplication.processEvents()

        try:
            result = self.api_client.register_landmark(payload)

            result_name = ""
            if isinstance(result, dict):
                result_name = result.get("name", result.get("id", ""))

            self.status_label.setText(f"Landmark registered successfully! ({result_name})")
            self.status_label.setStyleSheet("color: green; font-weight: bold;")

            QMessageBox.information(
                self,
                "Success",
                f"Landmark registered successfully!\n\nName: {result_name}"
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
        self.geom_status_label.setText("No location captured")
        self.geom_status_label.setStyleSheet("color: gray;")
        self.status_label.setText("")

    def _show_error(self, message):
        self.status_label.setText(message)
        self.status_label.setStyleSheet("color: red;")
