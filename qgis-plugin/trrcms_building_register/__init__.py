"""
TRRCMS GIS Register - QGIS Plugin
Registers buildings, landmarks, and streets on the TRRCMS server from QGIS map canvas.
"""


def classFactory(iface):
    from .plugin import TRRCMSBuildingRegisterPlugin
    return TRRCMSBuildingRegisterPlugin(iface)
