"""
Main QGIS plugin class for TRRCMS GIS Registration.
Supports registering buildings, landmarks, and streets on the TRRCMS server.
"""

import os
from qgis.PyQt.QtWidgets import QAction
from qgis.PyQt.QtGui import QIcon

from .login_dialog import LoginDialog
from .register_dialog import RegisterBuildingDialog
from .landmark_dialog import RegisterLandmarkDialog
from .street_dialog import RegisterStreetDialog


class TRRCMSBuildingRegisterPlugin:
    """QGIS Plugin — Register buildings, landmarks, and streets on the TRRCMS server."""

    def __init__(self, iface):
        self.iface = iface
        self.plugin_dir = os.path.dirname(__file__)
        self.actions = []
        self.api_client = None

    def initGui(self):
        """Called when the plugin is loaded. Adds toolbar buttons."""
        icon_path = os.path.join(self.plugin_dir, "icon.png")
        icon = QIcon(icon_path) if os.path.exists(icon_path) else QIcon()

        # Building registration
        action_building = QAction(icon, "TRRCMS Register Building", self.iface.mainWindow())
        action_building.setToolTip("Register a building on the TRRCMS server")
        action_building.triggered.connect(self._run_building)
        self.iface.addToolBarIcon(action_building)
        self.iface.addPluginToMenu("TRRCMS", action_building)
        self.actions.append(action_building)

        # Landmark registration
        action_landmark = QAction(icon, "TRRCMS Register Landmark", self.iface.mainWindow())
        action_landmark.setToolTip("Register a landmark on the TRRCMS server")
        action_landmark.triggered.connect(self._run_landmark)
        self.iface.addToolBarIcon(action_landmark)
        self.iface.addPluginToMenu("TRRCMS", action_landmark)
        self.actions.append(action_landmark)

        # Street registration
        action_street = QAction(icon, "TRRCMS Register Street", self.iface.mainWindow())
        action_street.setToolTip("Register a street on the TRRCMS server")
        action_street.triggered.connect(self._run_street)
        self.iface.addToolBarIcon(action_street)
        self.iface.addPluginToMenu("TRRCMS", action_street)
        self.actions.append(action_street)

    def unload(self):
        """Called when the plugin is unloaded. Removes toolbar buttons."""
        for action in self.actions:
            self.iface.removeToolBarIcon(action)
            self.iface.removePluginMenu("TRRCMS", action)
        self.actions.clear()

    def _ensure_authenticated(self):
        """Show login dialog if not authenticated. Returns True if authenticated."""
        if self.api_client is None or not self.api_client.is_authenticated():
            login_dlg = LoginDialog(self.iface.mainWindow(), self.api_client)
            result = login_dlg.exec_()
            if result != LoginDialog.Accepted:
                return False
            self.api_client = login_dlg.get_api_client()
        return True

    def _run_building(self):
        """Open building registration dialog."""
        if not self._ensure_authenticated():
            return
        dlg = RegisterBuildingDialog(self.iface, self.api_client, self.iface.mainWindow())
        dlg.exec_()

    def _run_landmark(self):
        """Open landmark registration dialog."""
        if not self._ensure_authenticated():
            return
        dlg = RegisterLandmarkDialog(self.iface, self.api_client, self.iface.mainWindow())
        dlg.exec_()

    def _run_street(self):
        """Open street registration dialog."""
        if not self._ensure_authenticated():
            return
        dlg = RegisterStreetDialog(self.iface, self.api_client, self.iface.mainWindow())
        dlg.exec_()
