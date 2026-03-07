"""
Main QGIS plugin class for TRRCMS Building Registration.
"""

import os
from qgis.PyQt.QtWidgets import QAction
from qgis.PyQt.QtGui import QIcon

from .login_dialog import LoginDialog
from .register_dialog import RegisterBuildingDialog


class TRRCMSBuildingRegisterPlugin:
    """QGIS Plugin — Register buildings on the TRRCMS server."""

    def __init__(self, iface):
        self.iface = iface
        self.plugin_dir = os.path.dirname(__file__)
        self.action = None
        self.api_client = None

    def initGui(self):
        """Called when the plugin is loaded. Adds toolbar button."""
        icon_path = os.path.join(self.plugin_dir, "icon.png")
        icon = QIcon(icon_path) if os.path.exists(icon_path) else QIcon()

        self.action = QAction(icon, "TRRCMS Register Building", self.iface.mainWindow())
        self.action.setToolTip("Register a building on the TRRCMS server")
        self.action.triggered.connect(self.run)

        self.iface.addToolBarIcon(self.action)
        self.iface.addPluginToMenu("TRRCMS", self.action)

    def unload(self):
        """Called when the plugin is unloaded. Removes toolbar button."""
        if self.action:
            self.iface.removeToolBarIcon(self.action)
            self.iface.removePluginMenu("TRRCMS", self.action)

    def run(self):
        """Main entry point — show login if needed, then registration dialog."""
        # Show login dialog if not authenticated
        if self.api_client is None or not self.api_client.is_authenticated():
            login_dlg = LoginDialog(self.iface.mainWindow(), self.api_client)
            result = login_dlg.exec_()
            if result != LoginDialog.Accepted:
                return
            self.api_client = login_dlg.get_api_client()

        # Show registration dialog
        register_dlg = RegisterBuildingDialog(self.iface, self.api_client, self.iface.mainWindow())
        register_dlg.exec_()
