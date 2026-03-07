"""
TRRCMS Building Register - QGIS Plugin
Registers buildings on the TRRCMS server from QGIS map canvas.
"""


def classFactory(iface):
    from .plugin import TRRCMSBuildingRegisterPlugin
    return TRRCMSBuildingRegisterPlugin(iface)
